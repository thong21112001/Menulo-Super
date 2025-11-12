"use strict";


// ------------------- Helper Functions -------------------
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(amount).replace(/\./g, ',');
}

let pollInterval;
const previousState = new Map();


// ------------------- Audio Management -------------------
class AudioManager {
    constructor() {
        this.audioContext = null;
        this.audioBuffer = null;
        this.isInitialized = false;
        this.fallbackAudio = null;  //unlock audio
        this.userInteracted = false;
        this.autoInteractionAttempted = false;
        this.interactionPromise = null;

        this.preloadAudio();
        this.setupAutoInteraction();
        this.setupUserInteractionListener();
    }


    //Load âm thanh dự phòng và backup
    preloadAudio() {
        // Preload multiple audio instances
        this.fallbackAudio = new Audio("/sounds/sound.mp3");
        this.fallbackAudio.preload = "auto";
        this.fallbackAudio.volume = 0.8;
        this.fallbackAudio.load();

        // Create a backup audio instance
        this.backupAudio = new Audio("/sounds/sound.mp3");
        this.backupAudio.preload = "auto";
        this.backupAudio.volume = 0.8;
        this.backupAudio.load();
    }


    //Tạo sự kiện click ảo để "unlock" audio
    setupAutoInteraction() {
        // Tự động thực hiện interaction ngay khi trang load
        const autoInteract = async () => {
            if (this.autoInteractionAttempted) return;
            this.autoInteractionAttempted = true;
            try {
                // Tạo một sự kiện click ảo để "unlock" audio
                const clickEvent = new MouseEvent('click', {
                    view: window,
                    bubbles: true,
                    cancelable: true
                });

                // Dispatch event lên document
                document.dispatchEvent(clickEvent);

                // Thử khởi tạo audio context ngay lập tức
                await this.initializeWebAudio();

                // Thử play một âm thanh im lặng để unlock
                await this.unlockAudio();

                this.userInteracted = true;
                console.log("[AudioManager] Auto-interaction completed successfully");

            } catch (error) {
                this.setupFallbackInteraction();
            }
        };

        // Thực hiện auto-interaction sau một delay ngắn
        setTimeout(autoInteract, 100);

        // Thử lại sau 1 giây nếu chưa thành công
        setTimeout(() => {
            if (!this.userInteracted) {
                autoInteract();
            }
        }, 1000);
    }


    setupFallbackInteraction() {
        // Hiển thị một overlay nhẹ yêu cầu user click
        this.showInteractionPrompt();
    }

