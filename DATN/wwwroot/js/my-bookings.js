// Hàm load danh sách đặt sân
async function loadBookedFields() {
    const bookedFieldsContainer = document.getElementById('booked-fields');
    bookedFieldsContainer.innerHTML = `
        <tr>
            <td colspan="6" class="text-center">
                <div class="spinner-border text-success" role="status">
                    <span class="visually-hidden">Đang tải...</span>
                </div>
            </td>
        </tr>
    `;
    
    try {
        // Giả lập dữ liệu từ API (sau này sẽ thay bằng API thật)
        const bookedFields = [
            {
                id: 1,
                name: 'Sân 5A',
                date: '2024-03-20',
                time: '18:00 - 19:00',
                price: 800000,
                status: 'pending'
            },
            {
                id: 2,
                name: 'Sân 7B',
                date: '2024-03-21',
                time: '19:00 - 20:00',
                price: 1000000,
                status: 'confirmed'
            }
        ];
        
        if (bookedFields.length === 0) {
            bookedFieldsContainer.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center py-5">
                        <i class="fas fa-calendar-times fa-3x text-muted mb-3"></i>
                        <h5>Bạn chưa có đặt sân nào</h5>
                        <p class="text-muted">Hãy đặt sân ngay để trải nghiệm dịch vụ của chúng tôi</p>
                        <a href="index.html#booking" class="btn btn-success mt-2">
                            <i class="fas fa-calendar-plus me-2"></i>Đặt sân ngay
                        </a>
                    </td>
                </tr>
            `;
            return;
        }
        
        // Hiển thị dữ liệu
        bookedFieldsContainer.innerHTML = bookedFields.map(field => `
            <tr>
                <td>
                    <div class="d-flex align-items-center">
                        <i class="fas fa-futbol text-success me-2"></i>
                        ${field.name}
                    </div>
                </td>
                <td>
                    <div class="d-flex align-items-center">
                        <i class="fas fa-calendar text-primary me-2"></i>
                        ${formatDate(field.date)}
                    </div>
                </td>
                <td>
                    <div class="d-flex align-items-center">
                        <i class="fas fa-clock text-info me-2"></i>
                        ${field.time}
                    </div>
                </td>
                <td>
                    <div class="d-flex align-items-center">
                        <i class="fas fa-money-bill text-warning me-2"></i>
                        ${field.price.toLocaleString('vi-VN')}đ
                    </div>
                </td>
                <td>
                    <span class="status-badge status-${field.status.toLowerCase()}">
                        ${getStatusText(field.status)}
                    </span>
                </td>
                <td>
                    ${field.status === 'pending' ? `
                        <button class="btn btn-sm btn-danger" onclick="cancelBooking(${field.id})">
                            <i class="fas fa-times me-1"></i>Hủy
                        </button>
                    ` : ''}
                </td>
            </tr>
        `).join('');
    } catch (error) {
        console.error('Error loading bookings:', error);
        bookedFieldsContainer.innerHTML = `
            <tr>
                <td colspan="6" class="text-center">
                    <div class="alert alert-danger d-flex align-items-center" role="alert">
                        <i class="fas fa-exclamation-circle me-2"></i>
                        <div>Không thể tải danh sách đặt sân. Vui lòng thử lại sau.</div>
                    </div>
                </td>
            </tr>
        `;
    }
}

// Hàm format ngày tháng
function formatDate(dateString) {
    const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('vi-VN', options);
}

// Hàm lấy text trạng thái
function getStatusText(status) {
    switch(status) {
        case 'pending':
            return 'Chờ xác nhận';
        case 'confirmed':
            return 'Đã xác nhận';
        case 'cancelled':
            return 'Đã hủy';
        default:
            return 'Không xác định';
    }
}

// Hàm hủy đặt sân
function cancelBooking(bookingId) {
    if (confirm('Bạn có chắc chắn muốn hủy đặt sân này?')) {
        // Hiển thị loading
        const row = event.target.closest('tr');
        row.innerHTML = `
            <td colspan="6" class="text-center">
                <div class="spinner-border text-success" role="status">
                    <span class="visually-hidden">Đang xử lý...</span>
                </div>
            </td>
        `;
        
        // Giả lập gọi API hủy đặt sân (sau này sẽ thay bằng API thật)
        setTimeout(() => {
            loadBookedFields(); // Load lại danh sách
            showNotification('Hủy đặt sân thành công!', 'success');
        }, 500);
    }
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
            document.body.removeChild(notification);
        }, 300);
    }, 5000);
}

// Load dữ liệu khi trang được tải
document.addEventListener('DOMContentLoaded', () => {
    loadBookedFields();
}); 