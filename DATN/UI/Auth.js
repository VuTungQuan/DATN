// Đóng modal khi đăng nhập thành công
function closeModal() {
    $('#authModal').modal('hide');
}

// Xử lý gửi form đăng nhập
document.getElementById('login-form-content').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;
    
    try {
        const response = await fetch('https://localhost:7074/api/Auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            localStorage.setItem('userEmail', email);
            setCookie('token', data.token, 1);
            closeModal();  // Đóng modal sau khi đăng nhập thành công
            decodeTokenAndRedirect(data.token);
        } else {
            alert('Đăng nhập không thành công. Vui lòng kiểm tra thông tin.');
        }
        
    } catch (error) {
        console.log(error);
        
        alert('Đã xảy ra lỗi trong quá trình đăng nhập.');
    }
});
