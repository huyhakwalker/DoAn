import React from "react";
import { Link } from "react-router-dom";

function ThanhDieuHuong() {
  return (
    <nav className="navbar navbar-expand-lg navbar-light bg-light">
      <div className="container-fluid">
        <Link className="navbar-brand" to="/">NTUCoder</Link>
        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarNav"
          aria-controls="navbarNav"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon"></span>
        </button>
        <div className="collapse navbar-collapse" id="navbarNav">
          <ul className="navbar-nav">
            <li className="nav-item">
              <Link className="nav-link" to="/">Trang chủ</Link>
            </li>
            <li className="nav-item">
              <Link className="nav-link" to="/baitap">Bài tập</Link>
            </li>
            <li className="nav-item">
              <Link className="nav-link" to="/kythi">Kỳ thi</Link>
            </li>
            <li className="nav-item">
              <Link className="nav-link" to="/faq">Câu hỏi thường gặp</Link>
            </li>
            <li className="nav-item">
              <Link className="nav-link" to="/chat">Chat</Link>
            </li>
          </ul>
        </div>
        <Link className="btn btn-primary" to="/dangnhap">Đăng nhập</Link>
      </div>
    </nav>
  );
}

export default ThanhDieuHuong;
