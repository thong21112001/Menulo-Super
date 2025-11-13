"use strict";

// ================== Add item (modal) ==================
function resolveTableId() {
    const input = document.getElementById('modalTableId');
    const current = Number(input?.value || 0);
    if (current) return current;
    const qs = Number(new URLSearchParams(location.search).get('tableId') || 0);
    return qs || 0;
}

function setOrderId(tableId) {
    const input = document.getElementById('modalTableId');
    const id = Number(tableId || resolveTableId() || 0);
    if (!input) return;
    input.value = id || "";
    if (!id) {
        Swal.fire({ icon: 'warning', title: 'Thiếu mã bàn' });
    }
}

function addRow() {
    const select = document.getElementById('batchItemSelect');
    const itemId = select.value;
    const itemName = select.options[select.selectedIndex]?.text;

    if (!itemId) {
        Swal.fire({ icon: 'warning', title: 'Chọn món trước' });
        return;
    }

    const tbody = document.querySelector('#batchItemsTable tbody');
    let existing = tbody.querySelector(`tr[data-id="${itemId}"]`);
    if (existing) {
        let qtyInput = existing.querySelector('.qty');
        qtyInput.value = parseInt(qtyInput.value || "0", 10) + 1;
        return;
    }

    const row = document.createElement('tr');
    row.dataset.id = itemId;
    row.innerHTML = `
    <td>${itemName}</td>
    <td><input type="number" class="form-control qty" value="1" min="1" style="width:80px"></td>
    <td><button class="btn btn-sm btn-danger" onclick="this.closest('tr').remove()"><i class="bi bi-trash"></i></button></td>
  `;
    tbody.appendChild(row);

    // Clear select2 + re-open search
    $('#batchItemSelect').val(null).trigger('change');
}

function collectBatchItems() {
    const rows = document.querySelectorAll('#batchItemsTable tbody tr');
    let items = [];
    rows.forEach(r => {
        const itemId = parseInt(r.dataset.id, 10);
        const qty = parseInt(r.querySelector('.qty').value, 10);
        if (itemId && qty > 0) items.push({ itemId, quantity: qty });
    });
    return items;
}

async function sendBatch() {
    const tableId = resolveTableId();
    const items = collectBatchItems();

    if (!tableId) {
        Swal.fire({ icon: 'warning', title: 'Thiếu mã bàn' });
        return;
    }
    if (!items.length) {
        Swal.fire({ icon: 'warning', title: 'Chưa có món nào' });
        return;
    }

    try {
        const res = await fetch(`/api/Orders/${tableId}/items`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ items })
        });

        if (res.ok) {
            Swal.fire({ icon: 'success', title: 'Đã thêm món vào danh sách chờ' })
                .then(() => location.reload());
        } else {
            const err = await res.json().catch(() => ({}));
            Swal.fire({ icon: 'error', title: 'Thất bại', text: err.message || 'Có lỗi' });
        }
    } catch (e) {
        Swal.fire({ icon: 'error', title: 'Lỗi hệ thống', text: e.message });
    }
}