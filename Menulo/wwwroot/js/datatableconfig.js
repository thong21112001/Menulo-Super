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

            // Ưu tiên 1: Tìm renderer khớp với 'data-type' (e.g., "count-link", "actions")
            // Đây là logic "bá đạo" nhất, nó sẽ bắt được BẤT KỲ data-type tùy chỉnh nào
            // mà bạn định nghĩa trong file ...-index.js
            if (type && typeof renderers[type] === "function") {
                colDef.render = renderers[type];
                colDef.orderable = false;
                colDef.searchable = false;
            }
            // Ưu tiên 2: Logic mặc định cho 'index'
            else if (type === "index") {
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
                const custom = typeof renderers[key] === "function"
                    ? renderers[key]
                    : (typeof renderers.actions === "function" ? renderers.actions : null);

                const fnName = $th.data("render-fn");
                const globalFn = fnName && typeof window[fnName] === "function" ? window[fnName] : null;

                if (custom) {
                    colDef.render = custom;
                } else if (globalFn) {
                    colDef.render = globalFn;
                } else {
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
                const key = name || data;
                if (key && typeof renderers[key] === "function") {
                    colDef.render = renderers[key];
                }
            }

            const override = overridesMap[name] || overridesMap[data];
            if (override) Object.assign(colDef, override);

            columns.push(colDef);
        });

        return columns;
    }

    /**
     * Khởi tạo DataTable dùng chung
     */
    function initDataTable(tableSelector, options = {}) {
        const $table = $(tableSelector);
        const serverSide = options.serverSide !== false;
        const ajaxUrl = options.ajaxUrl || $table.data("ajax");

        const columns = buildColumnsFromThead(tableSelector, options);

        /** @type import("datatables.net").Settings */
        const dtConfig = {
            processing: true,
            serverSide,
            responsive: false, // TẮT chế độ responsive mặc định
            scrollX: true,     // BẬT chế độ cuộn ngang
            autoWidth: false,
            order: [],
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

        // Tự động điều chỉnh lại cột khi resize trình duyệt
        // Dùng debounce để tránh gọi liên tục, chỉ gọi khi người dùng dừng resize
        let resizeTimer;
        $(window).on('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                dt.columns.adjust();
            }, 250); // delay 250ms
        });

        return dt;
    }

    window.initDataTable = initDataTable;

})(window, document, jQuery);