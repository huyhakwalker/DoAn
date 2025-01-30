function togglePassword(inputId, iconId) {
    const input = document.getElementById(inputId);
    const icon = document.getElementById(iconId);

    if (input.type === "password") {
        input.type = "text";
        icon.setAttribute("fill", "black"); // Đổi màu icon khi hiện mật khẩu
    } else {
        input.type = "password";
        icon.setAttribute("fill", "red"); // Đổi màu icon khi ẩn mật khẩu
    }
}