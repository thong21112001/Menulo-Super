"use strict";

(function (window, document, $) {
    const SELECTORS = Object.freeze({
        table: "#cateTable",
        metaXsrf: 'meta[name="xsrf-token"]',
        detailsModal: "#cateDetailsModal",
        loading: "#cate-details-loading",
        details: "#cate-details",
        empty: "#cate-details-empty",
        editLink: "#cate-edit-link",
        btnDeleteInDetails: "#btn-delete-in-details",
    });

    const STATE = {
        dt: null,
        basePath: "/Categories",
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
        const tableEl = q(SELECTORS.table);
        if (!tableEl) return null;

        STATE.basePath = tableEl.dataset.basePath || "/Categories";

        const dt = window.initDataTable(SELECTORS.table, {
            ajaxUrl: "/api/categories/datatable",
        });

        setTimeout(() => dt.columns.adjust().draw(false), 50);
        $(window).on("resize.DT-cateTable", () => dt.columns.adjust());
        $('a[data-bs-toggle="tab"], .modal').on("shown.bs.tab shown.bs.modal", () => dt.columns.adjust());
        $(window).one("unload", () => $(window).off("resize.DT-cateTable"));

        return dt;
    }

    // --- Details ---
    async function showDetailsById(id) {
        const detailsModal = safeModal(SELECTORS.detailsModal);
        const detailsModalEl = q(SELECTORS.detailsModal);
        if (!detailsModal || !detailsModalEl) return;

        STATE.currentId = String(id);

        $(SELECTORS.loading).removeClass("d-none");
        $(SELECTORS.details).addClass("d-none");
        $(SELECTORS.empty).addClass("d-none");
        $(SELECTORS.editLink).addClass("d-none").attr("href", "#");

        detailsModal.show();

        try {
            const res = await fetch(`/api/categories/${id}`, { headers: antiforgeryHeaders() });
            if (!res.ok) throw new Error("Load detail failed");
            const dto = await res.json();

            setDetailField(detailsModalEl, "categoryName", dto.categoryName);
            setDetailField(detailsModalEl, "restaurantName", dto.restaurantName);
            setDetailField(detailsModalEl, "priority", dto.priority);

            $(SELECTORS.editLink).removeClass("d-none").attr("href", `${STATE.basePath}/Edit?id=${id}`);

            $(SELECTORS.loading).addClass("d-none");
            $(SELECTORS.details).removeClass("d-none");
        } catch (err) {
            console.error(err);
            $(SELECTORS.loading).addClass("d-none");
            $(SELECTORS.empty).removeClass("d-none");
        }
    }

    function onClickDetails(e) {
        e.preventDefault();
        const id = this.getAttribute("data-id");
        if (id) showDetailsById(id);
    }

    // --- Delete from details ---
    async function onClickDeleteInDetails() {
        if (!STATE.currentId) return;

        const name = q(`${SELECTORS.details} [data-field="categoryName"]`)?.textContent?.trim() || "";
        const confirmed = await Swal.fire({
            icon: "warning",
            title: "Xóa danh mục?",
            text: `Bạn chắc chắn muốn xóa ${name}? Hành động này không thể hoàn tác.`,
            showCancelButton: true,
            confirmButtonText: "Xóa",
            cancelButtonText: "Hủy",
            confirmButtonColor: "#d33",
        }).then(r => r.isConfirmed);

        if (!confirmed) return;

        const btn = q(SELECTORS.btnDeleteInDetails);
        const detailsModal = safeModal(SELECTORS.detailsModal);

        btn.disabled = true;
        const prevHtml = btn.innerHTML;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang xóa...';

        try {
            const res = await fetch(`/api/categories/${STATE.currentId}`, {
                method: "DELETE",
                headers: antiforgeryHeaders(),
            });
            if (!res.ok) {
                const txt = await res.text();
                throw new Error(txt || "Delete failed");
            }

            detailsModal?.hide();
            const table = $(SELECTORS.table).DataTable();
            table.ajax.reload(null, false);

            Swal.fire({ icon: "success", title: "Đã xóa danh mục!" });
            STATE.currentId = null;
        } catch (err) {
            console.error(err);
            Swal.fire({ icon: "error", title: "Xóa thất bại", text: err.message });
        } finally {
            btn.disabled = false;
            btn.innerHTML = prevHtml;
        }
    }

    // --- Bind & Boot ---
    function bindEvents() {
        // Nút xem chi tiết trong bảng
        $(document).off("click.CateDetails", `${SELECTORS.table} .btn-details`);
        $(document).on("click.CateDetails", `${SELECTORS.table} .btn-details`, onClickDetails);

        // Nút XÓA trong footer modal chi tiết
        const btnDel = q(SELECTORS.btnDeleteInDetails);
        if (btnDel) {
            btnDel.removeEventListener("click", onClickDeleteInDetails);
            btnDel.addEventListener("click", onClickDeleteInDetails);
        }
    }

    function ready() {
        if (!window.bootstrap || !bootstrap.Modal) {
            console.error("[Categories] Bootstrap 5 chưa sẵn sàng.");
            return;
        }
        STATE.dt = initTable();
        bindEvents();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", ready);
    } else {
        ready();
    }
})(window, document, window.jQuery);