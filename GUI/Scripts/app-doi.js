(function () {
  const api = {
    all: "/DoiApi/All",
    mine: "/DoiApi/Mine",
    detail: "/DoiApi/Detail",
    games: "/DoiApi/TroChoi",
    positions: "/DoiApi/ViTri",
    create: "/DoiApi/Create",
    update: "/DoiApi/Update",
    upload: "/DoiApi/UploadLogo",
    invite: "/DoiApi/Invite",
    join: "/DoiApi/Join",
    role: "/DoiApi/SetRole",
    remove: "/DoiApi/RemoveMember",
    leave: "/DoiApi/LeaveTeam",
    toggle: "/DoiApi/ToggleRecruiting",
    deleteTeam: "/DoiApi/Delete",
    requests: "/DoiApi/Requests",
    handle: "/DoiApi/HandleRequest",
  };

  const state = { teams: [], games: [], selected: null, positions: [] };
  const $ = (id) => document.getElementById(id);

  function getData(res) {
    return res && res.success ? res.data : null;
  }
  function showMessage(message, ok) {
    const box = $("doiMessage");
    if (box)
      box.innerHTML = `<div class="alert ${ok ? "alert-success" : "alert-danger"}">${message}</div>`;
  }
  function post(url, data) {
    return fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data || {}),
    }).then((r) => r.json());
  }
  function text(v) {
    return v || "";
  }

  function hasText(v) {
    return v !== null && v !== undefined && String(v).trim().length > 0;
  }

  function setTabVisible(targetSelector, visible) {
    const btn = document.querySelector(`[data-bs-target="${targetSelector}"]`);
    if (!btn) return;
    const li = btn.closest("li");
    if (li) li.classList.toggle("d-none", !visible);
    else btn.classList.toggle("d-none", !visible);

    if (!visible) {
      const pane = document.querySelector(targetSelector);
      if (pane && pane.classList.contains("show")) {
        const firstVisible = document.querySelector(
          '.team-tabs [data-bs-toggle="pill"]:not(.d-none)',
        );
        if (firstVisible && typeof firstVisible.click === "function") {
          firstVisible.click();
        }
      }
    }
  }
  function roleLabel(role) {
    return (
      {
        chu_tich: "Chủ tịch",
        ban_dieu_hanh: "Ban điều hành",
        doi_truong: "Đội trưởng",
        thanh_vien: "Thành viên",
      }[role] || "Khách"
    );
  }

  function renderCards(targetId, teams) {
    const target = $(targetId);
    if (!target) return;
    if (!teams.length) {
      target.innerHTML =
        '<div class="empty-page-card"><h3>Chưa có đội phù hợp</h3><p class="text-muted">Hãy thử đổi bộ lọc hoặc tạo đội mới.</p></div>';
      return;
    }
    target.innerHTML = teams
      .map(
        (d) => `<article class="team-card" data-team="${d.ma_doi}">
            <div class="team-logo">${d.logo_url ? `<img src="${d.logo_url}" alt="${d.ten_doi}">` : `<span>${(d.ten_viet_tat || d.ten_doi).substring(0, 2).toUpperCase()}</span>`}</div>
            <div class="team-card-body"><div class="d-flex justify-content-between gap-2"><h3>${d.ten_doi}</h3><span class="team-game">${d.ten_game}</span></div>
            <p>${text(d.slogan)}</p><div class="team-meta"><span>${d.so_thanh_vien} thành viên</span><span>${d.dang_tuyen ? "Đang tuyển" : "Không tuyển"}</span><span>${roleLabel(d.vai_tro_cua_toi)}</span></div></div>
        </article>`,
      )
      .join("");
    target
      .querySelectorAll("[data-team]")
      .forEach((el) =>
        el.addEventListener("click", () =>
          goToDetail(parseInt(el.dataset.team)),
        ),
      );
  }

  function goToDetail(maDoi) {
    window.location.href = `/Doi/ChiTiet/${maDoi}`;
  }

  function resolveTeamId() {
    const detailPage = document.querySelector("[data-team-detail-page]");
    if (detailPage) {
      const raw =
        detailPage.getAttribute("data-team-id") ||
        detailPage.dataset.teamId ||
        detailPage.dataset.team ||
        "";
      const parsed = parseInt(raw);
      if (!Number.isNaN(parsed) && parsed > 0) return parsed;
    }

    const parts = (window.location.pathname || "").split("/").filter(Boolean);
    const last = parts.length ? parts[parts.length - 1] : "";
    const fromUrl = parseInt(last);
    if (!Number.isNaN(fromUrl) && fromUrl > 0) return fromUrl;

    return null;
  }

  function loadGames() {
    return fetch(api.games)
      .then((r) => r.json())
      .then((res) => {
        state.games = getData(res) || [];
        renderGameOptions();
      });
  }

  function renderGameOptions() {
    document.querySelectorAll("[data-game-select]").forEach((select) => {
      const current = select.value;
      select.innerHTML =
        '<option value="">Tất cả game</option>' +
        state.games
          .map((g) => `<option value="${g.ma_tro_choi}">${g.ten_game}</option>`)
          .join("");
      select.value = current;
    });
    const createGame = $("createGame");
    if (createGame)
      createGame.innerHTML =
        '<option value="">Chọn game</option>' +
        state.games
          .map((g) => `<option value="${g.ma_tro_choi}">${g.ten_game}</option>`)
          .join("");
  }

  function loadTeams() {
    const q = $("teamSearch") ? $("teamSearch").value : "";
    const game = $("gameFilter") ? $("gameFilter").value : "";
    return fetch(
      `${api.all}?q=${encodeURIComponent(q)}&maTroChoi=${encodeURIComponent(game)}`,
    )
      .then((r) => r.json())
      .then((res) => {
        state.teams = getData(res) || [];
        renderCards("teamList", state.teams);
      });
  }

  function loadMine() {
    return fetch(api.mine)
      .then((r) => r.json())
      .then((res) => renderCards("myTeamList", getData(res) || []));
  }

  function openDetail(maDoi) {
    console.log("Loading team detail for ID:", maDoi);
    fetch(`${api.detail}?maDoi=${maDoi}`)
      .then((r) => {
        if (!r.ok) {
          console.error("HTTP error:", r.status, r.statusText);
          showMessage(`Lỗi server (${r.status}). Vui lòng thử lại.`, false);
          return null;
        }
        return r.json();
      })
      .then((res) => {
        if (!res) return;
        console.log("Team detail response:", res);
        if (!res.success) {
          showMessage(res.message || "Không tải được dữ liệu đội.", false);
          return;
        }
        state.selected = res.data;
        renderDetail();
        if (state.selected && state.selected.doi) {
          loadPositions(state.selected.doi.ma_tro_choi);
        }
      })
      .catch((err) => {
        console.error("Error loading team detail:", err);
        showMessage("Lỗi kết nối: " + err.message, false);
      });
  }

  function loadPositions(maTroChoi) {
    const select = $("invitePosition");
    if (!select) return;
    fetch(`${api.positions}?maTroChoi=${maTroChoi}`)
      .then((r) => r.json())
      .then((res) => {
        const items = getData(res) || [];
        select.innerHTML =
          '<option value="">Chọn vị trí nếu có</option>' +
          items
            .map(
              (x) => `<option value="${x.ma_vi_tri}">${x.ten_vi_tri}</option>`,
            )
            .join("");
      });
  }

  function renderDetail() {
    console.log("Rendering team detail:", state.selected);
    const data = state.selected;
    if (!data) return;
    const d = data.doi;
    try {
      const title = $("detailTitle");
      const subtitle = $("detailSubtitle");
      const logo = $("detailLogo");
      const about = $("detailAbout");
      const stats = $("detailStats");
      if (!title || !subtitle || !logo || !about || !stats) return;

      title.textContent = d.ten_doi;
      subtitle.textContent = `${d.ten_game} • ${roleLabel(d.vai_tro_cua_toi)} • ${d.dang_tuyen ? "Đang tuyển dụng" : "Không tuyển dụng"}`;
      logo.innerHTML = d.logo_url
        ? `<img src="${d.logo_url}" alt="${d.ten_doi}">`
        : `<span>${(d.ten_viet_tat || d.ten_doi).substring(0, 2).toUpperCase()}</span>`;

      const ngayTao = d.ngay_tao
        ? new Date(d.ngay_tao).toLocaleDateString("vi-VN")
        : "—";
      about.innerHTML = `
        <div class="team-info-grid">
          <div><b>Tên viết tắt:</b> ${text(d.ten_viet_tat) || "—"}</div>
          <div><b>Game:</b> ${text(d.ten_game)}</div>
          <div><b>Chủ tịch:</b> ${text(d.ten_chu_tich)}</div>
          <div><b>Ngày tạo:</b> ${ngayTao}</div>
          <div><b>Thành viên:</b> ${d.so_thanh_vien || 0}</div>
          <div><b>Tuyển dụng:</b> ${d.dang_tuyen ? "Đang tuyển" : "Không tuyển"}</div>
        </div>
        ${d.slogan ? `<h4 class="mt-3">${text(d.slogan)}</h4>` : ""}
        ${d.mo_ta ? `<p>${text(d.mo_ta)}</p>` : ""}
      `;

      const members = data.thanh_vien || [];
      const history = data.lich_su_thi_dau || [];
      const tournaments = data.giai_dau || [];
      const nextMatches = data.tran_dau_tiep_theo || [];

      renderMembers(members, d);
      renderMatches("detailHistory", history);
      renderTournaments(tournaments);
      renderMatches("detailNextMatches", nextMatches);

      const tk = data.thong_ke || {
        tong_tran: 0,
        so_tran_thang: 0,
        so_tran_thua: 0,
        so_giai_tham_gia: 0,
      };
      stats.innerHTML = `<div class="stats-grid"><div><b>${tk.tong_tran}</b><span>Trận</span></div><div><b>${tk.so_tran_thang}</b><span>Thắng</span></div><div><b>${tk.so_tran_thua}</b><span>Thua</span></div><div><b>${tk.so_giai_tham_gia}</b><span>Giải</span></div></div><p class="text-muted mt-3">Giải thưởng sẽ hiển thị khi hệ thống có dữ liệu trao giải.</p>`;
      renderActions(d);
      console.log("Render detail complete.");
    } catch (e) {
      console.error("Error in renderDetail:", e);
    }
  }

  function renderMembers(items, d) {
    const canManage = d.vai_tro_cua_toi === "chu_tich";
    const box = $("detailMembers");
    if (!box) return;
    box.innerHTML =
      items.length > 0
        ? items
            .map(
              (m) =>
                `<div class="member-row">
              <div class="member-avatar">${m.avatar_url ? `<img src="${m.avatar_url}" alt="">` : `<span>${(m.ho_ten || m.username || "?").charAt(0).toUpperCase()}</span>`}</div>
              <div class="member-info"><b>${m.ho_ten || m.username || "Người dùng"}</b><span>${roleLabel(m.vai_tro_noi_bo)}${m.ten_vi_tri ? ` • ${m.ten_vi_tri}` : ""}${m.phan_he ? ` • ${m.phan_he}` : ""}</span></div>
              ${canManage && m.vai_tro_noi_bo !== "chu_tich" ? `<div class="member-actions"><select class="form-select form-select-sm" data-role="${m.ma_nguoi_dung}"><option value="thanh_vien" ${m.vai_tro_noi_bo === "thanh_vien" ? "selected" : ""}>Thành viên</option><option value="doi_truong" ${m.vai_tro_noi_bo === "doi_truong" ? "selected" : ""}>Đội trưởng</option><option value="ban_dieu_hanh" ${m.vai_tro_noi_bo === "ban_dieu_hanh" ? "selected" : ""}>Ban điều hành</option></select><button class="btn btn-sm btn-outline-danger" data-remove="${m.ma_nguoi_dung}">Loại</button></div>` : ""}
            </div>`,
            )
            .join("")
        : '<div class="empty-tab-state"><p>Chưa có thành viên nào.</p></div>';
    document
      .querySelectorAll("[data-role]")
      .forEach((el) =>
        el.addEventListener("change", () =>
          setRole(d.ma_doi, parseInt(el.dataset.role), el.value),
        ),
      );
    document
      .querySelectorAll("[data-remove]")
      .forEach((el) =>
        el.addEventListener("click", () =>
          removeMember(d.ma_doi, parseInt(el.dataset.remove)),
        ),
      );
  }

  function renderMatches(id, items) {
    const box = $(id);
    if (!box) return;
    box.innerHTML =
      items.length > 0
        ? items
            .map(
              (m) =>
                `<div class="list-row"><b>${m.ten_giai_dau}</b><span>${text(m.vong_dau)} • ${text(m.trang_thai)}</span></div>`,
            )
            .join("")
        : `<div class="empty-tab-state"><p>${id === "detailHistory" ? "Chưa có lịch sử thi đấu." : "Chưa có trận đấu nào sắp tới."}</p></div>`;
  }
  function renderTournaments(items) {
    const box = $("detailTournaments");
    if (!box) return;
    box.innerHTML =
      items.length > 0
        ? items
            .map(
              (g) =>
                `<div class="list-row"><b>${g.ten_giai_dau}</b><span>${g.trang_thai} • ${g.trang_thai_tham_gia}</span></div>`,
            )
            .join("")
        : '<div class="empty-tab-state"><p>Chưa tham gia giải đấu nào.</p></div>';
  }

  function renderActions(d) {
    const actions = $("detailActions");
    if (!actions) return;
    const canPresident = d.vai_tro_cua_toi === "chu_tich";
    const canInvite = ["chu_tich", "ban_dieu_hanh", "doi_truong"].includes(
      d.vai_tro_cua_toi,
    );
    const canJoin = !d.vai_tro_cua_toi && d.dang_tuyen;
    const canLeave = !!d.vai_tro_cua_toi && d.vai_tro_cua_toi !== "chu_tich";
    actions.innerHTML = `${canJoin ? '<button class="btn btn-success" id="joinTeam">Xin gia nhập</button>' : ""}${canInvite ? '<button class="btn btn-primary" id="openInviteForm">Mời thành viên</button>' : ""}${canLeave ? '<button class="btn btn-outline-danger" id="leaveTeam">Rời đội</button>' : ""}${canPresident ? `<button class="btn btn-outline-primary" id="toggleRecruiting">${d.dang_tuyen ? "Tắt tuyển dụng" : "Bật tuyển dụng"}</button><button class="btn btn-outline-secondary" id="fillEditTeam">Sửa thông tin</button><button class="btn btn-outline-success" id="registerTournamentBtn">Đăng ký giải đấu</button><button class="btn btn-outline-danger" id="deleteTeam">Xóa đội</button>` : ""}`;
    if ($("joinTeam"))
      $("joinTeam").onclick = () =>
        post(api.join, { ma_doi: d.ma_doi }).then((res) =>
          showMessage(res.message, res.success),
        );
    if ($("openInviteForm"))
      $("openInviteForm").onclick = () =>
        $("invitePanel").classList.toggle("d-none");
    if ($("leaveTeam"))
      $("leaveTeam").onclick = () => {
        if (confirm("Bạn chắc chắn muốn rời đội này?"))
          post(api.leave, { ma_doi: d.ma_doi }).then((res) => {
            showMessage(res.message, res.success);
            if (res.success) window.location.href = "/DoiCuaToi";
          });
      };
    if ($("toggleRecruiting"))
      $("toggleRecruiting").onclick = () =>
        post(api.toggle, { ma_doi: d.ma_doi, dang_tuyen: !d.dang_tuyen }).then(
          () => openDetail(d.ma_doi),
        );
    if ($("fillEditTeam")) $("fillEditTeam").onclick = () => openEditTeam(d);
    if ($("deleteTeam"))
      $("deleteTeam").onclick = () => {
        if (confirm("Bạn chắc chắn muốn xóa đội?"))
          post(`${api.deleteTeam}?maDoi=${d.ma_doi}`).then((res) => {
            showMessage(res.message, res.success);
            location.reload();
          });
      };
    if ($("registerTournamentBtn")) {
      $("registerTournamentBtn").onclick = () => {
        window.location.href = `/GiaiDau/Index?gameId=${d.ma_tro_choi || ""}`;
      };
    }
  }

  function openEditTeam(d) {
    const panel = $("editTeamPanel");
    if (!panel || !d) return;
    $("editTeamName").value = d.ten_doi || "";
    $("editTeamShortName").value = d.ten_viet_tat || "";
    $("editTeamLogoUrl").value = d.logo_url || "";
    $("editTeamSlogan").value = d.slogan || "";
    $("editTeamDescription").value = d.mo_ta || "";
    $("editTeamRecruiting").checked = !!d.dang_tuyen;
    if ($("editTeamLogo")) $("editTeamLogo").value = "";
    panel.classList.remove("d-none");
    panel.scrollIntoView({ behavior: "smooth", block: "start" });
  }

  function bindEditTeam() {
    const form = $("editTeamForm");
    if (!form) return;
    if ($("closeEditTeam"))
      $("closeEditTeam").onclick = () =>
        $("editTeamPanel").classList.add("d-none");

    form.addEventListener("submit", async (e) => {
      e.preventDefault();
      const selected = state.selected && state.selected.doi;
      if (!selected) return;

      let logoUrl = $("editTeamLogoUrl").value;
      const fileInput = $("editTeamLogo");
      const file = fileInput && fileInput.files ? fileInput.files[0] : null;
      if (file) {
        const fd = new FormData();
        fd.append("logo", file);
        const up = await fetch(api.upload, { method: "POST", body: fd }).then(
          (r) => r.json(),
        );
        if (!up.success) {
          showMessage(up.message, false);
          return;
        }
        logoUrl = up.data || "";
        $("editTeamLogoUrl").value = logoUrl;
      }

      const res = await post(api.update, {
        ma_doi: selected.ma_doi,
        ten_doi: $("editTeamName").value,
        ten_viet_tat: $("editTeamShortName").value,
        logo_url: logoUrl,
        slogan: $("editTeamSlogan").value,
        mo_ta: $("editTeamDescription").value,
        dang_tuyen: $("editTeamRecruiting").checked,
      });
      showMessage(res.message, res.success);
      if (res.success) {
        $("editTeamPanel").classList.add("d-none");
        openDetail(selected.ma_doi);
      }
    });
  }

  function setRole(maDoi, maNguoiDung, role) {
    post(api.role, {
      ma_doi: maDoi,
      ma_nguoi_dung: maNguoiDung,
      vai_tro_noi_bo: role,
    }).then((res) => {
      showMessage(res.message, res.success);
      openDetail(maDoi);
    });
  }
  function removeMember(maDoi, maNguoiDung) {
    if (confirm("Loại thành viên này khỏi đội?"))
      post(api.remove, { ma_doi: maDoi, ma_nguoi_dung: maNguoiDung }).then(
        (res) => {
          showMessage(res.message, res.success);
          openDetail(maDoi);
        },
      );
  }

  function bindCreate() {
    const form = $("createTeamForm");
    if (!form) return;
    form.addEventListener("submit", async (e) => {
      e.preventDefault();
      let logoUrl = "";
      const file = $("createLogo").files[0];
      if (file) {
        const fd = new FormData();
        fd.append("logo", file);
        const up = await fetch(api.upload, { method: "POST", body: fd }).then(
          (r) => r.json(),
        );
        if (!up.success) {
          showMessage(up.message, false);
          return;
        }
        logoUrl = up.data || "";
      }
      const data = {
        ten_doi: $("createName").value,
        ten_viet_tat: $("createShortName").value,
        ma_tro_choi: parseInt($("createGame").value || "0"),
        logo_url: logoUrl,
        slogan: $("createSlogan").value,
        mo_ta: $("createDescription").value,
      };
      const res = await post(api.create, data);
      showMessage(res.message, res.success);
      if (res.success) {
        form.reset();
        loadTeams();
        loadMine();
      }
    });
  }

  function bindInvite() {
    const form = $("inviteForm");
    if (!form) return;
    form.addEventListener("submit", (e) => {
      e.preventDefault();
      const d = state.selected.doi;
      post(api.invite, {
        ma_doi: d.ma_doi,
        username_or_email: $("inviteUser").value,
        ma_vi_tri: parseInt($("invitePosition").value || "0") || null,
        mo_ta: $("inviteNote").value,
      }).then((res) => showMessage(res.message, res.success));
    });
  }

  function loadRequests() {
    const list = $("requestList");
    if (!list) return;
    fetch(api.requests)
      .then((r) => r.json())
      .then((res) => {
        const items = getData(res) || [];
        list.innerHTML =
          items
            .map(
              (x) =>
                `<article class="request-card"><div><p class="eyebrow">${requestLabel(x.loai_yeu_cau)}</p><h3>${x.ten_doi}</h3><p>${x.ten_game} • Người gửi: ${text(x.ten_nguoi_gui)} • Người nhận: ${text(x.ten_nguoi_nhan)}</p><p>${text(x.mo_ta)}</p><div class="request-detail d-none" id="requestDetail${x.ma_yeu_cau}">${requestDetail(x)}</div></div><div class="d-flex gap-2 flex-wrap"><button class="btn btn-outline-primary" data-view-team="${x.ma_doi}">Xem đội</button><button class="btn btn-outline-secondary" data-view-request="${x.ma_yeu_cau}">Chi tiết</button><button class="btn btn-success" data-accept="${x.loai_yeu_cau}:${x.ma_yeu_cau}">Chấp nhận</button><button class="btn btn-outline-danger" data-reject="${x.loai_yeu_cau}:${x.ma_yeu_cau}">Bỏ</button></div></article>`,
            )
            .join("") ||
          '<section class="empty-page-card"><h3>Không có yêu cầu nào</h3></section>';
        document.querySelectorAll("[data-accept],[data-reject]").forEach(
          (btn) =>
            (btn.onclick = () => {
              const raw = btn.dataset.accept || btn.dataset.reject;
              const parts = raw.split(":");
              post(api.handle, {
                loai_yeu_cau: parts[0],
                ma_yeu_cau: parseInt(parts[1]),
                chap_nhan: !!btn.dataset.accept,
              }).then((r) => {
                showMessage(r.message, r.success);
                loadRequests();
              });
            }),
        );
        document
          .querySelectorAll("[data-view-team]")
          .forEach(
            (btn) =>
              (btn.onclick = () => goToDetail(parseInt(btn.dataset.viewTeam))),
          );
        document
          .querySelectorAll("[data-view-request]")
          .forEach(
            (btn) =>
              (btn.onclick = () =>
                $(`requestDetail${btn.dataset.viewRequest}`).classList.toggle(
                  "d-none",
                )),
          );
      });
  }

  function requestLabel(type) {
    if (type === "loi_moi") return "Lời mời vào đội";
    if (type === "xin_gia_nhap") return "Đơn xin gia nhập";
    if (type === "loi_moi_tham_gia_giai") return "Lời mời tham gia giải đấu";
    return "Yêu cầu mời thành viên";
  }

  function requestDetail(x) {
    if (x.loai_yeu_cau !== "xin_gia_nhap")
      return `<p><b>Đội:</b> ${x.ten_doi}</p><p><b>Game:</b> ${x.ten_game}</p>`;
    return `<p><b>Người xin gia nhập:</b> ${text(x.ten_nguoi_gui)}</p><p><b>Hồ sơ game:</b> ${text(x.ho_so_in_game_name)} (${text(x.ho_so_in_game_id)})</p><p><b>Vị trí:</b> ${text(x.ho_so_vi_tri)}</p><p><b>Thành tích:</b> ${text(x.ho_so_thanh_tich)}</p>`;
  }

  document.addEventListener("DOMContentLoaded", () => {
    const teamId = resolveTeamId();
    if (teamId) openDetail(teamId);

    loadGames()
      .catch((err) => console.error("Error loading games:", err))
      .finally(() => {
        if ($("teamList")) loadTeams();
        if ($("myTeamList")) loadMine();
      });
    if ($("teamSearch")) $("teamSearch").addEventListener("input", loadTeams);
    if ($("gameFilter")) $("gameFilter").addEventListener("change", loadTeams);
    bindCreate();
    bindInvite();
    bindEditTeam();
    loadRequests();
  });
})();
