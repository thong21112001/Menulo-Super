$(function () {
    // Gắn sự kiện submit cho form
    $('#createForm').on('submit', function (event) { // Sử dụng .on() thay vì .submit()
        // Kiểm tra xem form có hợp lệ (client-side validation)
        if ($(this).valid()) {
            // Vô hiệu hóa nút và đổi text để báo hiệu đang xử lý
            $('#submitBtn').prop('disabled', true).val('Đang xử lý...');
        }
        // Nếu form không hợp lệ, jQuery Validation sẽ ngăn chặn việc submit và nút sẽ không bị vô hiệu hóa
    });

    // Kích hoạt lại nút submit nếu có lỗi xác thực từ server (sau khi trang tải lại)
    // Điều này kiểm tra xem có bất kỳ thông báo lỗi nào trên trang không
    if ($('.text-danger').length > 0) {
        $('#submitBtn').prop('disabled', false).val('Lưu');
    }
});