(function (window, document, $) {
    "use strict";

    const SELECTORS = Object.freeze({
        table: "#resTable",                 // <<< Đổi id table
        metaXsrf: 'meta[name="xsrf-token"]',
        detailsModal: "#resDetailsModal",
        loading: "#res-details-loading",
        details: "#res-details",
        empty: "#res-details-empty",
        editLink: "#res-edit-link",
        btnDeleteInDetails: "#btn-delete-in-details",
        logo: "#res-logo",
        logoEmpty: "#res-logo-empty"
    });

    const STATE = {
        dt: null,
        currentId: null
    };

    // --- Helpers ---
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
    const fmtDate = (iso) => {
        if (!iso) return "—";
        try {
            const d = new Date(iso);
            return d.toLocaleString("vi-VN", {
                day: "2-digit", month: "2-digit", year: "numeric",
                hour: "2-digit", minute: "2-digit"
            });
        } catch { return "—"; }
    };

    // --- DataTables ---
    function initTable() {
        const $tbl = $(SELECTORS.table);
        if ($.fn.dataTable.isDataTable($tbl)) {
            console.warn('DataTable already initialized, skip init.');
            STATE.dt = $tbl.DataTable();
            return;
        }

        STATE.dt = window.initDataTable(SELECTORS.table, {
            ajaxUrl: "/api/restaurants/datatable",  // <<< endpoint Restaurant API
            renderers: {
                actions: (id, type, row, meta) => {
                    if (!id) return "";
                    return `
                      <div class="d-flex justify-content-center gap-1">
                        <a href="/sa/ds-nha-hang/${id}/chinh-sua" class="btn btn-sm btn-primary" title="Sửa">
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
            const res = await fetch(`/api/restaurants/${id}`, { headers: antiforgeryHeaders() });
            if (!res.ok) throw new Error("Load detail failed");
            const dto = await res.json(); // RestaurantDetailsDto

            setDetailField(detailsModalEl, "name", dto.name);
            setDetailField(detailsModalEl, "address", dto.address ?? "—");
            setDetailField(detailsModalEl, "phone", dto.phone ?? "—");
            setDetailField(detailsModalEl, "createdAt", fmtDate(dto.createdAt));

            // Logo (chỉ có ở Details)s
            if (dto.logoUrl && logoEl && logoEmpty) {
                let logoSrc = `/api/images/restaurants/${id}/logo?w=300&h=300`;
                if (dto.logoUpdatedAtUtc) {
                    const version = new Date(dto.logoUpdatedAtUtc).getTime();
                    logoSrc += `&v=${version}`;
                }
                logoEl.src = logoSrc;
                // Thêm loading="lazy" vào thẻ img trong file .cshtml của modal nếu cần
                logoEl.classList.remove("d-none");
                logoEmpty.classList.add("d-none");
            }

            // Footer
            $(SELECTORS.editLink).removeClass("d-none").attr("href", `/sa/ds-nha-hang/${id}/chinh-sua`);
            $(SELECTORS.btnDeleteInDetails).removeClass("d-none").attr("data-id", String(id));
            $(SELECTORS.loading).addClass("d-none");
            $(SELECTORS.details).removeClass("d-none");
        } catch (err) {
            console.error(err);
            $(SELECTORS.loading).addClass("d-none");
            $(SELECTORS.empty).removeClass("d-none");
        }
    }

    async function onClickDeleteInDetails() {
        const id = $(SELECTORS.btnDeleteInDetails).attr("data-id") || STATE.currentId;
        if (!id) return;

        const confirmDelete = async () => {
            const btn = q(SELECTORS.btnDeleteInDetails);
            btn.disabled = true; btn.setAttribute("aria-busy", "true");

            try {
                const r = await fetch(`/api/restaurants/${id}`, {
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
                    let msg = "Nhà hàng đã phát sinh dữ liệu nên không thể xoá.";
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
            const { isConfirmed } = await Swal.fire({
                icon: "warning",
                title: "Xoá nhà hàng này?",
                text: "Hành động này không thể hoàn tác.",
                showCancelButton: true,
                confirmButtonText: "Xoá",
                cancelButtonText: "Huỷ"
            });
            if (isConfirmed) await confirmDelete();
        } else {
            if (confirm("Xoá nhà hàng này?")) await confirmDelete();
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

    document.addEventListener("DOMContentLoaded", () => {
        initTable();
        bindEvents();
    });

})(window, document, jQuery);
