(function (window, document, $) {
    "use strict";

    const SELECTORS = Object.freeze({
        table: "#menuITable",
        metaXsrf: 'meta[name="xsrf-token"]',
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
        STATE.dt = window.initDataTable(SELECTORS.table, {
            ajaxUrl: "/api/menuitems/datatable",

            renderers: {
                // LƯU Ý: key phải lowercase để khớp data-type đã bị toLowerCase()
                "price": (data) => formatPrice(data),

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

    function bindEvents() {
        // Chỉ bắt click trên nút toggle, tránh lọc thủ công trong tbody
        $(SELECTORS.table).on('click', '.toggle-availability-btn', handleToggleAvailability);

    }

    document.addEventListener("DOMContentLoaded", () => {
        initTable();
        bindEvents();
    });

})(window, document, jQuery);