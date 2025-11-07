(function (window, document, $) {
    "use strict";

    const SELECTORS = Object.freeze({
        table: "#menuITable",
        metaXsrf: 'meta[name="xsrf-token"]',
        detailsModal: "#menuIDetailsModal",
        loading: "#menui-details-loading",
        details: "#menui-details",
        empty: "#menui-details-empty",
        editLink: "#menui-edit-link",
        btnDeleteInDetails: "#btn-delete-in-details",
        logo: "#menui-logo",
        logoEmpty: "#menui-logo-empty"
    });

    const STATE = { dt: null };

    // --- Helpers ---
    const q = (sel, root) => (root || document).querySelector(sel);
    const getXsrfToken = () => q(SELECTORS.metaXsrf)?.content ?? "";
    const antiforgeryHeaders = (includeContentType = false) => {
        const headers = { "RequestVerificationToken": getXsrfToken() };
        if (includeContentType) headers["Content-Type"] = "application/json";
        return headers;
    };
    const safeModal = (selector, options) => {
        const el = q(selector);
        if (!el) return null;
        return bootstrap.Modal.getOrCreateInstance(el, options || { backdrop: "static" });
    };
    const setDetailField = (modalEl, key, value) => {
        const t = modalEl.querySelector(`[data-field="${key}"]`);
        if (t) t.textContent = value ?? "";
    };
    // escape HTML để tránh XSS khi render description
    function escapeHtml(unsafe) {
        if (unsafe === null || unsafe === undefined) return "";
        return String(unsafe)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function formatPrice(price) {
        if (price === null || price === undefined) return "Theo thời giá";
        if (price === 0) return "Miễn phí";
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
    }

    // Cập nhật UI trong hàng hiện tại sau khi toggle
    function updateRowUI(button, isAvailable) {
        const trEl = button.closest('tr');
        if (!trEl) {
            console.warn("Không tìm thấy <tr> gần nút toggle.");
            button.disabled = false;
            STATE.dt?.ajax?.reload(null, false);
            return;
        }

        // datatableconfig.js luôn gắn data-type lowercase => 'isavailable'
        const statusCell =
            trEl.querySelector('td[data-type="isavailable"]') ||
            trEl.querySelector('td[data-type="isAvailable"]');

        if (!statusCell) {
            console.warn("Không tìm thấy ô trạng thái trong hàng:", trEl);
            button.disabled = false;
            STATE.dt?.ajax?.reload(null, false);
            return;
        }

        if (isAvailable) {
            statusCell.innerHTML = '<span class="badge bg-success">Đang bán</span>';
            button.title = 'Tạm ẩn món';
            button.innerHTML = '<i class="bi bi-eye-slash-fill"></i>';
        } else {
            statusCell.innerHTML = '<span class="badge bg-secondary">Tạm ẩn</span>';
            button.title = 'Bán lại';
            button.innerHTML = '<i class="bi bi-eye-fill"></i>';
        }
        button.disabled = false;
    }

    // --- DataTables ---
    function initTable() {
        const $tbl = $(SELECTORS.table);
        if ($.fn.dataTable.isDataTable($tbl)) {
            console.warn('DataTable already initialized, skip init.');
            STATE.dt = $tbl.DataTable();
            return;
        }

        STATE.dt = window.initDataTable(SELECTORS.table, {
            ajaxUrl: "/api/menuitems/datatable",

            renderers: {
                // LƯU Ý: key phải lowercase để khớp data-type đã bị toLowerCase()
                "price": (data) => formatPrice(data),

                "description": (data) => {
                    if (data === null || data === undefined) return ""; // hoặc return "—";
                    // giới hạn độ dài hiển thị trong table để tránh phá layout
                    const MAX = 60;
                    const text = String(data);
                    const trimmed = text.length > MAX ? text.slice(0, MAX).trim() + "…" : text;
                    return `<span title="${escapeHtml(text)}">${escapeHtml(trimmed)}</span>`;
                },

                // CHỖ QUAN TRỌNG: dùng 'isavailable' (lowercase), không phải 'isAvailable'
                "isavailable": (data) => {
                    return data
                        ? '<span class="badge bg-success">Đang bán</span>'
                        : '<span class="badge bg-secondary">Tạm ẩn</span>';
                },

                "actions": (id, _type, row) => {
                    const isAvailable = row.isAvailable;
                    const toggleIcon = isAvailable ? "bi-eye-slash-fill" : "bi-eye-fill";
                    const toggleTitle = isAvailable ? "Tạm ẩn món" : "Bán lại";

                    return `
                    <div class="d-flex justify-content-center gap-1">
                        <button type="button" 
                                class="btn btn-sm btn-outline-secondary toggle-availability-btn"
                                data-id="${id}"
                                title="${toggleTitle}">
                            <i class="bi ${toggleIcon}"></i>
                        </button>

                        <a href="/ds-mon-an/${id}/chinh-sua" class="btn btn-sm btn-primary" title="Sửa">
                            <i class="bi bi-pencil-square"></i>
                        </a>

                        <a href="#" class="btn btn-sm btn-info btn-details" data-id="${id}" title="Xem">
                            <i class="bi bi-info-square"></i>
                        </a>
                    </div>`;
                }
            }
        });
    }

    // --- Toggle ---
    async function handleToggleAvailability(e) {
        const button = e.currentTarget;
        const id = button?.dataset?.id;
        if (!id) return;

        button.disabled = true;
        button.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';

        try {
            const response = await fetch(`/api/menuitems/${id}/toggle-availability`, {
                method: 'PATCH',
                headers: antiforgeryHeaders()
            });
            const data = await response.json();
            if (!response.ok) throw new Error(data.message || "Lỗi không xác định");

            updateRowUI(button, data.isAvailable);

            const Toast = Swal.mixin({
                toast: true, position: 'top-end', showConfirmButton: false,
                timer: 3000, timerProgressBar: true
            });
            Toast.fire({ icon: 'success', title: data.message });

        } catch (error) {
            console.error('Lỗi AJAX:', error);
            Swal.fire('Lỗi!', error.message || 'Đã xảy ra lỗi', 'error');
            STATE.dt?.ajax?.reload(null, false);
        }
    }

    // --- Chi tiết (Details) ---
    async function showDetailsById(id) {
        const detailsModal = safeModal(SELECTORS.detailsModal);
        const detailsModalEl = q(SELECTORS.detailsModal);
        if (!detailsModal || !detailsModalEl) return;

        STATE.currentId = String(id);

        $(SELECTORS.loading).removeClass("d-none");
        $(SELECTORS.details).addClass("d-none");
        $(SELECTORS.empty).addClass("d-none");
        $(SELECTORS.editLink).addClass("d-none").attr("href", "#");
        $(SELECTORS.btnDeleteInDetails).addClass("d-none").removeAttr("data-id");

        // reset logo
        const logoEl = q(SELECTORS.logo);
        const logoEmpty = q(SELECTORS.logoEmpty);
        if (logoEl) { logoEl.src = ""; logoEl.classList.add("d-none"); }
        if (logoEmpty) logoEmpty.classList.remove("d-none");

        detailsModal.show();

        try {
            const res = await fetch(`/api/menuitems/${id}`, { headers: antiforgeryHeaders() });
            if (!res.ok) throw new Error("Load detail failed");
            const dto = await res.json();

            setDetailField(detailsModalEl, "name", dto.itemName);
            setDetailField(detailsModalEl, "description", dto.description ?? "—");
            setDetailField(detailsModalEl, "price", formatPrice(dto.price) ?? "—");
            setDetailField(detailsModalEl, "categoryName", dto.categoryName);

            // Hình món (chỉ có ở Details)s
            if (dto.imageData && logoEl && logoEmpty) {
                let logoSrc = `/api/images/menuitems/${id}?w=400&h=300`;
                logoEl.src = logoSrc;
                // Thêm loading="lazy" vào thẻ img trong file .cshtml của modal nếu cần
                logoEl.classList.remove("d-none");
                logoEmpty.classList.add("d-none");
            }

            // Footer
            $(SELECTORS.editLink).removeClass("d-none").attr("href", `/ds-mon-an/${id}/chinh-sua`);
            $(SELECTORS.btnDeleteInDetails).removeClass("d-none").attr("data-id", String(id));
            $(SELECTORS.loading).addClass("d-none");
            $(SELECTORS.details).removeClass("d-none");
        } catch (err) {
            console.error(err);
            $(SELECTORS.loading).addClass("d-none");
            $(SELECTORS.empty).removeClass("d-none");
        }
    }

    function bindEvents() {
        // Chỉ bắt click trên nút toggle, tránh lọc thủ công trong tbody
        $(SELECTORS.table).on('click', '.toggle-availability-btn', handleToggleAvailability);

        // mở chi tiết
        document.addEventListener("click", (e) => {
            const btn = e.target.closest(".btn-details");
            if (!btn) return;
            e.preventDefault();
            const id = btn.getAttribute("data-id");
            showDetailsById(id);
        });
    }

    document.addEventListener("DOMContentLoaded", () => {
        initTable();
        bindEvents();
    });

})(window, document, jQuery);