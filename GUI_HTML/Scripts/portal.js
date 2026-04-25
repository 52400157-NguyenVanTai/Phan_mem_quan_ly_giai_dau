(function () {
  const alertBox = document.getElementById("global-alert");
  const landingScreen = document.getElementById("landing-screen");
  const authScreen = document.getElementById("auth-screen");
  const dashboardScreen = document.getElementById("dashboard-screen");
  const loginPanel = document.getElementById("login-panel");
  const registerPanel = document.getElementById("register-panel");
  const currentUserLabel = document.getElementById("current-user-label");

  function showAlert(success, message, data) {
    if (!alertBox) return;
    alertBox.classList.remove(
      "d-none",
      "alert-success",
      "alert-danger",
      "alert-info",
    );
    alertBox.classList.add(success ? "alert-success" : "alert-danger");
    const payload = data ? " | Data: " + JSON.stringify(data) : "";
    alertBox.textContent = message + payload;
  }

  function showScreen(target) {
    if (landingScreen) landingScreen.classList.add("d-none");
    if (authScreen) authScreen.classList.add("d-none");
    if (dashboardScreen) dashboardScreen.classList.add("d-none");

    if (target === "landing" && landingScreen)
      landingScreen.classList.remove("d-none");
    if (target === "auth" && authScreen) authScreen.classList.remove("d-none");
    if (target === "dashboard" && dashboardScreen)
      dashboardScreen.classList.remove("d-none");
  }

  function showAuthPanel(panel) {
    if (!loginPanel || !registerPanel) return;

    if (panel === "login") {
      loginPanel.classList.remove("d-none");
      registerPanel.classList.remove("d-none");
    } else if (panel === "register") {
      loginPanel.classList.remove("d-none");
      registerPanel.classList.remove("d-none");
    }
  }

  function showModule(targetId) {
    const modules = document.querySelectorAll(".module-panel");
    modules.forEach(function (m) {
      m.classList.toggle("d-none", m.id !== targetId);
    });

    const navButtons = document.querySelectorAll(".module-nav");
    navButtons.forEach(function (btn) {
      const active = btn.getAttribute("data-target") === targetId;
      btn.classList.toggle("btn-primary", active);
      btn.classList.toggle("btn-outline-primary", !active);
    });
  }

  function setCurrentUserLabel(data) {
    if (!currentUserLabel) return;
    if (!data) {
      currentUserLabel.textContent = "Chưa đăng nhập";
      return;
    }
    const role = data.VaiTroHeThong ? " [" + data.VaiTroHeThong + "]" : "";
    currentUserLabel.textContent =
      (data.TenDangNhap || data.Email || "User") + role;
  }

  async function postJson(url, payload) {
    const res = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload || {}),
    });
    return res.json();
  }

  async function postForm(url, form) {
    const formData = new URLSearchParams(new FormData(form));
    const res = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
      },
      body: formData.toString(),
    });
    return res.json();
  }

  function bindForm(id, handler) {
    const form = document.getElementById(id);
    if (!form) return;
    form.addEventListener("submit", async function (e) {
      e.preventDefault();
      try {
        await handler(form);
      } catch (err) {
        showAlert(false, "Lỗi kết nối API", { error: err.message });
      }
    });
  }

  function disableButton(btn, disabled) {
    if (!btn) return;
    btn.disabled = !!disabled;
    btn.classList.toggle("opacity-50", !!disabled);
  }

  function renderRefereeMatchList(rows) {
    const container = document.getElementById("referee-match-list");
    if (!container) return;
    if (!Array.isArray(rows) || rows.length === 0) {
      container.innerHTML =
        "<span class='text-muted'>Không có trận nào trong tab này.</span>";
      return;
    }

    const html = rows
      .map(function (r) {
        return (
          "<div class='border rounded p-2 mb-2'>" +
          "<b>Trận #" +
          r.ma_tran +
          "</b> | GĐ: " +
          (r.ma_giai_doan || "-") +
          " | Trạng thái: " +
          (r.trang_thai || "-") +
          " | Bắt đầu: " +
          (r.thoi_gian_bat_dau || "-") +
          " | Nhập điểm: " +
          (r.thoi_gian_nhap_diem || "chưa") +
          " | Số lần sửa: " +
          (r.so_lan_sua || 0) +
          "</div>"
        );
      })
      .join("");
    container.innerHTML = html;
  }

  function renderDisputeList(rows) {
    const container = document.getElementById("referee-dispute-list");
    if (!container) return;
    if (!Array.isArray(rows) || rows.length === 0) {
      container.innerHTML =
        "<span class='text-muted'>Không có khiếu nại phù hợp.</span>";
      return;
    }

    container.innerHTML = rows
      .map(function (r) {
        return (
          "<div class='border rounded p-2 mb-2'>" +
          "<b>KN #" +
          r.ma_khieu_nai +
          "</b> | Trận #" +
          r.ma_tran +
          " | Nhóm #" +
          r.ma_nhom +
          " | Người gửi: " +
          (r.ten_nguoi_gui || r.ma_nguoi_gui) +
          " | Trạng thái: " +
          (r.trang_thai || "-") +
          "<div class='small mt-1'>" +
          (r.noi_dung || "") +
          "</div>" +
          "</div>"
        );
      })
      .join("");
  }

  function renderAuditLog(rows) {
    const container = document.getElementById("referee-audit-list");
    if (!container) return;
    if (!Array.isArray(rows) || rows.length === 0) {
      container.innerHTML =
        "<span class='text-muted'>Không có dữ liệu audit log phù hợp.</span>";
      return;
    }

    container.innerHTML = rows
      .map(function (r) {
        return (
          "<div class='border rounded p-2 mb-2'>" +
          "<b>Log #" +
          r.ma_log +
          "</b> | Trận #" +
          r.ma_tran +
          " | Người sửa: " +
          (r.ten_nguoi_sua || r.nguoi_sua || "-") +
          " | Thời gian: " +
          (r.thoi_gian_sua || "-") +
          "<div class='small mt-1'><b>Lý do:</b> " +
          (r.ly_do_sua || "(không có)") +
          "</div>" +
          "</div>"
        );
      })
      .join("");
  }

  function recalcRefereeKda() {
    const tbody = document.querySelector("#referee-player-grid tbody");
    if (!tbody) return;

    const rows = Array.from(tbody.querySelectorAll("tr"));
    let maxKda = -1;
    rows.forEach(function (row) {
      const k = Number(row.querySelector(".ref-kill")?.value || 0);
      const d = Number(row.querySelector(".ref-death")?.value || 0);
      const a = Number(row.querySelector(".ref-assist")?.value || 0);
      const kda = (k + a) / Math.max(1, d);
      const kdaCell = row.querySelector(".ref-kda");
      if (kdaCell) kdaCell.textContent = kda.toFixed(2);
      row.setAttribute("data-kda", String(kda));
      if (kda > maxKda) maxKda = kda;
    });

    rows.forEach(function (row) {
      const kda = Number(row.getAttribute("data-kda") || 0);
      row.classList.toggle(
        "table-success",
        maxKda >= 0 && Math.abs(kda - maxKda) < 0.0001,
      );
    });
  }

  function bindGridKeyboardNav() {
    const tbody = document.querySelector("#referee-player-grid tbody");
    if (!tbody) return;

    tbody.querySelectorAll("input[type='number']").forEach(function (input) {
      input.addEventListener("keydown", function (e) {
        if (e.key !== "Enter") return;
        e.preventDefault();
        const inputs = Array.from(
          tbody.querySelectorAll("input[type='number']"),
        );
        const idx = inputs.indexOf(e.target);
        if (idx >= 0 && idx + 1 < inputs.length) {
          inputs[idx + 1].focus();
          inputs[idx + 1].select();
        }
      });
    });
  }

  function renderRefereeGrid(roster) {
    const tbody = document.querySelector("#referee-player-grid tbody");
    if (!tbody) return;
    tbody.innerHTML = "";

    if (!Array.isArray(roster) || roster.length === 0) {
      tbody.innerHTML =
        "<tr><td colspan='7' class='text-muted'>Chưa có dữ liệu đội hình cho trận này.</td></tr>";
      return;
    }

    roster.forEach(function (p) {
      const tr = document.createElement("tr");
      tr.dataset.userId = p.ma_nguoi_dung;
      tr.dataset.maViTri = p.ma_vi_tri || "";
      tr.innerHTML =
        "<td>" +
        (p.ten_nhom || "#" + p.ma_nhom) +
        "</td>" +
        "<td>" +
        (p.ten_hien_thi || p.ten_dang_nhap || "User " + p.ma_nguoi_dung) +
        "</td>" +
        "<td><input class='form-control form-control-sm ref-kill' type='number' min='0' value='" +
        Number(p.so_kill || 0) +
        "' /></td>" +
        "<td><input class='form-control form-control-sm ref-death' type='number' min='0' value='" +
        Number(p.so_death || 0) +
        "' /></td>" +
        "<td><input class='form-control form-control-sm ref-assist' type='number' min='0' value='" +
        Number(p.so_assist || 0) +
        "' /></td>" +
        "<td><input class='form-control form-control-sm ref-survival' type='number' min='0' step='0.1' value='" +
        Number(p.diem_sinh_ton || 0) +
        "' /></td>" +
        "<td class='ref-kda'>0.00</td>";
      tbody.appendChild(tr);
    });

    tbody.querySelectorAll("input").forEach(function (input) {
      input.addEventListener("input", recalcRefereeKda);
    });

    bindGridKeyboardNav();
    recalcRefereeKda();
  }

  function buildRefereePayload() {
    const maTran = Number(
      document.getElementById("referee-ma-tran")?.value || 0,
    );
    const lyDo = (document.getElementById("referee-ly-do")?.value || "").trim();
    const rows = Array.from(
      document.querySelectorAll("#referee-player-grid tbody tr[data-user-id]"),
    );
    const chiSoNguoiChoi = rows.map(function (row) {
      return {
        MaNguoiDung: Number(row.dataset.userId || 0),
        MaViTri: row.dataset.maViTri ? Number(row.dataset.maViTri) : null,
        SoKill: Number(row.querySelector(".ref-kill")?.value || 0),
        SoDeath: Number(row.querySelector(".ref-death")?.value || 0),
        SoAssist: Number(row.querySelector(".ref-assist")?.value || 0),
        DiemSinhTon: Number(row.querySelector(".ref-survival")?.value || 0),
      };
    });

    return { MaTran: maTran, LyDo: lyDo, ChiSoNguoiChoi: chiSoNguoiChoi };
  }

  bindForm("form-register", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    const result = await postJson("/AuthApi/DangKy", payload);
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) {
      form.reset();
    }
  });

  bindForm("form-login", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    const result = await postJson("/AuthApi/DangNhap", payload);
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) {
      showScreen("dashboard");
      setCurrentUserLabel(result.Data || null);
      showModule("module-identity");
      form.reset();
    }
  });

  const logoutBtn = document.getElementById("btn-logout");
  if (logoutBtn) {
    logoutBtn.addEventListener("click", async function () {
      try {
        const result = await postJson("/AuthApi/DangXuat", {});
        showAlert(result.Success, result.Message, result.Data);
        if (result.Success) {
          setCurrentUserLabel(null);
          showScreen("landing");
        }
      } catch (err) {
        showAlert(false, "Không thể đăng xuất", { error: err.message });
      }
    });
  }

  const btnShowLogin = document.getElementById("btn-show-login");
  if (btnShowLogin) {
    btnShowLogin.addEventListener("click", function () {
      showScreen("auth");
      showAuthPanel("login");
    });
  }

  const btnShowRegister = document.getElementById("btn-show-register");
  if (btnShowRegister) {
    btnShowRegister.addEventListener("click", function () {
      showScreen("auth");
      showAuthPanel("register");
    });
  }

  const btnBackLanding = document.getElementById("btn-back-landing");
  if (btnBackLanding) {
    btnBackLanding.addEventListener("click", function () {
      showScreen("landing");
    });
  }

  const btnGoLanding = document.getElementById("btn-go-landing");
  if (btnGoLanding) {
    btnGoLanding.addEventListener("click", function () {
      showScreen("landing");
    });
  }

  const moduleNavButtons = document.querySelectorAll(".module-nav");
  moduleNavButtons.forEach(function (btn) {
    btn.addEventListener("click", function () {
      showModule(btn.getAttribute("data-target"));
    });
  });

  bindForm("form-password", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    const result = await postJson("/AuthApi/DoiMatKhau", payload);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-basic-profile", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    const result = await postJson("/AuthApi/CapNhatThongTin", payload);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-game-profile", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    payload.MaTroChoi = Number(payload.MaTroChoi || 0);
    payload.MaViTriSoTruong = Number(payload.MaViTriSoTruong || 0);
    const result = await postJson("/ProfileApi/TaoHoSo", payload);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-team", async function (form) {
    const result = await postForm("/TeamApi/TaoDoi", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-squad", async function (form) {
    const result = await postForm("/TeamApi/TaoNhom", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-member", async function (form) {
    const result = await postForm("/TeamApi/ThemThanhVien", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-dissolve", async function (form) {
    const result = await postForm("/TeamApi/GiaiTanDoi", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-post", async function (form) {
    const result = await postForm("/RecruitmentApi/TaoBaiDang", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-apply", async function (form) {
    const result = await postForm("/RecruitmentApi/UngTuyen", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-invite", async function (form) {
    const result = await postForm("/RecruitmentApi/GuiLoiMoi", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-review-application", async function (form) {
    const result = await postForm("/RecruitmentApi/DuyetDon", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-respond-invite", async function (form) {
    const result = await postForm("/RecruitmentApi/PhanHoiLoiMoi", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-assign-referee", async function (form) {
    const result = await postForm("/RefereeApi/GanTrongTai", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-referee-my-matches", async function (form) {
    const tab = new FormData(form).get("tab") || "can_nhap_diem";
    const response = await fetch(
      "/RefereeApi/TranCuaToi?tab=" + encodeURIComponent(String(tab)),
      { method: "GET" },
    );
    const result = await response.json();
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) {
      renderRefereeMatchList(result.Data || []);
    }
  });

  bindForm("form-referee-load-match", async function (form) {
    const maTran = Number(new FormData(form).get("maTran") || 0);
    if (maTran <= 0) {
      showAlert(false, "Mã trận không hợp lệ.");
      return;
    }

    const response = await fetch(
      "/RefereeApi/ChiTietTran?maTran=" + encodeURIComponent(maTran),
      { method: "GET" },
    );
    const result = await response.json();
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success && result.Data) {
      renderRefereeGrid(result.Data.Roster || []);
    }
  });

  async function submitRefereeResult(url, buttonId) {
    const payload = buildRefereePayload();
    if (
      payload.MaTran <= 0 ||
      !Array.isArray(payload.ChiSoNguoiChoi) ||
      payload.ChiSoNguoiChoi.length === 0
    ) {
      showAlert(false, "Thiếu mã trận hoặc chưa có dữ liệu người chơi để lưu.");
      return;
    }

    const btn = document.getElementById(buttonId);
    disableButton(btn, true);
    try {
      const result = await postJson(url, payload);
      showAlert(result.Success, result.Message, result.Data);
    } finally {
      disableButton(btn, false);
    }
  }

  const btnRefereeSave = document.getElementById("btn-referee-save");
  if (btnRefereeSave) {
    btnRefereeSave.addEventListener("click", function () {
      submitRefereeResult("/RefereeApi/NhapKetQua", "btn-referee-save");
    });
  }

  const btnRefereeEdit = document.getElementById("btn-referee-edit");
  if (btnRefereeEdit) {
    btnRefereeEdit.addEventListener("click", function () {
      submitRefereeResult("/RefereeApi/SuaKetQua", "btn-referee-edit");
    });
  }

  const btnAdminEdit = document.getElementById("btn-admin-edit");
  if (btnAdminEdit) {
    btnAdminEdit.addEventListener("click", function () {
      submitRefereeResult("/RefereeApi/AdminSuaKetQua", "btn-admin-edit");
    });
  }

  bindForm("form-referee-dispute", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    payload.MaTran = Number(payload.MaTran || 0);
    payload.MaNhom = Number(payload.MaNhom || 0);
    const result = await postJson("/RefereeApi/TaoKhieuNai", payload);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-referee-load-disputes", async function (form) {
    const trangThai = new FormData(form).get("trangThai") || "";
    const response = await fetch(
      "/RefereeApi/DanhSachKhieuNai?trangThai=" +
        encodeURIComponent(String(trangThai)),
      { method: "GET" },
    );
    const result = await response.json();
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) {
      renderDisputeList(result.Data || []);
    }
  });

  bindForm("form-referee-resolve-dispute", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    payload.MaKhieuNai = Number(payload.MaKhieuNai || 0);
    payload.ChapNhan = String(payload.ChapNhan).toLowerCase() === "true";
    const result = await postJson("/RefereeApi/XuLyKhieuNai", payload);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-referee-audit-log", async function (form) {
    const maTranRaw = new FormData(form).get("maTran");
    const maTran = Number(maTranRaw || 0);
    const query = maTran > 0 ? "?maTran=" + encodeURIComponent(maTran) : "";
    const response = await fetch("/RefereeApi/LichSuSuaKetQua" + query, {
      method: "GET",
    });
    const result = await response.json();
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) {
      renderAuditLog(result.Data || []);
    }
  });

  bindForm("form-create-draft", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    payload.MaTroChoi = payload.MaTroChoi ? Number(payload.MaTroChoi) : null;
    payload.TongGiaiThuong = Number(payload.TongGiaiThuong || 0);
    payload.ThoiGianMoDangKy = payload.ThoiGianMoDangKy
      ? new Date(payload.ThoiGianMoDangKy).toISOString()
      : null;
    payload.ThoiGianDongDangKy = payload.ThoiGianDongDangKy
      ? new Date(payload.ThoiGianDongDangKy).toISOString()
      : null;
    payload.NgayBatDau = payload.NgayBatDau
      ? new Date(payload.NgayBatDau).toISOString()
      : null;
    payload.NgayKetThuc = payload.NgayKetThuc
      ? new Date(payload.NgayKetThuc).toISOString()
      : null;
    const result = await postJson("/TournamentBuilderApi/TaoBanNhap", payload);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-submit-review", async function (form) {
    const result = await postForm("/TournamentBuilderApi/GuiXetDuyet", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-admin-approve", async function (form) {
    const result = await postForm("/TournamentBuilderApi/PheDuyet", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-admin-reject", async function (form) {
    const result = await postForm("/TournamentBuilderApi/TuChoi", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-lock-tournament", async function (form) {
    const result = await postForm("/TournamentBuilderApi/KhoaGiai", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-unlock-tournament", async function (form) {
    const result = await postForm("/TournamentBuilderApi/MoKhoaGiai", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-add-stage", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    payload.MaGiaiDau = Number(payload.MaGiaiDau || 0);
    payload.SoDoiDiTiep = Number(payload.SoDoiDiTiep || 0);
    payload.DiemNguongMatchPoint = payload.DiemNguongMatchPoint
      ? Number(payload.DiemNguongMatchPoint)
      : null;
    const result = await postJson(
      "/TournamentBuilderApi/ThemGiaiDoan",
      payload,
    );
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-start-tournament", async function (form) {
    const result = await postForm("/TournamentBuilderApi/BatDauGiai", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-stage-up", async function (form) {
    const result = await postForm(
      "/TournamentBuilderApi/LenThuTuGiaiDoan",
      form,
    );
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-stage-down", async function (form) {
    const result = await postForm(
      "/TournamentBuilderApi/XuongThuTuGiaiDoan",
      form,
    );
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-stage-delete", async function (form) {
    const result = await postForm("/TournamentBuilderApi/XoaGiaiDoan", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-approve-team", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    payload.MaGiaiDau = Number(payload.MaGiaiDau || 0);
    payload.MaNhom = Number(payload.MaNhom || 0);
    payload.ChapNhan = String(payload.ChapNhan).toLowerCase() === "true";
    const result = await postJson(
      "/TournamentBuilderApi/DuyetDangKyDoi",
      payload,
    );
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-seeding", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    payload.MaGiaiDau = Number(payload.MaGiaiDau || 0);
    payload.MaNhom = Number(payload.MaNhom || 0);
    payload.HatGiong = Number(payload.HatGiong || 0);
    const result = await postJson(
      "/TournamentBuilderApi/CapNhatHatGiong",
      payload,
    );
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-sync-roster", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    payload.MaGiaiDau = Number(payload.MaGiaiDau || 0);
    payload.MaNhom = Number(payload.MaNhom || 0);
    const result = await postJson(
      "/TournamentBuilderApi/DongBoRoster",
      payload,
    );
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-generate-schedule", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    payload.MaGiaiDau = Number(payload.MaGiaiDau || 0);
    payload.MaGiaiDoan = Number(payload.MaGiaiDoan || 0);
    payload.DungHatGiong =
      String(payload.DungHatGiong).toLowerCase() === "true";
    const result = await postJson("/MatchmakingApi/TaoLich", payload);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-generate-next-round", async function (form) {
    const result = await postForm("/MatchmakingApi/TaoVongTiepTheo", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  bindForm("form-public-link", async function (form) {
    const payload = Object.fromEntries(new FormData(form).entries());
    const ma = Number(payload.maGiaiDau || 0);
    if (ma <= 0) {
      showAlert(false, "Mã giải không hợp lệ.");
      return;
    }
    window.open("/giai/" + ma, "_blank");
  });

  async function loadGames() {
    try {
      const response = await fetch("/ProfileApi/TroChoi", { method: "GET" });
      const result = await response.json();
      if (!result.Success || !Array.isArray(result.Data)) {
        return;
      }

      const gameSelect = document.getElementById("game-select");
      if (!gameSelect) return;

      result.Data.forEach(function (g) {
        const option = document.createElement("option");
        option.value = g.MaTroChoi;
        option.textContent = g.TenGame + " (" + g.TheLoai + ")";
        gameSelect.appendChild(option);
      });
    } catch (e) {
      showAlert(false, "Không tải được danh sách trò chơi", {
        error: e.message,
      });
    }
  }

  async function loadPositions(maTroChoi) {
    const positionSelect = document.getElementById("position-select");
    if (!positionSelect) return;
    positionSelect.innerHTML = '<option value="">-- Chọn vị trí --</option>';

    if (!maTroChoi) return;

    try {
      const response = await fetch(
        "/ProfileApi/ViTri?maTroChoi=" + encodeURIComponent(maTroChoi),
        { method: "GET" },
      );
      const result = await response.json();
      if (!result.Success || !Array.isArray(result.Data)) {
        return;
      }

      result.Data.forEach(function (p) {
        const option = document.createElement("option");
        option.value = p.MaViTri;
        option.textContent = p.TenViTri + " [" + p.LoaiViTri + "]";
        positionSelect.appendChild(option);
      });
    } catch (e) {
      showAlert(false, "Không tải được danh sách vị trí", { error: e.message });
    }
  }

  const gameSelect = document.getElementById("game-select");
  if (gameSelect) {
    gameSelect.addEventListener("change", function (e) {
      loadPositions(e.target.value);
    });
  }

  async function restoreSession() {
    try {
      const response = await fetch("/AuthApi/Me", { method: "GET" });
      const result = await response.json();
      if (result.Success) {
        setCurrentUserLabel(result.Data || null);
        showScreen("dashboard");
        showModule("module-identity");
        return;
      }
    } catch (e) {
      showAlert(false, "Không kiểm tra được phiên đăng nhập", {
        error: e.message,
      });
    }
    showScreen("landing");
  }

  // ================================================================
  // ADMIN MODULE — Trụ cột 5
  // ================================================================

  function renderUserList(rows) {
    const container = document.getElementById("admin-user-list");
    if (!container) return;
    if (!Array.isArray(rows) || rows.length === 0) {
      container.innerHTML = "<span class='text-muted'>Không tìm thấy user nào.</span>";
      return;
    }
    container.innerHTML = "<div class='table-responsive'><table class='table table-sm table-bordered small'>" +
      "<thead><tr><th>ID</th><th>Tên đăng nhập</th><th>Email</th><th>Vai trò</th><th>Trạng thái</th><th>Lý do ban</th><th>Thời gian ban</th></tr></thead><tbody>" +
      rows.map(function (u) {
        const banned = Number(u.is_banned || 0) === 1;
        const cls = banned ? " class='table-danger'" : "";
        return "<tr" + cls + ">" +
          "<td>" + u.ma_nguoi_dung + "</td>" +
          "<td><b>" + (u.ten_dang_nhap || "-") + "</b></td>" +
          "<td>" + (u.email || "-") + "</td>" +
          "<td>" + (u.vai_tro_he_thong || "-") + "</td>" +
          "<td>" + (banned ? "<span class='badge bg-danger'>BAN</span>" : "<span class='badge bg-success'>Active</span>") + "</td>" +
          "<td>" + (u.ly_do_ban || "-") + "</td>" +
          "<td>" + (u.thoi_gian_ban || "-") + "</td>" +
          "</tr>";
      }).join("") +
      "</tbody></table></div>";
  }

  function renderGameList(rows) {
    const container = document.getElementById("admin-game-list");
    if (!container) return;
    if (!Array.isArray(rows) || rows.length === 0) {
      container.innerHTML = "<span class='text-muted'>Không có game nào.</span>";
      return;
    }
    container.innerHTML = "<div class='table-responsive'><table class='table table-sm table-bordered small'>" +
      "<thead><tr><th>ID</th><th>Tên game</th><th>Thể loại</th><th>Trạng thái</th><th>Số giải</th><th>Số vị trí</th></tr></thead><tbody>" +
      rows.map(function (g) {
        const active = Number(g.is_active || 1) === 1;
        const cls = active ? "" : " class='table-secondary'";
        return "<tr" + cls + ">" +
          "<td>" + g.ma_tro_choi + "</td>" +
          "<td><b>" + (g.ten_game || "-") + "</b></td>" +
          "<td>" + (g.the_loai || "-") + "</td>" +
          "<td>" + (active ? "<span class='badge bg-success'>Active</span>" : "<span class='badge bg-secondary'>Ẩn</span>") + "</td>" +
          "<td>" + (g.so_giai_dau || 0) + "</td>" +
          "<td>" + (g.so_vi_tri || 0) + "</td>" +
          "</tr>";
      }).join("") +
      "</tbody></table></div>";
  }

  function renderActionRequired(data) {
    const giaiEl = document.getElementById("admin-giai-cho-duyet");
    const khieuNaiEl = document.getElementById("admin-khieu-nai-cho-xu-ly");

    if (giaiEl) {
      const list = Array.isArray(data.GiaiChoXetDuyet) ? data.GiaiChoXetDuyet : [];
      if (list.length === 0) {
        giaiEl.innerHTML = "<span class='text-muted'>Không có giải đấu nào chờ duyệt.</span>";
      } else {
        giaiEl.innerHTML = list.map(function (g) {
          return "<div class='border rounded p-1 mb-1'>" +
            "<b>#" + g.ma_giai_dau + "</b> " + (g.ten_giai_dau || "-") +
            " | Người tạo: " + (g.ten_nguoi_tao || "-") +
            " | Game: " + (g.ten_game || "N/A") +
            "</div>";
        }).join("");
      }
    }

    if (khieuNaiEl) {
      const list = Array.isArray(data.KhieuNaiChoXuLy) ? data.KhieuNaiChoXuLy : [];
      if (list.length === 0) {
        khieuNaiEl.innerHTML = "<span class='text-muted'>Không có khiếu nại nào chờ xử lý.</span>";
      } else {
        khieuNaiEl.innerHTML = list.map(function (k) {
          return "<div class='border rounded p-1 mb-1'>" +
            "<b>KN #" + k.ma_khieu_nai + "</b>" +
            " | Trận #" + k.ma_tran +
            " | Nhóm: " + (k.ten_nhom || "#" + k.ma_nhom) +
            " | " + (k.ten_nguoi_gui || "User " + k.ma_nguoi_gui) +
            "</div>";
        }).join("");
      }
    }
  }

  // Hiển thị/ẩn nút Admin dựa trên vai trò
  function updateAdminVisibility(userData) {
    const btnAdmin = document.getElementById("btn-admin-module");
    if (!btnAdmin) return;
    const isAdmin = userData && String(userData.VaiTroHeThong).toLowerCase() === "admin";
    btnAdmin.classList.toggle("d-none", !isAdmin);
    if (isAdmin) {
      // Ẩn btn-admin-module không nên bị toggle sang outline khi chưa active
      btnAdmin.classList.remove("btn-outline-primary");
    }
  }

  // Patch restoreSession để cập nhật admin visibility
  const _origSetLabel = setCurrentUserLabel;
  function setCurrentUserLabel(data) {
    _origSetLabel(data);
    updateAdminVisibility(data);
  }

  // Dashboard
  const btnLoadDashboard = document.getElementById("btn-load-dashboard");
  if (btnLoadDashboard) {
    btnLoadDashboard.addEventListener("click", async function () {
      try {
        const res = await fetch("/AdminApi/Dashboard", { method: "GET" });
        const result = await res.json();
        showAlert(result.Success, result.Message);
        if (result.Success && result.Data) {
          const tk = result.Data.ThongKe || {};
          const el = function (id) { return document.getElementById(id); };
          if (el("stat-user-active")) el("stat-user-active").textContent = tk.tong_user_active || 0;
          if (el("stat-giai-chay")) el("stat-giai-chay").textContent = tk.giai_dang_chay || 0;
          if (el("stat-doi-hoat-dong")) el("stat-doi-hoat-dong").textContent = tk.tong_doi_hoat_dong || 0;
          if (el("stat-khieu-nai")) el("stat-khieu-nai").textContent = tk.khieu_nai_cho_xu_ly || 0;
          if (el("stat-giai-cho-duyet")) el("stat-giai-cho-duyet").textContent = tk.giai_cho_duyet || 0;
          renderActionRequired(result.Data.ActionRequired || {});
        }
      } catch (e) {
        showAlert(false, "Không tải được dashboard: " + e.message);
      }
    });
  }

  // Tìm kiếm user
  bindForm("form-admin-search-user", async function (form) {
    const d = Object.fromEntries(new FormData(form).entries());
    const params = new URLSearchParams();
    if (d.tuKhoa && d.tuKhoa.trim()) params.append("tuKhoa", d.tuKhoa.trim());
    if (d.isBanned !== "") params.append("isBanned", d.isBanned);
    const res = await fetch("/AdminApi/TimKiemUser?" + params.toString(), { method: "GET" });
    const result = await res.json();
    showAlert(result.Success, result.Message);
    if (result.Success) renderUserList(result.Data || []);
  });

  // Ban user
  bindForm("form-admin-ban-user", async function (form) {
    const result = await postForm("/AdminApi/BanUser", form);
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) form.reset();
  });

  // Ban cả đội
  bindForm("form-admin-ban-doi", async function (form) {
    const result = await postForm("/AdminApi/BanDoi", form);
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) form.reset();
  });

  // Unban user
  bindForm("form-admin-unban-user", async function (form) {
    const result = await postForm("/AdminApi/UnbanUser", form);
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) form.reset();
  });

  // Tải danh sách game
  const btnLoadGames = document.getElementById("btn-load-games");
  if (btnLoadGames) {
    btnLoadGames.addEventListener("click", async function () {
      const showInactive = document.getElementById("admin-show-inactive-game");
      const baoCaInactive = showInactive && showInactive.checked;
      try {
        const res = await fetch("/AdminApi/DanhSachGame?baoCaInactive=" + baoCaInactive, { method: "GET" });
        const result = await res.json();
        showAlert(result.Success, result.Message);
        if (result.Success) renderGameList(result.Data || []);
      } catch (e) {
        showAlert(false, "Không tải được danh sách game: " + e.message);
      }
    });
  }

  // Thêm game
  bindForm("form-admin-them-game", async function (form) {
    const result = await postForm("/AdminApi/ThemGame", form);
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) form.reset();
  });

  // Sửa game
  bindForm("form-admin-sua-game", async function (form) {
    const result = await postForm("/AdminApi/SuaGame", form);
    showAlert(result.Success, result.Message, result.Data);
  });

  // Ẩn game
  bindForm("form-admin-an-game", async function (form) {
    const result = await postForm("/AdminApi/AnGame", form);
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) form.reset();
  });

  // Kích hoạt game
  bindForm("form-admin-kich-hoat-game", async function (form) {
    const result = await postForm("/AdminApi/KichHoatGame", form);
    showAlert(result.Success, result.Message, result.Data);
    if (result.Success) form.reset();
  });

  // Hard Wipe giải đấu
  bindForm("form-admin-hard-wipe", async function (form) {
    const d = Object.fromEntries(new FormData(form).entries());
    if (String(d.xacNhan || "").trim().toUpperCase() !== "XOACUNG") {
      showAlert(false, "Xác nhận không đúng. Phải gõ chính xác XOACUNG (chữ hoa).");
      return;
    }
    const maGiaiDau = Number(d.maGiaiDau || 0);
    if (maGiaiDau <= 0) {
      showAlert(false, "Mã giải đấu không hợp lệ.");
      return;
    }
    if (!confirm("⚠️ BẠN CHẮC CHẮN MUỐN XÓA CỨNG GIẢI #" + maGiaiDau + "?\nHành động này KHÔNG THỂ HOÀN TÁC!")) {
      return;
    }
    try {
      const res = await fetch("/AdminApi/HardWipeGiai?maGiaiDau=" + maGiaiDau, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: "maGiaiDau=" + maGiaiDau
      });
      const result = await res.json();
      showAlert(result.Success, result.Message, result.Data);
      if (result.Success) form.reset();
    } catch (e) {
      showAlert(false, "Lỗi khi xóa cứng giải: " + e.message);
    }
  });

  loadGames();
  restoreSession();
})();
