(function (window, document, $) {
    "use strict";

    const SELECTORS = Object.freeze({
        table: "#cateTable",
        metaXsrf: 'meta[name="xsrf-token"]',
        detailsModal: "#cateDetailsModal",
        loading: "#cate-details-loading",
        details: "#cate-details",
        empty: "#cate-details-empty",
        editLink: "#cate-edit-link",
        btnDeleteInDetails: "#btn-delete-in-details",
        restaurantRow: ".restaurant-row"
    });

    const STATE = {
        dt: null,
        currentId: null,
    };

    // --- Helpers ---
    const q = (sel, root) => (root || document).querySelector(sel);
    const getXsrfToken = () => q(SELECTORS.metaXsrf)?.content ?? "";
    const antiforgeryHeaders = () => ({
        "Content-Type": "application/json",
        "RequestVerificationToken": getXsrfToken(),
    });
    const safeModal = (selector, options) => {
        const el = q(selector);
        if (!el) return null;
        return bootstrap.Modal.getOrCreateInstance(el, options || { backdrop: "static" });
    };
    const setDetailField = (modalEl, key, value) => {
        const t = modalEl.querySelector(`[data-field="${key}"]`);
        if (t) t.textContent = value ?? "";
    };

    // --- DataTables ---
    function initTable() {
        // ajaxUrl trỏ tới endpoint DataTables của Categories
        STATE.dt = window.initDataTable(SELECTORS.table, {
            ajaxUrl: "/api/categories/datatable",

            // >>> THÊM: renderer callback cho cột actions
            renderers: {
                // key có thể là 'actions' hoặc đúng name/data của cột (vd: 'CategoryId')
                actions: (id, type, row, meta) => {
                    if (!id) return "";
                    return `
                    <div class="d-flex justify-content-center gap-1">
                      <a href="ds-danh-muc/${id}/chinh-sua" class="btn btn-sm btn-primary" title="Sửa">
                        <i class="bi bi-pencil-square"></i>
                      </a>
                      <a href="#" class="btn btn-sm btn-info btn-details" data-id="${id}" title="Xem">
                        <i class="bi bi-info-square"></i>
                      </a>
                    </div>`;
                },
                // Ví dụ: nếu bạn muốn format Priority riêng
                // Priority: (value) => `<span class="badge text-bg-secondary">${value ?? ""}</span>`
            }
        });
    }

    // --- Phần chi tiết ---
    async function showDetailsById(id) {
        const detailsModal = safeModal(SELECTORS.detailsModal);
        const detailsModalEl = q(SELECTORS.detailsModal);
        if (!detailsModal || !detailsModalEl) return;

        STATE.currentId = String(id);

        $(SELECTORS.loading).removeClass("d-none");
        $(SELECTORS.details).addClass("d-none");
        $(SELECTORS.empty).addClass("d-none");
        $(SELECTORS.editLink).addClass("d-none").attr("href", "#");
        // Ẩn hàng "Nhà hàng" (restaurant-row) theo mặc định khi reset
        $(detailsModalEl).find(SELECTORS.restaurantRow).addClass("d-none");

        detailsModal.show();

        try {
            const res = await fetch(`/api/categories/${id}`, { headers: antiforgeryHeaders() });
            if (!res.ok) throw new Error("Load detail failed");
            const dto = await res.json();

            setDetailField(detailsModalEl, "categoryName", dto.categoryName);
            setDetailField(detailsModalEl, "priority", dto.priority);

            // Kiểm tra xem backend có trả về restaurantName không
            if (dto.restaurantName) {// Nếu CÓ (là SuperAdmin):
                // Điền dữ liệu
                setDetailField(detailsModalEl, "restaurantName", dto.restaurantName);
                // Hiển thị hàng "Nhà hàng"
                $(detailsModalEl).find(SELECTORS.restaurantRow).removeClass("d-none");
            }

            $(SELECTORS.editLink).removeClass("d-none").attr("href", `ds-danh-muc/${id}/chinh-sua`);

            $(SELECTORS.loading).addClass("d-none");
            $(SELECTORS.details).removeClass("d-none");
        } catch (err) {
            console.error(err);
            $(SELECTORS.loading).addClass("d-none");
            $(SELECTORS.empty).removeClass("d-none");
        }
    }

    function onClickDeleteInDetails() {
        const id = STATE.currentId;
        if (!id) return;
        const doDelete = () =>
            fetch(`/api/categories/${id}`, {
                method: "DELETE",
                headers: antiforgeryHeaders()
            });

        if (window.Swal) {
            Swal.fire({
                icon: "warning",
                title: "Xoá danh mục?",
                text: "Hành động này không thể hoàn tác.",
                showCancelButton: true,
                confirmButtonText: "Xoá",
                cancelButtonText: "Huỷ"
            }).then((res) => {
                if (res.isConfirmed) {
                    doDelete()
                        .then((r) => {
                            if (!r.ok) throw r;
                            return;
                        })
                        .then(() => {
                            Swal.fire({ icon: "success", title: "Đã xoá!" });
                            STATE.dt?.ajax?.reload(null, false);
                            const modalEl = q(SELECTORS.detailsModal);
                            const bsModal = bootstrap.Modal.getOrCreateInstance(modalEl);
                            bsModal.hide();
                        })
                        .catch(() => Swal.fire({ icon: "error", title: "Xoá thất bại" }));
                }
            });
            return;
        }

        // fallback
        doDelete().then(() => STATE.dt?.ajax?.reload(null, false));
    }

    function bindEvents() {
        // delegate click nút xem
        document.addEventListener("click", (e) => {
            const btn = e.target.closest(".btn-details");
            if (!btn) return;
            e.preventDefault();
            const id = btn.getAttribute("data-id");
            showDetailsById(id);
        });

        // xoá trong modal
        const btnDel = q(SELECTORS.btnDeleteInDetails);
        if (btnDel) btnDel.addEventListener("click", onClickDeleteInDetails);
    }

    document.addEventListener("DOMContentLoaded", () => {
        initTable();
        bindEvents();
    });

})(window, document, jQuery);
