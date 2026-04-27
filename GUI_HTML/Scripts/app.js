// ============================================================
// ESPORT HUB — app.js (Dashboard — Private Layout)
// ============================================================
(function () {
  "use strict";

  // ---- GAME CONFIG ----
  const GAMES = [
    {
      id: "game-aov",
      name: "Arena of Valor",
      emoji: "⚔️",
      color: "#ffa502",
      maGame: null,
    },
    {
      id: "game-lol",
      name: "League of Legends",
      emoji: "🏆",
      color: "#6c63ff",
      maGame: null,
    },
    {
      id: "game-ff",
      name: "Free Fire",
      emoji: "🔥",
      color: "#ff4757",
      maGame: null,
    },
    {
      id: "game-pubg",
      name: "PUBG",
      emoji: "🪂",
      color: "#ffd700",
      maGame: null,
    },
    {
      id: "game-val",
      name: "Valorant",
      emoji: "🎯",
      color: "#ff4655",
      maGame: null,
    },
    {
      id: "game-cs2",
      name: "CS2",
      emoji: "💣",
      color: "#f0a500",
      maGame: null,
    },
  ];

  // ---- STATE ----
  let currentUser = null;
  let currentPage = "home";
  let gameList = [];

  // ---- API HELPERS ----
  async function api(url, method, body) {
    const opts = { method: method || "GET", headers: {} };
    if (body && method === "POST") {
      opts.headers["Content-Type"] = "application/json";
      opts.body = JSON.stringify(body);
    }
    // Add timeout to prevent hanging
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 30000); // 30s timeout
    opts.signal = controller.signal;

    try {
      const res = await fetch(url, opts);
      clearTimeout(timeoutId);
      return await res.json();
    } catch (e) {
      clearTimeout(timeoutId);
      if (e.name === "AbortError") {
        return {
          Success: false,
          Message: "Request timeout - vui lòng thử lại",
        };
      }
      return { Success: false, Message: "Lỗi kết nối: " + e.message };
    }
  }
  async function postForm(url, data) {
    const body = new URLSearchParams(data).toString();
    try {
      const res = await fetch(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
        },
        body,
      });
      return await res.json();
    } catch (e) {
      return { Success: false, Message: "Lỗi kết nối: " + e.message };
    }
  }

  // (Auth forms are handled by auth.js on /login page)

  // ---- ENTER DASHBOARD ----
  function enterDashboard(user) {
    currentUser = user;
    updateAvatar(user);
    loadGames();
    loadNotifications();
    navigateTo("home");
  }

  function updateAvatar(user) {
    if (!user) return;
    const initials = (user.TenDangNhap || user.ten_dang_nhap || "?")
      .charAt(0)
      .toUpperCase();
    const topbarInitials = document.getElementById("avatar-initials");
    topbarInitials.textContent = initials;
    // Profile page
    const un = user.TenDangNhap || user.ten_dang_nhap || "";
    const em = user.Email || user.email || "";
    document.getElementById("profile-username-display").textContent = un;
    document.getElementById("profile-email-display").textContent = em;

    // Avatar big (click-to-upload)
    const bigInitials = document.getElementById("avatar-initials-big");
    const bigImg = document.getElementById("profile-avatar-img");
    const avatarUrl = user.AvatarUrl || user.avatar_url || "";
    if (bigInitials) bigInitials.textContent = initials;
    if (bigImg) {
      if (avatarUrl) {
        bigImg.src = avatarUrl;
        bigImg.style.display = "block";
        if (bigInitials) bigInitials.style.display = "none";
      } else {
        bigImg.style.display = "none";
        if (bigInitials) bigInitials.style.display = "";
      }
    }
    // Topbar avatar — if user has real avatar show it
    const avatarBtn = document.getElementById("avatar-btn");
    if (avatarBtn) {
      if (avatarUrl) {
        avatarBtn.style.backgroundImage = "url(" + avatarUrl + ")";
        avatarBtn.style.backgroundSize = "cover";
        avatarBtn.style.backgroundPosition = "center";
        avatarBtn.style.backgroundRepeat = "no-repeat";
        topbarInitials.style.display = "none";
      } else {
        avatarBtn.style.backgroundImage = "";
        avatarBtn.style.backgroundSize = "";
        avatarBtn.style.backgroundPosition = "";
        avatarBtn.style.backgroundRepeat = "";
        topbarInitials.style.display = "";
      }
    }

    document.getElementById("profile-role-display").textContent =
      user.VaiTroHeThong || user.vai_tro_he_thong || "";
    document.getElementById("pf-username").value = un;
    document.getElementById("pf-email").value = em;
    document.getElementById("pf-bio").value = user.Bio || user.bio || "";

    // Show admin button if admin
    const btnAdmin = document.getElementById("btn-admin-module");
    if (btnAdmin) {
      const isAdmin =
        String(
          user.VaiTroHeThong || user.vai_tro_he_thong || "",
        ).toLowerCase() === "admin";
      btnAdmin.classList.toggle("d-none", !isAdmin);
    }

    // Load team name for profile
    loadMyTeamForProfile();
  }

  // ---- LOGOUT ----
  window.doLogout = async function () {
    await api("/AuthApi/DangXuat", "POST", {});
    currentUser = null;
    window.location.href = "/login";
  };

  // ---- RESTORE SESSION (guard: redirect to /login if not authenticated) ----
  async function restoreSession() {
    const result = await api("/AuthApi/Me");
    if (result.Success && result.Data) {
      enterDashboard(result.Data);
    } else {
      window.location.href = "/login";
    }
  }

  // ---- SIDEBAR / HAMBURGER ----
  window.toggleSidebar = function () {
    document.getElementById("sidebar").classList.toggle("open");
  };
  window.toggleHamburger = function () {
    document.getElementById("hamburger-menu").classList.toggle("open");
  };
  function closeHamburger() {
    document.getElementById("hamburger-menu").classList.remove("open");
  }
  document.addEventListener("click", function (e) {
    const wrap = document.querySelector(".hamburger-menu-wrap");
    if (wrap && !wrap.contains(e.target)) closeHamburger();
  });

  // ---- NAVIGATION ----
  window.navigateTo = function (page) {
    currentPage = page;
    closeHamburger();
    // Hide all pages
    document
      .querySelectorAll(".page-section")
      .forEach((p) => (p.style.display = "none"));
    // Update sidebar active
    document
      .querySelectorAll(".sidebar-item")
      .forEach((b) => b.classList.remove("active"));
    const activeBtn = document.querySelector('[data-page="' + page + '"]');
    if (activeBtn) activeBtn.classList.add("active");

    // Map page id to game page
    const gameMatch = GAMES.find((g) => g.id === page);
    if (gameMatch) {
      showGamePage(gameMatch);
      return;
    }

    const pageMap = {
      home: "page-home",
      follow: "page-follow",
      notifications: "page-notifications",
      "my-tournaments": "page-my-tournaments",
      "my-teams": "page-my-teams",
      organize: "page-organize",
      "create-team": "page-create-team",
      profile: "page-profile",
      "player-profile": "page-player-profile",
      "manage-tournament": "page-manage-tournament",
    };
    const target = pageMap[page];
    if (target) {
      const el = document.getElementById(target);
      if (el) el.style.display = "";
      if (page === "home") loadHomePage();
      if (page === "notifications") loadNotifications();
      if (page === "player-profile") loadPlayerProfileTabs();
      if (page === "my-tournaments") loadMyTournaments();
      if (page === "manage-tournament") loadManageTournament();
      if (page === "my-teams") loadMyTeams();
      if (page === "create-team") initCreateTeamForm();
    }
  };

  // ---- LOAD GAMES LIST ----
  async function loadGames() {
    const result = await api("/AdminApi/DanhSachGame");
    if (!result.Success || !Array.isArray(result.Data)) return;
    gameList = result.Data;

    // Map tên game -> maGame
    GAMES.forEach((g) => {
      const found = gameList.find((d) => {
        const name = (d.ten_game || "").toLowerCase();
        const id = g.id;
        if (id === "game-aov")
          return (
            name.includes("valor") ||
            name.includes("aov") ||
            name.includes("liên quân")
          );
        if (id === "game-lol")
          return name.includes("liên minh") || name.includes("league");
        if (id === "game-ff") return name.includes("free fire");
        if (id === "game-pubg") return name.includes("pubg");
        if (id === "game-val") return name.includes("valorant");
        if (id === "game-cs2")
          return name.includes("cs") || name.includes("counter");
        return false;
      });
      if (found) g.maGame = found.ma_tro_choi;
    });

    // Populate organize/create-team selects
    populateGameSelects();
    loadPlayerProfileTabs();
  }

  function populateGameSelects() {
    ["org-game"].forEach((id) => {
      const sel = document.getElementById(id);
      if (!sel) return;
      sel.innerHTML = '<option value="">-- Chọn game --</option>';
      gameList.forEach((g) => {
        const opt = document.createElement("option");
        opt.value = g.ma_tro_choi;
        opt.textContent = g.ten_game;
        sel.appendChild(opt);
      });
    });

    // Add event listeners for tournament date changes
    const orgStart = document.getElementById("org-start");
    const orgEnd = document.getElementById("org-end");
    if (orgStart) {
      orgStart.addEventListener("change", function () {
        // If only date is selected (no time), set default time to 00:00
        const value = orgStart.value;
        if (value && value.length === 10) {
          orgStart.value = value + "T00:00";
        }
        autoFillStageDates();
      });
    }
    if (orgEnd) {
      orgEnd.addEventListener("change", function () {
        // If only date is selected (no time), set default time to 23:59
        const value = orgEnd.value;
        if (value && value.length === 10) {
          orgEnd.value = value + "T23:59";
        }
        autoFillStageDates();
      });
    }
  }

  // ---- HOME PAGE ----
  function loadHomePage() {
    loadFeaturedTournaments();
  }

  function buildTournamentCard(t, opts) {
    opts = opts || {};
    const gameInfo = GAMES.find((g) => g.maGame == t.ma_tro_choi) || {
      emoji: "🎮",
      color: "#6c63ff",
      name: "",
    };
    const status = t.trang_thai || "";
    let statusHtml = "";
    if (status === "dang_dien_ra")
      statusHtml = "<span class='tc-status live'>🔴 Live</span>";
    else if (status === "mo_dang_ky" || status === "sap_dien_ra")
      statusHtml = "<span class='tc-status upcoming'>🔵 Sắp diễn ra</span>";
    else statusHtml = "<span class='tc-status finished'>✅ Kết thúc</span>";

    const prize = t.tong_giai_thuong
      ? Number(t.tong_giai_thuong).toLocaleString("vi-VN") + "₫"
      : "N/A";
    const maGiai = t.ma_giai_dau;
    const likeAct = opts.da_like ? "tc-btn-like active" : "tc-btn-like";
    const followAct = opts.dang_theo_doi
      ? "tc-btn-follow active"
      : "tc-btn-follow";
    const tLike = opts.tong_like || 0;
    const tFollow = opts.tong_theo_doi || 0;

    return (
      '<div class="tournament-card" id="tc-' +
      maGiai +
      '">' +
      '<div class="tc-banner" style="background:linear-gradient(135deg,#1a1f36,' +
      gameInfo.color +
      '33)" onclick="openTournament(' +
      maGiai +
      ')" style="cursor:pointer">' +
      '<span style="font-size:2.5rem">' +
      gameInfo.emoji +
      "</span></div>" +
      '<div class="tc-body">' +
      '<div class="tc-game-badge">' +
      (t.ten_game || gameInfo.name) +
      "</div>" +
      '<div class="tc-name" onclick="openTournament(' +
      maGiai +
      ')" style="cursor:pointer">' +
      (t.ten_giai_dau || "Giải đấu") +
      "</div>" +
      '<div class="tc-meta">' +
      statusHtml +
      "<span>💰 " +
      prize +
      "</span></div>" +
      '<div class="tc-actions">' +
      '<button class="' +
      likeAct +
      '" onclick="toggleLike(' +
      maGiai +
      ',this)" title="Thích">' +
      '<span class="tc-icon">❤️</span> <span class="tc-like-count" id="like-count-' +
      maGiai +
      '">' +
      tLike +
      "</span>" +
      "</button>" +
      '<button class="' +
      followAct +
      '" onclick="toggleFollow(' +
      maGiai +
      ',this)" title="Theo dõi">' +
      '<span class="tc-icon">🔔</span> <span id="follow-label-' +
      maGiai +
      '">' +
      (opts.dang_theo_doi ? "Đang theo dõi" : "Theo dõi") +
      "</span>" +
      ' <span class="tc-follow-count" id="follow-count-' +
      maGiai +
      '">' +
      tFollow +
      "</span>" +
      "</button>" +
      "</div>" +
      "</div></div>"
    );
  }

  // ---- Toggle Like ----
  window.toggleLike = async function (maGiaiDau, btn) {
    if (!currentUser) {
      showToast("Vui lòng đăng nhập để thích giải đấu.");
      return;
    }
    btn.disabled = true;
    const res = await fetch("/TuongTacApi/Like?maGiaiDau=" + maGiaiDau, {
      method: "POST",
    });
    const result = await res.json();
    btn.disabled = false;
    if (result.Success && result.Data) {
      const d = result.Data;
      btn.classList.toggle("active", d.DaLike);
      const cnt = document.getElementById("like-count-" + maGiaiDau);
      if (cnt) cnt.textContent = d.TongLike || 0;
      // Sync tất cả card cùng giải trên trang
      document
        .querySelectorAll("#tc-" + maGiaiDau + " .tc-btn-like")
        .forEach((b) => b.classList.toggle("active", d.DaLike));
    } else {
      showToast(result.Message || "Lỗi.");
    }
  };

  // ---- Toggle Follow ----
  window.toggleFollow = async function (maGiaiDau, btn) {
    if (!currentUser) {
      showToast("Vui lòng đăng nhập để theo dõi giải đấu.");
      return;
    }
    btn.disabled = true;
    const res = await fetch("/TuongTacApi/Follow?maGiaiDau=" + maGiaiDau, {
      method: "POST",
    });
    const result = await res.json();
    btn.disabled = false;
    if (result.Success && result.Data) {
      const d = result.Data;
      btn.classList.toggle("active", d.DangTheoDoi);
      const lbl = document.getElementById("follow-label-" + maGiaiDau);
      if (lbl) lbl.textContent = d.DangTheoDoi ? "Đang theo dõi" : "Theo dõi";
      const cnt = document.getElementById("follow-count-" + maGiaiDau);
      if (cnt) cnt.textContent = d.TongTheoDoi || 0;
      // Cập nhật sidebar nếu đang theo dõi
      if (d.DangTheoDoi) showSidebarItem("side-my-follow");
    } else {
      showToast(result.Message || "Lỗi.");
    }
  };

  // ---- Load like/follow state cho các card sau khi render ----
  async function loadInteractionStates(maGiaiDauList) {
    if (!currentUser || !Array.isArray(maGiaiDauList)) return;
    maGiaiDauList.forEach(async function (id) {
      const res = await fetch("/TuongTacApi/TrangThai?maGiaiDau=" + id);
      const result = await res.json();
      if (!result.Success || !result.Data) return;
      const d = result.Data;
      // Like
      document
        .querySelectorAll("#tc-" + id + " .tc-btn-like")
        .forEach((b) =>
          b.classList.toggle("active", !!(d.caNhan && d.caNhan.da_like)),
        );
      const lc = document.getElementById("like-count-" + id);
      if (lc) lc.textContent = d.TongLike || 0;
      // Follow
      const following = !!(d.caNhan && d.caNhan.dang_theo_doi);
      document
        .querySelectorAll("#tc-" + id + " .tc-btn-follow")
        .forEach((b) => b.classList.toggle("active", following));
      const lbl = document.getElementById("follow-label-" + id);
      if (lbl) lbl.textContent = following ? "Đang theo dõi" : "Theo dõi";
      const fc = document.getElementById("follow-count-" + id);
      if (fc) fc.textContent = d.TongTheoDoi || 0;
    });
  }

  // ---- Toast notification ----
  function showToast(msg) {
    let t = document.getElementById("app-toast");
    if (!t) {
      t = document.createElement("div");
      t.id = "app-toast";
      t.style.cssText =
        "position:fixed;bottom:24px;right:24px;background:var(--bg-card);border:1px solid var(--border);border-radius:10px;padding:12px 20px;color:var(--text-primary);font-size:.88rem;z-index:9999;box-shadow:0 8px 24px rgba(0,0,0,.4);transition:opacity .3s;";
      document.body.appendChild(t);
    }
    t.textContent = msg;
    t.style.opacity = "1";
    clearTimeout(t._timer);
    t._timer = setTimeout(() => {
      t.style.opacity = "0";
    }, 3000);
  }

  async function loadFeaturedTournaments() {
    const grid = document.getElementById("featured-grid");
    const upGrid = document.getElementById("upcoming-grid");
    if (!grid) return;
    grid.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';

    const result = await api("/MatchmakingApi/DanhSachGiaiCongKhai");
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      grid.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">🏆</div><h4>Chưa có giải đấu nào</h4><p>Hãy là người đầu tiên tổ chức giải!</p></div>';
      if (upGrid) upGrid.innerHTML = "";
      return;
    }

    const active = result.Data.filter((t) => t.trang_thai === "dang_dien_ra");
    const upcoming = result.Data.filter(
      (t) => t.trang_thai === "mo_dang_ky" || t.trang_thai === "sap_dien_ra",
    );

    const activeList = active.slice(0, 6);
    const upList = upcoming.slice(0, 6);

    grid.innerHTML = activeList.length
      ? activeList.map((t) => buildTournamentCard(t)).join("")
      : '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">🏆</div><h4>Không có giải đang diễn ra</h4></div>';

    if (upGrid)
      upGrid.innerHTML = upList.length
        ? upList.map((t) => buildTournamentCard(t)).join("")
        : '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">📅</div><h4>Không có giải sắp diễn ra</h4></div>';

    // Load trạng thái like/follow sau khi render
    const allIds = [...activeList, ...upList].map((t) => t.ma_giai_dau);
    loadInteractionStates(allIds);
  }

  window.openTournament = function (maGiaiDau) {
    window.open("/giai/" + maGiaiDau, "_blank");
  };

  // ---- GAME PAGE ----
  function showGamePage(game) {
    document.getElementById("page-game").style.display = "";
    document.getElementById("game-page-title").textContent =
      game.emoji + " " + game.name;
    document.getElementById("game-page-sub").textContent =
      "Các giải đấu nổi bật · " + game.name;
    loadGameTournaments(game);
  }

  async function loadGameTournaments(game) {
    const grid = document.getElementById("game-grid");
    grid.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';
    let url = "/MatchmakingApi/DanhSachGiaiCongKhai";
    if (game.maGame) url += "?maTroChoi=" + game.maGame;
    const result = await api(url);
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      grid.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">' +
        game.emoji +
        "</div><h4>Chưa có giải đấu " +
        game.name +
        "</h4></div>";
      return;
    }
    const list = result.Data.slice(0, 12);
    grid.innerHTML = list.map((t) => buildTournamentCard(t)).join("");
    loadInteractionStates(list.map((t) => t.ma_giai_dau));
  }

  // ---- NOTIFICATIONS ----
  async function loadNotifications() {
    // Fallback to empty if no endpoint
    const list = document.getElementById("notif-list");
    if (!list) return;
    // You can call an API here if you have one
    // For now we show empty state
  }

  // ---- SEARCH ----
  document
    .getElementById("global-search")
    .addEventListener("keydown", function (e) {
      if (e.key === "Enter" && this.value.trim()) {
        alert("Chức năng tìm kiếm đang phát triển: " + this.value.trim());
      }
    });

  // Expose restoreSession
  window._restoreSession = restoreSession;

  // ============================================================
  // PART 2: Profile, Player Profile, Organize, Create Team
  // ============================================================

  // ---- SAVE PROFILE ----
  window.saveProfile = async function () {
    const msg = document.getElementById("pf-msg");
    const result = await api("/AuthApi/CapNhatThongTin", "POST", {
      Bio: document.getElementById("pf-bio").value.trim(),
      Email: document.getElementById("pf-email").value.trim(),
    });
    msg.style.color = result.Success ? "#2ed573" : "#ff4757";
    msg.textContent = result.Success
      ? "✅ Đã lưu!"
      : result.Message || "Lỗi lưu.";
  };

  // ---- CHANGE PASSWORD ----
  window.changePassword = async function () {
    const msg = document.getElementById("pw-msg");
    const old = document.getElementById("pw-old").value;
    const nw = document.getElementById("pw-new").value;
    const conf = document.getElementById("pw-confirm").value;
    if (nw !== conf) {
      msg.style.color = "#ff4757";
      msg.textContent = "Mật khẩu mới không khớp.";
      return;
    }
    if (nw.length < 8) {
      msg.style.color = "#ff4757";
      msg.textContent = "Mật khẩu phải ≥ 8 ký tự.";
      return;
    }
    const result = await api("/AuthApi/DoiMatKhau", "POST", {
      MatKhauCu: old,
      MatKhauMoi: nw,
    });
    msg.style.color = result.Success ? "#2ed573" : "#ff4757";
    msg.textContent = result.Success
      ? "✅ Đổi mật khẩu thành công!"
      : result.Message || "Sai mật khẩu cũ.";
    if (result.Success) {
      document.getElementById("pw-old").value = "";
      document.getElementById("pw-new").value = "";
      document.getElementById("pw-confirm").value = "";
    }
  };

  // ---- PLAYER PROFILE TABS ----
  const GAME_NAMES = [
    "Arena of Valor",
    "League of Legends",
    "Free Fire",
    "PUBG",
    "Valorant",
    "CS2",
  ];
  let activeGameTab = null;

  function loadPlayerProfileTabs() {
    const tabs = document.getElementById("game-profile-tabs");
    if (!tabs) return;
    tabs.innerHTML = "";
    const games = gameList.length
      ? gameList
      : GAME_NAMES.map((n, i) => ({ ma_tro_choi: i + 1, ten_game: n }));
    games.forEach((g) => {
      const btn = document.createElement("button");
      btn.className =
        "game-tab-btn" + (activeGameTab === g.ma_tro_choi ? " active" : "");
      btn.textContent = g.ten_game;
      btn.onclick = () => loadGameProfile(g);
      tabs.appendChild(btn);
    });
  }

  async function loadGameProfile(game) {
    activeGameTab = game.ma_tro_choi;
    document
      .querySelectorAll(".game-tab-btn")
      .forEach((b) =>
        b.classList.toggle("active", b.textContent === game.ten_game),
      );
    const title = document.getElementById("game-profile-title");
    const body = document.getElementById("game-profile-body");
    title.textContent = "🎮 " + game.ten_game;

    // Load existing profile
    const result = await api(
      "/ProfileApi/LayHoSo?maTroChoi=" + game.ma_tro_choi,
    );
    const existing = result.Success && result.Data ? result.Data : null;

    // Load positions
    const posResult = await api(
      "/ProfileApi/ViTri?maTroChoi=" + game.ma_tro_choi,
    );
    const positions =
      posResult.Success && Array.isArray(posResult.Data) ? posResult.Data : [];

    // Nhóm vị trí theo loai_vi_tri
    const groups = {};
    positions.forEach((p) => {
      const grp = p.LoaiViTri || "Khác";
      if (!groups[grp]) groups[grp] = [];
      groups[grp].push(p);
    });

    const loaiLabel = {
      ChuyenMon: "⚔️ Chuyên môn thi đấu",
      BanHuanLuyen: "🎓 Ban huấn luyện",
    };

    let posOptions = '<option value="">-- Chọn vị trí --</option>';
    Object.keys(groups).forEach((loai) => {
      const label = loaiLabel[loai] || loai;
      posOptions += '<optgroup label="' + label + '">';
      groups[loai].forEach((p) => {
        const sel =
          existing && existing.ma_vi_tri_so_truong == p.MaViTri
            ? " selected"
            : "";
        const ky = p.KyHieu ? " [" + p.KyHieu + "]" : "";
        posOptions +=
          '<option value="' +
          p.MaViTri +
          '"' +
          sel +
          ">" +
          p.TenViTri +
          ky +
          "</option>";
      });
      posOptions += "</optgroup>";
    });

    body.innerHTML =
      '<div class="form-group-dark"><label>ID trong game</label><input class="form-control-dark" id="gp-id" value="' +
      (existing ? existing.in_game_id || "" : "") +
      '" placeholder="ID trong game" /></div>' +
      '<div class="form-group-dark"><label>Tên hiển thị trong game</label><input class="form-control-dark" id="gp-name" value="' +
      (existing ? existing.in_game_name || "" : "") +
      '" placeholder="Nickname" /></div>' +
      '<div class="form-group-dark"><label>Vị trí sở trường</label><select class="select-dark" id="gp-vitri">' +
      posOptions +
      "</select></div>" +
      '<button class="btn-save" onclick="saveGameProfile(' +
      game.ma_tro_choi +
      ')">Lưu hồ sơ</button>' +
      '<span id="gp-msg" style="margin-left:12px;font-size:.85rem"></span>';
  }

  window.saveGameProfile = async function (maTroChoi) {
    const msg = document.getElementById("gp-msg");
    const result = await api("/ProfileApi/TaoHoSo", "POST", {
      MaTroChoi: maTroChoi,
      InGameId: document.getElementById("gp-id").value.trim(),
      InGameName: document.getElementById("gp-name").value.trim(),
      MaViTriSoTruong:
        Number(document.getElementById("gp-vitri").value) || null,
    });
    msg.style.color = result.Success ? "#2ed573" : "#ff4757";
    msg.textContent = result.Success
      ? "✅ Đã lưu hồ sơ!"
      : result.Message || "Lỗi lưu.";
  };

  // ---- ORGANIZE TOURNAMENT ----
  let prizeCount = 0;
  window.addPrizeInput = function () {
    const list = document.getElementById("org-prizes-list");
    prizeCount++;
    const div = document.createElement("div");
    div.id = "prize-item-" + prizeCount;
    div.style.border = "1px solid var(--border)";
    div.style.padding = "10px";
    div.style.marginBottom = "10px";
    div.style.borderRadius = "var(--radius)";
    div.innerHTML = `
    <div style="display:flex; justify-content:space-between; margin-bottom: 5px;">
      <strong>Giải thưởng #${prizeCount}</strong>
      <button class="btn-outline-glow" style="border-color:#ff4757; color:#ff4757; padding:2px 8px; font-size:0.8rem;" onclick="removePrizeInput(${prizeCount})">Xóa</button>
    </div>
    <div class="form-group-dark">
      <input class="form-control-dark prize-name" placeholder="Tên giải (VD: Top 1, MVP...)" />
    </div>
    <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px">
      <div class="form-group-dark">
        <input class="form-control-dark prize-amount" type="number" min="0" placeholder="Giá trị (VNĐ)" onchange="calculateTotalPrize()" />
      </div>
      <div class="form-group-dark">
        <input class="form-control-dark prize-quantity" type="number" min="1" value="1" placeholder="Số lượng" onchange="calculateTotalPrize()" />
      </div>
    </div>
    <div class="form-group-dark" style="margin-bottom:0;">
      <textarea class="form-control-dark prize-desc" placeholder="Mô tả thêm (tùy chọn)" rows="2"></textarea>
    </div>
  `;
    list.appendChild(div);
  };

  window.removePrizeInput = function (id) {
    const el = document.getElementById("prize-item-" + id);
    if (el) {
      el.remove();
      calculateTotalPrize();
    }
  };

  window.calculateTotalPrize = function () {
    const totalInput = document.getElementById("org-giai-thuong");
    if (!totalInput) return;

    let total = 0;
    document.querySelectorAll("#org-prizes-list > div").forEach((el) => {
      const amount = el.querySelector(".prize-amount");
      const quantity = el.querySelector(".prize-quantity");
      const amountValue = amount ? Number(amount.value) || 0 : 0;
      const quantityValue = quantity ? Number(quantity.value) || 1 : 1;
      total += amountValue * quantityValue;
    });

    totalInput.value = total;
  };

  let stageCount = 1;
  window.addStage = function () {
    if (stageCount >= 9) {
      alert("Tối đa chỉ có thể tạo 9 giai đoạn.");
      return;
    }
    stageCount++;
    const tbody = document.getElementById("org-stages-list");
    const tr = document.createElement("tr");
    tr.id = "stage-" + stageCount;
    tr.innerHTML = `
      <td style="padding:8px">Giai đoạn ${stageCount}</td>
      <td style="padding:8px">
        <select class="select-dark" id="stage-${stageCount}-format" style="width:100%">
          <option value="loai_truc_tiep">Loại trực tiếp</option>
          <option value="nhanh_thang_nhanh_thua">Nhánh thắng / Nhánh thua</option>
          <option value="vong_tron">Đấu vòng tròn</option>
          <option value="thuy_si">Hệ Thụy Sĩ (Swiss)</option>
          <option value="champion_rush">Champion Rush</option>
        </select>
      </td>
      <td style="padding:8px">
        <input class="form-control-dark" id="stage-${stageCount}-start" type="datetime-local" style="width:160px" />
      </td>
      <td style="padding:8px">
        <input class="form-control-dark" id="stage-${stageCount}-end" type="datetime-local" style="width:160px" />
      </td>
      <td style="padding:8px">
        <button class="btn-outline-glow" style="padding:4px 8px;font-size:0.8rem" onclick="removeStage(${stageCount})">Xóa</button>
      </td>
    `;
    tbody.appendChild(tr);
  };

  window.removeStage = function (id) {
    const el = document.getElementById("stage-" + id);
    if (!el) return;
    el.remove();
    // Renumber stages
    const tbody = document.getElementById("org-stages-list");
    const rows = tbody.querySelectorAll("tr");
    rows.forEach((row, index) => {
      const cells = row.querySelectorAll("td");
      if (cells.length > 0) {
        cells[0].textContent = "Giai đoạn " + (index + 1);
        const select = row.querySelector("select");
        const startInput = row.querySelector('input[id$="-start"]');
        const endInput = row.querySelector('input[id$="-end"]');
        const btn = row.querySelector("button");
        if (select) select.id = "stage-" + (index + 1) + "-format";
        if (startInput) startInput.id = "stage-" + (index + 1) + "-start";
        if (endInput) endInput.id = "stage-" + (index + 1) + "-end";
        if (btn)
          btn.onclick = function () {
            removeStage(index + 1);
          };
        row.id = "stage-" + (index + 1);
      }
    });
    stageCount = rows.length;
    autoFillStageDates();
  };

  window.autoFillStageDates = function () {
    const tournamentStart = document.getElementById("org-start").value;
    const tournamentEnd = document.getElementById("org-end").value;
    const tbody = document.getElementById("org-stages-list");
    const rows = tbody.querySelectorAll("tr");

    if (rows.length === 1) {
      // 1 stage: use tournament dates
      const startInput = rows[0].querySelector('input[id$="-start"]');
      const endInput = rows[0].querySelector('input[id$="-end"]');
      if (startInput) startInput.value = tournamentStart;
      if (endInput) endInput.value = tournamentEnd;
    } else {
      // Multiple stages
      rows.forEach((row, index) => {
        const startInput = row.querySelector('input[id$="-start"]');
        const endInput = row.querySelector('input[id$="-end"]');

        if (index === 0) {
          // Stage 1: start = tournament start
          if (startInput) startInput.value = tournamentStart;
        } else if (index === rows.length - 1) {
          // Last stage: end = tournament end
          if (endInput) endInput.value = tournamentEnd;
        }
      });
    }
  };

  window.previewStages = function () {
    const preview = document.getElementById("stage-preview");
    const minTeams =
      Number(document.getElementById("org-so-doi-toi-thieu").value) || 0;
    if (minTeams < 2) {
      preview.style.display = "none";
      alert("Vui lòng nhập số đội tối thiểu (tối thiểu 2).");
      return;
    }

    const tbody = document.getElementById("org-stages-list");
    const rows = tbody.querySelectorAll("tr");
    let html = "<h6 style='margin:0 0 12px 0'>📊 Mô phỏng luồng giải đấu</h6>";
    html += `<div style='font-size:0.9rem'><strong>Số đội tối thiểu:</strong> ${minTeams}</div><br>`;

    rows.forEach((row, index) => {
      const formatSelect = row.querySelector("select");
      const startInput = row.querySelector('input[id$="-start"]');
      const endInput = row.querySelector('input[id$="-end"]');
      const format = formatSelect ? formatSelect.value : "";

      const formatNames = {
        loai_truc_tiep: "Loại trực tiếp",
        nhanh_thang_nhanh_thua: "Nhánh thắng / Nhánh thua",
        vong_tron: "Đấu vòng tròn",
        thuy_si: "Hệ Thụy Sĩ",
        champion_rush: "Champion Rush",
      };

      const startDate =
        startInput && startInput.value
          ? new Date(startInput.value).toLocaleDateString("vi-VN")
          : "-";
      const endDate =
        endInput && endInput.value
          ? new Date(endInput.value).toLocaleDateString("vi-VN")
          : "-";

      html += `<div style='padding:8px;border-left:3px solid var(--accent);margin-bottom:8px'>`;
      html += `<strong>Giai đoạn ${index + 1}:</strong> ${formatNames[format] || format}<br>`;
      html += `<span style='color:var(--text-muted)'>Thời gian: ${startDate} - ${endDate}</span>`;
      html += `</div>`;
    });

    preview.innerHTML = html;
    preview.style.display = "block";
  };

  window.submitOrganize = async function () {
    const msg = document.getElementById("org-msg");

    const prizes = [];
    document.querySelectorAll("#org-prizes-list > div").forEach((el) => {
      const ten = el.querySelector(".prize-name").value.trim();
      const amount = el.querySelector(".prize-amount");
      const quantity = el.querySelector(".prize-quantity");
      const mota = el.querySelector(".prize-desc").value.trim();
      const amountValue = amount ? Number(amount.value) || 0 : 0;
      const quantityValue = quantity ? Number(quantity.value) || 1 : 1;
      if (ten || amountValue > 0) {
        prizes.push({
          TenGiai: ten,
          GiaTri: amountValue,
          SoLuong: quantityValue,
          MoTa: mota,
        });
      }
    });

    // Collect stage information
    const stages = [];
    const tbody = document.getElementById("org-stages-list");
    const rows = tbody.querySelectorAll("tr");
    rows.forEach((row, index) => {
      const formatSelect = row.querySelector("select");
      const startInput = row.querySelector('input[id$="-start"]');
      const endInput = row.querySelector('input[id$="-end"]');
      stages.push({
        ThuTu: index + 1,
        TheThuc: formatSelect ? formatSelect.value : "",
        NgayBatDau:
          startInput && startInput.value
            ? new Date(startInput.value).toISOString()
            : null,
        NgayKetThuc:
          endInput && endInput.value
            ? new Date(endInput.value).toISOString()
            : null,
      });
    });

    const maxTeamsValue = document.getElementById("org-so-doi-toi-da").value;
    const payload = {
      TenGiaiDau: document.getElementById("org-ten-giai").value.trim(),
      MoTa: document.getElementById("org-mo-ta").value.trim(),
      MaTroChoi: Number(document.getElementById("org-game").value) || null,
      SoDoiToiThieu:
        Number(document.getElementById("org-so-doi-toi-thieu").value) || 2,
      SoDoiToiDa: maxTeamsValue ? Number(maxTeamsValue) : null,
      TongGiaiThuong:
        Number(document.getElementById("org-giai-thuong").value) || 0,
      NgayBatDau: document.getElementById("org-start").value
        ? new Date(document.getElementById("org-start").value).toISOString()
        : null,
      NgayKetThuc: document.getElementById("org-end").value
        ? new Date(document.getElementById("org-end").value).toISOString()
        : null,
      GiaiThuongs: prizes,
      GiaiDoan: stages,
    };

    // Handle banner file upload
    const bannerFile = document.getElementById("org-banner-file").files[0];
    if (bannerFile) {
      const formData = new FormData();
      formData.append("file", bannerFile);
      try {
        const uploadRes = await fetch("/UploadApi/UploadBanner", {
          method: "POST",
          body: formData,
        });
        const uploadResult = await uploadRes.json();
        if (uploadResult.Success && uploadResult.Data) {
          payload.BannerUrl = uploadResult.Data.Url;
        }
      } catch (e) {
        msg.style.color = "#ff4757";
        msg.textContent = "Lỗi upload banner.";
        return;
      }
    }

    if (!payload.TenGiaiDau) {
      msg.style.color = "#ff4757";
      msg.textContent = "Nhập tên giải đấu.";
      return;
    }

    if (!payload.SoDoi || payload.SoDoi < 2) {
      msg.style.color = "#ff4757";
      msg.textContent = "Số đội tham gia phải tối thiểu 2.";
      return;
    }

    if (!payload.MaTroChoi) {
      msg.style.color = "#ff4757";
      msg.textContent = "Chọn game cho giải đấu.";
      return;
    }

    // Validate stage dates
    if (stages.length === 0) {
      msg.style.color = "#ff4757";
      msg.textContent = "Phải có ít nhất 1 giai đoạn.";
      return;
    }

    for (let i = 0; i < stages.length; i++) {
      if (!stages[i].NgayBatDau || !stages[i].NgayKetThuc) {
        msg.style.color = "#ff4757";
        msg.textContent = `Giai đoạn ${i + 1}: Thiếu ngày bắt đầu hoặc ngày kết thúc.`;
        return;
      }
      if (stages[i].NgayBatDau >= stages[i].NgayKetThuc) {
        msg.style.color = "#ff4757";
        msg.textContent = `Giai đoạn ${i + 1}: Ngày kết thúc phải sau ngày bắt đầu.`;
        return;
      }
      if (i > 0 && stages[i - 1].NgayKetThuc >= stages[i].NgayBatDau) {
        msg.style.color = "#ff4757";
        msg.textContent = `Giai đoạn ${i}: Ngày bắt đầu phải sau ngày kết thúc của giai đoạn ${i}.`;
        return;
      }
    }

    // Stage 1 start must equal tournament start
    if (stages[0].NgayBatDau !== payload.NgayBatDau) {
      msg.style.color = "#ff4757";
      msg.textContent = "Ngày bắt đầu giai đoạn 1 phải bằng ngày bắt đầu giải.";
      return;
    }

    // Last stage end must equal tournament end
    if (stages[stages.length - 1].NgayKetThuc !== payload.NgayKetThuc) {
      msg.style.color = "#ff4757";
      msg.textContent =
        "Ngày kết thúc giai đoạn cuối phải bằng ngày kết thúc giải.";
      return;
    }

    const result = await api(
      "/TournamentBuilderApi/TaoBanNhap",
      "POST",
      payload,
    );
    msg.style.color = result.Success ? "#2ed573" : "#ff4757";
    if (result.Success) {
      msg.textContent = result.Message;
      document.getElementById("org-ten-giai").value = "";
      document.getElementById("org-mo-ta").value = "";
      document.getElementById("org-banner").value = "";
      document.getElementById("org-prizes-list").innerHTML = "";

      showSidebarItem("side-my-tournaments");
      setTimeout(() => navigateTo("my-tournaments"), 1500);
    } else {
      msg.textContent = result.Message || "Tạo giải thất bại.";
    }
  };

  let myTournamentsData = [];

  async function loadMyTournaments() {
    const grid = document.getElementById("my-tournaments-grid");
    if (!grid) return;
    grid.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';

    const result = await api("/TournamentBuilderApi/DanhSachCuaToi");
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      grid.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">🏆</div><h4>Chưa có giải đấu nào</h4><p>Bạn chưa tổ chức giải đấu nào.</p></div>';
      return;
    }

    myTournamentsData = result.Data;

    grid.innerHTML = result.Data.map((t) => {
      const gameInfo = GAMES.find((g) => g.maGame == t.ma_tro_choi) || {
        emoji: "🎮",
        color: "#6c63ff",
        name: "",
      };
      const status = t.trang_thai || "";
      let statusHtml = "";
      if (status === "ban_nhap")
        statusHtml =
          "<span class='tc-status' style='color:#a4b0be'>📝 Bản nháp</span>";
      else if (status === "cho_phe_duyet")
        statusHtml =
          "<span class='tc-status' style='color:#eccc68'>⏳ Chờ duyệt</span>";
      else if (status === "dang_dien_ra")
        statusHtml = "<span class='tc-status live'>🔴 Live</span>";
      else
        statusHtml =
          "<span class='tc-status upcoming'>🔵 Đã duyệt (" +
          status +
          ")</span>";

      const prize = t.tong_giai_thuong
        ? Number(t.tong_giai_thuong).toLocaleString("vi-VN") + "₫"
        : "N/A";

      return (
        '<div class="tournament-card">' +
        '<div class="tc-banner" style="background:linear-gradient(135deg,#1a1f36,' +
        gameInfo.color +
        '33); cursor:default">' +
        '<span style="font-size:2.5rem">' +
        gameInfo.emoji +
        "</span></div>" +
        '<div class="tc-body">' +
        '<div class="tc-game-badge">' +
        (t.ten_game || gameInfo.name) +
        "</div>" +
        '<div class="tc-name">' +
        (t.ten_giai_dau || "Giải đấu") +
        "</div>" +
        '<div class="tc-meta">' +
        statusHtml +
        "<span>💰 " +
        prize +
        "</span></div>" +
        '<div style="margin-top:12px; display:flex; gap:8px;">' +
        '<button class="btn-primary-glow" style="padding:4px 8px; font-size:0.8rem; flex:1" onclick="manageTournament(' +
        t.ma_giai_dau +
        ')">Quản lý</button>' +
        '<button class="btn-outline-glow" style="padding:4px 8px; font-size:0.8rem; flex:1" onclick="openTournament(' +
        t.ma_giai_dau +
        ')">Xem public</button>' +
        "</div>" +
        "</div></div>"
      );
    }).join("");
  }

  let currentManageId = 0;
  let currentManageStatus = "";
  let currentDangMoDangKy = false;

  window.manageTournament = function (maGiaiDau) {
    currentManageId = maGiaiDau;
    navigateTo("manage-tournament");
  };

  async function loadManageTournament() {
    if (!currentManageId) return;
    document.getElementById("manage-tour-name").textContent =
      "Quản lý Giải đấu #" + currentManageId;
    document.getElementById("manage-tour-msg").textContent =
      "Đang tải dữ liệu...";

    // 1. Fetch stage list
    const stagesRes = await api(
      "/TournamentBuilderApi/DanhSachGiaiDoan?maGiaiDau=" + currentManageId,
    );

    // To get status, we look up myTournamentsData
    const t = myTournamentsData.find((x) => x.ma_giai_dau === currentManageId);
    document.getElementById("manage-tour-msg").textContent = "";

    const actionsEl = document.getElementById("manage-tour-actions");
    actionsEl.innerHTML = "";

    if (t) {
      const status = t.trang_thai || "";
      currentManageStatus = status;
      currentDangMoDangKy = t.dang_mo_dang_ky || false;

      let statusText = status;
      if (status === "ban_nhap") statusText = "Bản nháp";
      if (status === "cho_phe_duyet") statusText = "Đang chờ Admin duyệt";
      if (status === "sap_dien_ra" || status === "mo_dang_ky")
        statusText = "Sắp diễn ra";
      if (status === "chuan_bi_dien_ra") statusText = "Chuẩn bị diễn ra";

      document.getElementById("manage-tour-status").innerHTML =
        'Trạng thái hiện tại: <strong style="color:var(--primary)">' +
        statusText +
        "</strong>";

      // Action buttons based on status
      if (status === "ban_nhap") {
        actionsEl.innerHTML =
          '<button class="btn-primary-glow" onclick="submitGuiDuyet()">Gửi yêu cầu xét duyệt</button>' +
          '<p style="margin-top:10px; font-size: 0.9em; color:var(--text-muted)">Sau khi thêm xong các giai đoạn, hãy bấm Gửi yêu cầu để admin duyệt giải.</p>';
      } else if (status === "mo_dang_ky" || status === "sap_dien_ra") {
        actionsEl.innerHTML =
          '<button class="btn-primary-glow" onclick="submitBatDauGiai()">Bắt đầu giải ngay</button>' +
          '<p style="margin-top:10px; font-size: 0.9em; color:var(--text-muted)">Khóa danh sách đăng ký và bắt đầu thi đấu.</p>';
      } else if (status === "chuan_bi_dien_ra") {
        actionsEl.innerHTML =
          '<button class="btn-outline-glow" onclick="toggleTournamentRegistration()">' +
          (currentDangMoDangKy ? "🔴 Đóng đăng ký" : "🟢 Mở đăng ký") +
          "</button>" +
          '<p style="margin-top:10px; font-size: 0.9em; color:var(--text-muted)">Mở/đóng đăng ký cho giải đấu.</p>';
      } else if (status === "dang_dien_ra") {
        actionsEl.innerHTML =
          '<span style="color:#2ed573">Giải đang diễn ra live!</span>';
      }
    }

    // 2. Render stages
    const listEl = document.getElementById("manage-stage-list");
    if (stagesRes.Success && stagesRes.Data && stagesRes.Data.length > 0) {
      listEl.innerHTML = stagesRes.Data.map((s) => {
        return (
          '<div style="border:1px solid var(--border); border-radius: var(--radius); padding: 10px; margin-bottom: 5px; display:flex; justify-content:space-between">' +
          "<div><strong>" +
          s.TenGiaiDoan +
          '</strong> <span style="color:var(--text-muted);font-size:0.9em">(' +
          s.TheThuc +
          ")</span></div>" +
          '<button class="btn-outline-glow" style="padding: 2px 8px; border-color: #ff4757; color:#ff4757" onclick="submitXoaGiaiDoan(' +
          s.MaGiaiDoan +
          ')">Xóa</button>' +
          "</div>"
        );
      }).join("");
    } else {
      listEl.innerHTML =
        '<div class="text-muted">Chưa có giai đoạn thi đấu nào.</div>';
    }
  }

  window.submitAddStage = async function () {
    const name = document.getElementById("manage-stage-name").value.trim();
    const format = document.getElementById("manage-stage-format").value;
    const teams =
      Number(document.getElementById("manage-stage-teams").value) || 2;
    const msg = document.getElementById("manage-tour-msg");

    if (!name) {
      msg.textContent = "Vui lòng nhập tên giai đoạn.";
      msg.style.color = "#ff4757";
      return;
    }

    const payload = {
      MaGiaiDau: currentManageId,
      TenGiaiDoan: name,
      TheThuc: format,
      SoDoiDiTiep: teams,
      DiemNguongMatchPoint: 50, // default if champion_rush
    };

    const res = await api(
      "/TournamentBuilderApi/ThemGiaiDoan",
      "POST",
      payload,
    );
    if (res.Success) {
      document.getElementById("manage-stage-name").value = "";
      loadManageTournament();
    } else {
      msg.textContent = res.Message;
      msg.style.color = "#ff4757";
    }
  };

  window.submitXoaGiaiDoan = async function (maGiaiDoan) {
    if (!confirm("Bạn có chắc muốn xóa giai đoạn này?")) return;
    const res = await api("/TournamentBuilderApi/XoaGiaiDoan", "POST", {
      maGiaiDau: currentManageId,
      maGiaiDoan: maGiaiDoan,
    });
    if (res.Success) loadManageTournament();
    else alert(res.Message);
  };

  window.submitGuiDuyet = async function () {
    const msg = document.getElementById("manage-tour-msg");
    msg.textContent = "Đang gửi...";
    msg.style.color = "var(--text-muted)";

    const res = await api("/TournamentBuilderApi/GuiXetDuyet", "POST", {
      maGiaiDau: currentManageId,
    });
    if (res.Success) {
      // Reload my tournaments so the status updates in the array
      await loadMyTournaments();
      loadManageTournament();
    } else {
      msg.textContent = res.Message;
      msg.style.color = "#ff4757";
    }
  };

  window.submitBatDauGiai = async function () {
    const msg = document.getElementById("manage-tour-msg");
    if (
      !confirm(
        "Sau khi bắt đầu, bạn không thể thay đổi danh sách đội tham gia. Xác nhận bắt đầu giải?",
      )
    )
      return;

    const res = await api("/TournamentBuilderApi/BatDauGiai", "POST", {
      maGiaiDau: currentManageId,
    });
    if (res.Success) {
      await loadMyTournaments();
      loadManageTournament();
    } else {
      msg.textContent = res.Message;
      msg.style.color = "#ff4757";
    }
  };

  window.toggleTournamentRegistration = async function () {
    const msg = document.getElementById("manage-tour-msg");
    const newStatus = !currentDangMoDangKy;
    const res = await api("/TournamentBuilderApi/CapNhatDangMoDangKy", "POST", {
      maGiaiDau: currentManageId,
      dangMo: newStatus,
    });

    if (res.Success) {
      showToast(
        res.Message || (newStatus ? "Đã mở đăng ký." : "Đã đóng đăng ký."),
        "success",
      );
      currentDangMoDangKy = newStatus;
      await loadMyTournaments();
      loadManageTournament();
    } else {
      showToast(
        res.Message || "Cập nhật trạng thái đăng ký thất bại.",
        "error",
      );
    }
  };

  // ---- CREATE TEAM ----
  let teamSquadRowId = 0;

  function initCreateTeamForm() {
    const container = document.getElementById("team-squads-container");
    if (container) {
      container.innerHTML = "";
      teamSquadRowId = 0;
      addSquadRow(); // Add first row by default
    }
  }

  window.addSquadRow = function () {
    const container = document.getElementById("team-squads-container");
    if (!container) return;
    const id = ++teamSquadRowId;
    const row = document.createElement("div");
    row.className = "team-squad-row";
    row.id = "team-squad-row-" + id;
    row.style.display = "grid";
    row.style.gridTemplateColumns = "1fr 1fr 40px";
    row.style.gap = "8px";
    row.style.marginBottom = "8px";
    row.innerHTML =
      '<select class="select-dark team-squad-game" id="team-squad-game-' +
      id +
      '"><option value="">-- Chọn game --</option></select>' +
      '<input class="form-control-dark team-squad-name" id="team-squad-name-' +
      id +
      '" placeholder="VD: Team Alpha LoL" />' +
      '<button class="btn-outline-glow" style="padding:4px 8px;font-size:.75rem" onclick="removeSquadRow(' +
      id +
      ')">✕</button>';
    container.appendChild(row);
    const sel = row.querySelector(".team-squad-game");
    gameList.forEach((g) => {
      const opt = document.createElement("option");
      opt.value = g.ma_tro_choi;
      opt.textContent = g.ten_game;
      sel.appendChild(opt);
    });
  };

  window.removeSquadRow = function (id) {
    const row = document.getElementById("team-squad-row-" + id);
    if (row) row.remove();
  };

  window.submitCreateTeam = async function () {
    const msg = document.getElementById("team-msg");

    const ownTeam = await api("/TeamApi/DoiCuaToi");
    if (ownTeam.Success && ownTeam.Data) {
      msg.style.color = "#ff4757";
      msg.textContent =
        "Bạn đã có đội rồi, không thể tạo thêm đội mới. Hãy xóa đội hiện tại nếu muốn tạo lại.";
      return;
    }

    const tenDoi = document.getElementById("team-ten").value.trim();
    const tenVietTat = document.getElementById("team-tag").value.trim();
    const slogan = document.getElementById("team-slogan").value.trim();
    if (!tenDoi) {
      msg.style.color = "#ff4757";
      msg.textContent = "Nhập tên đội.";
      return;
    }

    const squadRows = document.querySelectorAll(".team-squad-row");
    if (squadRows.length === 0) {
      msg.style.color = "#ff4757";
      msg.textContent = "Phải có ít nhất 1 nhóm thi đấu.";
      return;
    }

    const squads = [];
    let hasError = false;
    squadRows.forEach((row) => {
      const gameSel = row.querySelector(".team-squad-game");
      const nameInput = row.querySelector(".team-squad-name");
      const maTroChoi = gameSel.value ? Number(gameSel.value) : 0;
      const tenNhom = (nameInput.value || "").trim();
      if (!maTroChoi || maTroChoi === 0 || !tenNhom) {
        hasError = true;
      }
      squads.push({ maTroChoi, tenNhom });
    });

    if (hasError) {
      msg.style.color = "#ff4757";
      msg.textContent = "Vui lòng chọn game và nhập tên cho tất cả các nhóm.";
      return;
    }

    const payload = {
      TenDoi: tenDoi,
      TenVietTat: tenVietTat,
      Slogan: slogan,
      LogoUrl: "",
      Squads: JSON.stringify(squads),
    };

    const fd = new URLSearchParams();
    Object.entries(payload).forEach(([k, v]) => fd.append(k, v));
    const res = await fetch("/TeamApi/TaoDoi", {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: fd.toString(),
    });
    const result = await res.json();
    msg.style.color = result.Success ? "#2ed573" : "#ff4757";
    if (result.Success) {
      msg.textContent = "✅ Đã tạo đội và các nhóm thành công!";
      showSidebarItem("side-my-teams");
      navigateTo("my-teams");
    } else {
      msg.textContent = result.Message || "Tạo đội thất bại.";
    }
  };

  // ---- SIDEBAR CONDITIONAL ITEMS ----
  function showSidebarItem(id) {
    const el = document.getElementById(id);
    if (el) el.classList.remove("d-none");
  }

  // ---- AVATAR UPLOAD ----
  window.triggerAvatarUpload = function () {
    const input = document.getElementById("avatar-file-input");
    if (input) input.click();
  };

  // ---- TEAM LOGO UPLOAD ----
  window.triggerTeamLogoUpload = function () {
    console.log("triggerTeamLogoUpload called, mtTeamData:", mtTeamData);
    if (!mtTeamData) return;
    const isChuTich = mtTeamData.vai_tro_hien_tai === "chu_tich";
    console.log(
      "isChuTich:",
      isChuTich,
      "vai_tro_hien_tai:",
      mtTeamData.vai_tro_hien_tai,
    );
    if (!isChuTich) {
      showToast("Chỉ Chủ tịch mới có thể đổi logo đội.", "error");
      return;
    }
    const input = document.getElementById("team-logo-file-input");
    if (input) input.click();
  };

  (function initTeamLogoUpload() {
    const input = document.getElementById("team-logo-file-input");
    if (!input) return;
    input.addEventListener("change", async function () {
      if (!this.files || !this.files[0]) return;
      if (!mtTeamData) return;

      const file = this.files[0];
      const formData = new FormData();
      formData.append("logo", file);
      formData.append("maDoi", mtTeamData.ma_doi);

      try {
        showToast("Đang tải logo đội...", "info");
        const res = await fetch("/TeamApi/CapNhatLogo", {
          method: "POST",
          body: formData,
        });
        const result = await res.json();
        if (result.Success) {
          showToast("Đã cập nhật logo đội thành công!", "success");
          // Reload team data to show new logo
          loadMyTeams();
        } else {
          showToast(result.Message || "Cập nhật logo thất bại.", "error");
        }
      } catch (e) {
        showToast("Lỗi kết nối: " + e.message, "error");
      }
      // Reset input
      this.value = "";
    });
  })();

  (function initAvatarUpload() {
    const input = document.getElementById("avatar-file-input");
    if (!input) return;
    input.addEventListener("change", async function () {
      if (!this.files || !this.files[0]) return;
      const file = this.files[0];
      const formData = new FormData();
      formData.append("avatar", file);

      const uploadMsg = document.getElementById("pf-msg");
      if (uploadMsg) {
        uploadMsg.style.color = "#aaa";
        uploadMsg.textContent = "Đang tải ảnh...";
      }

      try {
        const res = await fetch("/UploadApi/UploadAvatar", {
          method: "POST",
          body: formData,
        });
        const result = await res.json();
        if (result.Success && result.Data) {
          const url = result.Data.AvatarUrl;
          // Cập nhật avatar hiển thị
          const bigImg = document.getElementById("profile-avatar-img");
          const bigInitials = document.getElementById("avatar-initials-big");
          if (bigImg) {
            bigImg.src = url;
            bigImg.style.display = "block";
          }
          if (bigInitials) bigInitials.style.display = "none";
          // Cập nhật topbar avatar
          const avatarBtn = document.getElementById("avatar-btn");
          if (avatarBtn) {
            avatarBtn.style.backgroundImage = "url(" + url + ")";
            avatarBtn.style.backgroundSize = "cover";
            document.getElementById("avatar-initials").style.display = "none";
          }
          if (currentUser) currentUser.AvatarUrl = url;
          if (uploadMsg) {
            uploadMsg.style.color = "#2ed573";
            uploadMsg.textContent = "✅ Đã cập nhật ảnh đại diện!";
          }
        } else {
          if (uploadMsg) {
            uploadMsg.style.color = "#ff4757";
            uploadMsg.textContent = result.Message || "Upload thất bại.";
          }
        }
      } catch (e) {
        if (uploadMsg) {
          uploadMsg.style.color = "#ff4757";
          uploadMsg.textContent = "Lỗi kết nối.";
        }
      }
      this.value = ""; // reset input
    });
  })();

  // ---- MY TOURNAMENTS ----
  async function loadMyTournaments() {
    const grid = document.getElementById("my-tournaments-grid");
    if (!grid || !currentUser) return;
    grid.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';

    const result = await api("/TournamentBuilderApi/GiaiCuaToi");
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      grid.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">🏆</div><h4>Chưa có giải đấu nào</h4><p>Hãy tổ chức giải đầu tiên của bạn!</p></div>';
      return;
    }

    showSidebarItem("side-my-tournaments");
    grid.innerHTML = result.Data.map(function (t) {
      const statusMap = {
        ban_nhap: { label: "Bản nháp", cls: "tc-status" },
        cho_phe_duyet: { label: "Chờ duyệt", cls: "tc-status" },
        mo_dang_ky: { label: "Mở đăng ký", cls: "tc-status upcoming" },
        sap_dien_ra: { label: "Sắp diễn ra", cls: "tc-status upcoming" },
        dang_dien_ra: { label: "🔴 Live", cls: "tc-status live" },
        ket_thuc: { label: "✅ Kết thúc", cls: "tc-status finished" },
        khoa: { label: "🔒 Khóa", cls: "tc-status" },
      };
      const s = statusMap[t.trang_thai] || {
        label: t.trang_thai,
        cls: "tc-status",
      };
      const prize = t.tong_giai_thuong
        ? Number(t.tong_giai_thuong).toLocaleString("vi-VN") + "₫"
        : "N/A";
      return (
        '<div class="tournament-card" id="tc-' +
        t.ma_giai_dau +
        '">' +
        '<div class="tc-banner" onclick="openTournament(' +
        t.ma_giai_dau +
        ')" style="cursor:pointer;background:linear-gradient(135deg,#1a1f36,#6c63ff33)">' +
        '<span style="font-size:2.5rem">🏆</span></div>' +
        '<div class="tc-body">' +
        '<div class="tc-game-badge">' +
        (t.ten_game || "Giải hỗn hợp") +
        "</div>" +
        '<div class="tc-name" onclick="openTournament(' +
        t.ma_giai_dau +
        ')" style="cursor:pointer">' +
        t.ten_giai_dau +
        "</div>" +
        '<div class="tc-meta"><span class="' +
        s.cls +
        '">' +
        s.label +
        "</span><span>💰 " +
        prize +
        "</span></div>" +
        "</div></div>"
      );
    }).join("");
  }

  // ---- CONFIRMATION MODAL ----
  let confirmCallback = null;

  window.showConfirmModal = function (title, message, onConfirm, confirmText) {
    var modal = document.getElementById("confirm-modal");
    var titleEl = document.getElementById("modal-title");
    var msgEl = document.getElementById("modal-message");
    var confirmBtn = document.getElementById("modal-confirm-btn");

    if (titleEl) titleEl.textContent = title;
    if (msgEl) msgEl.textContent = message;
    if (confirmText) confirmBtn.textContent = confirmText;
    else confirmBtn.textContent = "Đồng ý";
    confirmCallback = onConfirm;

    // Remove old event listener
    var newBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newBtn, confirmBtn);

    newBtn.addEventListener("click", function () {
      if (confirmCallback) confirmCallback();
      closeConfirmModal();
    });

    if (modal) modal.style.display = "flex";
  };

  window.closeConfirmModal = function () {
    var modal = document.getElementById("confirm-modal");
    if (modal) modal.style.display = "none";
    confirmCallback = null;
  };

  window.showAlertModal = function (title, message, onClose) {
    var modal = document.getElementById("confirm-modal");
    var titleEl = document.getElementById("modal-title");
    var msgEl = document.getElementById("modal-message");
    var confirmBtn = document.getElementById("modal-confirm-btn");
    var cancelBtn = modal.querySelector(".btn-cancel");

    if (titleEl) titleEl.textContent = title;
    if (msgEl) msgEl.textContent = message;
    confirmCallback = onClose;

    // Change button text to "Đóng"
    confirmBtn.textContent = "Đóng";

    // Hide cancel button for alert modal
    if (cancelBtn) cancelBtn.style.display = "none";

    // Remove old event listener
    var newBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newBtn, confirmBtn);

    newBtn.addEventListener("click", function () {
      if (confirmCallback) confirmCallback();
      closeConfirmModal();
      // Reset button text back to "Đồng ý"
      confirmBtn.textContent = "Đồng ý";
      // Show cancel button again
      if (cancelBtn) cancelBtn.style.display = "";
    });

    if (modal) modal.style.display = "flex";
  };

  // ---- MY TEAMS WORKSPACE ----
  let mtTeamData = null;
  let mtSelectedSquad = null;
  let mtCurrentTab = "roster";

  async function loadMyTeams() {
    if (!currentUser) return;
    const emptyEl = document.getElementById("mt-empty");
    const headerEl = document.getElementById("mt-header");
    const filterEl = document.getElementById("mt-squad-filter");
    const layoutEl = document.querySelector(".mt-layout");
    const bcEl = document.getElementById("mt-breadcrumb");

    // First get user's team
    const r1 = await api("/TeamApi/DoiCuaToi");
    if (!r1.Success || !r1.Data) {
      if (headerEl) headerEl.style.display = "none";
      if (filterEl) filterEl.style.display = "none";
      if (layoutEl) layoutEl.style.display = "none";
      if (bcEl) bcEl.style.display = "none";
      if (emptyEl) emptyEl.style.display = "block";
      return;
    }

    // Show workspace
    if (headerEl) headerEl.style.display = "";
    if (filterEl) filterEl.style.display = "";
    if (layoutEl) layoutEl.style.display = "";
    if (bcEl) bcEl.style.display = "";
    if (emptyEl) emptyEl.style.display = "none";

    showSidebarItem("side-my-teams");
    const maDoi = r1.Data.ma_doi;

    // Get full team detail
    const r2 = await api("/TeamApi/ChiTietDoi?maDoi=" + maDoi);
    if (!r2.Success || !r2.Data) return;
    mtTeamData = r2.Data;
    const team = mtTeamData;
    const squads = Array.isArray(team.nhom_doi) ? team.nhom_doi : [];
    const isChuTich = team.vai_tro_hien_tai === "chu_tich";
    const isBanDieuHanh = team.vai_tro_hien_tai === "ban_dieu_hanh";
    const isDoiTruong = team.vai_tro_hien_tai === "doi_truong";
    const canManage = isChuTich || isBanDieuHanh || isDoiTruong;

    // --- Header ---
    var bcSpan = document.getElementById("mt-team-name-bc");
    if (bcSpan) bcSpan.textContent = team.ten_doi || "Đội";
    var nameEl = document.getElementById("mt-team-name");
    if (nameEl) nameEl.textContent = team.ten_doi || "Đội";
    var sloganEl = document.getElementById("mt-slogan");
    if (sloganEl) sloganEl.textContent = team.slogan || "";
    var logoEl = document.getElementById("mt-logo");
    if (logoEl)
      logoEl.innerHTML = team.logo_url
        ? '<img src="' + team.logo_url + '" alt="">'
        : (team.ten_doi || "?").charAt(0).toUpperCase();

    // Game badge and tag removed as requested

    // Meta badges
    var metaEl = document.getElementById("mt-meta-badges");
    if (metaEl) {
      var founder = team.ten_manager || "";
      var created = team.ngay_tao
        ? new Date(team.ngay_tao).toLocaleDateString("vi-VN")
        : "";
      metaEl.innerHTML =
        (founder
          ? '<span class="mt-meta-badge">👑 Người thành lập: <b>' +
            founder +
            "</b></span>"
          : "") +
        (created
          ? '<span class="mt-meta-badge">📅 Thành lập: <b>' +
            created +
            "</b></span>"
          : "") +
        '<span class="mt-meta-badge">👥 Thành viên: <b>' +
        (team.so_thanh_vien || 0) +
        "</b></span>" +
        (canManage
          ? '<button class="btn-outline-glow" style="margin-left:12px;padding:4px 12px;font-size:.8rem" onclick="toggleTeamRecruit()">' +
            (team.dang_tuyen ? "🔴 Tắt tuyển dụng" : "🟢 Bật tuyển dụng") +
            "</button>"
          : "") +
        (isChuTich
          ? '<button class="btn-outline-glow" style="margin-left:12px;padding:4px 12px;font-size:.8rem" onclick="showEditTeamModal()">✏️ Sửa thông tin đội</button>'
          : "");
    }

    // --- Squad Filter ---
    if (filterEl) {
      var allBtnHtml =
        '<button class="mt-squad-btn active" data-ma-nhom="all" onclick="selectMyTeamSquad(0, this)">📋 Tất cả thành viên</button>';
      var squadBtnsHtml = squads
        .map(function (sq) {
          return (
            '<button class="mt-squad-btn" data-ma-nhom="' +
            sq.ma_nhom +
            '" onclick="selectMyTeamSquad(' +
            sq.ma_nhom +
            ', this)">' +
            (sq.ten_game || "") +
            " - " +
            (sq.ten_nhom || "Nhóm") +
            "</button>"
          );
        })
        .join("");
      filterEl.innerHTML = allBtnHtml + squadBtnsHtml;

      // Default to "All" selection
      mtSelectedSquad = { ma_nhom: 0, is_all: true };
      loadMyTeamTabContent();
    }

    // --- Sidebar ---
    renderMyTeamSidebar(team, squads, canManage);

    // --- Add Squad game dropdown ---
    var addSqGame = document.getElementById("mt-add-squad-game");
    if (addSqGame) {
      addSqGame.innerHTML = '<option value="">-- Chọn game --</option>';
      gameList.forEach(function (g) {
        addSqGame.innerHTML +=
          '<option value="' + g.ma_tro_choi + '">' + g.ten_game + "</option>";
      });
    }

    // --- Populate squad dropdown for invitation ---
    var inviteSquad = document.getElementById("mt-invite-squad");
    if (inviteSquad) {
      inviteSquad.innerHTML =
        '<option value="">Chỉ mời vào đội (chưa phân nhóm)</option>';
      squads.forEach(function (sq) {
        inviteSquad.innerHTML +=
          '<option value="' +
          sq.ma_nhom +
          '">' +
          (sq.ten_game || "") +
          " - " +
          (sq.ten_nhom || "Nhóm") +
          "</option>";
      });
    }

    // Show/hide management controls
    var addSquadBtn = document.getElementById("mt-btn-add-squad");
    if (addSquadBtn)
      addSquadBtn.style.display = isChuTich || isBanDieuHanh ? "" : "none";
    var delSquadBtn = document.getElementById("mt-btn-delete-squad");
    if (delSquadBtn) {
      // Only Chủ tịch can delete squads
      delSquadBtn.style.display = isChuTich ? "" : "none";
    }
    var delTeamBtn = document.getElementById("mt-btn-delete-team");
    if (delTeamBtn) delTeamBtn.style.display = isChuTich ? "" : "none";
  }

  window.selectMyTeamSquad = function (maNhom, btnEl) {
    document.querySelectorAll(".mt-squad-btn").forEach(function (b) {
      b.classList.remove("active");
    });
    if (btnEl) btnEl.classList.add("active");
    var squads =
      mtTeamData && Array.isArray(mtTeamData.nhom_doi)
        ? mtTeamData.nhom_doi
        : [];
    if (maNhom === 0) {
      mtSelectedSquad = { ma_nhom: 0, is_all: true };
    } else {
      mtSelectedSquad =
        squads.find(function (s) {
          return s.ma_nhom === maNhom;
        }) || null;
    }

    // Always show delete button for all squads
    var delSquadBtn = document.getElementById("mt-btn-delete-squad");
    if (delSquadBtn) {
      delSquadBtn.style.display = "";
    }
    loadMyTeamTabContent();
  };

  window.switchMyTeamTab = function (tab, btnEl) {
    mtCurrentTab = tab;
    document.querySelectorAll(".mt-tab").forEach(function (b) {
      b.classList.remove("active");
    });
    if (btnEl) btnEl.classList.add("active");
    loadMyTeamTabContent();
  };

  async function loadMyTeamTabContent() {
    var content = document.getElementById("mt-tab-content");
    var inviteSection = document.getElementById("mt-invite-section");
    if (!content || !mtSelectedSquad) return;

    if (mtCurrentTab === "roster") {
      await loadMyTeamRoster(content);
      // Show invite for Chủ tịch/Ban điều hành/Đội trưởng
      var canManage =
        mtTeamData &&
        (mtTeamData.vai_tro_hien_tai === "chu_tich" ||
          mtTeamData.vai_tro_hien_tai === "ban_dieu_hanh" ||
          mtTeamData.vai_tro_hien_tai === "doi_truong");
      if (inviteSection)
        inviteSection.style.display = canManage ? "block" : "none";

      // Show join requests for Đội trưởng when viewing a specific squad
      var joinRequestsSection = document.getElementById(
        "mt-join-requests-section",
      );
      if (joinRequestsSection) {
        var isDoiTruong =
          mtTeamData && mtTeamData.vai_tro_hien_tai === "doi_truong";
        var isSquadSelected = mtSelectedSquad && !mtSelectedSquad.is_all;
        if (isDoiTruong && isSquadSelected) {
          joinRequestsSection.style.display = "block";
          loadSquadJoinRequests();
        } else {
          joinRequestsSection.style.display = "none";
        }
      }
    } else {
      if (inviteSection) inviteSection.style.display = "none";
      if (mtCurrentTab === "history") loadMyTeamHistory(content);
      else if (mtCurrentTab === "tournaments") loadMyTeamTournaments(content);
      else if (mtCurrentTab === "stats") loadMyTeamStats(content);
    }
  }

  async function loadMyTeamRoster(container) {
    container.innerHTML =
      '<div style="color:var(--text-muted);padding:16px">Đang tải roster...</div>';

    var allMembers = [];

    if (mtSelectedSquad.is_all) {
      // Fetch members from all squads + management group
      var squads =
        mtTeamData && Array.isArray(mtTeamData.nhom_doi)
          ? mtTeamData.nhom_doi
          : [];

      // Get squad members sequentially with error handling
      for (var i = 0; i < squads.length; i++) {
        try {
          var r = await api(
            "/TeamApi/ThanhVienNhom?maNhom=" + squads[i].ma_nhom,
          );
          if (r.Success && Array.isArray(r.Data)) {
            r.Data.forEach(function (m) {
              m.ten_nhom = squads[i].ten_nhom;
              m.ten_game = squads[i].ten_game;
            });
            allMembers = allMembers.concat(r.Data);
          }
        } catch (e) {
          console.error("Error loading squad members:", e);
        }
      }

      // Get management group members (without squad)
      try {
        var mgResult = await api(
          "/TeamApi/ThanhVienNhomQuanLy?maDoi=" + mtTeamData.ma_doi,
        );
        if (mgResult.Success && Array.isArray(mgResult.Data)) {
          mgResult.Data.forEach(function (m) {
            m.ten_nhom = "Nhóm quản lý";
            m.ten_game = "";
          });
          allMembers = allMembers.concat(mgResult.Data);
        }
      } catch (e) {
        console.error("Error loading management group members:", e);
      }

      // Deduplicate by ma_nguoi_dung to avoid showing same person multiple times
      var uniqueMembers = {};
      allMembers.forEach(function (m) {
        if (!uniqueMembers[m.ma_nguoi_dung]) {
          uniqueMembers[m.ma_nguoi_dung] = m;
        }
      });
      allMembers = Object.values(uniqueMembers);
    } else {
      try {
        var result = await api(
          "/TeamApi/ThanhVienNhom?maNhom=" + mtSelectedSquad.ma_nhom,
        );
        if (result.Success && Array.isArray(result.Data)) {
          allMembers = result.Data;
        }
      } catch (e) {
        console.error("Error loading squad members:", e);
      }
    }

    if (allMembers.length === 0) {
      container.innerHTML =
        '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">👥</div><h4>Chưa có thành viên</h4></div>';
      return;
    }

    var html = '<div class="mt-roster-grid">';
    allMembers.forEach(function (m) {
      var initials = (m.in_game_name || m.ten_dang_nhap || "?")
        .charAt(0)
        .toUpperCase();
      var role = m.vai_tro_noi_bo || "thanh_vien";
      var roleCls =
        role === "chu_tich"
          ? "chu_tich"
          : role === "doi_truong"
            ? "doi_truong"
            : role === "ban_dieu_hanh"
              ? "ban_dieu_hanh"
              : "thanh_vien";
      var roleLabel =
        role === "chu_tich"
          ? "Chủ tịch"
          : role === "doi_truong"
            ? "Đội trưởng"
            : role === "ban_dieu_hanh"
              ? "Ban điều hành"
              : "Thành viên";
      var posLabel = m.ten_vi_tri || m.ky_hieu_vi_tri || "";
      var joinDate = m.ngay_tham_gia
        ? new Date(m.ngay_tham_gia).toLocaleDateString("vi-VN")
        : "";
      var displayName = m.in_game_name || m.ten_dang_nhap || "Người chơi";
      var squadInfo = mtSelectedSquad.is_all
        ? '<div class="mt-member-squad">' +
          (m.ten_game || "") +
          " - " +
          (m.ten_nhom || "Nhóm") +
          "</div>"
        : "";

      // Add "Join Squad" button for management group members when viewing specific squad
      var joinBtn = "";
      if (
        mtSelectedSquad &&
        !mtSelectedSquad.is_all &&
        m.ten_nhom === "Nhóm quản lý"
      ) {
        joinBtn =
          '<button class="btn-primary-glow" style="margin-top:8px;padding:4px 12px;font-size:0.8rem" onclick="requestJoinSquad(' +
          mtSelectedSquad.ma_nhom +
          ')">Tham gia nhóm</button>';
      }

      // Add "Đặt làm Đội trưởng" button for Chủ tịch when viewing specific squad (not management group)
      var captainBtn = "";
      if (
        mtTeamData &&
        mtTeamData.vai_tro_hien_tai === "chu_tich" &&
        !mtSelectedSquad.is_all &&
        m.ten_nhom !== "Nhóm quản lý" &&
        role !== "chu_tich" &&
        role !== "doi_truong"
      ) {
        captainBtn =
          '<button class="btn-outline-glow" style="margin-top:8px;padding:4px 12px;font-size:0.8rem" onclick="setCaptainForMember(' +
          m.ma_nguoi_dung +
          ')">👑 Đặt làm Đội trưởng</button>';
      }

      html +=
        '<div class="mt-member-card" onclick="showMemberProfile(' +
        m.ma_nguoi_dung +
        ", " +
        (m.ma_tro_choi || "null") +
        ')" style="cursor:pointer">' +
        '<div class="mt-member-avatar">' +
        (m.avatar_url ? '<img src="' + m.avatar_url + '" alt="">' : initials) +
        "</div>" +
        '<div class="mt-member-info">' +
        '<div class="mt-member-name">' +
        displayName +
        "</div>" +
        '<div class="mt-member-badges">' +
        '<span class="mt-badge-role ' +
        roleCls +
        '">' +
        roleLabel +
        "</span>" +
        (posLabel ? '<span class="mt-badge-pos">' + posLabel + "</span>" : "") +
        "</div>" +
        squadInfo +
        (joinDate
          ? '<div class="mt-member-date">Tham gia: ' + joinDate + "</div>"
          : "") +
        joinBtn +
        captainBtn +
        "</div></div>";
    });
    html += "</div>";
    container.innerHTML = html;
  }

  window.setCaptainForMember = async function (maNguoiDung) {
    if (!mtSelectedSquad || mtSelectedSquad.is_all) return;
    if (
      !confirm(
        "Bạn có chắc muốn đặt thành viên này làm Đội trưởng của nhóm " +
          mtSelectedSquad.ten_nhom +
          "?",
      )
    )
      return;

    var result = await fetch("/TeamApi/CapNhatDoiTruongNhom", {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body:
        "maNhom=" + mtSelectedSquad.ma_nhom + "&maDoiTruongMoi=" + maNguoiDung,
    }).then(function (r) {
      return r.json();
    });

    if (result.Success) {
      showToast("Đã đặt Đội trưởng thành công!", "success");
      loadMyTeamRoster(document.getElementById("mt-tab-content"));
    } else {
      showToast(result.Message || "Đặt Đội trưởng thất bại.", "error");
    }
  };

  window.showMemberProfile = async function (maNguoiDung, maTroChoi) {
    if (!maNguoiDung) return;

    // Lấy thông tin hồ sơ thi đấu của người dùng
    var result = await api(
      "/GameProfileApi/LayHoSoTheoNguoiDung?maNguoiDung=" +
        maNguoiDung +
        "&maTroChoi=" +
        (maTroChoi || 0),
    );

    if (!result.Success || !result.Data) {
      showToast("Không thể tải hồ sơ thi đấu.", "error");
      return;
    }

    var profile = result.Data;
    var modal = document.getElementById("member-profile-modal");
    if (!modal) {
      // Tạo modal nếu chưa có
      modal = document.createElement("div");
      modal.id = "member-profile-modal";
      modal.className = "modal-overlay";
      modal.style.cssText =
        "position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.8);display:none;justify-content:center;align-items:center;z-index:10000";
      document.body.appendChild(modal);
    }

    modal.innerHTML =
      '<div class="modal-content" style="background:var(--bg-card);border-radius:12px;padding:24px;max-width:500px;width:90%;max-height:80vh;overflow-y:auto">' +
      '<div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:20px">' +
      '<h3 style="margin:0;color:var(--text-primary)">🎮 Hồ sơ thi đấu</h3>' +
      '<button onclick="closeMemberProfileModal()" style="background:none;border:none;color:var(--text-muted);font-size:24px;cursor:pointer">✕</button>' +
      "</div>" +
      '<div style="margin-bottom:16px">' +
      '<div style="font-size:0.9rem;color:var(--text-muted)">Tên trong game</div>' +
      '<div style="font-size:1.1rem;font-weight:600;color:var(--text-primary)">' +
      (profile.in_game_name || profile.ten_dang_nhap || "N/A") +
      "</div>" +
      "</div>" +
      '<div style="margin-bottom:16px">' +
      '<div style="font-size:0.9rem;color:var(--text-muted)">ID trong game</div>' +
      '<div style="font-size:1.1rem;color:var(--text-primary)">' +
      (profile.in_game_id || "N/A") +
      "</div>" +
      "</div>" +
      '<div style="margin-bottom:16px">' +
      '<div style="font-size:0.9rem;color:var(--text-muted)">Vị trí</div>' +
      '<div style="font-size:1.1rem;color:var(--text-primary)">' +
      (profile.ten_vi_tri || "N/A") +
      "</div>" +
      "</div>" +
      '<div style="margin-bottom:16px">' +
      '<div style="font-size:0.9rem;color:var(--text-muted)">Kinh nghiệm</div>' +
      '<div style="font-size:1.1rem;color:var(--text-primary)">' +
      (profile.kinh_nghiem || 0) +
      "</div>" +
      "</div>" +
      "</div>";

    modal.style.display = "flex";
  };

  window.closeMemberProfileModal = function () {
    var modal = document.getElementById("member-profile-modal");
    if (modal) modal.style.display = "none";
  };

  window.setCaptain = async function () {
    var select = document.getElementById("mt-captain-select");
    if (!select) return;
    var maDoiTruongMoi = parseInt(select.value);
    if (!maDoiTruongMoi || isNaN(maDoiTruongMoi)) {
      showToast("Vui lòng chọn thành viên để làm Đội trưởng.", "error");
      return;
    }

    var result = await api("/TeamApi/CapNhatDoiTruongNhom", {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body:
        "maNhom=" +
        mtSelectedSquad.ma_nhom +
        "&maDoiTruongMoi=" +
        maDoiTruongMoi,
    });

    if (result.Success) {
      showToast("Đã cập nhật Đội trưởng thành công!", "success");
      loadMyTeamRoster(document.getElementById("mt-tab-content"));
    } else {
      showToast(result.Message || "Cập nhật Đội trưởng thất bại.", "error");
    }
  };

  window.showEditTeamModal = function () {
    if (!mtTeamData) return;
    document.getElementById("edit-team-ten").value = mtTeamData.ten_doi || "";
    document.getElementById("edit-team-tag").value =
      mtTeamData.ten_viet_tat || "";
    document.getElementById("edit-team-slogan").value = mtTeamData.slogan || "";
    document.getElementById("edit-team-logo").value = mtTeamData.logo_url || "";
    document.getElementById("edit-team-modal").style.display = "flex";
  };

  window.toggleTeamRecruit = async function () {
    if (!mtTeamData) return;
    var newStatus = !mtTeamData.dang_tuyen;
    var result = await api("/TeamApi/CapNhatDangTuyen", "POST", {
      maDoi: mtTeamData.ma_doi,
      dangTuyen: newStatus,
    });

    if (result.Success) {
      showToast(
        result.Message || "Đã cập nhật trạng thái tuyển dụng.",
        "success",
      );
      mtTeamData.dang_tuyen = newStatus;
      loadMyTeams(); // Reload to update UI
    } else {
      showToast(
        result.Message || "Cập nhật trạng thái tuyển dụng thất bại.",
        "error",
      );
    }
  };

  window.closeEditTeamModal = function () {
    document.getElementById("edit-team-modal").style.display = "none";
  };

  window.submitEditTeam = async function () {
    if (!mtTeamData) return;
    const msg = document.getElementById("edit-team-msg");
    const tenDoi = document.getElementById("edit-team-ten").value.trim();
    const tenVietTat = document.getElementById("edit-team-tag").value.trim();
    const slogan = document.getElementById("edit-team-slogan").value.trim();
    const logoUrl = document.getElementById("edit-team-logo").value.trim();

    if (!tenDoi) {
      msg.style.color = "#ff4757";
      msg.textContent = "Vui lòng nhập tên đội.";
      return;
    }

    const result = await api("/TeamApi/CapNhatThongTinDoi", {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: `maDoi=${mtTeamData.ma_doi}&tenDoi=${encodeURIComponent(tenDoi)}&tenVietTat=${encodeURIComponent(tenVietTat)}&slogan=${encodeURIComponent(slogan)}&logoUrl=${encodeURIComponent(logoUrl)}`,
    });

    if (result.Success) {
      showToast("Đã cập nhật thông tin đội thành công!", "success");
      closeEditTeamModal();
      loadMyTeams(); // Reload to show updated info
    } else {
      msg.style.color = "#ff4757";
      msg.textContent = result.Message || "Cập nhật thất bại.";
    }
  };

  function loadMyTeamHistory(container) {
    container.innerHTML =
      '<div class="empty-state" style="padding:40px"><div class="empty-state-icon">⚡</div>' +
      "<h4>Lịch sử thi đấu</h4><p>Chưa có dữ liệu trận đấu cho nhóm này.</p></div>";
  }

  function loadMyTeamTournaments(container) {
    container.innerHTML =
      '<div class="empty-state" style="padding:40px"><div class="empty-state-icon">🏆</div>' +
      "<h4>Giải đấu</h4><p>Nhóm chưa tham gia giải đấu nào.</p></div>";
  }

  function loadMyTeamStats(container) {
    container.innerHTML =
      '<div class="empty-state" style="padding:40px"><div class="empty-state-icon">📊</div>' +
      "<h4>Thống kê</h4><p>Chưa có dữ liệu thống kê cho nhóm này.</p></div>";
  }

  function renderMyTeamSidebar(team, squads, canManage) {
    var infoEl = document.getElementById("mt-sidebar-info");
    if (!infoEl) return;
    var created = team.ngay_tao
      ? new Date(team.ngay_tao).toLocaleDateString("vi-VN")
      : "-";
    infoEl.innerHTML =
      '<div class="mt-sidebar-info-row"><span class="label">Thành viên</span><span class="value">' +
      (team.so_thanh_vien || 0) +
      "</span></div>" +
      '<div class="mt-sidebar-info-row"><span class="label">Ngày thành lập</span><span class="value">' +
      created +
      "</span></div>";
  }

  window.toggleMyTeamAddSquad = function () {
    var form = document.getElementById("mt-add-squad-form");
    if (form)
      form.style.display = form.style.display === "none" ? "block" : "none";
  };

  window.submitMyTeamAddSquad = async function () {
    var msg = document.getElementById("mt-add-squad-msg");
    var gameVal = document.getElementById("mt-add-squad-game").value;
    var nameVal = (
      document.getElementById("mt-add-squad-name").value || ""
    ).trim();
    if (!gameVal) {
      if (msg) {
        msg.style.color = "#ff4757";
        msg.textContent = "Vui lòng chọn game.";
      }
      return;
    }
    if (!nameVal) {
      if (msg) {
        msg.style.color = "#ff4757";
        msg.textContent = "Vui lòng nhập tên nhóm.";
      }
      return;
    }
    var result = await api("/TeamApi/TaoNhom", "POST", {
      maDoi: mtTeamData.ma_doi,
      maTroChoi: Number(gameVal),
      tenNhom: nameVal,
      maCaptain: 0, // Parameter name unchanged in BUS, value 0 means no captain assigned
    });
    if (msg) {
      msg.style.color = result.Success ? "#2ed573" : "#ff4757";
      msg.textContent = result.Success
        ? "Tạo nhóm thành công!"
        : result.Message || "Tạo nhóm thất bại.";
    }
    if (result.Success) loadMyTeams();
  };

  window.deleteSelectedMyTeamSquad = async function () {
    if (!mtTeamData || !mtSelectedSquad) return;

    // Check if it's a special group that cannot be deleted
    var isManagementGroup =
      mtSelectedSquad &&
      (mtSelectedSquad.ten_nhom === "Nhóm quản lý" ||
        mtSelectedSquad.ma_tro_choi === null ||
        mtSelectedSquad.is_all === true);

    if (isManagementGroup) {
      // Show alert modal for special groups
      showAlertModal(
        "Không thể xóa",
        "Bạn không thể xóa nhóm này.",
        function () {
          // Close modal - do nothing
        },
      );
      return;
    }

    // Check if this is the last squad - cannot delete
    var squads = Array.isArray(mtTeamData.nhom_doi) ? mtTeamData.nhom_doi : [];
    var gameSquads = squads.filter(function (s) {
      return s.ma_tro_choi !== null;
    });

    if (gameSquads.length <= 1) {
      showAlertModal(
        "Không thể xóa",
        "Đội phải có ít nhất một nhóm thi đấu.",
        function () {
          // Close modal - do nothing
        },
      );
      return;
    }

    // Normal squad deletion with confirm modal
    showConfirmModal(
      "Xác nhận xóa nhóm",
      'Bạn có chắc chắn muốn xóa nhóm "' +
        (mtSelectedSquad.ten_nhom || "") +
        '"? Hành động này không thể hoàn tác.',
      async function () {
        var result = await api("/TeamApi/XoaNhom", "POST", {
          maDoi: mtTeamData.ma_doi,
          maNhom: mtSelectedSquad.ma_nhom,
        });

        if (!result.Success) {
          showToast(result.Message || "Xóa nhóm thất bại.");
          return;
        }

        loadMyTeams();
      },
      "Xóa",
    );
  };

  window.deleteMyTeam = async function () {
    if (!mtTeamData) return;

    showConfirmModal(
      "Xác nhận xóa đội",
      'Bạn có chắc chắn muốn xóa đội "' +
        (mtTeamData.ten_doi || "") +
        '"? Tất cả nhóm trong đội sẽ bị xóa.',
      async function () {
        var result = await api("/TeamApi/XoaDoi", "POST", {
          maDoi: mtTeamData.ma_doi,
        });
        if (!result.Success) {
          showToast(result.Message || "Xóa đội thất bại.");
          return;
        }

        showToast("Đã xóa đội thành công.");
        mtTeamData = null;
        mtSelectedSquad = null;
        navigateTo("create-team");
      },
    );
  };

  window.submitInviteMember = async function () {
    var msg = document.getElementById("mt-invite-msg");
    var input = document.getElementById("mt-invite-input");
    var squadSelect = document.getElementById("mt-invite-squad");
    var val = (input ? input.value : "").trim();
    if (!val) {
      if (msg) {
        msg.style.color = "#ff4757";
        msg.textContent = "Vui lòng nhập tên hoặc email.";
      }
      return;
    }
    if (!mtTeamData) return;

    var maNhom =
      squadSelect && squadSelect.value ? parseInt(squadSelect.value) : null;
    var result = await api("/TeamApi/GuiLoiMoiGiaNhap", "POST", {
      maDoi: mtTeamData.ma_doi,
      maNhom: maNhom,
      tenNguoiNhan: val,
    });
    if (msg) {
      msg.style.color = result.Success ? "#2ed573" : "#ff4757";
      msg.textContent = result.Success
        ? "Đã gửi lời mời thành công!"
        : result.Message || "Gửi lời mời thất bại.";
    }
    if (result.Success && input) input.value = "";
  };

  async function loadSquadJoinRequests() {
    var container = document.getElementById("mt-join-requests-list");
    if (!container || !mtSelectedSquad) return;

    container.innerHTML =
      '<div style="color:var(--text-muted);padding:10px">Đang tải...</div>';

    var result = await api(
      "/TeamApi/LayYeuCauThamGiaNhom?maNhom=" + mtSelectedSquad.ma_nhom,
      "POST",
      {},
    );

    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      container.innerHTML =
        '<div style="color:var(--text-muted);padding:10px">Không có yêu cầu nào.</div>';
      return;
    }

    var html = "";
    result.Data.forEach(function (req) {
      html +=
        '<div class="mt-join-request-item" style="background:var(--card-bg);border:1px solid var(--border-color);padding:12px;margin-bottom:8px;border-radius:8px;display:flex;align-items:center;gap:12px">';
      html +=
        '<div style="width:40px;height:40px;border-radius:50%;background:var(--accent);display:flex;align-items:center;justify-content:center;font-weight:bold;color:#fff">' +
        (req.ten_dang_nhap || "").charAt(0).toUpperCase() +
        "</div>";
      html += '<div style="flex:1">';
      html +=
        '<div style="font-weight:600;color:var(--text-primary)">' +
        (req.ten_dang_nhap || "") +
        "</div>";
      html +=
        '<div style="font-size:0.85rem;color:var(--text-muted)">In-game: ' +
        (req.in_game_name || "") +
        " | Vị trí: " +
        (req.ten_vi_tri || "N/A") +
        "</div>";
      html += "</div>";
      html += '<div style="display:flex;gap:8px">';
      html +=
        '<button class="btn-primary-glow" style="padding:6px 12px;font-size:0.85rem" onclick="approveSquadJoinRequest(' +
        req.ma_yeu_cau +
        ', true)">✓ Duyệt</button>';
      html +=
        '<button class="btn-outline-glow" style="padding:6px 12px;font-size:0.85rem;border-color:#ff4757;color:#ff6b81" onclick="approveSquadJoinRequest(' +
        req.ma_yeu_cau +
        ', false)">✕ Từ chối</button>';
      html += "</div>";
      html += "</div>";
    });

    container.innerHTML = html;
  }

  window.approveSquadJoinRequest = async function (maYeuCau, chapNhan) {
    var result = await api("/TeamApi/DuyetYeuCauThamGiaNhom", "POST", {
      maYeuCau: maYeuCau,
      chapNhan: chapNhan,
    });

    if (result.Success) {
      showToast(
        chapNhan ? "Đã duyệt yêu cầu tham gia nhóm!" : "Đã từ chối yêu cầu.",
        "success",
      );
      loadSquadJoinRequests();
      loadMyTeamRoster(document.getElementById("mt-tab-content"));
    } else {
      showToast(result.Message || "Thao tác thất bại.", "error");
    }
  };

  window.requestJoinSquad = async function (maNhom) {
    // Get user's game profile for this squad's game
    var squad = mtTeamData.nhom_doi.find(function (s) {
      return s.ma_nhom === maNhom;
    });
    if (!squad) {
      showToast("Không tìm thấy thông tin nhóm.", "error");
      return;
    }

    // Check if user is already in a game squad
    var userSquads = mtTeamData.nhom_doi.filter(function (s) {
      return s.ma_tro_choi !== null;
    });
    if (userSquads.length > 0) {
      showToast(
        "Bạn đã thuộc một nhóm thi đấu. Một người chỉ có thể tham gia tối đa một nhóm thi đấu.",
        "error",
      );
      return;
    }

    var profileResult = await api(
      "/ProfileApi/LayHoSo?maTroChoi=" + squad.ma_tro_choi,
    );
    if (!profileResult.Success || !profileResult.Data) {
      showToast(
        "Bạn chưa có hồ sơ thi đấu cho game này. Vui lòng tạo hồ sơ trước.",
        "error",
      );
      return;
    }

    var maHoSo = profileResult.Data.ma_ho_so;
    var result = await api("/TeamApi/GuiYeuCauThamGiaNhom", "POST", {
      maNhom: maNhom,
      maHoSo: maHoSo,
    });

    if (result.Success) {
      showToast(
        "Đã gửi yêu cầu tham gia nhóm! Chờ Đội trưởng duyệt.",
        "success",
      );
    } else {
      showToast(result.Message || "Gửi yêu cầu thất bại.", "error");
    }
  };

  // ---- LOAD TEAM FOR PROFILE PAGE ----
  async function loadMyTeamForProfile() {
    const pfTeam = document.getElementById("pf-team");
    if (!pfTeam) return;
    const result = await api("/TeamApi/DoiCuaToi");
    if (result.Success && result.Data) {
      pfTeam.value = result.Data.ten_doi;
    } else {
      pfTeam.value = "";
      pfTeam.placeholder = "Chưa tham gia đội nào";
    }
  }

  function getProfileValue(source, upperKey, lowerKey) {
    if (!source) return "";
    if (source[upperKey] !== undefined && source[upperKey] !== null)
      return source[upperKey];
    if (source[lowerKey] !== undefined && source[lowerKey] !== null)
      return source[lowerKey];
    return "";
  }

  updateAvatar = function (user) {
    if (!user) return;

    const initials = (
      getProfileValue(user, "TenDangNhap", "ten_dang_nhap") || "?"
    )
      .charAt(0)
      .toUpperCase();
    const topbarInitials = document.getElementById("avatar-initials");
    const profileUserName = document.getElementById("profile-username-display");
    const profileEmail = document.getElementById("profile-email-display");
    const bigInitials = document.getElementById("avatar-initials-big");
    const bigImg = document.getElementById("profile-avatar-img");
    const avatarBtn = document.getElementById("avatar-btn");
    const avatarUrl = getProfileValue(user, "AvatarUrl", "avatar_url");

    if (topbarInitials) {
      topbarInitials.textContent = initials;
    }
    if (profileUserName) {
      profileUserName.textContent =
        getProfileValue(user, "TenDangNhap", "ten_dang_nhap") || "";
    }
    if (profileEmail) {
      profileEmail.textContent = getProfileValue(user, "Email", "email") || "";
    }
    if (bigInitials) {
      bigInitials.textContent = initials;
    }

    if (bigImg) {
      if (avatarUrl) {
        bigImg.src = avatarUrl;
        bigImg.style.display = "block";
        if (bigInitials) bigInitials.style.display = "none";
      } else {
        bigImg.src = "";
        bigImg.style.display = "none";
        if (bigInitials) bigInitials.style.display = "";
      }
    }

    if (avatarBtn) {
      if (avatarUrl) {
        avatarBtn.style.backgroundImage = "url(" + avatarUrl + ")";
        avatarBtn.style.backgroundSize = "cover";
        avatarBtn.style.backgroundPosition = "center";
        avatarBtn.style.backgroundRepeat = "no-repeat";
        if (topbarInitials) topbarInitials.style.display = "none";
      } else {
        avatarBtn.style.backgroundImage = "";
        avatarBtn.style.backgroundSize = "";
        avatarBtn.style.backgroundPosition = "";
        avatarBtn.style.backgroundRepeat = "";
        if (topbarInitials) topbarInitials.style.display = "";
      }
    }

    const roleEl = document.getElementById("profile-role-display");
    const usernameInput = document.getElementById("pf-username");
    const emailInput = document.getElementById("pf-email");
    const bioInput = document.getElementById("pf-bio");

    if (roleEl)
      roleEl.textContent =
        getProfileValue(user, "VaiTroHeThong", "vai_tro_he_thong") || "";
    if (usernameInput)
      usernameInput.value =
        getProfileValue(user, "TenDangNhap", "ten_dang_nhap") || "";
    if (emailInput)
      emailInput.value = getProfileValue(user, "Email", "email") || "";
    if (bioInput) bioInput.value = getProfileValue(user, "Bio", "bio") || "";

    const btnAdmin = document.getElementById("btn-admin-module");
    if (btnAdmin) {
      const isAdmin =
        String(
          getProfileValue(user, "VaiTroHeThong", "vai_tro_he_thong") || "",
        ).toLowerCase() === "admin";
      btnAdmin.classList.toggle("d-none", !isAdmin);
    }

    loadMyTeamForProfile();
  };

  loadGameProfile = async function (game) {
    activeGameTab = game.ma_tro_choi;
    document
      .querySelectorAll(".game-tab-btn")
      .forEach((b) =>
        b.classList.toggle("active", b.textContent === game.ten_game),
      );

    const title = document.getElementById("game-profile-title");
    const body = document.getElementById("game-profile-body");
    if (title) {
      title.textContent = game.ten_game;
    }
    if (!body) return;

    const [profileResult, positionResult] = await Promise.all([
      api("/ProfileApi/LayHoSo?maTroChoi=" + game.ma_tro_choi),
      api("/ProfileApi/ViTri?maTroChoi=" + game.ma_tro_choi),
    ]);

    const existing =
      profileResult.Success && profileResult.Data ? profileResult.Data : null;
    const positions =
      positionResult.Success && Array.isArray(positionResult.Data)
        ? positionResult.Data
        : [];
    const existingPositionId = getProfileValue(
      existing,
      "MaViTriSoTruong",
      "ma_vi_tri_so_truong",
    );
    const existingGameId = getProfileValue(existing, "InGameId", "in_game_id");
    const existingGameName = getProfileValue(
      existing,
      "InGameName",
      "in_game_name",
    );

    const groups = {};
    positions.forEach((p) => {
      const grp = p.LoaiViTri || p.loai_vi_tri || "Khac";
      if (!groups[grp]) groups[grp] = [];
      groups[grp].push(p);
    });

    const labels = {
      ChuyenMon: "Chuyen mon thi dau",
      BanHuanLuyen: "Ban huan luyen",
    };

    let posOptions = '<option value="">-- Chon vi tri --</option>';
    Object.keys(groups).forEach((groupName) => {
      posOptions +=
        '<optgroup label="' + (labels[groupName] || groupName) + '">';
      groups[groupName].forEach((p) => {
        const maViTri = p.MaViTri || p.ma_vi_tri;
        const tenViTri = p.TenViTri || p.ten_vi_tri || "";
        const kyHieu = p.KyHieu || p.ky_hieu || "";
        const selected =
          String(existingPositionId) === String(maViTri) ? " selected" : "";
        posOptions +=
          '<option value="' +
          maViTri +
          '"' +
          selected +
          ">" +
          tenViTri +
          (kyHieu ? " [" + kyHieu + "]" : "") +
          "</option>";
      });
      posOptions += "</optgroup>";
    });

    if (!positions.length) {
      posOptions = '<option value="">-- Chua co vi tri --</option>';
    }

    body.innerHTML =
      '<div class="form-group-dark"><label>ID trong game</label><input class="form-control-dark" id="gp-id" value="' +
      existingGameId +
      '" placeholder="ID trong game" /></div>' +
      '<div class="form-group-dark"><label>Ten hien thi trong game</label><input class="form-control-dark" id="gp-name" value="' +
      existingGameName +
      '" placeholder="Nickname" /></div>' +
      '<div class="form-group-dark"><label>Vi tri so truong</label><select class="select-dark" id="gp-vitri"' +
      (positions.length ? "" : " disabled") +
      ">" +
      posOptions +
      "</select></div>" +
      '<button class="btn-save" onclick="saveGameProfile(' +
      game.ma_tro_choi +
      ')">Luu ho so</button>' +
      '<span id="gp-msg" style="margin-left:12px;font-size:.85rem"></span>';
  };

  window.saveGameProfile = async function (maTroChoi) {
    const msg = document.getElementById("gp-msg");
    const maViTri = Number(document.getElementById("gp-vitri").value) || 0;

    if (!maViTri) {
      if (msg) {
        msg.style.color = "#ff4757";
        msg.textContent = "Vui long chon vi tri so truong.";
      }
      return;
    }

    const result = await api("/ProfileApi/TaoHoSo", "POST", {
      MaTroChoi: maTroChoi,
      InGameId: document.getElementById("gp-id").value.trim(),
      InGameName: document.getElementById("gp-name").value.trim(),
      MaViTriSoTruong: maViTri,
    });

    if (msg) {
      msg.style.color = result.Success ? "#2ed573" : "#ff4757";
      msg.textContent = result.Success
        ? "Da luu ho so thanh cong."
        : result.Message || "Loi luu ho so.";
    }
  };

  (function setupAvatarUploadOverride() {
    const oldInput = document.getElementById("avatar-file-input");
    if (!oldInput) return;

    const newInput = oldInput.cloneNode(true);
    oldInput.parentNode.replaceChild(newInput, oldInput);

    window.triggerAvatarUpload = function () {
      newInput.click();
    };

    newInput.addEventListener("change", async function () {
      if (!this.files || !this.files.length) return;

      const file = this.files[0];
      const uploadMsg = document.getElementById("pf-msg");
      const formData = new FormData();
      formData.append("avatar", file);

      if (uploadMsg) {
        uploadMsg.style.color = "#aaa";
        uploadMsg.textContent = "Dang tai anh...";
      }

      try {
        const res = await fetch("/UploadApi/UploadAvatar", {
          method: "POST",
          body: formData,
        });
        const result = await res.json();

        if (result.Success && result.Data && result.Data.AvatarUrl) {
          const url = result.Data.AvatarUrl + "?v=" + Date.now();
          if (currentUser) {
            currentUser.AvatarUrl = url;
            currentUser.avatar_url = url;
          }
          updateAvatar(currentUser || { AvatarUrl: url });
          if (uploadMsg) {
            uploadMsg.style.color = "#2ed573";
            uploadMsg.textContent = "Cap nhat anh dai dien thanh cong.";
          }
        } else if (uploadMsg) {
          uploadMsg.style.color = "#ff4757";
          uploadMsg.textContent = result.Message || "Upload that bai.";
        }
      } catch (e) {
        if (uploadMsg) {
          uploadMsg.style.color = "#ff4757";
          uploadMsg.textContent = "Khong the tai anh len luc nay.";
        }
      }

      this.value = "";
    });
  })();

  function formatTournamentStatus(status) {
    const map = {
      ban_nhap: "Ban nhap",
      cho_phe_duyet: "Cho phe duyet",
      mo_dang_ky: "Mo dang ky",
      sap_dien_ra: "Sap dien ra",
      dang_dien_ra: "Dang dien ra",
      ket_thuc: "Ket thuc",
      khoa: "Khoa",
    };
    return map[status] || status || "";
  }

  window.navigateTo = function (page) {
    currentPage = page;
    closeHamburger();
    document
      .querySelectorAll(".page-section")
      .forEach((p) => (p.style.display = "none"));
    document
      .querySelectorAll(".sidebar-item")
      .forEach((b) => b.classList.remove("active"));
    const activeBtn = document.querySelector('[data-page="' + page + '"]');
    if (activeBtn) activeBtn.classList.add("active");

    const gameMatch = GAMES.find((g) => g.id === page);
    if (gameMatch) {
      showGamePage(gameMatch);
      return;
    }

    const pageMap = {
      home: "page-home",
      follow: "page-follow",
      notifications: "page-notifications",
      "my-tournaments": "page-my-tournaments",
      "my-teams": "page-my-teams",
      organize: "page-organize",
      "create-team": "page-create-team",
      profile: "page-profile",
      "player-profile": "page-player-profile",
      "admin-requests": "page-admin-requests",
      "manage-tournament": "page-manage-tournament",
      "team-explorer": "page-team-explorer",
      "team-detail": "page-team-detail",
      follow: "page-follow",
    };
    const target = pageMap[page];
    if (!target) return;

    const el = document.getElementById(target);
    if (el) el.style.display = "";
    if (page === "home") loadHomePage();
    if (page === "follow") loadFollowedTournaments();
    if (page === "notifications") loadNotifications();
    if (page === "player-profile") loadPlayerProfileTabs();
    if (page === "my-tournaments") loadMyTournaments();
    if (page === "my-teams") loadMyTeams();
    if (page === "admin-requests") loadAdminRequests();
    if (page === "team-explorer") loadTeamExplorer();
  };

  loadPlayerProfileTabs = function () {
    const tabs = document.getElementById("game-profile-tabs");
    if (!tabs) return;
    tabs.innerHTML = "";
    const games = gameList.length
      ? gameList
      : GAME_NAMES.map((n, i) => ({ ma_tro_choi: i + 1, ten_game: n }));
    games.forEach((g) => {
      const btn = document.createElement("button");
      btn.className =
        "game-tab-btn" + (activeGameTab === g.ma_tro_choi ? " active" : "");
      btn.textContent = g.ten_game;
      btn.onclick = () => loadGameProfile(g);
      tabs.appendChild(btn);
    });

    if (games.length && !activeGameTab) {
      loadGameProfile(games[0]);
    }
  };

  loadMyTournaments = async function () {
    const grid = document.getElementById("my-tournaments-grid");
    if (!grid || !currentUser) return;
    grid.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Dang tai...</div>';

    const result = await api("/TournamentBuilderApi/GiaiCuaToi");
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      grid.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">T</div><h4>Chua co giai dau nao</h4><p>Ban chua tao hoac tham gia giai dau nao.</p></div>';
      return;
    }

    grid.innerHTML = result.Data.map(function (t) {
      const gameName = t.ten_game || "Giai hon hop";
      const prize = t.tong_giai_thuong
        ? Number(t.tong_giai_thuong).toLocaleString("vi-VN") + " VND"
        : "N/A";
      const ownerActions = t.is_owner
        ? '<div class="tc-actions" style="margin-top:12px">' +
          '<button class="btn-outline-glow" onclick="openManageTournament(' +
          t.ma_giai_dau +
          ", '" +
          String(t.ten_giai_dau || "").replace(/'/g, "\\'") +
          "')\">Duyet doi</button>" +
          (t.trang_thai === "ban_nhap"
            ? '<button class="btn-primary-glow" onclick="submitTournamentApproval(' +
              t.ma_giai_dau +
              ')">Gui admin</button>'
            : "") +
          "</div>"
        : '<div style="margin-top:12px;color:var(--text-muted);font-size:.85rem">Ban dang tham gia giai nay.</div>';

      return (
        '<div class="tournament-card">' +
        '<div class="tc-banner" onclick="openTournament(' +
        t.ma_giai_dau +
        ')" style="cursor:pointer;background:linear-gradient(135deg,#1a1f36,#18a0fb33)"><span style="font-size:2.5rem">T</span></div>' +
        '<div class="tc-body">' +
        '<div class="tc-game-badge">' +
        gameName +
        "</div>" +
        '<div class="tc-name" onclick="openTournament(' +
        t.ma_giai_dau +
        ')" style="cursor:pointer">' +
        t.ten_giai_dau +
        "</div>" +
        '<div class="tc-meta"><span class="tc-status">' +
        formatTournamentStatus(t.trang_thai) +
        "</span><span>" +
        prize +
        "</span></div>" +
        ownerActions +
        "</div>" +
        "</div>"
      );
    }).join("");
  };

  window.submitTournamentApproval = async function (maGiaiDau) {
    const result = await api(
      "/TournamentBuilderApi/GuiXetDuyet?maGiaiDau=" + maGiaiDau,
      "POST",
      {},
    );
    showToast(
      result.Success
        ? "Da gui yeu cau len admin."
        : result.Message || "Khong the gui yeu cau.",
    );
    if (result.Success) loadMyTournaments();
  };

  window.openManageTournament = async function (maGiaiDau, tenGiaiDau) {
    navigateTo("manage-tournament");
    const sub = document.getElementById("manage-tournament-sub");
    const list = document.getElementById("manage-tournament-list");
    if (sub)
      sub.textContent =
        "Duyet doi tham gia - " + (tenGiaiDau || "Giai #" + maGiaiDau);
    if (!list) return;
    list.innerHTML =
      '<div class="profile-card">Dang tai danh sach dang ky...</div>';

    const result = await api(
      "/TournamentBuilderApi/DanhSachDangKyDoi?maGiaiDau=" + maGiaiDau,
    );
    if (!result.Success || !Array.isArray(result.Data)) {
      list.innerHTML =
        '<div class="profile-card">' +
        (result.Message || "Khong the tai danh sach dang ky doi.") +
        "</div>";
      return;
    }

    if (!result.Data.length) {
      list.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">N</div><h4>Chua co doi dang ky</h4></div>';
      return;
    }

    list.innerHTML = result.Data.map(function (item) {
      const canReview = item.trang_thai_duyet === "cho_duyet";
      return (
        '<div class="profile-card" style="margin-bottom:16px">' +
        '<div style="display:flex;justify-content:space-between;gap:12px;flex-wrap:wrap;align-items:flex-start">' +
        "<div>" +
        '<h5 style="margin-bottom:6px">' +
        item.ten_doi +
        " - " +
        item.ten_nhom +
        "</h5>" +
        '<div style="color:var(--text-muted);font-size:.9rem">' +
        (item.ten_game || "") +
        "</div>" +
        '<div style="color:var(--text-muted);font-size:.9rem;margin-top:4px">' +
        (item.slogan || "") +
        "</div>" +
        "</div>" +
        '<div style="text-align:right">' +
        '<div class="tc-status" style="display:inline-flex">' +
        item.trang_thai_duyet +
        "</div>" +
        (canReview
          ? '<div style="display:flex;gap:8px;margin-top:12px;justify-content:flex-end">' +
            '<button class="btn-primary-glow" onclick="reviewTeamRegistration(' +
            maGiaiDau +
            "," +
            item.ma_nhom +
            ',true)">Duyet</button>' +
            '<button class="btn-outline-glow" onclick="reviewTeamRegistration(' +
            maGiaiDau +
            "," +
            item.ma_nhom +
            ',false)">Tu choi</button>' +
            "</div>"
          : "") +
        "</div>" +
        "</div>" +
        "</div>"
      );
    }).join("");
  };

  window.reviewTeamRegistration = async function (maGiaiDau, maNhom, chapNhan) {
    const result = await api("/TournamentBuilderApi/DuyetDangKyDoi", "POST", {
      MaGiaiDau: maGiaiDau,
      MaNhom: maNhom,
      ChapNhan: !!chapNhan,
    });
    showToast(
      result.Success
        ? chapNhan
          ? "Da duyet doi tham gia."
          : "Da tu choi doi tham gia."
        : result.Message || "Khong the cap nhat dang ky.",
    );
    if (result.Success) openManageTournament(maGiaiDau, "");
  };

  async function loadAdminRequests() {
    const wrap = document.getElementById("admin-request-list");
    if (!wrap) return;
    wrap.innerHTML =
      '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">!</div><h4>Đang tải yêu cầu...</h4></div>';

    const result = await api("/AdminApi/Dashboard");
    const requests =
      result.Success && result.Data && result.Data.ActionRequired
        ? result.Data.ActionRequired.GiaiChoXetDuyet
        : [];
    if (!Array.isArray(requests) || !requests.length) {
      wrap.innerHTML =
        '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">OK</div><h4>Không có yêu cầu nào</h4></div>';
      return;
    }

    // Check for expired requests
    const now = new Date();
    const expiredRequests = requests.filter(function (item) {
      const ngayBatDau = item.ngay_bat_dau ? new Date(item.ngay_bat_dau) : null;
      return ngayBatDau && ngayBatDau < now;
    });

    let html = "";
    if (expiredRequests.length > 0) {
      html +=
        '<div class="profile-card" style="margin-bottom:16px;border-left:4px solid #ff4757">' +
        '<h5 style="color:#ff4757;margin-bottom:8px">⚠️ Cảnh báo: ' +
        expiredRequests.length +
        " yêu cầu đã quá hạn (ngày bắt đầu đã qua)</h5>" +
        '<p style="font-size:0.9rem;color:var(--text-muted)">Các giải đấu này nên được hủy hoặc xử lý ngay lập tức.</p>' +
        "</div>";
    }

    // Bulk approve button
    html +=
      '<div style="margin-bottom:16px">' +
      '<button class="btn-primary-glow" onclick="bulkApproveRequests()">✅ Duyệt tất cả (' +
      requests.length +
      ")</button>" +
      "</div>";

    html += requests
      .map(function (item) {
        const ma = item.ma_giai_dau;
        const ten = item.ten_giai_dau || "Giai #" + ma;
        const ngayBatDau = item.ngay_bat_dau
          ? new Date(item.ngay_bat_dau)
          : null;
        const isExpired = ngayBatDau && ngayBatDau < now;
        const expiredBadge = isExpired
          ? '<span style="background:#ff4757;color:white;padding:2px 8px;border-radius:4px;font-size:0.75rem;margin-left:8px">QUÁ HẠN</span>'
          : "";

        return (
          '<div class="profile-card" style="margin-bottom:16px;' +
          (isExpired ? "border:1px solid #ff4757" : "") +
          '">' +
          '<div style="display:flex;justify-content:space-between;gap:12px;flex-wrap:wrap">' +
          "<div>" +
          '<h5 style="margin-bottom:6px">' +
          ten +
          expiredBadge +
          "</h5>" +
          '<div style="color:var(--text-muted);font-size:.9rem">Người tạo: ' +
          (item.ten_nguoi_tao || "Không rõ") +
          "</div>" +
          '<div style="color:var(--text-muted);font-size:.9rem">Game: ' +
          (item.ten_game || "Không rõ") +
          "</div>" +
          (ngayBatDau
            ? '<div style="color:var(--text-muted);font-size:.9rem">Ngày bắt đầu: ' +
              ngayBatDau.toLocaleDateString("vi-VN") +
              " " +
              ngayBatDau.toLocaleTimeString("vi-VN") +
              "</div>"
            : "") +
          "</div>" +
          '<div style="display:flex;gap:8px;align-items:center">' +
          '<button class="btn-primary-glow" onclick="approveTournamentRequest(' +
          ma +
          ')">Duyệt</button>' +
          '<button class="btn-outline-glow" onclick="rejectTournamentRequest(' +
          ma +
          ')">Hủy</button>' +
          "</div>" +
          "</div>" +
          "</div>"
        );
      })
      .join("");

    wrap.innerHTML = html;
  }

  window.approveTournamentRequest = async function (maGiaiDau) {
    const result = await api(
      "/TournamentBuilderApi/PheDuyet?maGiaiDau=" + maGiaiDau,
      "POST",
      {},
    );
    showToast(
      result.Success
        ? "Admin đã phê duyệt giải đấu."
        : result.Message || "Không thể phê duyệt.",
    );
    if (result.Success) loadAdminRequests();
  };

  window.rejectTournamentRequest = async function (maGiaiDau) {
    const result = await api(
      "/TournamentBuilderApi/TuChoi?maGiaiDau=" + maGiaiDau,
      "POST",
      {},
    );
    showToast(
      result.Success
        ? "Admin đã từ chối yêu cầu tạo giải."
        : result.Message || "Không thể từ chối.",
    );
    if (result.Success) loadAdminRequests();
  };

  window.bulkApproveRequests = async function () {
    if (!confirm("Bạn có chắc muốn duyệt tất cả các yêu cầu chờ duyệt?"))
      return;

    const result = await api("/TournamentBuilderApi/BulkPheDuyet", "POST", {});

    if (result.Success) {
      showToast(
        "Đã duyệt " +
          (result.Data?.approvedCount || 0) +
          " giải đấu thành công.",
        "success",
      );
      loadAdminRequests();
    } else {
      showToast(result.Message || "Không thể duyệt hàng loạt.", "error");
    }
  };

  // ============================================================
  // MODULE 5: Notifications, Follow, Team Explorer, Team Detail, Search
  // ============================================================

  // ---- NOTIFICATIONS (real implementation) ----
  loadNotifications = async function () {
    const list = document.getElementById("notif-list");
    if (!list || !currentUser) return;
    list.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Đang tải thông báo...</div>';

    const result = await api("/TeamApi/ThongBao");
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      list.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">🔔</div><h4>Không có thông báo nào</h4><p>Bạn sẽ nhận thông báo khi có cập nhật mới.</p></div>';
      updateNotifBadge(0);
      return;
    }

    const unreadCount = result.Data.filter((n) => !n.da_doc).length;
    updateNotifBadge(unreadCount);

    list.innerHTML = result.Data.map(function (n) {
      const unread = !n.da_doc ? " unread" : "";
      const icon =
        n.loai === "loi_moi"
          ? "📩"
          : n.loai === "xin_gia_nhap"
            ? "🙋"
            : n.loai === "duyet"
              ? "✅"
              : "🔔";
      const time = n.ngay_tao
        ? new Date(n.ngay_tao).toLocaleString("vi-VN")
        : "";
      return (
        '<div class="notif-item' +
        unread +
        '" onclick="markNotifRead(' +
        n.ma_thong_bao +
        ', this)">' +
        '<div class="notif-icon">' +
        icon +
        "</div>" +
        '<div class="notif-body">' +
        '<div class="notif-title">' +
        (n.tieu_de || "Thông báo") +
        "</div>" +
        '<div class="notif-desc">' +
        (n.noi_dung || "") +
        "</div>" +
        '<div class="notif-time">' +
        time +
        "</div>" +
        "</div>" +
        "</div>"
      );
    }).join("");
  };

  function updateNotifBadge(count) {
    const badge = document.getElementById("sidebar-notif-count");
    const dot = document.getElementById("notif-dot");
    if (badge) {
      badge.textContent = count;
      badge.style.display = count > 0 ? "" : "none";
    }
    if (dot) dot.style.display = count > 0 ? "" : "none";
  }

  window.markNotifRead = async function (maThongBao, el) {
    if (el) el.classList.remove("unread");
    await api("/TeamApi/DanhDauDaDoc", "POST", { maThongBao: maThongBao });
  };

  window.markAllRead = async function () {
    const result = await api("/TeamApi/DanhDauTatCaDaDoc", "POST", {});
    if (result.Success) {
      document
        .querySelectorAll(".notif-item.unread")
        .forEach((el) => el.classList.remove("unread"));
      updateNotifBadge(0);
      showToast("Đã đánh dấu tất cả đã đọc.");
    }
  };

  // ---- FOLLOWED TOURNAMENTS ----
  async function loadFollowedTournaments() {
    const grid = document.getElementById("follow-grid");
    if (!grid || !currentUser) return;
    grid.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';

    const result = await api("/TeamApi/GiaiDangTheoDoi");
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      grid.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">❤️</div><h4>Chưa theo dõi giải nào</h4><p>Hãy theo dõi giải đấu bạn yêu thích để nhận thông báo.</p></div>';
      return;
    }

    grid.innerHTML = result.Data.map((t) =>
      buildTournamentCard(t, { dang_theo_doi: true }),
    ).join("");
    loadInteractionStates(result.Data.map((t) => t.ma_giai_dau));
  }

  // ---- MY TOURNAMENTS TABS (created / joined) ----
  let myTournamentTab = "created";

  window.switchMyTournamentTab = function (tab) {
    myTournamentTab = tab;
    document
      .getElementById("mt-tab-created")
      .classList.toggle("active", tab === "created");
    document
      .getElementById("mt-tab-joined")
      .classList.toggle("active", tab === "joined");
    loadMyTournaments();
  };

  // Override loadMyTournaments to support tabs
  loadMyTournaments = async function () {
    const grid = document.getElementById("my-tournaments-grid");
    if (!grid || !currentUser) return;
    grid.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';

    if (myTournamentTab === "joined") {
      const result = await api("/TeamApi/GiaiDaThamGia");
      if (
        !result.Success ||
        !Array.isArray(result.Data) ||
        result.Data.length === 0
      ) {
        grid.innerHTML =
          '<div class="empty-state"><div class="empty-state-icon">🏆</div><h4>Chưa tham gia giải nào</h4><p>Hãy đăng ký đội của bạn vào giải đấu!</p></div>';
        return;
      }
      grid.innerHTML = result.Data.map(function (t) {
        const gameName = t.ten_game || "Hỗn hợp";
        const prize = t.tong_giai_thuong
          ? Number(t.tong_giai_thuong).toLocaleString("vi-VN") + "₫"
          : "N/A";
        return (
          '<div class="tournament-card" onclick="openTournament(' +
          t.ma_giai_dau +
          ')">' +
          '<div class="tc-banner" style="cursor:pointer;background:linear-gradient(135deg,#1a1f36,#2ed57333)"><span style="font-size:2.5rem">🏆</span></div>' +
          '<div class="tc-body">' +
          '<div class="tc-game-badge">' +
          gameName +
          "</div>" +
          '<div class="tc-name">' +
          (t.ten_giai_dau || "Giải đấu") +
          "</div>" +
          '<div class="tc-meta"><span class="tc-status">' +
          formatTournamentStatus(t.trang_thai) +
          "</span><span>💰 " +
          prize +
          "</span></div>" +
          "</div></div>"
        );
      }).join("");
      return;
    }

    // Default: created tab
    const result = await api("/TournamentBuilderApi/GiaiCuaToi");
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      grid.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">🏆</div><h4>Chưa tạo giải nào</h4><p>Hãy tổ chức giải đầu tiên!</p></div>';
      return;
    }

    myTournamentsData = result.Data;
    grid.innerHTML = result.Data.map(function (t) {
      const gameName = t.ten_game || "Hỗn hợp";
      const prize = t.tong_giai_thuong
        ? Number(t.tong_giai_thuong).toLocaleString("vi-VN") + "₫"
        : "N/A";
      return (
        '<div class="tournament-card">' +
        '<div class="tc-banner" onclick="openTournament(' +
        t.ma_giai_dau +
        ')" style="cursor:pointer;background:linear-gradient(135deg,#1a1f36,#6c63ff33)"><span style="font-size:2.5rem">🏆</span></div>' +
        '<div class="tc-body">' +
        '<div class="tc-game-badge">' +
        gameName +
        "</div>" +
        '<div class="tc-name" onclick="openTournament(' +
        t.ma_giai_dau +
        ')" style="cursor:pointer">' +
        (t.ten_giai_dau || "Giải đấu") +
        "</div>" +
        '<div class="tc-meta"><span class="tc-status">' +
        formatTournamentStatus(t.trang_thai) +
        "</span><span>💰 " +
        prize +
        "</span></div>" +
        '<div style="margin-top:12px;display:flex;gap:8px">' +
        '<button class="btn-primary-glow" style="padding:6px 14px;font-size:.82rem;flex:1" onclick="event.stopPropagation();manageTournament(' +
        t.ma_giai_dau +
        ')">⚙️ Quản lý</button>' +
        (t.trang_thai === "ban_nhap"
          ? '<button class="btn-outline-glow" style="padding:6px 14px;font-size:.82rem;flex:1" onclick="event.stopPropagation();submitTournamentApproval(' +
            t.ma_giai_dau +
            ')">📤 Gửi duyệt</button>'
          : "") +
        "</div>" +
        "</div></div>"
      );
    }).join("");
  };

  // ---- TEAM EXPLORER ----
  async function loadTeamExplorer() {
    const grid = document.getElementById("team-explorer-grid");
    if (!grid) return;
    grid.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Đang tải danh sách đội...</div>';

    // Populate game filter if empty
    const gameFilter = document.getElementById("te-game-filter");
    if (gameFilter && gameFilter.options.length <= 1 && gameList.length) {
      gameList.forEach((g) => {
        const opt = document.createElement("option");
        opt.value = g.ma_tro_choi;
        opt.textContent = g.ten_game;
        gameFilter.appendChild(opt);
      });
    }

    const maTroChoi = document.getElementById("te-game-filter")
      ? document.getElementById("te-game-filter").value
      : "";
    const dangTuyen = document.getElementById("te-recruit-filter")
      ? document.getElementById("te-recruit-filter").checked
      : false;
    const tuKhoa = document.getElementById("te-search")
      ? document.getElementById("te-search").value.trim()
      : "";

    let url = "/TeamApi/DanhSachDoiCongKhai?";
    if (maTroChoi) url += "maTroChoi=" + maTroChoi + "&";
    if (dangTuyen) url += "dangTuyen=true&";
    if (tuKhoa) url += "tuKhoa=" + encodeURIComponent(tuKhoa) + "&";

    const result = await api(url);
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      grid.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">🔍</div><h4>Không tìm thấy đội nào</h4><p>Thử thay đổi bộ lọc hoặc tạo đội mới!</p></div>';
      return;
    }

    grid.innerHTML = result.Data.map(function (team) {
      const logoLetter = (team.ten_doi || "?").charAt(0).toUpperCase();
      const recruiting = team.dang_tuyen
        ? '<span class="badge-recruit">🟢 Đang tuyển</span>'
        : "";
      const gameBadge = team.ten_game
        ? '<span class="badge-game">' + team.ten_game + "</span>"
        : "";
      return (
        '<div class="team-card" onclick="openTeamDetail(' +
        team.ma_doi +
        ')">' +
        '<div class="team-card-header">' +
        '<div class="team-card-logo">' +
        (team.logo_url
          ? '<img src="' + team.logo_url + '" alt="">'
          : logoLetter) +
        "</div>" +
        "<div>" +
        '<div class="team-card-name">' +
        (team.ten_doi || "Đội") +
        "</div>" +
        '<div class="team-card-slogan">' +
        (team.slogan || "") +
        "</div>" +
        "</div>" +
        "</div>" +
        '<div class="team-card-meta">' +
        "<span>👥 " +
        (team.so_thanh_vien || 0) +
        " thành viên</span>" +
        "<span>📋 " +
        (team.so_nhom || 0) +
        " nhóm</span>" +
        "</div>" +
        '<div class="team-card-badges">' +
        recruiting +
        gameBadge +
        "</div>" +
        "</div>"
      );
    }).join("");
  }

  // ---- TEAM DETAIL ----
  let currentTeamDetailId = 0;

  window.openTeamDetail = function (maDoi) {
    currentTeamDetailId = maDoi;
    navigateTo("team-detail");
    loadTeamDetail(maDoi);
  };

  async function loadTeamDetail(maDoi) {
    const header = document.getElementById("team-detail-header");
    const tabsEl = document.getElementById("team-squad-tabs");
    const content = document.getElementById("team-squad-content");
    if (!header) return;

    header.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';
    if (tabsEl) tabsEl.innerHTML = "";
    if (content) content.innerHTML = "";

    const result = await api("/TeamApi/ChiTietDoi?maDoi=" + maDoi);
    if (!result.Success || !result.Data) {
      header.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">❌</div><h4>Không tìm thấy đội</h4></div>';
      return;
    }

    const team = result.Data;
    const logoLetter = (team.ten_doi || "?").charAt(0).toUpperCase();
    const isChuTich = team.vai_tro_hien_tai === "chu_tich";
    const isBanDieuHanh = team.vai_tro_hien_tai === "ban_dieu_hanh";
    const canManage = isChuTich || isBanDieuHanh;

    header.innerHTML =
      '<div class="team-detail-banner">' +
      '<div class="team-detail-logo">' +
      (team.logo_url
        ? '<img src="' + team.logo_url + '" alt="">'
        : logoLetter) +
      "</div>" +
      '<div class="team-detail-info">' +
      "<h2>" +
      (team.ten_doi || "Đội") +
      "</h2>" +
      '<div class="team-slogan">' +
      (team.slogan || "") +
      "</div>" +
      '<div class="team-card-meta" style="margin-bottom:8px">' +
      "<span>👥 " +
      (team.so_thanh_vien || 0) +
      " thành viên</span>" +
      (team.dang_tuyen
        ? '<span class="badge-recruit">🟢 Đang tuyển</span>'
        : '<span style="color:var(--text-muted)">🔴 Không tuyển</span>') +
      "</div>" +
      '<div class="team-detail-actions">' +
      (isChuTich || isBanDieuHanh
        ? '<button class="btn-outline-glow" style="padding:6px 14px;font-size:.82rem" onclick="toggleTeamRecruit(' +
          maDoi +
          "," +
          !team.dang_tuyen +
          ')">' +
          (team.dang_tuyen ? "🔴 Tắt tuyển dụng" : "🟢 Bật tuyển dụng") +
          "</button>"
        : "") +
      (isChuTich || isBanDieuHanh
        ? '<button class="btn-primary-glow" style="padding:6px 14px;font-size:.82rem;margin-left:8px" onclick="toggleAddSquadForm(' +
          maDoi +
          ')">➕ Thêm nhóm</button>'
        : "") +
      (!team.vai_tro_hien_tai && team.dang_tuyen
        ? '<button class="btn-primary-glow" style="padding:6px 14px;font-size:.82rem" onclick="requestJoinTeam(' +
          maDoi +
          ')">📩 Xin gia nhập</button>'
        : "") +
      "</div>" +
      "</div>" +
      "</div>";

    // Render add-squad form (hidden by default, only for Chủ tịch/Ban điều hành)
    const oldForm = document.getElementById("add-squad-form");
    if (oldForm) oldForm.remove();
    let addSquadHtml = "";
    if (isChuTich || isBanDieuHanh) {
      let gameOpts = '<option value="">-- Chọn game --</option>';
      gameList.forEach(function (g) {
        gameOpts +=
          '<option value="' + g.ma_tro_choi + '">' + g.ten_game + "</option>";
      });
      addSquadHtml =
        '<div id="add-squad-form" style="display:none;margin-top:12px" class="profile-card">' +
        "<h5>Thêm nhóm thi đấu mới</h5>" +
        '<div style="display:grid;grid-template-columns:1fr 1fr auto;gap:12px;align-items:end">' +
        '<div class="form-group-dark" style="margin-bottom:0">' +
        "<label>Game</label>" +
        '<select class="select-dark" id="add-squad-game">' +
        gameOpts +
        "</select>" +
        "</div>" +
        '<div class="form-group-dark" style="margin-bottom:0">' +
        "<label>Tên nhóm</label>" +
        '<input class="form-control-dark" id="add-squad-name" placeholder="VD: Team Alpha LoL" />' +
        "</div>" +
        '<button class="btn-save" style="height:38px" onclick="submitAddSquad(' +
        maDoi +
        ')">Tạo nhóm</button>' +
        "</div>" +
        '<span id="add-squad-msg" style="display:block;margin-top:8px;font-size:.85rem"></span>' +
        "</div>";
    }
    header.insertAdjacentHTML("afterend", addSquadHtml);

    // Render squad tabs
    const squads = Array.isArray(team.nhom_doi) ? team.nhom_doi : [];
    if (squads.length && tabsEl) {
      tabsEl.innerHTML = squads
        .map(function (sq, idx) {
          return (
            '<button class="tab-btn' +
            (idx === 0 ? " active" : "") +
            '" onclick="loadSquadMembers(' +
            sq.ma_nhom +
            ', this)">' +
            (sq.ten_nhom || "Nhóm") +
            " (" +
            (sq.ten_game || "") +
            ")</button>"
          );
        })
        .join("");
      // Auto-load first squad
      if (squads[0]) loadSquadMembers(squads[0].ma_nhom, null);
    } else if (tabsEl) {
      tabsEl.innerHTML = "";
      if (content)
        content.innerHTML =
          '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">📋</div><h4>Chưa có nhóm thi đấu</h4></div>';
    }
  }

  async function loadSquadMembers(maNhom, btnEl) {
    if (btnEl) {
      document
        .querySelectorAll("#team-squad-tabs .tab-btn")
        .forEach((b) => b.classList.remove("active"));
      btnEl.classList.add("active");
    }
    const content = document.getElementById("team-squad-content");
    if (!content) return;
    content.innerHTML =
      '<div style="color:var(--text-muted);padding:16px">Đang tải thành viên...</div>';

    const result = await api("/TeamApi/ThanhVienNhom?maNhom=" + maNhom);
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      content.innerHTML =
        '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">👥</div><h4>Chưa có thành viên</h4></div>';
      return;
    }

    const roleLabels = {
      chu_tich: "Chủ tịch",
      ban_dieu_hanh: "Ban điều hành",
      doi_truong: "Đội trưởng",
      thanh_vien: "Thành viên",
    };
    const roleCls = {
      chu_tich: "role-chu-tich",
      ban_dieu_hanh: "role-ban-dieu-hanh",
      doi_truong: "role-doi-truong",
      thanh_vien: "role-thanh-vien",
    };

    // Check if current user has management rights
    const hasJoinRequests =
      currentUser &&
      result.Data.some(
        (m) => m.vai_tro === "chu_tich" || m.vai_tro === "doi_truong",
      );

    let html = '<div class="member-list">';
    html += result.Data.map(function (m) {
      const initials = (m.ten_dang_nhap || "?").charAt(0).toUpperCase();
      const role = m.vai_tro || "thanh_vien";
      return (
        '<div class="member-item">' +
        '<div class="member-avatar">' +
        (m.avatar_url ? '<img src="' + m.avatar_url + '" alt="">' : initials) +
        "</div>" +
        '<div class="member-info">' +
        '<div class="member-name">' +
        (m.ten_dang_nhap || "Người chơi") +
        "</div>" +
        '<div class="member-meta">' +
        (m.in_game_name || "") +
        (m.ten_vi_tri ? " · " + m.ten_vi_tri : "") +
        "</div>" +
        "</div>" +
        '<span class="role-badge ' +
        (roleCls[role] || "role-member") +
        '">' +
        (roleLabels[role] || role) +
        "</span>" +
        "</div>"
      );
    }).join("");
    html += "</div>";

    // Join requests section (only for Chủ tịch/Đội trưởng)
    if (hasJoinRequests) {
      html +=
        '<div style="margin-top:24px"><div class="section-title">📩 Đơn xin gia nhập</div><div id="join-requests-' +
        maNhom +
        '">Đang tải...</div></div>';
    }

    content.innerHTML = html;

    if (hasJoinRequests) {
      loadJoinRequests(maNhom);
    }
  }

  async function loadJoinRequests(maNhom) {
    const wrap = document.getElementById("join-requests-" + maNhom);
    if (!wrap) return;

    const result = await api("/TeamApi/DanhSachXinGiaNhap?maNhom=" + maNhom);
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      wrap.innerHTML =
        '<div style="color:var(--text-muted);font-size:.88rem;padding:8px 0">Không có đơn xin gia nhập nào.</div>';
      return;
    }

    wrap.innerHTML = result.Data.map(function (req) {
      const initials = (req.ten_dang_nhap || "?").charAt(0).toUpperCase();
      return (
        '<div class="join-request-item">' +
        '<div class="member-avatar">' +
        initials +
        "</div>" +
        '<div class="jr-info">' +
        '<div class="jr-name">' +
        (req.ten_dang_nhap || "Người chơi") +
        "</div>" +
        '<div class="jr-meta">' +
        (req.ngay_gui
          ? new Date(req.ngay_gui).toLocaleDateString("vi-VN")
          : "") +
        "</div>" +
        "</div>" +
        '<div class="jr-actions">' +
        '<button class="btn-sm-accept" onclick="reviewJoinRequest(' +
        req.ma_don_xin +
        ", true, " +
        maNhom +
        ')">Duyệt</button>' +
        '<button class="btn-sm-reject" onclick="reviewJoinRequest(' +
        req.ma_don_xin +
        ", false, " +
        maNhom +
        ')">Từ chối</button>' +
        "</div>" +
        "</div>"
      );
    }).join("");
  }

  window.reviewJoinRequest = async function (maDonXin, chapNhan, maNhom) {
    const result = await api("/TeamApi/DuyetXinGiaNhap", "POST", {
      maDonXin: maDonXin,
      chapNhan: chapNhan,
    });
    showToast(
      result.Success
        ? chapNhan
          ? "Đã duyệt đơn xin gia nhập."
          : "Đã từ chối đơn xin."
        : result.Message || "Lỗi xử lý đơn.",
    );
    if (result.Success) loadJoinRequests(maNhom);
  };

  window.requestJoinTeam = async function (maDoi) {
    // Show modal to select squad based on user's game profiles
    const result = await api("/TeamApi/ChiTietDoi?maDoi=" + maDoi);
    if (!result.Success || !result.Data) {
      showToast("Không thể tải thông tin đội.", "error");
      return;
    }

    const team = result.Data;
    const profilesResult = await api("/ProfileApi/LayTatCaHoSo");
    if (!profilesResult.Success || !Array.isArray(profilesResult.Data)) {
      showToast("Không thể tải hồ sơ thi đấu của bạn.", "error");
      return;
    }

    const userProfiles = profilesResult.Data;
    const teamSquads = Array.isArray(team.nhom_doi) ? team.nhom_doi : [];

    // Filter squads based on user's game profiles
    const matchingSquads = teamSquads.filter(function (squad) {
      return userProfiles.some(function (profile) {
        return profile.ma_tro_choi === squad.ma_tro_choi;
      });
    });

    // Create modal
    const modal = document.createElement("div");
    modal.id = "join-squad-modal";
    modal.className = "modal-overlay";
    modal.style.cssText =
      "position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.8);display:flex;justify-content:center;align-items:center;z-index:10000";

    let contentHtml = "";
    if (matchingSquads.length === 0) {
      // No matching squads - user will join without a squad
      contentHtml =
        '<div style="text-align:center;padding:20px">' +
        '<h3 style="margin-bottom:16px">Chọn nhóm để gia nhập</h3>' +
        '<p style="color:var(--text-muted);margin-bottom:24px">Đội này chưa có nhóm phù hợp với hồ sơ thi đấu của bạn. Bạn sẽ gia nhập đội như thành viên không có nhóm.</p>' +
        '<button class="btn-primary-glow" onclick="submitJoinWithoutSquad(' +
        maDoi +
        ')">Đồng ý gia nhập</button>' +
        "</div>";
    } else {
      // Show matching squads
      contentHtml =
        '<div style="padding:20px">' +
        '<h3 style="margin-bottom:16px">Chọn nhóm để gia nhập</h3>' +
        '<p style="color:var(--text-muted);margin-bottom:16px">Dựa trên hồ sơ thi đấu của bạn, bạn có thể gia nhập các nhóm sau:</p>' +
        '<div style="display:flex;flex-direction:column;gap:12px">';
      matchingSquads.forEach(function (squad) {
        contentHtml +=
          '<button class="btn-outline-glow" style="padding:12px;text-align:left" onclick="submitJoinSquad(' +
          squad.ma_nhom +
          ')">' +
          '<div style="font-weight:600">' +
          (squad.ten_nhom || "Nhóm") +
          "</div>" +
          '<div style="font-size:0.85rem;color:var(--text-muted)">' +
          (squad.ten_game || "") +
          "</div>" +
          "</button>";
      });
      contentHtml += "</div></div>";
    }

    modal.innerHTML =
      '<div class="modal-content" style="background:var(--bg-card);border-radius:12px;max-width:500px;width:90%;max-height:80vh;overflow-y:auto;position:relative">' +
      '<button onclick="closeJoinSquadModal()" style="position:absolute;top:16px;right:16px;background:none;border:none;color:var(--text-muted);font-size:24px;cursor:pointer;padding:8px">✕</button>' +
      contentHtml +
      "</div>";

    document.body.appendChild(modal);
  };

  window.closeJoinSquadModal = function () {
    const modal = document.getElementById("join-squad-modal");
    if (modal) modal.remove();
  };

  window.submitJoinSquad = async function (maNhom) {
    const joinResult = await api("/TeamApi/XinGiaNhap", "POST", {
      maNhom: maNhom,
    });
    closeJoinSquadModal();
    showToast(
      joinResult.Success
        ? "Đã gửi đơn xin gia nhập! Chờ đội trưởng duyệt."
        : joinResult.Message || "Không thể gửi đơn xin.",
      joinResult.Success ? "success" : "error",
    );
  };

  window.submitJoinWithoutSquad = async function (maDoi) {
    const result = await api("/TeamApi/ChiTietDoi?maDoi=" + maDoi);
    if (
      !result.Success ||
      !result.Data ||
      !result.Data.nhom_doi ||
      !result.Data.nhom_doi.length
    ) {
      closeJoinSquadModal();
      showToast("Đội này chưa có nhóm nào.", "error");
      return;
    }
    // Join management group (ma_tro_choi IS NULL)
    const managementGroup = result.Data.nhom_doi.find(function (squad) {
      return squad.ma_tro_choi === null || squad.ma_tro_choi === undefined;
    });
    if (!managementGroup) {
      closeJoinSquadModal();
      showToast("Không tìm thấy nhóm quản lý của đội.", "error");
      return;
    }
    const joinResult = await api("/TeamApi/XinGiaNhap", "POST", {
      maNhom: managementGroup.ma_nhom,
    });
    closeJoinSquadModal();
    showToast(
      joinResult.Success
        ? "Đã gửi đơn xin gia nhập! Chờ đội trưởng duyệt."
        : joinResult.Message || "Không thể gửi đơn xin.",
      joinResult.Success ? "success" : "error",
    );
  };

  window.toggleAddSquadForm = function (maDoi) {
    const form = document.getElementById("add-squad-form");
    if (!form) return;
    form.style.display = form.style.display === "none" ? "block" : "none";
  };

  window.submitAddSquad = async function (maDoi) {
    const msg = document.getElementById("add-squad-msg");
    const gameVal = document.getElementById("add-squad-game").value;
    const nameVal = (
      document.getElementById("add-squad-name").value || ""
    ).trim();
    if (!gameVal) {
      if (msg) {
        msg.style.color = "#ff4757";
        msg.textContent = "Vui lòng chọn game.";
      }
      return;
    }
    if (!nameVal) {
      if (msg) {
        msg.style.color = "#ff4757";
        msg.textContent = "Vui lòng nhập tên nhóm.";
      }
      return;
    }
    const result = await api("/TeamApi/TaoNhom", "POST", {
      maDoi: maDoi,
      maTroChoi: Number(gameVal),
      tenNhom: nameVal,
      maCaptain: 0, // Parameter name unchanged in BUS, value 0 means no captain assigned
    });
    if (msg) {
      msg.style.color = result.Success ? "#2ed573" : "#ff4757";
      msg.textContent = result.Success
        ? "Tạo nhóm thành công!"
        : result.Message || "Tạo nhóm thất bại.";
    }
    if (result.Success) {
      loadTeamDetail(maDoi);
    }
  };

  // ---- GLOBAL SEARCH ----
  (function initGlobalSearch() {
    const input = document.getElementById("global-search");
    const dropdown = document.getElementById("search-dropdown");
    if (!input || !dropdown) return;
    let debounce = null;

    input.addEventListener("input", function () {
      clearTimeout(debounce);
      const q = this.value.trim();
      if (q.length < 2) {
        dropdown.style.display = "none";
        return;
      }
      debounce = setTimeout(async function () {
        const result = await api("/TeamApi/TimKiem?q=" + encodeURIComponent(q));
        if (!result.Success || !result.Data) {
          dropdown.style.display = "none";
          return;
        }
        const d = result.Data;
        let html = "";
        if (d.GiaiDau && d.GiaiDau.length) {
          d.GiaiDau.forEach(function (t) {
            html +=
              '<div class="search-result-item" onclick="openTournament(' +
              t.ma_giai_dau +
              ')">' +
              '<div class="sr-icon">🏆</div>' +
              '<div><div class="sr-name">' +
              t.ten_giai_dau +
              '</div><div class="sr-meta">Giải đấu · ' +
              (t.ten_game || "") +
              "</div></div></div>";
          });
        }
        if (d.Doi && d.Doi.length) {
          d.Doi.forEach(function (team) {
            html +=
              '<div class="search-result-item" onclick="openTeamDetail(' +
              team.ma_doi +
              ')">' +
              '<div class="sr-icon">👥</div>' +
              '<div><div class="sr-name">' +
              team.ten_doi +
              '</div><div class="sr-meta">Đội tuyển</div></div></div>';
          });
        }
        if (d.NguoiChoi && d.NguoiChoi.length) {
          d.NguoiChoi.forEach(function (p) {
            html +=
              '<div class="search-result-item">' +
              '<div class="sr-icon">👤</div>' +
              '<div><div class="sr-name">' +
              p.ten_dang_nhap +
              '</div><div class="sr-meta">Người chơi</div></div></div>';
          });
        }
        if (!html)
          html =
            '<div style="padding:16px;color:var(--text-muted);text-align:center">Không tìm thấy kết quả</div>';
        dropdown.innerHTML = html;
        dropdown.style.display = "";
      }, 350);
    });

    input.addEventListener("blur", function () {
      setTimeout(function () {
        dropdown.style.display = "none";
      }, 200);
    });
    input.addEventListener("focus", function () {
      if (dropdown.innerHTML.trim()) dropdown.style.display = "";
    });
  })();

  // ---- GAME PAGE FILTER ----
  window.applyGameFilter = function () {
    const currentGameObj = GAMES.find((g) => g.id === currentPage);
    if (currentGameObj) loadGameTournaments(currentGameObj);
  };

  // Override loadGameTournaments to support filter & search
  loadGameTournaments = async function (game) {
    const grid = document.getElementById("game-grid");
    if (!grid) return;
    grid.innerHTML =
      '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';

    const statusFilter = document.getElementById("game-status-filter")
      ? document.getElementById("game-status-filter").value
      : "";
    const searchText = document.getElementById("game-search")
      ? document.getElementById("game-search").value.trim().toLowerCase()
      : "";

    let url = "/TeamApi/GiaiTheoGame?maTroChoi=" + (game.maGame || 0);
    if (statusFilter) url += "&trangThai=" + statusFilter;

    const result = await api(url);
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      grid.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">' +
        game.emoji +
        "</div><h4>Chưa có giải đấu " +
        game.name +
        "</h4></div>";
      return;
    }

    let filtered = result.Data;
    if (searchText) {
      filtered = filtered.filter((t) =>
        (t.ten_giai_dau || "").toLowerCase().includes(searchText),
      );
    }

    if (!filtered.length) {
      grid.innerHTML =
        '<div class="empty-state"><div class="empty-state-icon">🔍</div><h4>Không tìm thấy giải phù hợp</h4></div>';
      return;
    }

    grid.innerHTML = filtered
      .slice(0, 20)
      .map((t) => buildTournamentCard(t))
      .join("");
    loadInteractionStates(filtered.slice(0, 20).map((t) => t.ma_giai_dau));
  };

  // ---- HOME PAGE — open registration section ----
  loadHomePage = function () {
    loadFeaturedTournaments();
    loadOpenRegTournaments();
  };

  async function loadOpenRegTournaments() {
    const grid = document.getElementById("open-reg-grid");
    if (!grid) return;
    grid.innerHTML =
      '<div style="color:var(--text-muted);padding:10px">Đang tải...</div>';

    const result = await api("/TeamApi/GiaiDangMoDangKy");
    if (
      !result.Success ||
      !Array.isArray(result.Data) ||
      result.Data.length === 0
    ) {
      grid.innerHTML =
        '<div class="empty-state" style="padding:20px"><div class="empty-state-icon">📝</div><h4>Chưa có giải mở đăng ký</h4></div>';
      return;
    }

    grid.innerHTML = result.Data.slice(0, 6)
      .map((t) => buildTournamentCard(t))
      .join("");
    loadInteractionStates(result.Data.slice(0, 6).map((t) => t.ma_giai_dau));
  }

  // ---- INIT ----
  restoreSession();
})();
