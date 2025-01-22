import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import axios from "axios";

function BaiTap() {
  const [baiTap, setBaiTap] = useState([]);

  useEffect(() => {
    // Giả sử API trả danh sách bài tập
    axios.get("/api/baitap").then((response) => {
      setBaiTap(response.data);
    });
  }, []);

  return (
    <div className="container">
      <h1>Danh sách bài tập</h1>
      <table className="table">
        <thead>
          <tr>
            <th>Mã</th>
            <th>Tên bài tập</th>
            <th>Độ khó</th>
          </tr>
        </thead>
        <tbody>
          {baiTap.map((bt) => (
            <tr key={bt.id}>
              <td>{bt.ma}</td>
              <td>
                <Link to={`/baitap/${bt.id}`}>{bt.ten}</Link>
              </td>
              <td>{bt.doKho}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default BaiTap;
