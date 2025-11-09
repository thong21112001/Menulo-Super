"use strict";

function initializeGlobalNotifications(restaurantId) {
    if (!restaurantId) return;

    // Sử dụng lại file hubclient.js để kết nối
    const connection = startHubConnection({
        restaurantId: restaurantId,
    });

    // Tạo sẵn một audio object để phát âm thanh
    const notificationSound = new Audio('/sounds/sound.mp3');
    notificationSound.preload = 'auto';

    // Lắng nghe sự kiện "ShowGlobalNewOrderAlert" từ server
    connection.on("ShowGlobalNewOrderAlert", function (data) {
        console.log("Nhận được thông báo toàn cục:", data.message);

        // Phát âm thanh
        notificationSound.play().catch(error => {
            console.warn("Không thể phát âm thanh thông báo:", error);
        });

        Swal.fire({
            position: 'center', // Hiển thị ở giữa
            icon: 'info', // Biểu tượng nổi bật hơn
            title: '<strong>Thông báo đơn hàng mới!</strong>',
            imageUrl: '/images/nyan-cat.gif',
            imageWidth: 150,
            imageAlt: 'Thông báo đơn hàng mới',
            html: `
            <p style="font-size: 1.2rem;">${data.message}</p>
            <a href="${data.url}" class="btn btn-primary mt-2">Xem ngay</a>
            `,
            showConfirmButton: false, // Không cần nút OK
            timer: 30000, // Tự động tắt sau 30 giây
            timerProgressBar: true,
            backdrop: `rgba(0,0,123,0.4)`,
        });
    });
}