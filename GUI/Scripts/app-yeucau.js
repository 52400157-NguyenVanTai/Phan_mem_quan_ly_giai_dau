(function () {
  const api = {
    all: "/YeuCauApi/All",
    handle: "/YeuCauApi/HandleRequest",
    detailGd: "/YeuCauApi/GetTournamentDetail"
  };

  const state = { requests: [], filter: "all" };
  const $ = (id) => document.getElementById(id);

  function getData(res) {
    return res && res.success ? res.data : null;
  }

  function showMessage(message, ok) {
    const box = $("yeuCauMessage");
    if (box) {
      box.innerHTML = `<div class="alert ${ok ? "alert-success" : "alert-danger"} alert-dismissible fade show">
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
      </div>`;
    }
  }

  function post(url, data) {
    return fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data || {}),
    }).then((r) => r.json());
  }

  function text(v) {
    return v || "Chưa cập nhật";
  }

  function loadRequests() {
    fetch(api.all)
      .then((r) => r.json())
      .then((res) => {
        state.requests = getData(res) || [];
        renderRequests();
      });
  }

  function renderRequests() {
    const list = $("requestList");
    if (!list) return;

    let items = state.requests;
    if (state.filter === "giai_dau") {
      items = items.filter(x => ["yeu_cau_tao_giai_dau", "dang_ky_tham_gia_giai_dau", "loi_moi_tham_gia_giai", "loi_moi_trong_tai", "loi_moi_btc", "phan_cong_trong_tai", "yeu_cau_lineup"].includes(x.loai_yeu_cau));
    } else if (state.filter === "doi") {
      items = items.filter(x => ["loi_moi", "xin_gia_nhap"].includes(x.loai_yeu_cau));
    }

    if (items.length === 0) {
      list.innerHTML = '<div class="empty-page-card"><h3>Không có yêu cầu nào</h3></div>';
      return;
    }

    list.innerHTML = items.map(x => {
      let title = "";
      let desc = "";
      let actionButtons = "";
      let detailBtn = "";

      switch (x.loai_yeu_cau) {
        case "yeu_cau_tao_giai_dau":
          title = `Duyệt giải đấu: ${x.ten_giai_dau}`;
          desc = `Người tạo: ${text(x.ten_nguoi_gui)} • Game: ${text(x.ten_game)}`;
          detailBtn = `<button class="btn btn-outline-info" onclick="viewTournamentDetail(${x.ma_giai_dau}, true)">Xem chi tiết</button>`;
          actionButtons = `
            <button class="btn btn-success" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, true)">Phê duyệt</button>
            <button class="btn btn-danger" onclick="rejectTournament(${x.ma_yeu_cau})">Từ chối</button>`;
          break;
        case "dang_ky_tham_gia_giai_dau":
          title = `Đội ${x.ten_doi} xin tham gia giải`;
          desc = `Giải đấu: ${x.ten_giai_dau} • Chủ tịch: ${text(x.ten_nguoi_gui)}`;
          detailBtn = `<a class="btn btn-outline-info" href="/Doi/ChiTiet/${x.ma_doi}">Xem Đội</a>`;
          actionButtons = `
            <button class="btn btn-success" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, true)">Duyệt</button>
            <button class="btn btn-danger" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, false)">Từ chối</button>`;
          break;
        case "loi_moi_tham_gia_giai":
          title = `Mời đội tham gia giải`;
          desc = `Giải đấu: ${x.ten_giai_dau} mời đội của bạn tham gia.`;
          detailBtn = `<a class="btn btn-outline-info" href="/GiaiDau/ChiTiet/${x.ma_giai_dau}">Xem Giải Đấu</a>`;
          actionButtons = `
            <button class="btn btn-success" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, true)">Tham gia</button>
            <button class="btn btn-danger" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, false)">Từ chối</button>`;
          break;
        case "loi_moi_trong_tai":
        case "loi_moi_btc":
          title = x.tieu_de;
          desc = `Giải đấu: ${text(x.ten_giai_dau)} • Lời nhắn: ${text(x.noi_dung)}`;
          detailBtn = x.ma_giai_dau ? `<a class="btn btn-outline-info" href="/GiaiDau/ChiTiet/${x.ma_giai_dau}">Xem Giải Đấu</a>` : "";
          actionButtons = `
            <button class="btn btn-success" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, true)">Chấp nhận</button>
            <button class="btn btn-danger" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, false)">Từ chối</button>`;
          break;
        case "phan_cong_trong_tai":
          title = x.tieu_de || "Phân công trọng tài";
          desc = `Giải đấu: ${text(x.ten_giai_dau)} - ${text(x.noi_dung)}`;
          detailBtn = x.ma_giai_dau ? `<a class="btn btn-outline-info" href="/GiaiDau/ChiTiet/${x.ma_giai_dau}">Vào trận đấu</a>` : "";
          actionButtons = `<button class="btn btn-success" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, true)">Đã nhận</button>`;
          break;
        case "yeu_cau_lineup":
          title = x.tieu_de || "Yêu cầu chốt đội hình";
          desc = `Giải đấu: ${text(x.ten_giai_dau)} - Đội: ${text(x.ten_doi)} - ${text(x.noi_dung)}`;
          detailBtn = x.ma_giai_dau ? `<a class="btn btn-outline-info" href="/GiaiDau/ChiTiet/${x.ma_giai_dau}">Chốt đội hình</a>` : "";
          actionButtons = `<button class="btn btn-success" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, true)">Đã xem</button>`;
          break;
        case "loi_moi":
          title = `Lời mời vào đội: ${x.ten_doi}`;
          desc = `Lời nhắn: ${text(x.noi_dung)}`;
          detailBtn = `<a class="btn btn-outline-info" href="/Doi/ChiTiet/${x.ma_doi}">Xem Đội</a>`;
          actionButtons = `
            <button class="btn btn-success" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, true)">Chấp nhận</button>
            <button class="btn btn-danger" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, false)">Từ chối</button>`;
          break;
        case "xin_gia_nhap":
          title = `Đơn xin gia nhập đội: ${x.ten_doi}`;
          desc = `Người gửi: ${text(x.ten_nguoi_gui)} • Lời nhắn: ${text(x.noi_dung)}`;
          actionButtons = `
            <button class="btn btn-success" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, true)">Chấp nhận</button>
            <button class="btn btn-danger" onclick="handleReq('${x.loai_yeu_cau}', ${x.ma_yeu_cau}, false)">Từ chối</button>`;
          break;
      }

      const dateStr = new Date(x.ngay_tao).toLocaleString("vi-VN");

      return `
        <article class="request-card mb-3 p-3" style="background-color: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-md);">
          <div class="d-flex justify-content-between align-items-start">
            <div>
              <h4 class="mb-1">${title}</h4>
              <p class="text-muted mb-2">${desc}</p>
              <small class="text-secondary">${dateStr}</small>
            </div>
            <div class="d-flex gap-2">
              ${detailBtn}
              ${actionButtons}
            </div>
          </div>
        </article>`;
    }).join("");
  }

  window.handleReq = function (loai, id, isAccept, lyDo = "") {
    post(api.handle, {
      loai_yeu_cau: loai,
      ma_yeu_cau: id,
      chap_nhan: isAccept,
      ly_do: lyDo
    }).then(r => {
      showMessage(r.message, r.success);
      if (r.success) {
        if ($("modalTournamentDetail")) {
          const modal = bootstrap.Modal.getInstance($("modalTournamentDetail"));
          if (modal) modal.hide();
        }
        loadRequests();
      }
    });
  };

  window.rejectTournament = function (maGiaiDau) {
    const reason = prompt("Nhập lý do từ chối giải đấu:");
    if (reason) {
      handleReq("yeu_cau_tao_giai_dau", maGiaiDau, false, reason);
    }
  };

  window.viewTournamentDetail = function (id, forAdmin) {
    fetch(`${api.detailGd}?maGiaiDau=${id}`)
      .then(r => r.json())
      .then(res => {
        if (!res.success) {
          showMessage(res.message, false);
          return;
        }
        const data = res.data;
        const gd = data.giai_dau;
        let html = `
          <h5>${gd.ten_giai_dau}</h5>
          <p><b>Game:</b> ${text(gd.ten_game)}</p>
          <p><b>Thể thức:</b> ${gd.the_thuc}</p>
          <p><b>Số đội:</b> ${gd.so_doi_toi_thieu} - ${gd.so_doi_toi_da || 'Không giới hạn'}</p>
          <p><b>Mô tả:</b> ${text(gd.mo_ta)}</p>
          <hr/>
          <h6>Các giai đoạn thi đấu:</h6>
          <ul>
            ${data.giai_doan.map(st => `<li>${st.ten_giai_doan} (${st.the_thuc}) - ${st.so_doi} đội</li>`).join("")}
          </ul>
        `;
        $("tournamentDetailBody").innerHTML = html;
        
        if (forAdmin) {
          document.querySelector("#tournamentDetailFooter .admin-actions").classList.remove("d-none");
          $("btnApproveTournament").onclick = () => handleReq("yeu_cau_tao_giai_dau", id, true);
          $("btnRejectTournament").onclick = () => rejectTournament(id);
        } else {
          document.querySelector("#tournamentDetailFooter .admin-actions").classList.add("d-none");
        }

        const modal = new bootstrap.Modal($("modalTournamentDetail"));
        modal.show();
      });
  };

  document.addEventListener("DOMContentLoaded", () => {
    loadRequests();

    document.querySelectorAll(".request-filters .btn").forEach(btn => {
      btn.addEventListener("click", (e) => {
        document.querySelectorAll(".request-filters .btn").forEach(b => b.classList.remove("active"));
        e.target.classList.add("active");
        state.filter = e.target.dataset.filter;
        renderRequests();
      });
    });
  });
})();
