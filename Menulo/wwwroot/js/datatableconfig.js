// datatableconfig.js
// Khởi tạo DataTable dùng chung + hỗ trợ truyền renderer callback qua options.renderers

(function (window, document, $) {
    "use strict";

    // --- Helpers chung ---
    const q = (sel, root) => (root || document).querySelector(sel);
    const getXsrfToken = () => q('meta[name="xsrf-token"]')?.content ?? "";
    const antiforgeryHeaders = () => ({
        "Content-Type": "application/json",
        "RequestVerificationToken": getXsrfToken(),
    });

    /**
     * Đọc <thead> để build columns cho DataTables
     * Ưu tiên render theo thứ tự:
     *   1) options.renderers[key] (key = name || data || 'actions')
     *   2) data-render-fn="globalFnName" (hàm global trên window)
     *   3) default behaviors (index/actions mặc định)
     */
    function buildColumnsFromThead(tableSelector, options = {}) {
        const columns = [];
        const $table = $(tableSelector);
        const basePath = $table.data("base-path");
        const renderers = options.renderers || {};
        const overridesMap = options.columnsOverrides || {};
        const $thead = $table.find("thead tr").first();
        const $ths = $thead.find("th");

        $ths.each(function () {
            const $th = $(this);
            const data = $th.data("data");
            const name = $th.data("name");
            const type = ($th.data("type") || "").toString().toLowerCase();
            const orderable = !$th.hasClass("no-sort");
            const searchable = !$th.hasClass("no-search");
            const className = $th.attr("class");

            /** @type import("datatables.net").ColumnSettings */
            const colDef = { data, name, orderable, searchable };
            if (className) colDef.className = className;

            // --- Cột đặc biệt: auto index (#) ---
            if (type === "index") {
                colDef.data = null;
                colDef.orderable = false;
                colDef.searchable = false;
                colDef.render = function (_data, _type, _row, meta) {
                    return meta.row + 1 + meta.settings._iDisplayStart;
                };
            }
            // --- Cột đặc biệt: actions (tuỳ biến) ---
            else if (type === "actions") {
                colDef.data = data; // thường là Id
                colDef.orderable = false;
                colDef.searchable = false;

                const key = name || data || "actions";

                // 1) options.renderers
                const custom = typeof renderers[key] === "function"
                    ? renderers[key]
                    : (typeof renderers.actions === "function" ? renderers.actions : null);

                // 2) data-render-fn="globalFnName"
                const fnName = $th.data("render-fn");
                const globalFn = fnName && typeof window[fnName] === "function" ? window[fnName] : null;

                if (custom) {
                    colDef.render = custom; // dùng callback truyền vào
                } else if (globalFn) {
                    colDef.render = globalFn; // fallback dùng hàm global (nếu có)
                } else {
                    // 3) default (nhẹ nhàng): nếu có basePath thì cho Edit + View cơ bản
                    if (basePath) {
                        colDef.render = function (id) {
                            if (!id) return "";
                            return `
                                <div class="d-flex justify-content-center gap-1">
                                  <a href="${basePath}/Edit?id=${id}" class="btn btn-sm btn-primary" title="Sửa">
                                    <i class="bi bi-pencil-square"></i>
                                  </a>
                                  <a href="#" class="btn btn-sm btn-info btn-details" data-id="${id}" title="Xem">
                                    <i class="bi bi-info-square"></i>
                                  </a>
                                </div>`;
                        };
                    }
                }
            }
            // --- Cột dữ liệu thường ---
            else {
                // giữ nguyên data/name; nếu cần format riêng hãy xài options.columnsOverrides hoặc options.renderers[key]
                const key = name || data;
                if (key && typeof renderers[key] === "function") {
                    colDef.render = renderers[key];
                }
            }

            // Cho phép override nhanh 1 số thuộc tính cột theo tên/data
            const override = overridesMap[name] || overridesMap[data];
            if (override) Object.assign(colDef, override);

            columns.push(colDef);
        });

        return columns;
    }

    /**
     * Khởi tạo DataTable dùng chung
     * @param {string} tableSelector - selector tới <table>
     * @param {object} options - cấu hình mở rộng
     *   - ajaxUrl: string (bắt buộc nếu serverSide)
     *   - serverSide: boolean (default: true)
     *   - renderers: { [key: string]: (data, type, row, meta) => string }
     *   - columnsOverrides: { [key: string]: Partial<ColumnSettings> }
     *   - language: object (ghi đè i18n)
     *   - ajaxData: (d) => void (bổ sung param cho request)
     */
    function initDataTable(tableSelector, options = {}) {
        const $table = $(tableSelector);
        const serverSide = options.serverSide !== false; // mặc định true
        const ajaxUrl = options.ajaxUrl || $table.data("ajax");

        const columns = buildColumnsFromThead(tableSelector, options);

        /** @type import("datatables.net").Settings */
        const dtConfig = {
            processing: true,
            serverSide,
            responsive: true,
            autoWidth: false,
            order: [], // để client quyết định từ <th> hoặc tự set
            columns,
            language: Object.assign(
                {
                    processing: "Đang tải...",
                    lengthMenu: "Hiển thị _MENU_ dòng",
                    zeroRecords: "Không có dữ liệu",
                    info: "Hiển thị từ _START_ đến _END_ dòng. Tổng: _TOTAL_ dòng",
                    infoEmpty: "Không có dữ liệu",
                    infoFiltered: "(lọc từ _MAX_ tổng dòng)",
                    search: "Tìm:",
                    paginate: { first: "Đầu", last: "Cuối", next: "›", previous: "‹" },
                },
                options.language || {}
            ),
            drawCallback: function () {
                // Nếu có cột index, đã render trong colDef
                // Căn chỉnh lại cột (tránh lệch khi hiển thị trong tab/modal)
                $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
            }
        };

        if (serverSide) {
            if (!ajaxUrl) {
                console.error("initDataTable: thiếu ajaxUrl cho serverSide mode.");
            }
            dtConfig.ajax = {
                url: ajaxUrl,
                type: "POST",
                contentType: "application/json",
                dataType: "json",
                headers: antiforgeryHeaders(),
                data: function (d) {
                    // DataTables gửi object phức tạp => stringify
                    if (typeof options.ajaxData === "function") options.ajaxData(d);
                    return JSON.stringify(d);
                },
                error: function (xhr) {
                    const msg = xhr?.responseJSON?.message || xhr?.statusText || "Có lỗi khi tải dữ liệu";
                    if (window.Swal) {
                        Swal.fire({ icon: "error", title: "Lỗi", text: msg });
                    } else {
                        alert(msg);
                    }
                }
            };
        }

        const dt = $table.DataTable(dtConfig);
        return dt;
    }

    // xuất hàm global
    window.initDataTable = initDataTable;

})(window, document, jQuery);