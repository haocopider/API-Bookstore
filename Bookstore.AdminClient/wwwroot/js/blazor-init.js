// Function để chạy lại các đoạn khởi tạo cần thiết
function initSneatLayout() {
    console.log('Running initSneatLayout after Blazor render...');

    // 1. Re-initialize Menu
    // Lấy lại các element của menu
    let layoutMenuEl = document.querySelectorAll('#layout-menu');

    // Kiểm tra và chạy lại Menu Constructor nếu nó tồn tại (được định nghĩa trong menu.js/main.js)
    if (typeof Menu !== 'undefined' && layoutMenuEl.length > 0) {
        layoutMenuEl.forEach(function (element) {
            // Giả sử Menu constructor đã được load
            window.Helpers.mainMenu = new Menu(element, {
                orientation: 'vertical',
                closeChildren: false
            });
            window.Helpers.scrollToActive(false);
        });
    }

    // 2. Init Bootstrap Tooltips (nếu cần)
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // 3. Re-attach event listeners cho Menu Toggle Button
    let menuToggler = document.querySelectorAll('.layout-menu-toggle');
    menuToggler.forEach(item => {
        item.addEventListener('click', event => {
            event.preventDefault();
            window.Helpers.toggleCollapsed();
        });
    });

    // Các hàm khởi tạo khác (như charts/dashboards-analytics.js) sẽ tự chạy hoặc bạn có thể gọi thủ công nếu cần
    if (typeof loadApexCharts !== 'undefined') {
        loadApexCharts(); // Nếu có hàm load charts riêng biệt
    }
}