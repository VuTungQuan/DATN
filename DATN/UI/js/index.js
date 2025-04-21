        // Open Modal for login when clicking the button
        function openLoginModal() {
            $('#authModal').modal('show');
            $('#login-form').show();
            $('#register-form').hide();
        }

        // Show Register Form inside the modal
        function openRegisterForm() {
            $('#login-form').hide();
            $('#register-form').show();
        }
        function openRegisterModal() {
            $('#authModal').modal('show');
            $('#register-form').show();
            $('#login-form').hide();
        }
        function showRegisterForm() {
            $('#login-form').hide();
            $('#register-form').show();
        }
        // Close the modal
        function closeModal() {
            $('#authModal').modal('hide');
        }

        // Cancel Register form and show Login form
        function cancelRegister() {
            $('#register-form').hide();
            $('#login-form').show();
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

                if (!response.ok) {
                    throw new Error(data.message || 'Đăng nhập thất bại');
                }

                // Lưu token vào localStorage
                localStorage.setItem('token', data.token);
                
                // Đóng modal đăng nhập
                $('#authModal').modal('hide');
                
                // Hiển thị thông báo thành công
                showNotification('Đăng nhập thành công', 'success');
                
                // Cập nhật UI sau khi đăng nhập
                updateUIAfterLogin();
                
            } catch (error) {
                console.error('Login error:', error);
                showNotification(error.message || 'Đăng nhập thất bại', 'error');
            }
        }

        // Hàm xử lý đăng ký
        async function handleRegister(event) {
            event.preventDefault();
            
            const fullName = document.getElementById('register-name').value;
            const email = document.getElementById('register-email').value;
            const phoneNumber = document.getElementById('register-phone').value;
            const passwordHash = document.getElementById('register-password').value;

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
                        passwordHash: passwordHash
                    })
                });

                const data = await response.json();

                if (!response.ok) {
                    throw new Error(data.message || 'Đăng ký thất bại');
                }

                // Hiển thị thông báo thành công
                showNotification('Đăng ký thành công! Vui lòng đăng nhập', 'success');
                
                // Chuyển về form đăng nhập
                $('#register-form').hide();
                $('#login-form').show();
                
            } catch (error) {
                console.error('Register error:', error);
                showNotification(error.message || 'Đăng ký thất bại', 'error');
            }
        }

        // Hàm cập nhật UI sau khi đăng nhập
        function updateUIAfterLogin() {
            // Thay đổi nút dropdown
            const userDropdown = document.getElementById('userDropdown');
            const dropdownMenu = document.getElementById('dropdownMenu');
            
            userDropdown.innerHTML = 'Tài khoản của tôi';
            dropdownMenu.innerHTML = `
                <a class="dropdown-item" href="#" onclick="viewProfile()">Thông tin cá nhân</a>
                <a class="dropdown-item" href="#" onclick="viewBookings()">Lịch sử đặt sân</a>
                <a class="dropdown-item" href="#" onclick="handleLogout()">Đăng xuất</a>
            `;
        }

        // Hàm xử lý đăng xuất
        function handleLogout() {
            // Xóa token
            localStorage.removeItem('token');
            
            // Cập nhật UI
            const userDropdown = document.getElementById('userDropdown');
            const dropdownMenu = document.getElementById('dropdownMenu');
            
            userDropdown.innerHTML = 'Tài khoản';
            dropdownMenu.innerHTML = `
                <button class="dropdown-item" id="loginBtn" onclick="openLoginModal()">Đăng nhập</button>
                <button class="dropdown-item" id="registerBtn" onclick="openRegisterModal()">Đăng ký</button>
            `;
            
            // Hiển thị thông báo
            showNotification('Đã đăng xuất thành công', 'success');
        }

        // Hàm hiển thị thông báo
        function showNotification(message, type = 'success') {
            const notification = document.getElementById('notification');
            notification.textContent = message;
            notification.className = `notification ${type}`;
            notification.style.display = 'block';
            
            setTimeout(() => {
                notification.style.display = 'none';
            }, 3000);
        }

        // Thêm event listeners
        document.addEventListener('DOMContentLoaded', function() {
            // Event listener cho form đăng nhập
            document.getElementById('login-form-content').addEventListener('submit', handleLogin);
            
            // Event listener cho form đăng ký
            document.getElementById('register-form-content').addEventListener('submit', handleRegister);
            
            // Kiểm tra trạng thái đăng nhập khi tải trang
            const token = localStorage.getItem('token');
            if (token) {
                updateUIAfterLogin();
            }
        });

        // Hàm lấy thông tin người dùng từ email sau khi đăng nhập
        async function getUserInfoByEmail(email) {
            const encodedEmail = encodeURIComponent(email); // Mã hóa email để dùng trong URL
            const response = await fetch(`https://localhost:7290/api/Users/email/${encodedEmail}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`, // Thêm token vào header để xác thực
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                // Sau khi cập nhật UI, giải mã token và chuyển hướng
                const token = localStorage.getItem('token');
                decodeTokenAndRedirect(token);  // Gọi hàm chuyển hướng sau khi nhận thông tin người dùng

                const userData = await response.json();
                
                // Lưu thông tin người dùng vào localStorage
                localStorage.setItem('user', JSON.stringify(userData)); // Lưu thông tin người dùng để sử dụng sau này
                localStorage.setItem('userID', userData.userID); // Lưu userID vào localStorage
                updateUserUI(userData); 

                
            } else {
                alert('Không thể lấy thông tin người dùng.');
            }
        }

        // Cập nhật giao diện UI sau khi lấy thông tin người dùng
        function updateUserUI(user) {
            const userDropdown = document.getElementById('userDropdown');
            const dropdownMenu = document.getElementById('dropdownMenu');

            // Cập nhật tên đầy đủ của người dùng vào dropdown
            userDropdown.innerText = user.fullName;  // Gán tên đầy đủ vào dropdown
            
            // Thay đổi các mục menu để hiển thị các tùy chọn cho người dùng
            dropdownMenu.innerHTML = `
                <button class="dropdown-item" onclick="viewProfile()">Thông tin cá nhân</button>
                <button class="dropdown-item" onclick="viewBookings()">Lịch sử đặt sân</button>
                <button class="dropdown-item" onclick="logout()">Đăng xuất</button>
            `;
            // Hiển thị menu người dùng sau khi đăng nhập
            document.getElementById('user-menu').style.display = 'block';
        }

        // Xử lý khi đăng xuất
        function logout() {
            localStorage.removeItem('token');  // Xóa token khỏi localStorage
            localStorage.removeItem('user');   // Xóa thông tin người dùng khỏi localStorage
            window.location.reload();  // Tải lại trang để cập nhật giao diện
        }

        // Hàm hiển thị thông tin cá nhân của người dùng
        function viewProfile() {
            window.location.href = '../account.html';
        }

        // Hàm hiển thị lịch sử đặt sân của người dùng
        function viewBookings() {
            alert('Hiển thị lịch sử đặt sân');
        }

        // Giải mã JWT và chuyển hướng dựa trên vai trò
        function decodeTokenAndRedirect(token) {
            try {
                const base64Url = token.split('.')[1];
                const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
                const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
                    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
                }).join(''));

                const payload = JSON.parse(jsonPayload);
                const role = payload['role']; // Nhận vai trò từ payload của token

                console.log('Role from token:', role); // Log để debug

                // Chuyển hướng người dùng dựa trên vai trò
                if (role === 'Admin') {
                    window.location.href = '/admin/index.html'; // Chuyển hướng đến trang Admin
                } else if (role === 'User') {
                    window.location.href = '/index.html'; // Chuyển hướng đến trang Người dùng
                } else {
                    console.log('Unknown role:', role); // Log để debug
                    window.location.href = '/'; // Chuyển hướng mặc định
                }
            } catch (error) {
                console.error('Error decoding token:', error);
                showNotification('Có lỗi xảy ra khi xác thực', 'error');
            }
        }

        // Kiểm tra nếu người dùng đã đăng nhập trước đó
        if (localStorage.getItem('token')) {
            // Lấy thông tin người dùng từ localStorage và cập nhật giao diện
            const user = JSON.parse(localStorage.getItem('user'));
            updateUserUI(user);  // Cập nhật thông tin người dùng trong dropdown
        }


