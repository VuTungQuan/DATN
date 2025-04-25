// Khởi tạo Flatpickr cho date picker
flatpickr("#date-picker", {
    locale: "vn",
    dateFormat: "d/m/Y",
    minDate: "today",
    maxDate: new Date().fp_incr(14), // Cho phép đặt trước 14 ngày
    disable: [
        function(date) {
            // Disable Sundays
            if (date.getDay() === 0) {
                return true;
            }
            // Disable các ngày đã qua trong ngày hiện tại
            if (date.toDateString() === new Date().toDateString()) {
                return date.getHours() < new Date().getHours();
            }
            return false;
        }
    ],
    onChange: function(selectedDates, dateStr, instance) {
        // Hiệu ứng khi chọn ngày
        const element = instance.element;
        element.classList.add('selected-date');
        setTimeout(() => {
            element.classList.remove('selected-date');
        }, 300);
    }
});

// Khởi tạo các biến
let selectedDate = null;
let selectedTime = null;
let selectedField = null;

// Hàm tạo time slots
function generateTimeSlots() {
    const timeSlotSelect = document.getElementById('time-slot');
    
    // Kiểm tra xem phần tử có tồn tại không
    if (!timeSlotSelect) {
        console.log('Không tìm thấy phần tử time-slot');
        return;
    }
    
    timeSlotSelect.innerHTML = '<option value="">Chọn giờ</option>';
    
    // Tạo các khung giờ từ 6:00 đến 22:00
    for (let hour = 6; hour < 22; hour++) {
        const time = `${hour.toString().padStart(2, '0')}:00`;
        const option = document.createElement('option');
        option.value = time;
        option.textContent = time;
        timeSlotSelect.appendChild(option);
    }
}

