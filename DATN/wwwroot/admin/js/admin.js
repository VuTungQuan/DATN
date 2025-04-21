// Kiểm tra quyền admin khi tải trang
document.addEventListener('DOMContentLoaded', function() {
    checkAdminAuth();
    updateAdminUI();
});

// Hàm kiểm tra quyền admin
function checkAdminAuth() {
    const accessToken = localStorage.getItem('accessToken');
    
    if (!accessToken) {
        window.location.href = '../index.html';
        return;
    }

    try {
        const base64Url = accessToken.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        const payload = JSON.parse(jsonPayload);
        const role = payload['role'];

        if (role !== 'Admin') {
            showNotification('Bạn không có quyền truy cập trang này', 'danger');
            window.location.href = '../index.html';
        }
    } catch (error) {
        console.error('Error checking admin auth:', error);
        window.location.href = '../index.html';
    }
}

// Hàm cập nhật giao diện admin
function updateAdminUI() {
    const user = JSON.parse(localStorage.getItem('user'));
    if (user) {
        // Cập nhật tên admin trong header
        const userDropdown = document.getElementById('userDropdown');
        if (userDropdown) {
            userDropdown.textContent = user.fullName || 'Admin';
        }
    }
}

// Hàm đăng xuất
function logout() {
    // Xóa tất cả dữ liệu trong localStorage
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('userID');
    localStorage.removeItem('userEmail');
    
    // Hiển thị thông báo đăng xuất thành công
    showNotification('Đã đăng xuất thành công!', 'success');
    
    // Chuyển hướng về trang chủ
    setTimeout(() => {
        window.location.href = '../index.html';
    }, 1000);
}

// Hàm hiển thị thông báo
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed top-0 end-0 m-3`;
    notification.style.zIndex = '9999';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Tự động đóng sau 5 giây
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 5000);
} 