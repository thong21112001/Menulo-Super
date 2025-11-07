document.addEventListener("DOMContentLoaded", function () {
    let lastInner = { w: window.innerWidth, h: window.innerHeight };

    // --- Helper adjust DataTables: cải thiện và tối ưu hóa ---
    function debounce(func, wait = 200) {
        let timeout;
        return function () {
            clearTimeout(timeout);
            timeout = setTimeout(func, wait);
        };
    }

    function shouldAdjust() {
        const dx = Math.abs(window.innerWidth - lastInner.w);
        const dy = Math.abs(window.innerHeight - lastInner.h);
        // chỉ adjust nếu thay đổi lớn hơn ngưỡng (px) — điều chỉnh threshold nếu cần
        const THRESH = 40;
        if (dx > THRESH || dy > THRESH) {
            lastInner.w = window.innerWidth;
            lastInner.h = window.innerHeight;
            return true;
        }
        return false;
    }

    function adjustTables() {
        $('.dataTable').each(function () {
            // guard: nếu chưa init DataTable thì skip
            if (!$.fn.DataTable.isDataTable(this)) return;

            const table = $(this).DataTable();
            try {
                // Force recalculation nhưng KHÔNG draw() — draw() sẽ gọi lại AJAX trên serverSide
                // chỉ adjust columns + responsive recalc nếu cần
                setTimeout(() => {
                    table.columns.adjust(); // chỉ cần adjust
                    if (table.responsive && typeof table.responsive.recalc === 'function') {
                        table.responsive.recalc();
                    }
                }, 100);
            } catch (err) {
                console.warn('adjustTables error', err);
            }
        });
    }


    const adjustTablesDebounced = debounce(() => {
        if (shouldAdjust()) adjustTables();
    }, 150);


    // Event listeners cho resize tables
    window.addEventListener('resize', adjustTablesDebounced);
    window.addEventListener('orientationchange', adjustTablesDebounced);

    // Custom event khi sidebar toggle
    document.addEventListener('sidebarToggled', () => {
        setTimeout(adjustTables, 350); // Đợi animation hoàn thành
    });

    // --- LOGIC GIAO DIỆN ---
    const body = document.body;
    const htmlEl = document.documentElement;
    const sidebar = document.getElementById("sidebar");
    const themeToggle = document.getElementById("theme-toggle");
    const sunIcon = document.getElementById("theme-icon-sun");
    const moonIcon = document.getElementById("theme-icon-moon");

    // 1. Chuyển đổi theme Sáng/Tối
    if (themeToggle && sunIcon && moonIcon) {
        const applyTheme = (theme) => {
            htmlEl.setAttribute("data-bs-theme", theme);
            sunIcon.classList.toggle("d-none", theme === "dark");
            moonIcon.classList.toggle("d-none", theme !== "dark");
        };

        const savedTheme = localStorage.getItem("theme") || "light";
        applyTheme(savedTheme);

        themeToggle.addEventListener("click", () => {
            const currentTheme = htmlEl.getAttribute("data-bs-theme");
            const newTheme = currentTheme === "dark" ? "light" : "dark";
            localStorage.setItem("theme", newTheme);
            applyTheme(newTheme);
        });
    }

    // 2. Mobile Sidebar Backdrop
    const backdrop = document.createElement("div");
    backdrop.className = "sidebar-backdrop";
    body.appendChild(backdrop);

    const hideMobileSidebar = () => {
        if (sidebar) sidebar.classList.remove("active");
        backdrop.classList.remove("show");

        // Dispatch custom event
        document.dispatchEvent(new CustomEvent('sidebarToggled'));
    };

    backdrop.addEventListener("click", hideMobileSidebar);

    // 3. Toggle sidebar trên mobile
    const mobileSidebarToggle = document.getElementById("mobile-sidebar-toggle");
    if (mobileSidebarToggle) {
        mobileSidebarToggle.addEventListener("click", () => {
            if (sidebar) sidebar.classList.toggle("active");
            backdrop.classList.toggle("show");

            // Dispatch custom event
            document.dispatchEvent(new CustomEvent('sidebarToggled'));
        });
    }

    // 4. Thu gọn/mở rộng sidebar trên desktop - CẢI THIỆN MƯỢT MÀ
    const desktopSidebarToggle = document.getElementById("desktop-sidebar-toggle");

    const applySidebarState = (state) => {
        // Thêm class transition trước khi thay đổi
        body.classList.add('sidebar-transitioning');

        if (state === "collapsed") {
            body.classList.add("sidebar-collapsed");
        } else {
            body.classList.remove("sidebar-collapsed");
        }

        // Dispatch resize event để các component khác biết sidebar đã thay đổi
        window.dispatchEvent(new Event("resize"));

        // Dispatch custom event cho DataTables
        document.dispatchEvent(new CustomEvent('sidebarToggled'));

        // Remove transition class sau khi animation hoàn thành
        setTimeout(() => {
            body.classList.remove('sidebar-transitioning');
        }, 300);
    };

    const savedSidebarState = localStorage.getItem("sidebarState");
    if (savedSidebarState) {
        applySidebarState(savedSidebarState);
    }

    if (desktopSidebarToggle) {
        desktopSidebarToggle.addEventListener("click", () => {
            const isCollapsed = body.classList.contains("sidebar-collapsed");
            const newState = isCollapsed ? "expanded" : "collapsed";
            localStorage.setItem("sidebarState", newState);
            applySidebarState(newState);
        });
    }

    // 5. Xử lý trạng thái active của menu khi click
    const navLinks = document.querySelectorAll(
        '.sidebar .nav-link:not([data-bs-toggle="collapse"])'
    );

    navLinks.forEach((link) => {
        link.addEventListener("click", function () {
            // Chỉ xử lý khi click vào link con trong menu popout khi sidebar thu gọn
            if (body.classList.contains('sidebar-collapsed') && this.closest('.collapse')) {
                // Không cần làm gì thêm, để link điều hướng bình thường
            } else {
                // Xử lý active cho các trường hợp khác
                document
                    .querySelectorAll(".sidebar .nav-link.active")
                    .forEach((l) => l.classList.remove("active"));
                this.classList.add("active");

                const parentCollapse = this.closest(".collapse");
                if (parentCollapse) {
                    const parentLink = document.querySelector(
                        `a[href="#${parentCollapse.id}"]`
                    );
                    if (parentLink) parentLink.classList.add("active");
                }
            }


            if (window.innerWidth < 992) {
                hideMobileSidebar();
            }
        });
    });

    // --- 6. Chuyển trang mượt ---
    document.body.classList.add("fade-in");

    document.querySelectorAll("a[href]:not([target]):not([data-bs-toggle]):not([href^='#']):not([href^='http'])").forEach(link => {
        const href = link.getAttribute("href");
        if (href && href !== '#' && !href.startsWith("javascript:")) {
            link.addEventListener("click", function (e) {
                try {
                    const url = new URL(href, window.location.origin);
                    if (url.origin === window.location.origin && url.pathname !== window.location.pathname) {
                        e.preventDefault();
                        document.body.classList.remove("fade-in");
                        document.body.classList.add("fade-out");
                        setTimeout(() => {
                            window.location.href = href;
                        }, 250);
                    }
                } catch (error) {
                    console.warn('Could not parse URL:', href);
                }
            });
        }
    });

    // --- 7. Khởi tạo lại tables khi cần thiết ---
    const tableObserver = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            if (mutation.type === 'childList') {
                mutation.addedNodes.forEach((node) => {
                    if (node.nodeType === 1) { // Element node
                        const tables = node.querySelectorAll?.('.dataTable') ||
                            (node.classList?.contains('dataTable') ? [node] : []);
                        if (tables.length > 0) {
                            setTimeout(adjustTables, 100);
                        }
                    }
                });
            }
        });
    });

    tableObserver.observe(document.body, {
        childList: true,
        subtree: true
    });

    // --- 8. Cleanup khi trang unload ---
    window.addEventListener('beforeunload', () => {
        tableObserver.disconnect();
    });

    // --- 9. Logout ---
    const logoutButton = document.getElementById('logoutButton');

    if (logoutButton) {
        logoutButton.addEventListener('click', async function (event) {
            event.preventDefault(); // Ngăn thẻ <a> chuyển trang

            // Lấy anti-forgery token để tăng cường bảo mật
            const afTokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            const afToken = afTokenInput ? afTokenInput.value : null;

            try {
                const response = await fetch('/api/auth/logout', {
                    method: 'POST',
                    headers: {
                        // Thêm token vào header nếu có
                        'RequestVerificationToken': afToken
                    }
                });

                if (response.ok) {
                    window.location.href = '/Admin/Login';
                } else {
                    alert('Đăng xuất không thành công. Vui lòng thử lại.');
                }
            } catch (error) {
                console.error('Lỗi khi đăng xuất:', error);
                alert('Đã xảy ra lỗi kết nối.');
            }
        });
    }

    // --- Final adjustment sau khi DOM hoàn toàn ready ---
    setTimeout(() => {
        adjustTables();
        document.body.classList.remove("page-transitioning", "fade-out");
        if (!document.body.classList.contains("fade-in")) {
            document.body.classList.add("fade-in");
        }
    }, 100);
});