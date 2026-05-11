// ========================================================
// app-giai-dau.js — Tournament Engine Frontend (Phase 1)
// ========================================================

(function () {
  if (!document.getElementById("tournamentPage")) return;

  var STATE_LABELS = {
    nhap: "Bản nháp",
    cho_xet_duyet: "Chờ duyệt",
    bi_tu_choi: "Bị từ chối",
    sap_dien_ra: "Sắp diễn ra",
    mo_dang_ky: "Mở đăng ký",
    khoa_dang_ky: "Khóa đăng ký",
    dang_dien_ra: "Đang diễn ra",
    ket_thuc: "Kết thúc",
    da_huy: "Đã hủy",
  };

  var STATE_COLORS = {
    nhap: "#95A5A6",
    cho_xet_duyet: "#F39C12",
    bi_tu_choi: "#C0392B",
    sap_dien_ra: "#2E86AB",
    mo_dang_ky: "#27AE60",
    khoa_dang_ky: "#8E44AD",
    dang_dien_ra: "#E74C3C",
    ket_thuc: "#2C3E50",
    da_huy: "#7F8C8D",
  };

  var FORMAT_LABELS = {
    loai_truc_tiep: "Single Elimination",
    nhanh_thang_nhanh_thua: "Double Elimination",
    vong_tron: "Round Robin",
    thuy_si: "Swiss",
    battle_royale: "Battle Royale",
    champion_rush: "Champion Rush",
  };

  var currentRejectId = null;

  // ---- TABS ----
  document.querySelectorAll(".tab-btn").forEach(function (btn) {
    btn.addEventListener("click", function () {
      document.querySelectorAll(".tab-btn").forEach(function (b) { b.classList.remove("active"); });
      document.querySelectorAll(".tab-content").forEach(function (c) { c.classList.remove("active"); });
      btn.classList.add("active");
      var tabId = btn.getAttribute("data-tab");
      var tab = document.getElementById(tabId);
      if (tab) tab.classList.add("active");
      if (tabId === "tab-my") loadMyTournaments();
      else if (tabId === "tab-public") loadPublicTournaments();
      else if (tabId === "tab-pending") loadPendingTournaments();
      else if (tabId === "tab-create") loadGamesDropdown();
    });
  });

  // ---- WIZARD STEPPER ----
  function showStep(n) {
    document.querySelectorAll(".wizard-panel").forEach(function (p) { p.classList.remove("active"); });
    document.querySelectorAll(".wizard-stepper .step").forEach(function (s) { s.classList.remove("active"); });
    var panel = document.getElementById("wizardStep" + n);
    if (panel) panel.classList.add("active");
    var step = document.querySelector('.wizard-stepper .step[data-step="' + n + '"]');
    if (step) step.classList.add("active");
    // Mark completed steps
    document.querySelectorAll(".wizard-stepper .step").forEach(function (s) {
      if (parseInt(s.getAttribute("data-step")) < n) s.classList.add("completed");
      else s.classList.remove("completed");
    });
  }

  var toStep2Btn = document.getElementById("toStep2");
  var toStep3Btn = document.getElementById("toStep3");
  var backStep1Btn = document.getElementById("backStep1");
  var backStep2Btn = document.getElementById("backStep2");
  if (toStep2Btn) toStep2Btn.addEventListener("click", function () {
    if (!document.getElementById("tenGiaiDau").value.trim()) { alert("Vui lòng nhập tên giải đấu."); return; }
    showStep(2);
  });
  if (toStep3Btn) toStep3Btn.addEventListener("click", function () {
    if (!document.getElementById("maTroChoi").value) { alert("Vui lòng chọn game."); return; }
    showStep(3);
    if (document.getElementById("stageContainer").children.length === 0) addStage();
  });
  if (backStep1Btn) backStep1Btn.addEventListener("click", function () { showStep(1); });
  if (backStep2Btn) backStep2Btn.addEventListener("click", function () { showStep(2); });

  // ---- STAGE BUILDER ----
  var stageIndex = 0;
  var addStageBtn = document.getElementById("addStageBtn");
  if (addStageBtn) addStageBtn.addEventListener("click", addStage);

  function addStage() {
    stageIndex++;
    var container = document.getElementById("stageContainer");
    var div = document.createElement("div");
    div.className = "stage-card";
    div.setAttribute("data-index", stageIndex);
    div.innerHTML =
      '<div class="stage-card-header">' +
        '<span class="stage-number">Giai đoạn ' + stageIndex + '</span>' +
        '<button type="button" class="btn btn-outline-danger btn-sm remove-stage-btn">&times; Xóa</button>' +
      '</div>' +
      '<div class="form-row">' +
        '<div class="form-group"><label>Tên giai đoạn *</label>' +
          '<input type="text" class="form-control stage-name" placeholder="VD: Vòng Bảng" required></div>' +
        '<div class="form-group"><label>Thể thức *</label>' +
          '<select class="form-control stage-format">' +
            '<option value="">Chọn...</option>' +
            '<option value="loai_truc_tiep">Single Elimination</option>' +
            '<option value="nhanh_thang_nhanh_thua">Double Elimination</option>' +
            '<option value="vong_tron">Round Robin</option>' +
            '<option value="thuy_si">Swiss</option>' +
            '<option value="battle_royale">Battle Royale</option>' +
            '<option value="champion_rush">Champion Rush</option>' +
          '</select></div>' +
      '</div>' +
      '<div class="form-row">' +
        '<div class="form-group"><label>Số đội</label>' +
          '<input type="number" class="form-control stage-teams" min="2" value="16"></div>' +
        '<div class="form-group"><label>Số đội đi tiếp</label>' +
          '<input type="number" class="form-control stage-advance" min="0" placeholder="NULL = cuối"></div>' +
      '</div>';
    container.appendChild(div);
    div.querySelector(".remove-stage-btn").addEventListener("click", function () {
      div.remove();
      renumberStages();
    });
  }

  function renumberStages() {
    var cards = document.querySelectorAll("#stageContainer .stage-card");
    stageIndex = 0;
    cards.forEach(function (card) {
      stageIndex++;
      card.setAttribute("data-index", stageIndex);
      card.querySelector(".stage-number").textContent = "Giai đoạn " + stageIndex;
    });
  }

  function collectStages() {
    var stages = [];
    document.querySelectorAll("#stageContainer .stage-card").forEach(function (card, i) {
      var adv = card.querySelector(".stage-advance").value;
      stages.push({
        so_thu_tu: i + 1,
        ten_giai_doan: card.querySelector(".stage-name").value,
        the_thuc: card.querySelector(".stage-format").value,
        so_doi: parseInt(card.querySelector(".stage-teams").value) || 0,
        so_doi_di_tiep: adv ? parseInt(adv) : null,
        nguong_match_point: null,
        bang_diem_json: null,
      });
    });
    return stages;
  }

  // ---- BANNER UPLOAD ----
  var bannerInput = document.getElementById("bannerFileInput");
  if (bannerInput) {
    bannerInput.addEventListener("change", async function () {
      var file = bannerInput.files && bannerInput.files[0];
      if (!file) return;
      var preview = document.getElementById("bannerPreview");
      if (preview) { preview.src = URL.createObjectURL(file); preview.style.display = "block"; }
      var fd = new FormData();
      fd.append("banner", file);
      var result = await postFormApi("/GiaiDauApi/UploadBanner", fd);
      showMessage("createMessage", result);
      if (result.success && result.data) {
        document.getElementById("bannerUrl").value = result.data;
      }
    });
  }

  // ---- GAMES DROPDOWN ----
  var gamesLoaded = false;
  async function loadGamesDropdown() {
    if (gamesLoaded) return;
    var result = await getApi("/DoiApi/TroChoi");
    if (result.success || result.Success) {
      var data = result.data || result.Data || [];
      var sel = document.getElementById("maTroChoi");
      if (sel && Array.isArray(data)) {
        data.forEach(function (g) {
          var opt = document.createElement("option");
          opt.value = g.ma_tro_choi;
          opt.textContent = g.ten_game;
          sel.appendChild(opt);
        });
        gamesLoaded = true;
      }
    }
  }
  loadGamesDropdown();

  // ---- CREATE TOURNAMENT FORM ----
  var createForm = document.getElementById("createTournamentForm");
  if (createForm) {
    createForm.addEventListener("submit", async function (e) {
      e.preventDefault();
      var stages = collectStages();
      if (stages.length === 0) { alert("Vui lòng thêm ít nhất 1 giai đoạn."); return; }
      for (var i = 0; i < stages.length; i++) {
        if (!stages[i].ten_giai_doan) { alert("Vui lòng nhập tên cho giai đoạn " + (i + 1)); return; }
        if (!stages[i].the_thuc) { alert("Vui lòng chọn thể thức cho giai đoạn " + (i + 1)); return; }
      }
      var body = {
        ten_giai_dau: document.getElementById("tenGiaiDau").value,
        banner_url: document.getElementById("bannerUrl").value,
        mo_ta: document.getElementById("moTa").value,
        ma_tro_choi: parseInt(document.getElementById("maTroChoi").value) || null,
        so_doi_toi_thieu: parseInt(document.getElementById("minTeams").value) || 2,
        so_doi_toi_da: parseInt(document.getElementById("maxTeams").value) || null,
        min_members_per_team: parseInt(document.getElementById("minMembers").value) || 1,
        giai_doan: stages,
      };
      var result = await postApi("/GiaiDauApi/Create", body);
      showMessage("createMessage", result);
      if (result.success) {
        createForm.reset();
        document.getElementById("stageContainer").innerHTML = "";
        stageIndex = 0;
        var preview = document.getElementById("bannerPreview");
        if (preview) preview.style.display = "none";
        showStep(1);
        // Switch to my tab
        document.querySelector('.tab-btn[data-tab="tab-my"]').click();
      }
    });
  }

  // ---- LOAD LISTS ----
  async function loadMyTournaments() {
    var list = document.getElementById("myTournamentList");
    var empty = document.getElementById("myTournamentEmpty");
    list.innerHTML = '<p class="text-muted">Đang tải...</p>';
    var result = await getApi("/GiaiDauApi/Mine");
    var data = (result.data || result.Data || []);
    if (!Array.isArray(data)) data = [];
    if (data.length === 0) { list.innerHTML = ""; empty.style.display = "block"; return; }
    empty.style.display = "none";
    list.innerHTML = data.map(renderTournamentCard).join("");
    attachCardEvents(list);
  }

  async function loadPublicTournaments() {
    var list = document.getElementById("publicTournamentList");
    var empty = document.getElementById("publicTournamentEmpty");
    list.innerHTML = '<p class="text-muted">Đang tải...</p>';
    var result = await getApi("/GiaiDauApi/All");
    var data = (result.data || result.Data || []);
    if (!Array.isArray(data)) data = [];
    if (data.length === 0) { list.innerHTML = ""; empty.style.display = "block"; return; }
    empty.style.display = "none";
    list.innerHTML = data.map(renderTournamentCard).join("");
    attachCardEvents(list);
  }

  async function loadPendingTournaments() {
    var list = document.getElementById("pendingTournamentList");
    var empty = document.getElementById("pendingTournamentEmpty");
    var badge = document.getElementById("pendingCount");
    if (!list) return;
    list.innerHTML = '<p class="text-muted">Đang tải...</p>';
    var result = await getApi("/GiaiDauApi/PendingApproval");
    var data = (result.data || result.Data || []);
    if (!Array.isArray(data)) data = [];
    if (badge) badge.textContent = data.length > 0 ? data.length : "";
    if (data.length === 0) { list.innerHTML = ""; empty.style.display = "block"; return; }
    empty.style.display = "none";
    list.innerHTML = data.map(renderTournamentCard).join("");
    attachCardEvents(list);
  }

  // ---- RENDER CARD ----
  function renderTournamentCard(t) {
    var stateLabel = STATE_LABELS[t.trang_thai] || t.trang_thai;
    var stateColor = STATE_COLORS[t.trang_thai] || "#95A5A6";
    var bannerHtml = t.banner_url
      ? '<div class="card-banner" style="background-image:url(' + esc(t.banner_url) + ')"></div>'
      : '<div class="card-banner card-banner-default"></div>';
    return (
      '<div class="tournament-card" data-id="' + t.ma_giai_dau + '">' +
        bannerHtml +
        '<div class="card-body">' +
          '<div class="card-top-row">' +
            '<span class="state-badge" style="background:' + stateColor + '">' + esc(stateLabel) + '</span>' +
            '<span class="card-game">' + esc(t.ten_game || "Chưa chọn game") + '</span>' +
          '</div>' +
          '<h3 class="card-title">' + esc(t.ten_giai_dau) + '</h3>' +
          '<div class="card-meta">' +
            '<span>👥 ' + (t.so_doi_da_duyet || 0) + '/' + (t.so_doi_toi_da || "∞") + ' đội</span>' +
            '<span>🎯 Min ' + t.so_doi_toi_thieu + '</span>' +
          '</div>' +
          '<button class="btn btn-outline-primary btn-sm view-detail-btn" data-id="' + t.ma_giai_dau + '">Xem chi tiết</button>' +
        '</div>' +
      '</div>'
    );
  }

  function attachCardEvents(container) {
    container.querySelectorAll(".view-detail-btn").forEach(function (btn) {
      btn.addEventListener("click", function (e) {
        e.stopPropagation();
        openDetail(parseInt(btn.getAttribute("data-id")));
      });
    });
  }

  // ---- DETAIL MODAL ----
  async function openDetail(id) {
    var modal = document.getElementById("tournamentDetailModal");
    modal.style.display = "flex";
    document.getElementById("detailTitle").textContent = "Đang tải...";
    document.getElementById("detailBody").innerHTML = "";
    document.getElementById("detailFooter").innerHTML = "";

    var result = await getApi("/GiaiDauApi/Detail?maGiaiDau=" + id);
    if (!result.success && !result.Success) {
      document.getElementById("detailTitle").textContent = "Lỗi";
      document.getElementById("detailBody").innerHTML = '<p class="text-muted">' + esc(result.message || "Không tải được.") + '</p>';
      return;
    }
    var detail = result.data || result.Data;
    var gd = detail.giai_dau;
    var stages = detail.giai_doan || [];
    var teams = detail.doi_tham_gia || [];
    var stateLabel = STATE_LABELS[gd.trang_thai] || gd.trang_thai;
    var stateColor = STATE_COLORS[gd.trang_thai] || "#95A5A6";

    document.getElementById("detailTitle").textContent = gd.ten_giai_dau;

    var bodyHtml =
      '<div class="detail-state"><span class="state-badge" style="background:' + stateColor + '">' + esc(stateLabel) + '</span></div>';

    if (gd.banner_url) {
      bodyHtml += '<img src="' + esc(gd.banner_url) + '" class="detail-banner" alt="Banner">';
    }

    if (gd.trang_thai === "bi_tu_choi" && gd.ly_do_tu_choi) {
      bodyHtml += '<div class="reject-reason"><strong>Lý do từ chối:</strong> ' + esc(gd.ly_do_tu_choi) + '</div>';
    }

    bodyHtml +=
      '<dl class="detail-dl">' +
        '<dt>Game</dt><dd>' + esc(gd.ten_game || "Chưa chọn") + '</dd>' +
        '<dt>Người tạo</dt><dd>' + esc(gd.ten_nguoi_tao || "") + '</dd>' +
        '<dt>Số đội tối thiểu</dt><dd>' + gd.so_doi_toi_thieu + '</dd>' +
        '<dt>Số đội tối đa</dt><dd>' + (gd.so_doi_toi_da || "Không giới hạn") + '</dd>' +
        '<dt>Thành viên/đội tối thiểu</dt><dd>' + gd.min_members_per_team + '</dd>' +
        '<dt>Đội đã đăng ký</dt><dd>' + (gd.so_doi_dang_ky || 0) + '</dd>' +
        '<dt>Đội đã duyệt</dt><dd>' + (gd.so_doi_da_duyet || 0) + '</dd>' +
      '</dl>';

    if (gd.mo_ta) {
      bodyHtml += '<div class="detail-desc"><strong>Mô tả:</strong><p>' + esc(gd.mo_ta) + '</p></div>';
    }

    if (stages.length > 0) {
      bodyHtml += '<h4>Giai đoạn thi đấu</h4><table class="detail-table"><thead><tr><th>#</th><th>Tên</th><th>Thể thức</th><th>Số đội</th><th>Đi tiếp</th></tr></thead><tbody>';
      stages.forEach(function (s) {
        bodyHtml += '<tr><td>' + s.so_thu_tu + '</td><td>' + esc(s.ten_giai_doan) + '</td><td>' + esc(FORMAT_LABELS[s.the_thuc] || s.the_thuc) + '</td><td>' + s.so_doi + '</td><td>' + (s.so_doi_di_tiep != null ? s.so_doi_di_tiep : "—") + '</td></tr>';
      });
      bodyHtml += '</tbody></table>';
    }

    document.getElementById("detailBody").innerHTML = bodyHtml;

    // Footer actions based on state
    var footerHtml = '';
    var tt = gd.trang_thai;
    if (tt === "nhap" || tt === "bi_tu_choi") {
      footerHtml += '<button class="btn btn-primary action-btn" data-action="submit" data-id="' + id + '">Gửi phê duyệt</button>';
      footerHtml += '<button class="btn btn-danger action-btn" data-action="cancel" data-id="' + id + '">Hủy giải</button>';
    }
    if (tt === "cho_xet_duyet") {
      footerHtml += '<button class="btn btn-success action-btn" data-action="approve" data-id="' + id + '">Phê duyệt</button>';
      footerHtml += '<button class="btn btn-danger reject-btn" data-id="' + id + '">Từ chối</button>';
    }
    if (tt === "sap_dien_ra") {
      footerHtml += '<button class="btn btn-success action-btn" data-action="open-reg" data-id="' + id + '">Mở đăng ký</button>';
      footerHtml += '<button class="btn btn-danger action-btn" data-action="cancel" data-id="' + id + '">Hủy giải</button>';
    }
    if (tt === "mo_dang_ky") {
      footerHtml += '<button class="btn btn-warning action-btn" data-action="close-reg" data-id="' + id + '">Chốt sổ đăng ký</button>';
      footerHtml += '<button class="btn btn-danger action-btn" data-action="cancel" data-id="' + id + '">Hủy giải</button>';
    }
    if (tt === "khoa_dang_ky") {
      footerHtml += '<button class="btn btn-success action-btn" data-action="start" data-id="' + id + '">Khởi tranh</button>';
      footerHtml += '<button class="btn btn-outline-primary action-btn" data-action="reopen-reg" data-id="' + id + '">Mở lại đăng ký</button>';
      footerHtml += '<button class="btn btn-danger action-btn" data-action="cancel" data-id="' + id + '">Hủy giải</button>';
    }
    if (tt === "dang_dien_ra") {
      footerHtml += '<button class="btn btn-primary action-btn" data-action="complete" data-id="' + id + '">Bế mạc giải</button>';
    }
    
    // Nút "Đăng ký tham gia" nếu đang mở đăng ký
    if (tt === "mo_dang_ky") {
      footerHtml += '<button class="btn btn-success" onclick="openRegisterTeamModal(' + id + ')">Đăng ký tham gia</button>';
    }

    // Nút mời đội / nhân sự cho BTC
    if (tt !== "nhap" && tt !== "bi_tu_choi") {
      footerHtml += '<button class="btn btn-outline-info" onclick="openInviteModal(' + id + ', \'doi\')">Mời Đội</button>';
      footerHtml += '<button class="btn btn-outline-secondary" onclick="openInviteModal(' + id + ', \'trong_tai\')">Mời Trọng Tài</button>';
      footerHtml += '<button class="btn btn-outline-dark" onclick="openInviteModal(' + id + ', \'btc\')">Mời BTC</button>';
    }
    
    document.getElementById("detailFooter").innerHTML = footerHtml;

    // Attach action events
    document.querySelectorAll("#detailFooter .action-btn").forEach(function (btn) {
      btn.addEventListener("click", function () { handleAction(btn.getAttribute("data-action"), parseInt(btn.getAttribute("data-id"))); });
    });
    document.querySelectorAll("#detailFooter .reject-btn").forEach(function (btn) {
      btn.addEventListener("click", function () { openRejectModal(parseInt(btn.getAttribute("data-id"))); });
    });
  }

  // ---- ACTIONS ----
  var ACTION_ENDPOINTS = {
    "submit": "/GiaiDauApi/Submit",
    "approve": "/GiaiDauApi/Approve",
    "open-reg": "/GiaiDauApi/OpenRegistration",
    "close-reg": "/GiaiDauApi/CloseRegistration",
    "reopen-reg": "/GiaiDauApi/ReopenRegistration",
    "start": "/GiaiDauApi/Start",
    "complete": "/GiaiDauApi/Complete",
    "cancel": "/GiaiDauApi/Cancel",
  };

  async function handleAction(action, id) {
    var confirmMessages = {
      "submit": "Gửi yêu cầu phê duyệt?",
      "approve": "Phê duyệt giải đấu này?",
      "cancel": "Hủy giải đấu? Hành động này không thể hoàn tác.",
      "start": "Khởi tranh giải đấu?",
      "complete": "Bế mạc giải đấu? Hành động này không thể hoàn tác.",
    };
    if (confirmMessages[action] && !confirm(confirmMessages[action])) return;
    var url = ACTION_ENDPOINTS[action];
    if (!url) return;
    var result = await postApi(url, { ma_giai_dau: id });
    alert(result.message || "Đã xử lý.");
    if (result.success) {
      document.getElementById("tournamentDetailModal").style.display = "none";
      loadMyTournaments();
      if (document.getElementById("tab-pending")) loadPendingTournaments();
    }
  }

  // ---- REJECT MODAL ----
  function openRejectModal(id) {
    currentRejectId = id;
    document.getElementById("rejectModal").style.display = "flex";
    document.getElementById("rejectReason").value = "";
  }
  var closeRejectBtn = document.getElementById("closeRejectModal");
  var cancelRejectBtn = document.getElementById("cancelReject");
  if (closeRejectBtn) closeRejectBtn.addEventListener("click", function () { document.getElementById("rejectModal").style.display = "none"; });
  if (cancelRejectBtn) cancelRejectBtn.addEventListener("click", function () { document.getElementById("rejectModal").style.display = "none"; });

  var confirmRejectBtn = document.getElementById("confirmReject");
  if (confirmRejectBtn) {
    confirmRejectBtn.addEventListener("click", async function () {
      var reason = document.getElementById("rejectReason").value.trim();
      if (!reason) { alert("Vui lòng nhập lý do từ chối."); return; }
      var result = await postApi("/GiaiDauApi/Reject", { ma_giai_dau: currentRejectId, ly_do: reason });
      showMessage("rejectMessage", result);
      if (result.success) {
        document.getElementById("rejectModal").style.display = "none";
        document.getElementById("tournamentDetailModal").style.display = "none";
        loadPendingTournaments();
      }
    });
  }

  // ---- CLOSE DETAIL MODAL ----
  var closeDetailBtn = document.getElementById("closeDetailModal");
  if (closeDetailBtn) closeDetailBtn.addEventListener("click", function () {
    document.getElementById("tournamentDetailModal").style.display = "none";
  });
  var detailModal = document.getElementById("tournamentDetailModal");
  if (detailModal) detailModal.addEventListener("click", function (e) {
    if (e.target === detailModal) detailModal.style.display = "none";
  });

  // ---- HELPERS ----
  function esc(v) { var d = document.createElement("div"); d.textContent = v == null ? "" : String(v); return d.innerHTML; }

  // ---- MỜI & ĐĂNG KÝ ----
  window.openRegisterTeamModal = function(maGiaiDau) {
      var maDoi = prompt("Nhập mã Đội của bạn để đăng ký tham gia giải:");
      if (maDoi) {
          postApi("/GiaiDauApi/RegisterTeam", { ma_giai_dau: maGiaiDau, ma_doi: parseInt(maDoi) })
              .then(res => alert(res.message));
      }
  };

  window.openInviteModal = function(maGiaiDau, loai) {
      if (loai === 'doi') {
          var maDoi = prompt("Nhập mã Đội bạn muốn mời:");
          if (maDoi) {
              postApi("/GiaiDauApi/InviteTeam", { ma_giai_dau: maGiaiDau, ma_doi: parseInt(maDoi), loi_nhan: "Xin mời tham gia giải đấu" })
                  .then(res => alert(res.message));
          }
      } else {
          var user = prompt("Nhập Username hoặc Email của người muốn mời làm " + (loai === "btc" ? "BTC" : "Trọng tài") + ":");
          if (user) {
              postApi("/GiaiDauApi/InviteNhanSu", { ma_giai_dau: maGiaiDau, username_or_email: user, vai_tro: loai, loi_nhan: "Mời hợp tác giải đấu" })
                  .then(res => alert(res.message));
          }
      }
  };

  // ---- INIT ----
  loadMyTournaments();
  if (document.getElementById("tab-pending")) loadPendingTournaments();
})();