// Hàm load danh sách sân
async function loadFields() {
    const fieldsContainer = document.getElementById('fields-container');
    if (!fieldsContainer) return; // Nếu không tìm thấy container, thoát khỏi hàm

    try {
        // TODO: Gọi API lấy danh sách sân
        const fields = [
            { id: 1, name: 'Sân A', type: 'Sân 5', price: '150.000đ/giờ', image: 'images/field-a.jpg' },
            { id: 2, name: 'Sân B', type: 'Sân 7', price: '250.000đ/giờ', image: 'images/field-b.jpg' },
            { id: 3, name: 'Sân C', type: 'Sân 11', price: '500.000đ/giờ', image: 'images/field-c.jpg' }
        ];

        fieldsContainer.innerHTML = fields.map(field => `
            <div class="col-md-4 mb-4">
                <div class="card field-card">
                    <img src="${field.image}" class="card-img-top" alt="${field.name}">
                    <div class="card-body">
                        <h5 class="card-title">${field.name}</h5>
                        <p class="card-text">
                            <span class="badge bg-primary">${field.type}</span>
                            <span class="text-success ms-2">${field.price}</span>
                        </p>
                        <button class="btn btn-success w-100" onclick="bookField(${field.id})">
                            <i class="fas fa-calendar-check me-2"></i>Đặt sân
                        </button>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Lỗi khi tải danh sách sân:', error);
        fieldsContainer.innerHTML = '<div class="col-12 text-center text-danger">Không thể tải danh sách sân. Vui lòng thử lại sau.</div>';
    }
}

// Hàm tạo card sân
function createFieldCard(field) {
    const div = document.createElement('div');
    div.className = 'col-md-4';
    div.innerHTML = `
        <div class="field-card">
            <div class="field-image-container">
                <img src="${field.image || 'images/default-field.jpg'}" alt="${field.name}">
                ${field.isMerged ? `<div class="merged-badge"><i class="fas fa-link"></i> Sân gộp</div>` : ''}
            </div>
            <div class="card-body">
                <h5 class="card-title">${field.name}</h5>
                <p class="card-text">Loại sân: ${field.type}</p>
                <div class="field-features mb-3">
                    <span class="badge bg-light text-dark me-2"><i class="fas fa-users"></i> ${field.capacity} người</span>
                    <span class="badge bg-light text-dark"><i class="fas fa-lightbulb"></i> ${field.hasLight ? 'Có đèn' : 'Không đèn'}</span>
                </div>
                <p class="price">${field.price.toLocaleString('vi-VN')}đ/giờ</p>
                <button class="btn btn-success w-100" onclick="selectField(${field.id})">
                    <i class="fas fa-calendar-check me-2"></i>Đặt sân
                </button>
            </div>
        </div>
    `;
    return div;
}

// Hàm chọn sân
function selectField(fieldId) {
    selectedField = fieldId;
    
    if (!selectedDate || !selectedTime) {
        showNotification('Vui lòng chọn ngày và giờ đặt sân', 'warning');
        return;
    }
    
    // Hiển thị modal xác nhận đặt sân
    const modal = document.createElement('div');
    modal.className = 'modal fade';
    modal.id = 'bookingModal';
    modal.setAttribute('tabindex', '-1');
    modal.setAttribute('aria-labelledby', 'bookingModalLabel');
    modal.setAttribute('aria-hidden', 'true');
    
    modal.innerHTML = `
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="bookingModalLabel">Xác nhận đặt sân</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p>Bạn có chắc chắn muốn đặt sân này?</p>
                    <ul class="list-group mb-3">
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <span><i class="fas fa-calendar me-2"></i>Ngày:</span>
                            <span class="badge bg-primary rounded-pill">${selectedDate}</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <span><i class="fas fa-clock me-2"></i>Giờ:</span>
                            <span class="badge bg-primary rounded-pill">${selectedTime}</span>
                        </li>
                    </ul>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                    <button type="button" class="btn btn-success" onclick="bookField(${fieldId})">
                        <i class="fas fa-check me-2"></i>Xác nhận
                    </button>
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    const bookingModal = new bootstrap.Modal(modal);
    bookingModal.show();
    
    // Xóa modal khi đóng
    modal.addEventListener('hidden.bs.modal', function () {
        document.body.removeChild(modal);
    });
}

// Hàm đặt sân
async function bookField(fieldId) {
    if (!selectedDate || !selectedTime) {
        showNotification('Vui lòng chọn ngày và giờ đặt sân', 'warning');
        return;
    }
    
    try {
        const response = await fetch('/api/bookings', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                fieldId,
                date: selectedDate,
                time: selectedTime
            })
        });
        
        if (response.ok) {
            // Đóng modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('bookingModal'));
            modal.hide();
            
            showNotification('Đặt sân thành công!', 'success');
            loadMyBookings();
        } else {
            const error = await response.json();
            showNotification(error.message || 'Không thể đặt sân. Vui lòng thử lại.', 'danger');
        }
    } catch (error) {
        console.error('Error booking field:', error);
        showNotification('Có lỗi xảy ra. Vui lòng thử lại sau.', 'danger');
    }
}

// Hàm load lịch đặt của tôi
async function loadMyBookings() {
    const bookingsContainer = document.getElementById('my-bookings-container');
    if (!bookingsContainer) return; // Nếu không tìm thấy container, thoát khỏi hàm

    try {
        // TODO: Gọi API lấy lịch đặt
        const bookings = [
            { id: 1, fieldName: 'Sân A', date: '2024-03-20', time: '18:00', status: 'pending' },
            { id: 2, fieldName: 'Sân B', date: '2024-03-21', time: '19:00', status: 'confirmed' }
        ];

        bookingsContainer.innerHTML = bookings.map(booking => `
            <tr>
                <td>${booking.fieldName}</td>
                <td>${formatDate(booking.date)}</td>
                <td>${booking.time}</td>
                <td>
                    <span class="badge ${getStatusBadgeClass(booking.status)}">
                        ${getStatusText(booking.status)}
                    </span>
                </td>
                <td>
                    ${booking.status === 'pending' ? `
                        <button class="btn btn-sm btn-danger" onclick="cancelBooking(${booking.id})">
                            <i class="fas fa-times"></i>
                        </button>
                    ` : ''}
                </td>
            </tr>
        `).join('');
    } catch (error) {
        console.error('Lỗi khi tải lịch đặt:', error);
        bookingsContainer.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Không thể tải lịch đặt. Vui lòng thử lại sau.</td></tr>';
    }
}

