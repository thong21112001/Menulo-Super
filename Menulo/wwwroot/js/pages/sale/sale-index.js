// Bọc trong một IIFE (Immediately Invoked Function Expression)
(function (window, document, $) {
    "use strict";

    // <<< Cấu hình URL trang Edit Sale tại đây >>>
    const PAGE_URLS = Object.freeze({
        edit: "/sa/ds-sale" // Ví dụ: /Superadmin/Sales/Edit
        // URL cuối cùng sẽ là: /sa/ds-sale/{id}
    });

    const SELECTORS = Object.freeze({
        table: "#saleTable", // <<< Đổi id table
        metaXsrf: 'meta[name="xsrf-token"]',
        detailsModal: "#saleDetailsModal",
        loading: "#sale-details-loading",
        details: "#sale-details",
        empty: "#sale-details-empty",
        editLink: "#sale-edit-link",
        btnDeleteInDetails: "#btn-delete-in-details"
    });

    const STATE = {
        dt: null,
        currentId: null
    };

    // --- Helpers ---
    // Các hàm helper này được giữ nguyên từ file res-index.js của bạn
    const q = (sel, root) => (root || document).querySelector(sel);
    const getXsrfToken = () => q(SELECTORS.metaXsrf)?.content ?? "";
    const antiforgeryHeaders = () => ({
        "Content-Type": "application/json",
        "RequestVerificationToken": getXsrfToken()
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
        //
        STATE.dt = window.initDataTable(SELECTORS.table, {
            ajaxUrl: "/api/sales/datatable",  // <<< endpoint Sale API
            renderers: {
                "count-link": (data, type, row, meta) => {
                    // 'data' chính là giá trị của 'restaurantCount'
                    const count = parseInt(data, 10) || 0;

                    if (count > 0) {
                        // Nếu có, trả về một thẻ <a>
                        // Chúng ta sẽ thêm data-bs-toggle sau
                        return `
                            <a href="#" 
                               class="fw-bold text-primary show-res-list" 
                               data-sale-id="${row.userId}" 
                               data-sale-name="${row.fullName}"
                               title="Xem danh sách ${count} nhà hàng">
                                ${count}
                            </a>`;
                    }
                    // Nếu không có, chỉ trả về text
                    return `<span class="text-muted">0</span>`;
                },

                "actions": (id, type, row, meta) => {
                    //
                    if (!id) return "";
                    // ID của Sale là string, nên không cần chuyển đổi
                    return `
                      <div class="d-flex justify-content-center gap-1">
                        <a href="${PAGE_URLS.edit}/${id}" class="btn btn-sm btn-primary" title="Sửa">
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

    // --- Chi tiết (Details) ---
    async function showDetailsById(id) {
        //
        const detailsModal = safeModal(SELECTORS.detailsModal);
        const detailsModalEl = q(SELECTORS.detailsModal);
        if (!detailsModal || !detailsModalEl) return;

        STATE.currentId = String(id); // ID của Sale đã là string

        // Reset trạng thái modal
        $(SELECTORS.loading).removeClass("d-none");
        $(SELECTORS.details).addClass("d-none");
        $(SELECTORS.empty).addClass("d-none");
        $(SELECTORS.editLink).addClass("d-none").attr("href", "#");
        $(SELECTORS.btnDeleteInDetails).addClass("d-none").removeAttr("data-id");

        detailsModal.show();

        try {
            const res = await fetch(`/api/sales/${id}`, { headers: antiforgeryHeaders() });
            if (!res.ok) throw new Error("Load detail failed");
            const dto = await res.json(); // SaleDto

            // <<< THAY ĐỔI CÁC TRƯỜNG DỮ LIỆU >>>
            setDetailField(detailsModalEl, "fullName", dto.fullName);
            setDetailField(detailsModalEl, "username", dto.username);
            setDetailField(detailsModalEl, "email", dto.email);
            setDetailField(detailsModalEl, "phoneNumber", dto.phoneNumber ?? "—");

            // Footer (cập nhật link edit)
            $(SELECTORS.editLink).removeClass("d-none").attr("href", `${PAGE_URLS.edit}/${id}`);
            $(SELECTORS.btnDeleteInDetails).removeClass("d-none").attr("data-id", String(id));
            $(SELECTORS.loading).addClass("d-none");
            $(SELECTORS.details).removeClass("d-none");
        } catch (err) {
            console.error(err);
            $(SELECTORS.loading).addClass("d-none");
            $(SELECTORS.empty).removeClass("d-none");
        }
    }

    // --- Xóa (Delete) ---
    async function onClickDeleteInDetails() {
        const id = $(SELECTORS.btnDeleteInDetails).attr("data-id") || STATE.currentId;
        if (!id) return;

        const confirmDelete = async () => {
            const btn = q(SELECTORS.btnDeleteInDetails);
            btn.disabled = true; btn.setAttribute("aria-busy", "true");

            try {
                // <<< THAY ĐỔI API ENDPOINT >>>
                const r = await fetch(`/api/sales/${id}`, {
                    method: "DELETE",
                    headers: antiforgeryHeaders()
                });

                if (r.status === 204) {
                    if (window.Swal) await Swal.fire({ icon: "success", title: "Đã xoá!" });
                    STATE.dt?.ajax?.reload(null, false);
                    bootstrap.Modal.getOrCreateInstance(q(SELECTORS.detailsModal)).hide();
                    return;
                }

                if (r.status === 409) {
                    // <<< THAY ĐỔI TEXT LỖI >>>
                    let msg = "Tài khoản Sale đã phát sinh dữ liệu nên không thể xoá.";
                    try {
                        const json = await r.json();
                        if (json?.message) msg = json.message;
                    } catch { }
                    if (window.Swal) await Swal.fire({ icon: "info", title: "Không thể xoá", text: msg });
                    else alert(msg);
                    return;
                }

                // Lỗi khác
                throw r;
            } catch {
                if (window.Swal) Swal.fire({ icon: "error", title: "Xoá thất bại" });
                else alert("Xoá thất bại");
            } finally {
                btn.disabled = false; btn.removeAttribute("aria-busy");
            }
        };

        if (window.Swal) {
            // <<< THAY ĐỔI TEXT XÁC NHẬN >>>
            const { isConfirmed } = await Swal.fire({
                icon: "warning",
                title: "Xoá tài khoản Sale này?",
                text: "Hành động này không thể hoàn tác.",
                showCancelButton: true,
                confirmButtonText: "Xoá",
                cancelButtonText: "Huỷ"
            });
            if (isConfirmed) await confirmDelete();
        } else {
            if (confirm("Xoá tài khoản Sale này?")) await confirmDelete();
        }
    }

    function bindEvents() {
        // mở chi tiết
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

    // --- Khởi chạy ---
    document.addEventListener("DOMContentLoaded", () => {
        initTable();
        bindEvents();
    });

})(window, document, jQuery);