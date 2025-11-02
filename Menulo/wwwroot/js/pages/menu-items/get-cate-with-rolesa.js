document.addEventListener("DOMContentLoaded", function () {
    const restaurantSelect = document.getElementById("restaurantSelect");
    const categorySelect = document.getElementById("categorySelect");

    if (!restaurantSelect || !categorySelect) return;

    restaurantSelect.addEventListener("change", async function () {
        const restaurantId = this.value;

        // Xóa các lựa chọn cũ
        categorySelect.innerHTML = '<option value="">-- Đang tải... --</option>';

        if (!restaurantId) {
            categorySelect.innerHTML = '<option value="">-- Chọn nhà hàng --</option>';
            return;
        }

        try {
            // Gọi API đã tạo ở Bước 2.2
            const response = await fetch(`/api/categories/for-restaurant/${restaurantId}`);
            if (!response.ok) throw new Error("Tải thất bại");

            const categories = await response.json();

            // Đổ dữ liệu mới
            categorySelect.innerHTML = '<option value="">-- Chọn danh mục --</option>'; // Reset
            categories.forEach(cat => {
                const option = document.createElement("option");
                option.value = cat.categoryId;
                option.textContent = cat.categoryName;
                categorySelect.appendChild(option);
            });

        } catch (err) {
            console.error(err);
            categorySelect.innerHTML = '<option value="">-- Lỗi tải danh mục --</option>';
        }
    });
});