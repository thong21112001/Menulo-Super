"use strict";

(function () {
    let cached = null; // cache 1 lần cho nhẹ

    function formatPrice(v) {
        try { return new Intl.NumberFormat('vi-VN').format(v) + ' đ'; } catch { return v + ' đ'; }
    }

    async function fetchMenu() {
        if (cached) return cached;
        const res = await fetch('/api/admin/menu-items/simple');
        if (!res.ok) throw new Error('Không tải được menu');
        cached = await res.json(); // [{itemId,itemName,price}]
        return cached;
    }

    async function populateSelect() {
        const select = document.getElementById('batchItemSelect');
        if (!select) return;

        // clear all except placeholder (option đầu tiên)
        select.querySelectorAll('option:not([disabled])').forEach(o => o.remove());

        const data = await fetchMenu();
        if (!data.length) {
            const opt = document.createElement("option");
            opt.disabled = true;
            opt.textContent = "(Chưa có món khả dụng)";
            select.appendChild(opt);
        } else {
            const frag = document.createDocumentFragment();
            for (const it of data) {
                const opt = document.createElement("option");
                opt.value = it.itemId;
                opt.textContent = `${it.itemName} - ${formatPrice(it.price)}`;
                frag.appendChild(opt);
            }
            select.appendChild(frag);
        }

        // đồng bộ Select2 nếu đã init
        if (window.$ && $("#batchItemSelect").data("select2")) {
            $("#batchItemSelect").val(null).trigger("change");
        }
    }

    // nạp khi mở modal để đảm bảo dữ liệu mới
    document.addEventListener('DOMContentLoaded', () => {
        const $select = $("#batchItemSelect");
        const $modal = $("#addItemsBatchModal");

        // Init Select2
        if ($select.length) {
            $select.select2({
                placeholder: "-- Gõ tên món để tìm kiếm --",
                allowClear: true,
                dropdownParent: $modal,
                width: "100%"
            });
        }

        // Trước khi modal hiển thị: nạp list menu mới nhất
        const modalEl = document.getElementById("addItemsBatchModal");
        if (modalEl) {
            modalEl.addEventListener("show.bs.modal", async () => {
                try {
                    await populateSelect();
                    // reset value mỗi lần mở
                    $("#batchItemSelect").val(null).trigger("change");
                } catch (err) {
                    console.error(err);
                    Swal.fire({ icon: "error", title: "Không tải được menu", text: String(err.message || err) });
                }
            });
        }

        // Khi modal đã hiển thị: auto mở dropdown + focus ô search
        $modal.on("shown.bs.modal", function () {
            setTimeout(function () {
                $select.select2("open");
                const searchField = document.querySelector(".select2-search__field");
                if (searchField) searchField.focus();
            }, 100);
        });

        // Khi modal đóng: clear bảng & reset select
        $modal.on("hidden.bs.modal", function () {
            const tbody = document.querySelector("#batchItemsTable tbody");
            if (tbody) tbody.innerHTML = "";
            $select.val(null).trigger("change");
        });
    });
})();