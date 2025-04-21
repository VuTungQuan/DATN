// Kiểm tra đăng nhập khi tải trang
document.addEventListener('DOMContentLoaded', function() {
    checkAuth();
    loadUserInfo();
    checkAdminRole();
});

// Kiểm tra xác thực
function checkAuth() {
    const token = localStorage.getItem('accessToken');
    if (!token) {
        showNotification('Vui lòng đăng nhập để xem thông tin cá nhân', 'warning');
        setTimeout(() => {
            window.location.href = 'index.html';
        }, 2000);
        return;
    }
}

// Kiểm tra role Admin
function checkAdminRole() {
    try {
        const userString = localStorage.getItem('user');
        if (!userString) return;

        const user = JSON.parse(userString);
        const adminMenu = document.getElementById('admin-menu');
        
        if (user.role === 'Admin') {
            adminMenu.style.display = 'block';
        } else {
            adminMenu.style.display = 'none';
        }
    } catch (error) {
        console.error('Lỗi khi kiểm tra role:', error);
    }
}

// Tải thông tin người dùng
function loadUserInfo() {
    try {
        const userString = localStorage.getItem('user');
        if (!userString) {
            showNotification('Không tìm thấy thông tin người dùng', 'error');
            return;
        }

        const user = JSON.parse(userString);
        
        // Cập nhật thông tin trong sidebar
        document.getElementById('sidebar-fullname').textContent = user.fullName || 'Chưa cập nhật';
        document.getElementById('sidebar-email').textContent = user.email || 'Chưa cập nhật';
        document.getElementById('sidebar-phone').textContent = user.phoneNumber || 'Chưa cập nhật';
        document.getElementById('sidebar-role').textContent = user.role || 'User';
        document.getElementById('sidebar-created').textContent = user.createdAt|| 'Chưa cập nhật';

        // Cập nhật tên người dùng trong navbar
        document.getElementById('user-name').textContent = user.fullName || 'Tài khoản';

        // Điền thông tin vào form
        document.getElementById('fullName').value = user.fullName || '';
        document.getElementById('phoneNumber').value = user.phoneNumber || '';
    } catch (error) {
        console.error('Lỗi khi tải thông tin người dùng:', error);
        showNotification('Có lỗi xảy ra khi tải thông tin người dùng', 'error');
    }
}

// Xử lý cập nhật thông tin
document.getElementById('updateForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const userID = localStorage.getItem('userID');
    const token = localStorage.getItem('accessToken');
    
    const updateData = {
        userID: parseInt(userID), // Chuyển userID thành số
        fullName: document.getElementById('fullName').value,
        phoneNumber: document.getElementById('phoneNumber').value,
        email: localStorage.getItem('userEmail'),
        passwordHash: localStorage.getItem('passwordHash'),
        role: "User",
        createdAt: new Date().toISOString()
    };

    try {
        const response = await fetch(`https://localhost:7290/api/Users/${userID}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(updateData)
        });

        // Kiểm tra nếu response là 204 No Content hoặc 200 OK
        if (response.status === 204 || response.status === 200) {
            // Cập nhật thông tin trong localStorage
            const updatedUser = {
                ...JSON.parse(localStorage.getItem('user')),
                ...updateData
            };
            localStorage.setItem('user', JSON.stringify(updatedUser));
            
            // Hiển thị thông báo thành công
            Swal.fire({
                icon: 'success',
                title: 'Thành công!',
                text: 'Cập nhật thông tin thành công',
                timer: 1500
            });
            
            // Tải lại thông tin
            loadUserInfo();
        } else {
            // Đọc nội dung lỗi từ response
            const errorText = await response.text();
            throw new Error(errorText || 'Lỗi khi cập nhật thông tin');
        }
    } catch (error) {
        console.error('Lỗi:', error);
        Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Đã có lỗi xảy ra khi cập nhật thông tin: ' + error.message
        });
    }
});

// Xử lý đổi mật khẩu
document.getElementById('changePasswordForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const oldPassword = document.getElementById('oldPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    const storedPasswordHash = localStorage.getItem('passwordHash');

    if (newPassword !== confirmPassword) {
        Swal.fire({
            icon: 'warning',
            title: 'Lỗi!',
            text: 'Mật khẩu mới không khớp!'
        });
        return;
    }

    const userID = localStorage.getItem('userID');
    const token = localStorage.getItem('accessToken');

    try {
        // Kiểm tra mật khẩu cũ
        const checkOldPasswordResponse = await fetch(`https://localhost:7290/api/Users/hash?oldpassword=${oldPassword}&oldhashpassword=${storedPasswordHash}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            }
        });

        if (!checkOldPasswordResponse.ok) {
            throw new Error('Lỗi khi kiểm tra mật khẩu cũ');
        }

        const hashResult = await checkOldPasswordResponse.json();
        console.log('Hash từ API:', hashResult);
        console.log('Hash trong localStorage:', storedPasswordHash);
        
        if (hashResult == false ) {
            console.log('Hash không khớp');
            Swal.fire({
                icon: 'error',
                title: 'Lỗi!',
                text: 'Mật khẩu cũ không đúng!'
            });
            return;
        }

        // Nếu mật khẩu cũ đúng, tiến hành đổi mật khẩu
        const response = await fetch(`https://localhost:7290/api/Users/${userID}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                userID: parseInt(userID),
                fullName: document.getElementById('fullName').value,
                phoneNumber: document.getElementById('phoneNumber').value,
                email: localStorage.getItem('userEmail'),
                passwordHash: newPassword,
                role: "User",
                createdAt: new Date().toISOString()
            })
        });

        if (response.ok) {
            Swal.fire({
                icon: 'success',
                title: 'Thành công!',
                text: 'Đổi mật khẩu thành công!',
                timer: 1500
            });
            document.getElementById('changePasswordForm').reset();
            const modal = bootstrap.Modal.getInstance(document.getElementById('changePasswordModal'));
            modal.hide();
        } else {
            throw new Error('Lỗi khi đổi mật khẩu');
        }
    } catch (error) {
        console.error('Lỗi:', error);
        Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Có lỗi xảy ra khi đổi mật khẩu: ' + error.message
        });
    }
});

// Xử lý hiện/ẩn mật khẩu
document.querySelectorAll('.input-group .btn-outline-secondary').forEach(button => {
    button.addEventListener('click', function() {
        const input = this.parentElement.querySelector('input');
        const icon = this.querySelector('i');
        
        if (input.type === 'password') {
            input.type = 'text';
            icon.classList.remove('fa-eye');
            icon.classList.add('fa-eye-slash');
        } else {
            input.type = 'password';
            icon.classList.remove('fa-eye-slash');
            icon.classList.add('fa-eye');
        }
    });
});

// Hàm đăng xuất
function logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('userID');
    localStorage.removeItem('userEmail');
    
    showNotification('Đã đăng xuất thành công!', 'success');
    
    setTimeout(() => {
        window.location.href = 'index.html';
    }, 1000);
}

// Hàm hiển thị thông báo
function showNotification(message, type = 'info') {
    const toast = document.getElementById('notification');
    const toastBody = toast.querySelector('.toast-body');
    
    // Thiết lập màu sắc dựa trên loại thông báo
    toast.className = `toast bg-${type === 'error' ? 'danger' : type}`;
    
    // Cập nhật nội dung
    toastBody.textContent = message;
    
    // Hiển thị toast
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
}

// Hàm format ngày tháng
function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
} 