    showInteractionPrompt() {
        if (this.userInteracted) return;

        const overlay = document.createElement('div');
        overlay.id = 'audio-interaction-overlay';
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.1);
            z-index: 10000;
            display: flex;
            align-items: center;
            justify-content: center;
            backdrop-filter: blur(1px);
        `;

        const prompt = document.createElement('div');
        prompt.style.cssText = `
            background: white;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.3);
            text-align: center;
            max-width: 300px;
            cursor: pointer;
            transition: transform 0.2s ease;
        `;

        prompt.innerHTML = `
            <h4 style="margin: 0 0 10px 0; color: #333;">🔊 Kích hoạt âm thanh</h4>
            <p style="margin: 0; color: #666; font-size: 14px;">Nhấn để bật thông báo âm thanh</p>
        `;

        const enableAudio = async () => {
            try {
                this.userInteracted = true;
                await this.initializeWebAudio();
                await this.unlockAudio();
                document.body.removeChild(overlay);
                console.log("[AudioManager] User interaction completed via prompt");
            } catch (error) {
                console.error("[AudioManager] Failed to enable audio:", error);
            }
        };

        prompt.addEventListener('click', enableAudio);
        overlay.addEventListener('click', enableAudio);

        // Hover effect
        prompt.addEventListener('mouseenter', () => {
            prompt.style.transform = 'scale(1.05)';
        });
        prompt.addEventListener('mouseleave', () => {
            prompt.style.transform = 'scale(1)';
        });

        overlay.appendChild(prompt);
        document.body.appendChild(overlay);

        // Auto-remove after 10 seconds
        setTimeout(() => {
            if (document.body.contains(overlay)) {
                document.body.removeChild(overlay);
            }
        }, 10000);
    }

    async unlockAudio() {
        try {
            // Thử unlock bằng Web Audio API
            if (this.audioContext) {
                const oscillator = this.audioContext.createOscillator();
                const gainNode = this.audioContext.createGain();

                oscillator.connect(gainNode);
                gainNode.connect(this.audioContext.destination);

                gainNode.gain.setValueAtTime(0, this.audioContext.currentTime);
                oscillator.start(this.audioContext.currentTime);
                oscillator.stop(this.audioContext.currentTime + 0.01);
            }

            // Thử unlock bằng HTML5 Audio
            const tempAudio = new Audio("/sounds/sound.mp3");
            tempAudio.volume = 0;
            await tempAudio.play();
            tempAudio.pause();
            tempAudio.currentTime = 0;
            return true;
        } catch (error) {
            console.warn("[AudioManager] Failed to unlock audio:", error);
            return false;
        }
    }

    setupUserInteractionListener() {
        const events = ['touchstart', 'touchend', 'mousedown', 'keydown', 'click', 'scroll'];
        const enableAudio = async () => {
            if (this.userInteracted) return;

            this.userInteracted = true;
            console.log("[AudioManager] User interaction detected, initializing audio...");

            await this.initializeWebAudio();
            await this.unlockAudio();

            // Remove interaction prompt if exists
            const overlay = document.getElementById('audio-interaction-overlay');
            if (overlay) {
                document.body.removeChild(overlay);
            }

            events.forEach(event => {
                document.removeEventListener(event, enableAudio, true);
            });
        };

        events.forEach(event => {
            document.addEventListener(event, enableAudio, true);
        });
    }

    async initializeWebAudio() {
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) {
                console.warn("[AudioManager] Web Audio API not supported");
                return;
            }

            if (!this.audioContext) {
                this.audioContext = new AudioContext();
            }

            if (this.audioContext.state === 'suspended') {
                await this.audioContext.resume();
            }

            if (!this.audioBuffer) {
                const response = await fetch("/sounds/sound.mp3");
                const arrayBuffer = await response.arrayBuffer();
                this.audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
            }

            this.isInitialized = true;
            console.log("[AudioManager] Web Audio API initialized successfully");
        } catch (error) {
            console.warn("[AudioManager] Failed to initialize Web Audio API:", error);
        }
    }

    async reinitializeAudio() {
        console.log("[AudioManager] Attempting to reinitialize audio...");
        try {
            this.isInitialized = false;
            this.audioContext = null;
            this.audioBuffer = null;

            await this.initializeWebAudio();
            this.fallbackAudio.load();
            this.backupAudio.load();

            console.log("[AudioManager] Audio reinitialized successfully");
        } catch (error) {
            console.warn("[AudioManager] Failed to reinitialize audio:", error);
        }
    }

    async playSound() {
        // Nếu chưa có user interaction, thử auto-unlock một lần nữa
        if (!this.userInteracted) {
            console.log("[AudioManager] No user interaction, attempting auto-unlock...");
            const unlocked = await this.unlockAudio();
            if (unlocked) {
                this.userInteracted = true;
            } else {
                console.warn("[AudioManager] Cannot play sound - no user interaction");
                return false;
            }
        }

        // Thử Web Audio API trước
        if (this.isInitialized && this.audioContext && this.audioBuffer) {
            try {
                if (this.audioContext.state === 'suspended') {
                    console.log("[AudioManager] AudioContext suspended, attempting to resume...");
                    await this.audioContext.resume();
                    await new Promise(resolve => setTimeout(resolve, 100));
                    if (this.audioContext.state !== 'running') {
                        throw new Error("AudioContext not running");
                    }
                }

                const source = this.audioContext.createBufferSource();
                source.buffer = this.audioBuffer;

                // Thêm gain node để control volume
                const gainNode = this.audioContext.createGain();
                gainNode.gain.setValueAtTime(0.8, this.audioContext.currentTime);

                source.connect(gainNode);
                gainNode.connect(this.audioContext.destination);
                source.start(0);

                console.log("[AudioManager] Sound played via Web Audio API");
                return true;
            } catch (error) {
                console.warn("[AudioManager] Web Audio API playback failed:", error);
                await this.reinitializeAudio();
            }
        }

        // Dự phòng HTML5 Audio với multiple attempts
        const audioSources = [this.fallbackAudio, this.backupAudio];

        for (let audio of audioSources) {
            try {
                // Reset audio state
                audio.currentTime = 0;
                audio.volume = 0.8;

                const playPromise = audio.play();
                if (playPromise !== undefined) {
                    await playPromise;
                }

                console.log("[AudioManager] Sound played via HTML5 Audio");
                return true;
            } catch (error) {
                console.warn("[AudioManager] HTML5 Audio playback failed:", error);
                continue;
            }
        }

        // Thử tạo audio mới nếu tất cả đều thất bại
        try {
            const newAudio = new Audio("/sounds/sound.mp3");
            newAudio.volume = 0.8;
            await newAudio.play();
            console.log("[AudioManager] Sound played via new Audio instance");
            return true;
        } catch (error) {
            console.warn("[AudioManager] All audio playback methods failed:", error);
            return false;
        }
    }
}

const audioManager = new AudioManager();


// ------------------- Visual Notification System -------------------
class NotificationManager {
    constructor() {
        this.setupNotificationStyles();
    }

    setupNotificationStyles() {
        const style = document.createElement('style');
        style.textContent = `
            .toast-notification {
                position: fixed;
                top: 20px;
                right: 20px;
                background: #28a745;
                color: white;
                padding: 15px 20px;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.3);
                z-index: 9999;
                transform: translateX(100%);
                transition: transform 0.3s ease;
                max-width: 300px;
                font-weight: bold;
            }
            .toast-notification.show {
                transform: translateX(0);
            }
            .toast-notification.error {
                background: #dc3545;
            }
            .visual-pulse {
                animation: visualPulse 1s ease-in-out 3;
            }
            @keyframes visualPulse {
                0%, 100% { transform: scale(1); }
                50% { transform: scale(1.05); }
            }
            .screen-flash {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(40, 167, 69, 0.3);
                z-index: 9998;
                pointer-events: none;
                opacity: 0;
                animation: screenFlash 0.5s ease-out;
            }
            @keyframes screenFlash {
                0% { opacity: 0; }
                50% { opacity: 1; }
                100% { opacity: 0; }
            }
        `;
        document.head.appendChild(style);
    }

    showToast(message, type = 'success') {
        const toast = document.createElement('div');
        toast.className = `toast-notification ${type}`;
        toast.textContent = message;
        document.body.appendChild(toast);
        setTimeout(() => toast.classList.add('show'), 100);
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => document.body.removeChild(toast), 300);
        }, 3000);
    }

    flashScreen() {
        const flash = document.createElement('div');
        flash.className = 'screen-flash';
        document.body.appendChild(flash);
        setTimeout(() => document.body.removeChild(flash), 500);
    }

    pulseElement(element) {
        element.classList.add('visual-pulse');
        setTimeout(() => element.classList.remove('visual-pulse'), 3000);
    }
}

const notificationManager = new NotificationManager();


// ------------------- Enhanced Notification Function -------------------
async function showOrderNotification(tableId) {
    console.log(`[Notification] New order for table ${tableId}`);

    // Hiệu ứng trực quan
    const card = document.querySelector(`a[data-table-id="${tableId}"] .card`);
    if (card) {
        card.classList.add("highlight-new");
        notificationManager.pulseElement(card);
        setTimeout(() => card.classList.remove("highlight-new"), 2000);
    }

    // Thử phát âm thanh với cơ chế thử lại
    let audioPlayed = false;
    let retryCount = 0;
    const maxRetries = 3;

    while (!audioPlayed && retryCount < maxRetries) {
        audioPlayed = await audioManager.playSound();
        if (!audioPlayed) {
            retryCount++;
            console.warn(`[Notification] Audio attempt ${retryCount} failed, retrying...`);
            if (retryCount < maxRetries) {
                await new Promise(resolve => setTimeout(resolve, 300));
            }
        }
    }

    // Rung thiết bị (nếu hỗ trợ)
    if ('vibrate' in navigator) {
        navigator.vibrate([300, 100, 300, 100, 300]);
    }
}


// ------------------- Request Notification Permission -------------------
async function requestNotificationPermission() {
    if (!("Notification" in window)) {
        console.log("Browser doesn't support notifications");
        return false;
    }

    if (Notification.permission === "granted") {
        return true;
    }

    if (Notification.permission !== "denied") {
        const permission = await Notification.requestPermission();
        return permission === "granted";
    }

    return false;
}


// ------------------- Main -------------------
document.addEventListener("DOMContentLoaded", async () => {
    await requestNotificationPermission();

    const connection = startHubConnection({
        restaurantId: RESTAURANT_ID,
        onTableUpdate: async tableId => {
            await showOrderNotification(tableId);
            updateTableData(); // Vẫn gọi hàm update polling
        }
    });

    updateTableData(); // Gọi lần đầu tiên
    startPolling();

    document.addEventListener('visibilitychange', async () => {
        if (document.hidden) {
            clearInterval(pollInterval);
        } else {
            console.log("[Main] Tab became visible, checking audio state...");
            startPolling();

            // Thử khởi động lại audio khi tab active
            if (!audioManager.userInteracted) {
                setTimeout(async () => {
                    await audioManager.unlockAudio();
                    audioManager.userInteracted = true;
                }, 100);
            }

            if (audioManager.audioContext && audioManager.audioContext.state === 'suspended') {
                console.log("[Main] Resuming suspended AudioContext...");
                try {
                    await audioManager.audioContext.resume();
                } catch (error) {
                    console.warn("[Main] Failed to resume AudioContext:", error);
                    await audioManager.reinitializeAudio();
                }
            }
            audioManager.fallbackAudio.load();
            audioManager.backupAudio.load();
        }
    });
});


// ------------------- Polling & Render (ĐÃ SỬA) -------------------
function startPolling() {
    clearInterval(pollInterval);
    pollInterval = setInterval(updateTableData, 5000);
}

async function updateTableData() {
    try {
        // ===== THAY ĐỔI 1: LẤY TOKEN TỪ META TAG =====
        const tokenMeta = document.querySelector('meta[name="xsrf-token"]');
        const token = tokenMeta ? tokenMeta.content : '';

        if (!token) {
            console.warn("XSRF Token not found. Polling stopped.");
            clearInterval(pollInterval);
            return;
        }

        // ===== THAY ĐỔI 2: GỌI API CONTROLLER MỚI =====
        const res = await fetch('/api/restable/status', { // Endpoint mới
            method: 'GET',
            headers: { 'RequestVerificationToken': token }
        });

        if (!res.ok) {
            // Nếu lỗi 401/403 (vd: hết hạn login), dừng polling
            if (res.status === 401 || res.status === 403) {
                console.error('Unauthorized. Polling stopped.');
                clearInterval(pollInterval);
                document.getElementById('loading-spinner').innerHTML =
                    '<div class="alert alert-danger">Phiên đăng nhập hết hạn. Vui lòng tải lại trang.</div>';
            }
            throw new Error(`Fetch error: ${res.statusText}`);
        }

        const tables = await res.json();
        renderTables(tables); // Gọi hàm render đã được viết lại
    } catch (e) {
        console.error('Fetch error', e);
        // Có thể dừng polling nếu lỗi
        // clearInterval(pollInterval); 
    }
}


// ===== THAY ĐỔI 3: VIẾT LẠI HOÀN TOÀN HÀM RENDER =====
function renderTables(tables) {
    const container = document.getElementById('table-container');

    // Ẩn spinner, hiện container (chỉ làm 1 lần)
    if (container.classList.contains('d-none')) {
        document.getElementById('loading-spinner').classList.add('d-none');
        container.classList.remove('d-none');
    }

    // Tối ưu: Dùng DocumentFragment để chèn 1 LẦN, giảm flicker
    const fragment = document.createDocumentFragment();

    // Xử lý trường hợp không có bàn
    if (tables.length === 0) {
        container.innerHTML = `
            <div class="col-12">
                <p>Hiện không có bàn nào. Vui lòng tạo bàn mới cho nhà hàng.</p>
                <a href="/ds-ban/tao-moi" class="btn btn-primary">Tạo bàn mới</a>
            </div>`;
        return;
    }

    // Tạo HTML cho mỗi bàn
    tables.forEach(table => {
        const headerClass = table.hasOrder || table.hasPendingOrder
            ? "bg-description text-white"
            : "bg-light text-dark";

        // 1. Tạo <div> bọc ngoài (col)
        const colDiv = document.createElement('div');
        colDiv.className = 'col-6 col-sm-4 col-md-3 mb-3';

        // 2. Tạo thẻ <a>
        const cardLink = document.createElement('a');
        cardLink.href = `/TableOrders/Details?tableId=${table.tableId}`; // (Kiểm tra lại URL này)
        cardLink.dataset.tableId = table.tableId;
        cardLink.className = 'text-decoration-none';

        // 3. Tạo thẻ <div class="card">
        const card = document.createElement('div');
        card.className = 'card card-position-relative h-100';
        card.setAttribute('aria-live', 'polite');

        // 4. Tạo card-header
        const header = document.createElement('div');
        header.className = `card-header ${headerClass}`;
        header.textContent = table.description.length > 50 ? table.description.substring(0, 20) + "…" : table.description;

        // 5. Thêm badge (nếu có)
        if (table.hasPendingOrder) {
            const badge = document.createElement('span');
            badge.className = 'notification-badge blink';
            badge.textContent = table.pendingTotalQuantity;
            header.appendChild(badge);
        }

        // 6. Thêm card-body (nếu có)
        let bodyHtml = '';
        if (table.hasOrder) {
            bodyHtml = `
                <div class="card-body p-2">
                    <h6 class="mb-1"><strong>Tổng món: ${table.totalQuantity}</strong></h6>
                    <p class="text-danger mb-0">
                        Tạm tính: <strong>${formatCurrency(table.totalAmount)}</strong>
                    </p>
                </div>`;
        }

        // 7. Lắp ráp
        card.appendChild(header);
        card.innerHTML += bodyHtml; // Chèn body (nếu có)
        cardLink.appendChild(card);
        colDiv.appendChild(cardLink);
        fragment.appendChild(colDiv);

        // 8. Kích hoạt hiệu ứng (từ logic cũ của bạn)
        const prev = previousState.get(table.tableId) || { pendingTotalQuantity: 0 };
        if (table.hasPendingOrder && table.pendingTotalQuantity > prev.pendingTotalQuantity) {
            // Polling đã phát hiện một đơn hàng mới mà SignalR (có thể) đã bỏ lỡ.
            // Kích hoạt thông báo!
            showOrderNotification(table.tableId);
        }
        previousState.set(table.tableId, { pendingTotalQuantity: table.pendingTotalQuantity });
    });

    // 9. Chèn 1 LẦN VÀO DOM (xóa nội dung cũ, thêm nội dung mới)
    container.innerHTML = '';
    container.appendChild(fragment);
}