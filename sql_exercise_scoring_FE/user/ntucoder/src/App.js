import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import ThanhDieuHuong from "./components/ThanhDieuHuong";
import TrangChu from "./components/TrangChu";
import BaiTap from "./components/BaiTap";
import ChiTietBaiTap from "./components/ChiTietBaiTap";
import NopBai from "./components/NopBai";
import KyThi from "./components/KyThi";
import CauHoiThuongGap from "./components/CauHoiThuongGap";
import HopChat from "./components/HopChat";
import DangNhap from "./components/DangNhap";
import ThongTinNguoiDung from "./components/ThongTinNguoiDung";
import ChanTrang from "./components/ChanTrang";

function App() {
  return (
    <Router>
      <div>
        <ThanhDieuHuong />
        <Routes>
          <Route path="/" element={<TrangChu />} />
          <Route path="/baitap" element={<BaiTap />} />
          <Route path="/baitap/:id" element={<ChiTietBaiTap />} />
          <Route path="/kythi" element={<KyThi />} />
          <Route path="/faq" element={<CauHoiThuongGap />} />
          <Route path="/chat" element={<HopChat />} />
          <Route path="/dangnhap" element={<DangNhap />} />
          <Route path="/thongtin" element={<ThongTinNguoiDung />} />
        </Routes>
        <ChanTrang />
      </div>
    </Router>
  );
}

export default App;