// Hàm hủy đặt sân
async function cancelBooking(bookingId) {
    // Hiển thị modal xác nhận hủy
    const modal = document.createElement('div');
    modal.className = 'modal fade';
    modal.id = 'cancelModal';
    modal.setAttribute('tabindex', '-1');
    modal.setAttribute('aria-labelledby', 'cancelModalLabel');
    modal.setAttribute('aria-hidden', 'true');
    
    modal.innerHTML = `
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="cancelModalLabel">Xác nhận hủy đặt sân</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <p>Bạn có chắc chắn muốn hủy đặt sân này?</p>
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        Lưu ý: Việc hủy đặt sân có thể bị tính phí theo quy định của chúng tôi.
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                    <button type="button" class="btn btn-danger" onclick="confirmCancelBooking(${bookingId})">
                        <i class="fas fa-times me-2"></i>Xác nhận hủy
                    </button>
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    const cancelModal = new bootstrap.Modal(modal);
    cancelModal.show();
    
    // Xóa modal khi đóng
    modal.addEventListener('hidden.bs.modal', function () {
        document.body.removeChild(modal);
    });
}

// Hàm xác nhận hủy đặt sân
async function confirmCancelBooking(bookingId) {
    try {
        const response = await fetch(`/api/bookings/${bookingId}`, {
            method: 'DELETE'
        });
        
        if (response.ok) {
            // Đóng modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('cancelModal'));
            modal.hide();
            
            showNotification('Hủy đặt sân thành công!', 'success');
            loadMyBookings();
        } else {
            showNotification('Không thể hủy đặt sân. Vui lòng thử lại.', 'danger');
        }
    } catch (error) {
        console.error('Error canceling booking:', error);
        showNotification('Có lỗi xảy ra. Vui lòng thử lại sau.', 'danger');
    }
}

// Hàm format ngày
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

// Hàm lấy text trạng thái
function getStatusText(status) {
    const statusMap = {
        'PENDING': 'Chờ xác nhận',
        'CONFIRMED': 'Đã xác nhận',
        'CANCELLED': 'Đã hủy'
    };
    return statusMap[status] || status;
}

// Hàm hiển thị thông báo
function showNotification(message, type = 'info') {
    const toast = document.getElementById('notification');
    if (!toast) {
        // Nếu không có toast, tạo thông báo alert
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
        return;
    }
    
    // Nếu có toast, sử dụng toast
    const toastBody = toast.querySelector('.toast-body');
    const toastIcon = toast.querySelector('.toast-header i');
    
    toastBody.textContent = message;
    
    // Cập nhật icon và màu sắc dựa trên loại thông báo
    switch (type) {
        case 'success':
            toastIcon.className = 'fas fa-check-circle me-2 text-success';
            break;
        case 'warning':
            toastIcon.className = 'fas fa-exclamation-circle me-2 text-warning';
            break;
        case 'danger':
            toastIcon.className = 'fas fa-times-circle me-2 text-danger';
            break;
        default:
            toastIcon.className = 'fas fa-info-circle me-2 text-info';
    }
    
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
}

// Hàm scroll mượt
function smoothScroll(target) {
    const element = document.querySelector(target);
    if (element) {
        window.scrollTo({
            top: element.offsetTop - 70,
            behavior: 'smooth'
        });
    }
}

// Hàm cập nhật UI sau khi đăng nhập
function updateUIAfterLogin(userData) {
    // Ẩn nút đăng nhập/đăng ký
    const authButtons = document.getElementById('auth-buttons');
    if (authButtons) {
        authButtons.style.display = 'none';
    }

    // Hiển thị menu người dùng
    const userMenu = document.getElementById('user-menu');
    if (userMenu) {
        userMenu.style.display = 'block';
    }

    // Cập nhật tên người dùng trong dropdown
    const userDropdown = document.getElementById('userDropdown');
    if (userDropdown && userData.fullName) {
        userDropdown.textContent = userData.fullName;
    }

    // Cập nhật menu dropdown
    const dropdownMenu = document.getElementById('dropdownMenu');
    if (dropdownMenu) {
        dropdownMenu.innerHTML = `
            <a class="dropdown-item" href="profile.html">
                <i class="fas fa-user me-2"></i>Thông tin cá nhân
            </a>
            <a class="dropdown-item" href="#" onclick="viewBookings()">
                <i class="fas fa-history me-2"></i>Lịch sử đặt sân
            </a>
            <div class="dropdown-divider"></div>
            <a class="dropdown-item text-danger" href="#" onclick="logout()">
                <i class="fas fa-sign-out-alt me-2"></i>Đăng xuất
            </a>
        `;
    }

    // Giải mã token để kiểm tra role và chuyển hướng nếu cần
    const accessToken = localStorage.getItem('accessToken');
    if (accessToken) {
        try {
            const base64Url = accessToken.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));

            const payload = JSON.parse(jsonPayload);
            const role = payload['role'];

            console.log('Role from token:', role);

            // Chuyển hướng dựa trên role
            if (role === 'Admin') {
                window.location.href = '/admin/index.html';
            }
            // Nếu là User thì ở lại trang hiện tại
        } catch (error) {
            console.error('Error decoding token:', error);
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
    
    // Cập nhật UI
    const authButtons = document.getElementById('auth-buttons');
    const userMenu = document.getElementById('user-menu');
    
    if (authButtons) {
        authButtons.style.display = 'block';
    }
    
    if (userMenu) {
        userMenu.style.display = 'none';
    }
    
    // Tải lại trang sau 1 giây
    setTimeout(() => {
        window.location.href = '/index.html';
    }, 1000);
}

// Kiểm tra đăng nhập khi tải trang
document.addEventListener('DOMContentLoaded', () => {
    const accessToken = localStorage.getItem('accessToken');
    const user = JSON.parse(localStorage.getItem('user'));
    
    if (accessToken && user) {
        updateUIAfterLogin(user);
    }
});

// Event Listeners
document.addEventListener('DOMContentLoaded', () => {
    // Kiểm tra trạng thái đăng nhập
    const user = JSON.parse(localStorage.getItem('user'));
    const token = localStorage.getItem('token');
    if (user && token) {
        updateUIAfterLogin();
    }

    // Chỉ gọi các hàm nếu đang ở trang chủ
    if (window.location.pathname.endsWith('index.html') || window.location.pathname.endsWith('/')) {
        generateTimeSlots();
        loadFields();
        loadMyBookings();

        // Xử lý form đặt sân
        const bookingForm = document.getElementById('bookingForm');
        if (bookingForm) {
            bookingForm.addEventListener('submit', function(e) {
                e.preventDefault();
                
                // Kiểm tra xem đã chọn thời gian chưa
                const selectedSlot = document.querySelector('.table td.selected');
                if (!selectedSlot) {
                    alert('Vui lòng chọn thời gian đặt sân');
                    return;
                }

                // TODO: Gửi dữ liệu đặt sân lên server
                const formData = {
                    name: this.querySelector('input[placeholder="Họ và tên"]').value,
                    email: this.querySelector('input[placeholder="Email"]').value,
                    phone: this.querySelector('input[placeholder="Số điện thoại"]').value,
                    date: document.getElementById('date-picker').value,
                    duration: document.getElementById('duration').value,
                    note: this.querySelector('textarea').value,
                    time: selectedSlot.closest('table').querySelector('thead th:nth-child(' + (selectedSlot.cellIndex + 1) + ')').textContent,
                    price: selectedSlot.textContent.trim()
                };

                console.log('Booking data:', formData);
                // TODO: Implement API call
            });
        }

        // Xử lý nút khung giờ
        const timeFrameBtns = document.querySelectorAll('.btn-outline-primary');
        if (timeFrameBtns.length > 0) {
            timeFrameBtns.forEach(btn => {
                btn.addEventListener('click', function() {
                    timeFrameBtns.forEach(b => b.classList.remove('active'));
                    this.classList.add('active');
                    // TODO: Lọc hiển thị theo khung giờ
                });
            });
        }
    }

    // Xử lý chuyển đổi giữa modal đăng nhập và đăng ký
    const modalButtons = document.querySelectorAll('[data-bs-toggle="modal"]');
    if (modalButtons.length > 0) {
        modalButtons.forEach(button => {
            button.addEventListener('click', function() {
                const currentModal = bootstrap.Modal.getInstance(document.querySelector('.modal.show'));
                if (currentModal) {
                    currentModal.hide();
                }
            });
        });
    }

    // Xử lý hiện/ẩn mật khẩu
    const passwordButtons = document.querySelectorAll('.btn-outline-secondary');
    if (passwordButtons.length > 0) {
        passwordButtons.forEach(button => {
            button.addEventListener('click', function() {
                const input = this.previousElementSibling;
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
    }

    // Xử lý form đăng nhập
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }

    // Xử lý form đăng ký
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', handleRegister);
    }
});

// Hàm load sân đã đặt
function loadBookedFields() {
    const bookedFieldsContainer = document.getElementById('booked-fields');
    bookedFieldsContainer.innerHTML = ''; // Xóa dữ liệu cũ
    
    // Hiển thị loading
    bookedFieldsContainer.innerHTML = `
        <tr>
            <td colspan="5" class="text-center">
                <div class="spinner-border text-success" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </td>
        </tr>
    `;
    
    // Giả lập dữ liệu từ API (sau này sẽ thay bằng API thật)
    setTimeout(() => {
        const bookedFields = [
            {
                id: 1,
                name: 'Sân 5A',
                date: '2024-03-20',
                time: '18:00 - 19:00',
                status: 'pending'
            },
            {
                id: 2,
                name: 'Sân 7B',
                date: '2024-03-21',
                time: '19:00 - 20:00',
                status: 'confirmed'
            }
        ];
        
        // Hiển thị dữ liệu
        bookedFieldsContainer.innerHTML = bookedFields.map(field => `
            <tr>
                <td>${field.name}</td>
                <td>${formatDate(field.date)}</td>
                <td>${field.time}</td>
                <td>
                    <span class="badge ${getStatusBadgeClass(field.status)}">
                        ${getStatusText(field.status)}
                    </span>
                </td>
                <td>
                    ${field.status === 'pending' ? `
                        <button class="btn btn-sm btn-danger" onclick="cancelBooking(${field.id})">
                            <i class="fas fa-times"></i> Hủy
                        </button>
                    ` : ''}
                </td>
            </tr>
        `).join('');
    }, 500);
}

// Hàm format ngày tháng
function formatDate(dateString) {
    const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('vi-VN', options);
}

// Hàm lấy class cho badge trạng thái
function getStatusBadgeClass(status) {
    switch(status) {
        case 'pending':
            return 'bg-warning';
        case 'confirmed':
            return 'bg-success';
        case 'cancelled':
            return 'bg-danger';
        default:
            return 'bg-secondary';
    }
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
            <td colspan="5" class="text-center">
                <div class="spinner-border text-success" role="status">
                    <span class="visually-hidden">Đang xử lý...</span>
                </div>
            </td>
        `;
        
        // Giả lập gọi API hủy đặt sân (sau này sẽ thay bằng API thật)
        setTimeout(() => {
            loadBookedFields(); // Load lại danh sách
        }, 500);
    }
}

