document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById("items-container");
    const template = document.getElementById("item-template");
    const addButton = document.getElementById("add-item-btn");

    if (!container || !template || !addButton) return;

    // Hàm cập nhật lại name-attribute của các input
    function updateRowIndices() {
        const rows = container.querySelectorAll(".item-row");
        rows.forEach((row, index) => {
            // Cập nhật tất cả input/span
            row.querySelectorAll("[data-name]").forEach(input => {
                const name = input.getAttribute("data-name");
                input.name = `Items[${index}].${name}`;
            });
            row.querySelectorAll("[data-valmsg-for]").forEach(span => {
                const name = span.getAttribute("data-valmsg-for");
                span.setAttribute("data-valmsg-for", `Items[${index}].${name}`);
            });
        });
    }

    // Nút "Thêm món"
    addButton.addEventListener("click", function () {
        // Sao chép nội dung của template
        const newRow = template.firstElementChild.cloneNode(true);

        // Thêm hàng mới vào container
        container.appendChild(newRow);

        // Cập nhật lại chỉ số (index) cho tất cả các hàng
        updateRowIndices();

        // (Tùy chọn) Kích hoạt jQuery validation cho hàng mới nếu bạn dùng
        const form = $(container).closest('form');
        if (form.length > 0 && form.data('validator')) {
            form.removeData('validator');
            form.removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(form);
        }
    });

    // Nút "Xóa hàng" (dùng event delegation)
    container.addEventListener("click", function (e) {
        const removeButton = e.target.closest(".remove-item-btn");
        if (removeButton) {
            // Chỉ xóa nếu còn nhiều hơn 1 hàng
            if (container.querySelectorAll(".item-row").length > 1) {
                removeButton.closest(".item-row").remove();
                // Cập nhật lại chỉ số (index)
                updateRowIndices();
            } else {
                alert("Bạn phải giữ lại ít nhất một món.");
            }
        }
    });

    // Khởi tạo chỉ số (index) cho các hàng đã có sẵn (hàng đầu tiên)
    updateRowIndices();
});