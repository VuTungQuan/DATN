// Hàm để lấy danh sách PitchTypes từ API
async function fetchPitchTypes() {
    try {
        const response = await fetch('https://localhost:7290/api/PitchType?page=1&pageSize=10', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        const data = await response.json();
        displayPitchTypes(data);
    } catch (error) {
        console.error('Error fetching pitch types:', error);
        showNotification('Có lỗi xảy ra khi tải danh sách loại sân', 'error');
    }
}

// Hàm hiển thị danh sách PitchTypes
function displayPitchTypes(data) {
    const pitchListContainer = document.getElementById('pitch-list');
    pitchListContainer.innerHTML = ''; // Xóa nội dung cũ

    data.forEach(pitchType => {
        const pitchTypeCard = document.createElement('div');
        pitchTypeCard.className = 'col-lg-4 col-md-6 col-sm-12 pb-1';
        pitchTypeCard.innerHTML = `
            <div class="product-item bg-light mb-4">
                <div class="product-img position-relative overflow-hidden">
                    <img class="img-fluid w-100" src="${pitchType.imageUrl || 'img/default-pitch.jpg'}" alt="${pitchType.name}">
                </div>
                <div class="text-center py-4">
                    <h5 class="text-uppercase">${pitchType.name}</h5>
                    <p class="text-muted">${pitchType.description || 'Không có mô tả'}</p>
                    <a href="#" class="btn btn-primary" onclick="viewPitchTypeDetail(${pitchType.id})">Xem chi tiết</a>
                </div>
            </div>
        `;
        pitchListContainer.appendChild(pitchTypeCard);
    });
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

// Gọi hàm fetchPitchTypes khi trang được tải
document.addEventListener('DOMContentLoaded', fetchPitchTypes); 