// Hàm xử lý đăng nhập
async function handleLogin(event) {
    event.preventDefault();
    
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;
    
    try {
        const response = await fetch('https://localhost:7290/api/Auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email: email,
                password: password
            })
        });

        const data = await response.json();
        console.log('Raw login response:', data);

        if (!response.ok) {
            throw new Error(data.message || 'Đăng nhập thất bại');
        }

        // Lưu token vào localStorage
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('userEmail', email); // Lưu email ngay sau khi đăng nhập thành công
        
        // Lấy thông tin người dùng
        await getUserInfoByEmail();
        
        // Đóng modal đăng nhập
        const loginModal = document.getElementById('loginModal');
        if (loginModal) {
            bootstrap.Modal.getInstance(loginModal).hide();
        }
        
        // Hiển thị thông báo thành công
        showNotification('Đăng nhập thành công', 'success');
        
    } catch (error) {
        showNotification('Vui lòng kiểm tra lại tài khoản và mật khẩu ', 'danger');
    }
}

// Hàm xử lý đăng ký
async function handleRegister(event) {
    event.preventDefault();
    
    const fullName = document.getElementById('register-name').value;
    const email = document.getElementById('register-email').value;
    const phoneNumber = document.getElementById('register-phone').value;
    const password = document.getElementById('register-password').value;
    const confirmPassword = document.getElementById('register-confirm-password').value;

    // Kiểm tra mật khẩu xác nhận
    if (password !== confirmPassword) {
        showNotification('Mật khẩu xác nhận không khớp!', 'danger');
        return;
    }

    try {
        const response = await fetch('https://localhost:7290/api/Auth/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                fullName: fullName,
                email: email,
                phoneNumber: phoneNumber,
                passwordHash: password,
                role: "User"
            })
        });

        if (response.ok) {
            showNotification('Đăng ký thành công!', 'success');
            
            // Đóng modal đăng ký
            const registerModal = bootstrap.Modal.getInstance(document.getElementById('registerModal'));
            registerModal.hide();
            
            // Mở modal đăng nhập sau 1 giây
            setTimeout(() => {
                const loginModal = new bootstrap.Modal(document.getElementById('loginModal'));
                loginModal.show();
            }, 1000);
        } else {
            const error = await response.json();
            showNotification(error.message || 'Đăng ký thất bại!', 'danger');
        }
    } catch (error) {
        console.error('Lỗi đăng ký:', error);
        showNotification('Có lỗi xảy ra khi đăng ký!', 'danger');
    }
}

