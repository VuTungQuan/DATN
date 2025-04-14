
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
        // Xử lý khi gửi form đăng nhập
        document.getElementById('login-form-content').addEventListener('submit', async (e) => {
            e.preventDefault();
            const email = document.getElementById('login-email').value;
            const password = document.getElementById('login-password').value;

            const response = await fetch('https://localhost:7290/api/Auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email, password })
            });

            const data = await response.json();

            if (response.ok) {
                // Lưu token vào localStorage
                localStorage.setItem('token', data.accessToken); // Lưu JWT token để sử dụng sau này
                
                // Sau khi đăng nhập thành công, lấy thông tin người dùng
                getUserInfoByEmail(email); // Lấy thông tin người dùng qua email
            } else {
                alert('Đăng nhập không thành công.');
            }
            closeModal(); // Đóng modal sau khi đăng nhập thành công
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
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));

            const payload = JSON.parse(jsonPayload);
            const role = payload['role']; // Nhận vai trò từ payload của token

            // Chuyển hướng người dùng dựa trên vai trò
            switch (role) {
                case 'Admin':
                    window.location.href = 'http://127.0.0.1:5500/admin/index.html'; // Chuyển hướng đến trang Admin
                    break;
                case 'User':
                    window.location.href = 'http://127.0.0.1:5500/index.html'; // Chuyển hướng đến trang Người dùng
                    break;
                default:
                    window.location.href = '/'; // Chuyển hướng mặc định
                    break;
            }
        }

        // Kiểm tra nếu người dùng đã đăng nhập trước đó
        if (localStorage.getItem('token')) {
            // Lấy thông tin người dùng từ localStorage và cập nhật giao diện
            const user = JSON.parse(localStorage.getItem('user'));
            updateUserUI(user);  // Cập nhật thông tin người dùng trong dropdown
        }


        // Xử lý gửi form đăng ký
        document.getElementById('register-form-content').addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const fullName = document.getElementById('register-name').value;
            const email = document.getElementById('register-email').value;
            const phoneNumber = document.getElementById('register-phone').value;
            const passwordHash = document.getElementById('register-password').value;
            const role = "User";
            
            try {
                const response = await fetch('https://localhost:7290/api/Auth/register', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ fullName, phoneNumber, email, passwordHash, role })
                });
                const data = await response.json();
                
                if (response.ok) {
                    alert('Đăng ký thành công! Vui lòng đăng nhập.');
                    cancelRegister(); // Quay lại form đăng nhập
                } else if (response.status === 400) {
                    alert('Email đã tồn tại. Vui lòng chọn email khác.');
                } else if (response.status === 500) {
                    alert('Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại sau.');
                }
            } catch (error) {
                document.getElementById('register-message').textContent = 'Đã xảy ra lỗi trong quá trình đăng ký.';
            }
        });