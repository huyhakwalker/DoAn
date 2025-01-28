//// Lắng nghe sự kiện DOMContentLoaded khi trang được tải
//document.addEventListener('DOMContentLoaded', function () {
//    const menuItems = document.querySelectorAll('.menu-link'); // Chọn tất cả các mục menu
//    const currentUrl = window.location.href; // Lấy URL hiện tại của trang

//    menuItems.forEach(item => {
//        // Kiểm tra nếu URL của mục menu trùng với URL hiện tại
//        if (currentUrl.includes(item.href)) {
//            item.classList.add('active'); // Thêm lớp "active" vào mục menu
//        } else {
//            item.classList.remove('active'); // Xóa lớp "active" khỏi các mục khác
//        }
//    });
//});

// Lắng nghe sự kiện DOMContentLoaded khi trang được tải
document.addEventListener('DOMContentLoaded', function() {
    const menuItems = document.querySelectorAll('.menu-link'); // Chọn tất cả các mục menu
    const currentPath = window.location.pathname; // Lấy đường dẫn hiện tại (không bao gồm domain)

    menuItems.forEach(item => {
        const itemPath = new URL(item.href).pathname; // Lấy đường dẫn từ href của mục menu

        // Kiểm tra nếu đường dẫn của mục menu trùng chính xác với đường dẫn hiện tại
        if (currentPath === itemPath) {
            item.classList.add('active'); // Thêm lớp "active" vào mục menu
        } else {
            item.classList.remove('active'); // Xóa lớp "active" khỏi các mục khác
        }
    });
});

// ẩn hiện con mắt đăng nhập
const passwordInput = document.getElementById('password');
const eyeIcon = document.getElementById('eye-icon');
const togglePasword = document.getElementById('toggle-password');
if (togglePasword) {
    togglePasword.addEventListener('click', () => {
        const isPassword = passwordInput.type === 'password';
        passwordInput.type = isPassword ? 'text' : 'password';
        eyeIcon.setAttribute('fill', isPassword ? '#0d6efd' : 'gray'); // Change color
    });
}

// Sinh mã captcha ngẫu nhiên và gắng vào canvas truyền vào, đồng thời trả về mã captcha
// begin
function generateCaptcha(formCaptcha) {
    const inputCaptcha = formCaptcha.querySelector(".img input");
    const canvas = formCaptcha.querySelector(".img canvas");
    const ctx = canvas.getContext('2d');

    const characters = '0123456789';
    let captchaText = '';

    for (let i = 0; i < 6; i++) {
        captchaText += characters.charAt(Math.floor(Math.random() * characters.length));
    }

    // Gán cho value của inputCaptcha
    inputCaptcha.value = captchaText;

    // Xoá nội dung trước đó
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Set màu nền cho ảnh
    ctx.fillStyle = '#f2f2f2';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Tạo đường ngang ngẫu nhiên
    for (let i = 0; i < 6; i++) {
        ctx.beginPath();
        ctx.moveTo(0, Math.random() * canvas.height);
        ctx.lineTo(canvas.width, Math.random() * canvas.height);
        ctx.strokeStyle = '#ccc';
        ctx.lineWidth = 2;
        ctx.stroke();
    }

    // Ghi captcha vào
    ctx.font = '50px Arial';
    for (let i = 0; i < captchaText.length; i++) {
        const char = captchaText[i];

        // Randomly rotate each character
        const angle = (Math.random() - 0.5) * Math.PI / 4;
        ctx.save();
        ctx.translate(40 * i + 30, 80);
        ctx.rotate(angle);
        ctx.fillStyle = '#000';
        ctx.fillText(char, 0, 10);
        ctx.restore();
    }
}

// Load lúc đầu tải trang
const formCaptcha = document.querySelector("#form-captcha")
if (formCaptcha) generateCaptcha(formCaptcha);

// Khi click đổi mã
const btnRefersh = document.querySelector("#btn-refersh");
if (btnRefersh) {
    btnRefersh.addEventListener("click", function() {
        generateCaptcha(formCaptcha);
    });
}

console.log("Run file site.js");

//Ckeditor 
const editor = document.querySelector("#editor");
if (editor) {
    ClassicEditor.create(editor).catch((error) => {
        console.error(error);
    });
}

// Click <li> hiển thị chờ...
const vinhSideBarMenu = document.querySelector("#vinhsidebar-menu");
if (vinhSideBarMenu) {
    let lis = vinhSideBarMenu.querySelectorAll("ul li");
    if (lis) {
        lis.forEach((item) => {
            item.addEventListener("click", () => wait());
        });
    }
}

function statusSideBar() {
    const toggleSidebar = document.querySelector("#vinhnav-toggle");
    if (!toggleSidebar) return;

    const savedStatus = localStorage.getItem("sidebar");
    if (savedStatus != null) {
        toggleSidebar.checked = savedStatus == "true";
    }

    toggleSidebar.addEventListener("change", function() {
        localStorage.setItem("sidebar", this.checked);
    });
}

// Gọi hàm khi trang được load
statusSideBar();


console.log("Run file site.js");


// Hàm mở modal và hiển thị thông tin người dùng
function showUserInfo(FullName, Email, PhoneNumber, title) {
    document.getElementById('info_fullname').textContent = FullName;
    document.getElementById('info_email').textContent = Email;
    document.getElementById('info_phonenumber').textContent = PhoneNumber;
    document.getElementById('info_title').textContent = title;

    document.getElementById('info_modal').style.display = 'block';
}

function closeModal() {
    document.getElementById('info_modal').style.display = 'none';
}