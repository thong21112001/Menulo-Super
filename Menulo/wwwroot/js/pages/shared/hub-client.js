"use strict";

window.startHubConnection = function (params) {
    // Tạo query string chứa restaurantId / tableId (nếu có)
    const qs = new URLSearchParams();
    if (params.restaurantId) qs.set("restaurantId", params.restaurantId);
    if (params.tableId) qs.set("tableId", params.tableId);

    // Khởi HubConnection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`/tableHub?${qs.toString()}`)
        .withAutomaticReconnect([0, 2000, 5000])
        .build();

    // --- BẮT SỰ KIỆN ReceiveTableUpdate và gọi callback onTableUpdate ---
    if (typeof params.onTableUpdate === "function") {
        connection.on("ReceiveTableUpdate", tableId => {
            console.log("[hubclient] Đã nhận ReceiveTableUpdate, tableId =", tableId);
            try {
                params.onTableUpdate(tableId);
            } catch (ex) {
                console.error("[hubclient] Lỗi khi gọi onTableUpdate:", ex);
            }
        });
    }

    // Hàm khởi kết nối (và retry khi lỗi)
    const start = () => {
        connection.start()
            .then(() => {
                console.log("SignalR connected successfully");
                if (params.tableId) {
                    console.log(`Joined table group: table_${params.tableId}`);
                }
                if (params.restaurantId) {
                    console.log(`Joined restaurant group: restaurant_${params.restaurantId}`);

                    // Xin permission Notification (nếu chưa grant)
                    if ("Notification" in window && Notification.permission !== "granted") {
                        Notification.requestPermission()
                            .then(permission => {
                                console.log("[hubclient] Notification permission:", permission);
                            });
                    }
                }
            })
            .catch(err => {
                console.error("SignalR connection failed, retrying in 3s", err);
                setTimeout(start, 3000);
            });
    };

    start();

    // Nếu connection bị đóng bất ngờ, tự động reconnect
    connection.onclose(err => {
        console.warn("SignalR closed unexpectedly, reconnecting in 3s", err);
        setTimeout(start, 3000);
    });

    return connection;
};