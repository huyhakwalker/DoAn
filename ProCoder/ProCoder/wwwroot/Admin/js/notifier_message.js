// Kích hoạt thông báo
function active_notifer(linkTo) {
    const toast = document.querySelector("#toast");
    if (!toast) return;

    toast.classList.add("show_or_hidden");
    const time = 3000;

    setTimeout(() => {
        toast.classList.remove("show_or_hidden");
        if (linkTo != "") location.href = linkTo;
    }, time);
}

// Hiển thị chờ đợi...
function wait() {
    const loading = document.querySelector("#loading");
    if (!loading) return;

    loading.classList.remove("d-none");
}

// Hiển thi/thay đổi số lượng yêu cầu do người dùng gửi
function showTotalTickets() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/listenerTickets")
        .build();

    connection.start().catch(error => console.erroror(`Lỗi: ${error}`));

    connection.on("UpdateIconNotification", (count) => {
        const iconNotification = document.querySelector("#icon-notification");
        if (iconNotification) {
            iconNotification.innerText = count == 0 ? "" : count;
        }
    });
}
showTotalTickets();

console.log("Run file notifer_message.js");