// Hàm lấy thông tin người dùng từ email
async function getUserInfoByEmail() {
    console.log(localStorage.getItem('userEmail'));
    const email = localStorage.getItem('userEmail');
    try {
        const encodedEmail = encodeURIComponent(email);
        const accessToken = localStorage.getItem('accessToken');
        
        console.log('Access Token:', accessToken); // Log để debug

        const response = await fetch(`https://localhost:7290/api/Users/email/${encodedEmail}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${accessToken}`,
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Không thể lấy thông tin người dùng');
        }

        const userData = await response.json();
        console.log('User API response:', userData);
        
        // Lưu thông tin người dùng vào localStorage
        localStorage.setItem('user', JSON.stringify(userData));
        localStorage.setItem('userID', userData.serID);
        localStorage.setItem('userEmail', userData.Email);
        localStorage.setItem('passwordHash', userData.PPasswordHash);
        // Cập nhật UI
        updateUIAfterLogin(userData);
        
        

    } catch (error) {
        console.error('Error getting user info:', error);
        showNotification('Không thể lấy thông tin người dùng', 'danger');
    }
}

// Hàm giải mã token và chuyển hướng
function decodeTokenAndRedirect(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        const payload = JSON.parse(jsonPayload);
        const role = payload['role'];

        console.log('Role from token:', role);

        // Chuyển hướng người dùng dựa trên vai trò
        if (role === 'Admin') {
            window.location.href = '/admin/index.html';
        } else if (role === 'User') {
            window.location.href = '/index.html';
        } else {
            console.log('Unknown role:', role);
            window.location.href = '/';
        }
    } catch (error) {
        console.error('Error decoding token:', error);
        showNotification('Có lỗi xảy ra khi xác thực', 'danger');
    }
} 