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
  var editTournamentId = null;
  var editTournamentStatus = null;
  var editTournamentDetail = null;

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
  var submitCreateBtn = document.getElementById("submitCreateBtn");
  var resubmitEditBtn = document.getElementById("resubmitEditBtn");
  var cancelEditBtn = document.getElementById("cancelEditBtn");
  var editRejectAlert = document.getElementById("editRejectAlert");
  if (toStep2Btn) toStep2Btn.addEventListener("click", function () {
    if (!document.getElementById("tenGiaiDau").value.trim()) { alert("Vui lòng nhập tên giải đấu."); return; }
    if (!validatePrizes()) { alert("Tổng giá trị các giải thưởng chi tiết đang vượt quá Tổng ngân sách công bố!"); return; }
    showStep(2);
  });

  // --- PRIZE LOGIC ---
  var tongGiaiThuongInput = document.getElementById("tongGiaiThuong");
  var addPrizeBtn = document.getElementById("addPrizeBtn");
  var prizeListContainer = document.getElementById("prizeListContainer");
  var prizeValidationMsg = document.getElementById("prizeValidationMsg");

  function unformatPrice(val) {
    if (!val) return 0;
    return parseInt(val.toString().replace(/,/g, "")) || 0;
  }
  function formatPrice(val) {
    return val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
  }

  if (tongGiaiThuongInput) {
    tongGiaiThuongInput.addEventListener("input", function(e) {
      var raw = unformatPrice(e.target.value);
      e.target.value = formatPrice(raw);
      validatePrizes();
    });
  }

  if (addPrizeBtn) {
    addPrizeBtn.addEventListener("click", function() {
      var row = document.createElement("div");
      row.style.display = "flex";
      row.style.gap = "8px";
      row.innerHTML = `
        <input type="text" class="form-control prize-name" placeholder="Tên giải (VD: Á Quân)" style="flex: 1;" required>
        <input type="text" class="form-control price-input prize-val" placeholder="Giá trị (VNĐ)" style="width: 150px;" required>
        <button type="button" class="ak-btn-pill remove-prize-btn" style="color: #ef4444; border-color: #ef4444; padding: 0 10px;">🗑️</button>
      `;
      prizeListContainer.appendChild(row);

      var valInput = row.querySelector(".prize-val");
      valInput.addEventListener("input", function(e) {
        var raw = unformatPrice(e.target.value);
        e.target.value = formatPrice(raw);
        validatePrizes();
      });

      var removeBtn = row.querySelector(".remove-prize-btn");
      removeBtn.addEventListener("click", function() {
        row.remove();
        validatePrizes();
      });
    });
  }

  function appendPrizeRow(name, value) {
    var row = document.createElement("div");
    row.style.display = "flex";
    row.style.gap = "8px";
    row.innerHTML =
      '<input type="text" class="form-control prize-name" placeholder="Ten giai" style="flex: 1;" required>' +
      '<input type="text" class="form-control price-input prize-val" placeholder="Gia tri" style="width: 150px;" required>' +
      '<button type="button" class="ak-btn-pill remove-prize-btn" style="color: #ef4444; border-color: #ef4444; padding: 0 10px;">Xoa</button>';
    prizeListContainer.appendChild(row);
    row.querySelector(".prize-name").value = name || "";
    row.querySelector(".prize-val").value = formatPrice(parseInt(value || 0));
    row.querySelector(".prize-val").addEventListener("input", function(e) {
      var raw = unformatPrice(e.target.value);
      e.target.value = formatPrice(raw);
      validatePrizes();
    });
    row.querySelector(".remove-prize-btn").addEventListener("click", function() {
      row.remove();
      validatePrizes();
    });
    validatePrizes();
    return row;
  }

  function validatePrizes() {
    if (!tongGiaiThuongInput) return true;
    var totalBudget = unformatPrice(tongGiaiThuongInput.value);
    var currentSum = 0;
    var vals = document.querySelectorAll(".prize-val");
    vals.forEach(function(el) {
      currentSum += unformatPrice(el.value);
    });

    if (currentSum > totalBudget) {
      if (prizeValidationMsg) prizeValidationMsg.style.display = "block";
      if (toStep2Btn) toStep2Btn.disabled = true;
      return false;
    } else {
      if (prizeValidationMsg) prizeValidationMsg.style.display = "none";
      if (toStep2Btn) toStep2Btn.disabled = false;
      return true;
    }
  }

  function collectPrizes() {
    var prizes = [];
    var rows = document.querySelectorAll("#prizeListContainer > div");
    rows.forEach(function(row) {
      var name = row.querySelector(".prize-name").value;
      var val = unformatPrice(row.querySelector(".prize-val").value);
      if (name && val >= 0) {
        prizes.push({ ten_giai: name, gia_tri: val });
      }
    });
    return prizes;
  }
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
    var maxTeamsInput = document.getElementById("maxTeams");
    var defaultTeams = (maxTeamsInput && maxTeamsInput.value) ? maxTeamsInput.value : "16";
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
          '<input type="number" class="form-control stage-teams" min="2" value="' + defaultTeams + '"></div>' +
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

  function setCreateTabActive() {
    var btn = document.querySelector('.tab-btn[data-tab="tab-create"]');
    if (btn) btn.click();
  }

  function resetTournamentForm() {
    editTournamentId = null;
    editTournamentStatus = null;
    editTournamentDetail = null;
    if (createForm) createForm.reset();
    if (tongGiaiThuongInput) tongGiaiThuongInput.value = "0";
    if (prizeListContainer) prizeListContainer.innerHTML = "";
    var stageContainer = document.getElementById("stageContainer");
    if (stageContainer) stageContainer.innerHTML = "";
    stageIndex = 0;
    var preview = document.getElementById("bannerPreview");
    if (preview) {
      preview.src = "";
      preview.style.display = "none";
    }
    if (editRejectAlert) {
      editRejectAlert.innerHTML = "";
      editRejectAlert.style.display = "none";
    }
    var title = document.querySelector("#tab-create .section-title");
    if (title) title.textContent = "Tạo giải đấu mới";
    if (submitCreateBtn) submitCreateBtn.textContent = "Lưu bản nháp";
    if (resubmitEditBtn) resubmitEditBtn.style.display = "none";
    if (cancelEditBtn) cancelEditBtn.style.display = "none";
    showStep(1);
  }

  function fillStage(stage) {
    addStage();
    var cards = document.querySelectorAll("#stageContainer .stage-card");
    var card = cards[cards.length - 1];
    if (!card) return;
    card.querySelector(".stage-name").value = stage.ten_giai_doan || "";
    card.querySelector(".stage-format").value = stage.the_thuc || "";
    card.querySelector(".stage-teams").value = stage.so_doi || "";
    card.querySelector(".stage-advance").value = stage.so_doi_di_tiep != null ? stage.so_doi_di_tiep : "";
  }

  async function openEditTournament(detail) {
    var gd = detail.giai_dau;
    await loadGamesDropdown();
    resetTournamentForm();
    editTournamentId = gd.ma_giai_dau;
    editTournamentStatus = gd.trang_thai;
    editTournamentDetail = detail;
    setCreateTabActive();
    var title = document.querySelector("#tab-create .section-title");
    if (title) title.textContent = "Chỉnh sửa giải đấu";

    document.getElementById("tenGiaiDau").value = gd.ten_giai_dau || "";
    document.getElementById("bannerUrl").value = gd.banner_url || "";
    document.getElementById("moTa").value = gd.mo_ta || "";
    if (tongGiaiThuongInput) tongGiaiThuongInput.value = formatPrice(parseInt(gd.tong_giai_thuong || 0));
    document.getElementById("maTroChoi").value = gd.ma_tro_choi || "";
    document.getElementById("minTeams").value = gd.so_doi_toi_thieu || 2;
    document.getElementById("maxTeams").value = gd.so_doi_toi_da || "";
    document.getElementById("minMembers").value = gd.min_members_per_team || 1;

    var preview = document.getElementById("bannerPreview");
    if (preview && gd.banner_url) {
      preview.src = gd.banner_url;
      preview.style.display = "block";
    }

    (detail.danh_sach_giai_thuong || []).forEach(function (p) {
      appendPrizeRow(p.ten_giai, p.gia_tri);
    });
    (detail.giai_doan || []).forEach(fillStage);
    if (!document.getElementById("stageContainer").children.length) addStage();

    if (gd.trang_thai === "bi_tu_choi" && gd.ly_do_tu_choi && editRejectAlert) {
      editRejectAlert.innerHTML = "<strong>Giải đấu bị từ chối do:</strong> " + esc(gd.ly_do_tu_choi) + ". Vui lòng sửa lại.";
      editRejectAlert.style.display = "block";
    }
    if (submitCreateBtn) submitCreateBtn.textContent = "Lưu bản nháp";
    if (resubmitEditBtn) resubmitEditBtn.style.display = "inline-block";
    if (cancelEditBtn) cancelEditBtn.style.display = "inline-block";
    validatePrizes();
    showStep(1);
  }

  function validateStages(stages) {
    if (stages.length === 0) { alert("Vui lòng thêm ít nhất 1 giai đoạn."); return false; }
    for (var i = 0; i < stages.length; i++) {
      if (!stages[i].ten_giai_doan) { alert("Vui lòng nhập tên cho giai đoạn " + (i + 1)); return false; }
      if (!stages[i].the_thuc) { alert("Vui lòng chọn thể thức cho giai đoạn " + (i + 1)); return false; }
      if (i < stages.length - 1) {
        if (stages[i].so_doi_di_tiep == null || stages[i].so_doi_di_tiep <= 0) {
          alert("Giai đoạn " + (i + 1) + " bắt buộc phải nhập số đội đi tiếp.");
          return false;
        }
      } else {
        stages[i].so_doi_di_tiep = null;
      }
    }
    return true;
  }

  function buildTournamentRequest() {
    var stages = collectStages();
    if (!validateStages(stages)) return null;
    var body = {
      ten_giai_dau: document.getElementById("tenGiaiDau").value,
      banner_url: document.getElementById("bannerUrl").value,
      mo_ta: document.getElementById("moTa").value,
      tong_giai_thuong: tongGiaiThuongInput ? unformatPrice(tongGiaiThuongInput.value) : 0,
      danh_sach_giai_thuong: collectPrizes(),
      ma_tro_choi: parseInt(document.getElementById("maTroChoi").value) || null,
      so_doi_toi_thieu: parseInt(document.getElementById("minTeams").value) || 2,
      so_doi_toi_da: parseInt(document.getElementById("maxTeams").value) || null,
      min_members_per_team: parseInt(document.getElementById("minMembers").value) || 1,
      giai_doan: stages,
    };
    if (editTournamentId) body.ma_giai_dau = editTournamentId;
    return body;
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
      var requestBody = buildTournamentRequest();
      if (!requestBody) return;
      var saveResult = editTournamentId
        ? await postApi("/GiaiDauApi/SaveDraft", requestBody)
        : await postApi("/GiaiDauApi/Create", requestBody);
      showMessage("createMessage", saveResult);
      if (saveResult.success) {
        resetTournamentForm();
        document.querySelector('.tab-btn[data-tab="tab-my"]').click();
      }
      return;
      var stages = collectStages();
      if (stages.length === 0) { alert("Vui lòng thêm ít nhất 1 giai đoạn."); return; }
      for (var i = 0; i < stages.length; i++) {
        if (!stages[i].ten_giai_doan) { alert("Vui lòng nhập tên cho giai đoạn " + (i + 1)); return; }
        if (!stages[i].the_thuc) { alert("Vui lòng chọn thể thức cho giai đoạn " + (i + 1)); return; }
        // Giai đoạn cuối (tìm nhà vô địch) → không cần số đội đi tiếp
        // Các giai đoạn trước bắt buộc phải nhập số đội đi tiếp
        if (i < stages.length - 1) {
          if (stages[i].so_doi_di_tiep == null || stages[i].so_doi_di_tiep <= 0) {
            alert("Giai đoạn " + (i + 1) + " bắt buộc phải nhập số đội đi tiếp (vì không phải giai đoạn cuối).");
            return;
          }
        } else {
          // Giai đoạn cuối: tự động set null (sẽ thành 0 ở backend)
          stages[i].so_doi_di_tiep = null;
        }
      }
      var body = {
        ten_giai_dau: document.getElementById("tenGiaiDau").value,
        banner_url: document.getElementById("bannerUrl").value,
        mo_ta: document.getElementById("moTa").value,
        tong_giai_thuong: tongGiaiThuongInput ? unformatPrice(tongGiaiThuongInput.value) : 0,
        danh_sach_giai_thuong: typeof collectPrizes === "function" ? collectPrizes() : [],
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
        if (prizeListContainer) prizeListContainer.innerHTML = "";
        stageIndex = 0;
        var preview = document.getElementById("bannerPreview");
        if (preview) preview.style.display = "none";
        showStep(1);
        // Switch to my tab
        document.querySelector('.tab-btn[data-tab="tab-my"]').click();
      }
    });
  }

  if (resubmitEditBtn) {
    resubmitEditBtn.addEventListener("click", async function () {
      if (!editTournamentId) return;
      var requestBody = buildTournamentRequest();
      if (!requestBody) return;
      var updateResult = await postApi("/GiaiDauApi/Update", requestBody);
      if (!updateResult.success) {
        showMessage("createMessage", updateResult);
        return;
      }
      var submitResult = await postApi("/GiaiDauApi/Submit", { ma_giai_dau: editTournamentId });
      showMessage("createMessage", submitResult);
      if (submitResult.success) {
        resetTournamentForm();
        document.querySelector('.tab-btn[data-tab="tab-my"]').click();
      }
    });
  }

  if (cancelEditBtn) {
    cancelEditBtn.addEventListener("click", function () {
      resetTournamentForm();
      document.querySelector('.tab-btn[data-tab="tab-my"]').click();
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

    // Nút hành động nhanh ngay trên card (không cần mở modal)
    var quickActions = '';
    if (t.trang_thai === 'nhap' || t.trang_thai === 'bi_tu_choi') {
      quickActions += '<button class="btn btn-outline-primary btn-sm quick-edit-btn" data-id="' + t.ma_giai_dau + '">Chỉnh sửa</button>';
      if (t.trang_thai === 'nhap') quickActions += '<button class="btn btn-primary btn-sm quick-submit-btn" data-id="' + t.ma_giai_dau + '" title="Gửi lên Admin để phê duyệt">📤 Gửi lên Admin</button>';
    }


    // Hiển thị lý do từ chối nếu có
    var rejectNote = '';
    if (t.trang_thai === 'bi_tu_choi' && t.ly_do_tu_choi) {
      rejectNote = '<div class="reject-note"><strong>Lý do từ chối:</strong> ' + esc(t.ly_do_tu_choi) + '</div>';
    }

    return (
      '<div class="tournament-card" data-id="' + t.ma_giai_dau + '" data-status="' + esc(t.trang_thai) + '">' +
        bannerHtml +
        '<div class="card-body">' +
          '<div class="card-top-row">' +
            '<span class="state-badge" style="background:' + stateColor + '">' + esc(stateLabel) + '</span>' +
            '<span class="card-game">' + esc(t.ten_game || "Chưa chọn game") + '</span>' +
          '</div>' +
          '<h3 class="card-title">' + esc(t.ten_giai_dau) + '</h3>' +
          rejectNote +
          '<div class="card-meta">' +
            '<span>👥 ' + (t.so_doi_da_duyet || 0) + '/' + (t.so_doi_toi_da || "∞") + ' đội</span>' +
            '<span>🎯 Min ' + t.so_doi_toi_thieu + '</span>' +
          '</div>' +
          '<div class="card-actions">' +
            quickActions +
            '<button class="btn btn-outline-primary btn-sm view-detail-btn" data-id="' + t.ma_giai_dau + '">Xem chi tiết</button>' +
          '</div>' +
        '</div>' +
      '</div>'
    );
  }

  function attachCardEvents(container) {
    // Click vào toàn bộ card để xem chi tiết (Redirect hoặc Modal tùy trạng thái)
    container.querySelectorAll(".tournament-card").forEach(function (card) {
      var status = card.getAttribute("data-status");
      var detailBtn = card.querySelector(".view-detail-btn");
      if (status === "nhap" && detailBtn) {
        detailBtn.classList.remove("btn-outline-primary", "view-detail-btn");
        detailBtn.classList.add("btn-danger", "quick-delete-draft-btn");
        detailBtn.textContent = "Xóa bản nháp";
      }
      card.addEventListener("click", function (e) {
        // Nếu click vào nút hành động nhanh thì bỏ qua card click
        if (e.target.closest(".quick-submit-btn") || e.target.closest(".quick-edit-btn") || e.target.closest(".quick-delete-draft-btn")) return;
        
        var id = parseInt(card.getAttribute("data-id"));
        openDetail(id);
      });
    });

    // Nút Gửi lên Admin nhanh (không cần mở modal)
    container.querySelectorAll(".quick-edit-btn").forEach(function (btn) {
      btn.addEventListener("click", function (e) {
        e.stopPropagation();
        openDetail(parseInt(btn.getAttribute("data-id")));
      });
    });

    container.querySelectorAll(".quick-submit-btn").forEach(function (btn) {
      btn.addEventListener("click", async function (e) {
        e.stopPropagation();
        var id = parseInt(btn.getAttribute("data-id"));
        if (!confirm("Gửi giải đấu này lên Admin để phê duyệt?")) return;
        btn.disabled = true;
        btn.textContent = "Đang gửi...";
        var result = await postApi("/GiaiDauApi/Submit", { ma_giai_dau: id });
        alert(result.message || "Đã xử lý.");
        if (result.success) loadMyTournaments();
        else { btn.disabled = false; btn.textContent = "📤 Gửi lên Admin"; }
      });
    });

    container.querySelectorAll(".quick-delete-draft-btn").forEach(function (btn) {
      btn.addEventListener("click", async function (e) {
        e.stopPropagation();
        var id = parseInt(btn.getAttribute("data-id"));
        if (!confirm("Bạn có chắc chắn muốn xóa bản nháp này không?")) return;
        btn.disabled = true;
        btn.textContent = "Đang xóa...";
        var result = await postApi("/GiaiDauApi/DeleteDraft", { ma_giai_dau: id });
        alert(result.message || "Đã xử lý.");
        if (result.success) loadMyTournaments();
        else { btn.disabled = false; btn.textContent = "Xóa bản nháp"; }
      });
    });
  }

  function setTournamentCardLoading(id, isLoading) {
    var card = document.querySelector('.tournament-card[data-id="' + id + '"]');
    if (!card) return;
    card.setAttribute("aria-busy", isLoading ? "true" : "false");
    card.style.opacity = isLoading ? "0.65" : "";
    card.style.pointerEvents = isLoading ? "none" : "";
    card.querySelectorAll("button").forEach(function (btn) {
      btn.disabled = isLoading;
    });
  }

  // ---- DETAIL MODAL ----
  async function openDetail(id) {
    // DEBUG: console.log để tester kiểm tra trạng thái thực tế từ API
    console.log("[Tournament] Opening detail for ID:", id);

    setTournamentCardLoading(id, true);
    try {
      var result = await getApi("/GiaiDauApi/Detail?maGiaiDau=" + id);
      if (!result.success && !result.Success) {
        alert(result.message || "Không thể tải dữ liệu giải đấu lúc này, vui lòng thử lại sau!");
        return;
      }

      var detail = result.data || result.Data;
      if (!detail || !detail.giai_dau) {
        alert("Không thể tải dữ liệu giải đấu lúc này, vui lòng thử lại sau!");
        return;
      }
      var gd = detail.giai_dau;
      var currentStatus = (gd.trang_thai || "").toLowerCase();

      console.log("[Tournament] Current Status:", currentStatus);

      // IF NOT DRAFT (nhap) -> REDIRECT TO FULL PAGE
      if (currentStatus === "nhap" || currentStatus === "bi_tu_choi") {
        console.log("[Tournament] Opening edit wizard...");
        await openEditTournament(detail);
        return;
      }

      if (["sap_dien_ra", "mo_dang_ky", "khoa_dang_ky", "dang_dien_ra", "ket_thuc", "da_huy"].indexOf(currentStatus) >= 0) {
        console.log("[Tournament] Redirecting to hub...");
        window.location.href = "/tournaments/" + id;
        return;
      }

    // IF DRAFT -> OPEN MODAL (Logic cũ)
    console.log("[Tournament] Opening modal for Draft...");
    var modal = document.getElementById("tournamentDetailModal");
    modal.style.display = "flex";
    document.getElementById("detailTitle").textContent = "Đang tải...";
    document.getElementById("detailBody").innerHTML = "";
    document.getElementById("detailFooter").innerHTML = "";
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
    if (tt === "nhap") {
      // Bản nháp chưa public: Gửi duyệt + Xóa hẳn khỏi DB
      footerHtml += '<button class="btn btn-primary action-btn" onclick="handleAction(\'submit\', ' + id + ')">Gửi phê duyệt</button>';
      footerHtml += '<button class="btn btn-danger action-btn" onclick="handleAction(\'delete-draft\', ' + id + ')">🗑 Xóa bản nháp</button>';
    }
    if (tt === "bi_tu_choi") {
      // Bị từ chối: Gửi lại + Hủy (soft)
      footerHtml += '<button class="btn btn-primary action-btn" onclick="handleAction(\'submit\', ' + id + ')">Gửi phê duyệt lại</button>';
      footerHtml += '<button class="btn btn-danger action-btn" onclick="handleAction(\'cancel\', ' + id + ')">Hủy giải</button>';
    }
    if (tt === "cho_xet_duyet") {
      footerHtml += '<button class="btn btn-success action-btn" onclick="handleAction(\'approve\', ' + id + ')">Phê duyệt</button>';
      footerHtml += '<button class="btn btn-danger reject-btn" data-id="' + id + '">Từ chối</button>';
    }
    if (tt === "sap_dien_ra") {
      footerHtml += '<button class="btn btn-success action-btn" onclick="handleAction(\'open-reg\', ' + id + ')">Mở đăng ký</button>';
      footerHtml += '<button class="btn btn-danger action-btn" onclick="handleAction(\'cancel\', ' + id + ')">Hủy giải</button>';
    }
    if (tt === "mo_dang_ky") {
      footerHtml += '<button class="btn btn-warning action-btn" onclick="handleAction(\'close-reg\', ' + id + ')">Chốt sổ đăng ký</button>';
      footerHtml += '<button class="btn btn-danger action-btn" onclick="handleAction(\'cancel\', ' + id + ')">Hủy giải</button>';
    }
    if (tt === "khoa_dang_ky") {
      footerHtml += '<button class="btn btn-success action-btn" onclick="handleAction(\'start\', ' + id + ')">Khởi tranh</button>';
      footerHtml += '<button class="btn btn-outline-primary action-btn" onclick="handleAction(\'reopen-reg\', ' + id + ')">Mở lại đăng ký</button>';
      footerHtml += '<button class="btn btn-danger action-btn" onclick="handleAction(\'cancel\', ' + id + ')">Hủy giải</button>';
    }
    if (tt === "dang_dien_ra") {
      footerHtml += '<button class="btn btn-primary action-btn" onclick="handleAction(\'complete\', ' + id + ')">Bế mạc giải</button>';
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

    // Attach reject btn event
    document.querySelectorAll("#detailFooter .reject-btn").forEach(function (btn) {
      btn.addEventListener("click", function () { openRejectModal(parseInt(btn.getAttribute("data-id"))); });
    });
    } catch (e) {
      console.error("[Tournament] Detail load failed:", e);
      alert("Không thể tải dữ liệu giải đấu lúc này, vui lòng thử lại sau!");
    } finally {
      setTournamentCardLoading(id, false);
    }
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
    "delete-draft": "/GiaiDauApi/DeleteDraft",   // Hard delete bản nháp
  };

  window.handleAction = async function (action, id) {
    try {
      var confirmMessages = {
        "submit": "Gửi yêu cầu phê duyệt?",
        "approve": "Phê duyệt giải đấu này?",
        "cancel": "Hủy giải đấu? Hành động này không thể hoàn tác.",
        "delete-draft": "Xóa bản nháp? Giải đấu sẽ bị xóa hoàn toàn khỏi database!",
        "start": "Khởi tranh giải đấu?",
        "complete": "Bế mạc giải đấu? Hành động này không thể hoàn tác.",
      };
      if (confirmMessages[action] && !confirm(confirmMessages[action])) return;
      var url = ACTION_ENDPOINTS[action];
      if (!url) {
        alert("Thao tác không hợp lệ.");
        return;
      }
      var result = await postApi(url, { ma_giai_dau: id });
      alert(result.message || "Đã xử lý.");
      if (result.success) {
        document.getElementById("tournamentDetailModal").style.display = "none";
        loadMyTournaments();
        if (document.getElementById("tab-pending")) loadPendingTournaments();
      }
    } catch (e) {
      alert("Lỗi: " + e.message);
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
      postApi("/GiaiDauApi/RegisterTeam", { ma_giai_dau: maGiaiDau, ma_doi: 0 })
          .then(res => alert(res.message));
  };

  // ---- CUSTOM AUTOCOMPLETE MODAL VARIABLES ----
  var currentInviteContext = {
      maGiaiDau: null,
      loai: null,
      selectedId: null
  };
  var inviteDebounceTimer = null;
  var inviteModal = document.getElementById("inviteModal");
  var inviteSearchInput = document.getElementById("inviteSearchInput");
  var inviteSearchSpinner = document.getElementById("inviteSearchSpinner");
  var inviteAutocompleteDropdown = document.getElementById("inviteAutocompleteDropdown");
  var inviteValidationMessage = document.getElementById("inviteValidationMessage");
  var inviteMessage = document.getElementById("inviteMessage");
  var inviteRoleGroup = document.getElementById("inviteRoleGroup");
  var inviteRoleSelect = document.getElementById("inviteRoleSelect");

  window.openInviteModal = function(maGiaiDau, loai) {
      currentInviteContext.maGiaiDau = maGiaiDau;
      currentInviteContext.loai = loai;
      currentInviteContext.selectedId = null;

      // Reset UI
      inviteSearchInput.value = "";
      inviteAutocompleteDropdown.style.display = "none";
      inviteAutocompleteDropdown.innerHTML = "";
      inviteValidationMessage.style.display = "none";
      inviteMessage.value = "";

      var title = "Gửi lời mời";
      if (loai === 'doi') {
          title = "Mời Đội tham gia";
          document.getElementById("inviteInputLabel").innerHTML = "Tìm kiếm Đội <span class='required'>*</span>";
          inviteSearchInput.placeholder = "Gõ tên đội hoặc tên viết tắt...";
          inviteRoleGroup.style.display = "none";
      } else {
          title = loai === 'btc' ? "Mời Ban Tổ Chức" : "Mời Trọng Tài";
          document.getElementById("inviteInputLabel").innerHTML = "Tìm kiếm Người dùng <span class='required'>*</span>";
          inviteSearchInput.placeholder = "Gõ username, email hoặc tên...";
          inviteRoleGroup.style.display = "block";
          
          inviteRoleSelect.innerHTML = "";
          if (loai === 'btc') {
              inviteRoleSelect.innerHTML = '<option value="btc">Ban Tổ Chức</option>';
          } else {
              inviteRoleSelect.innerHTML = '<option value="trong_tai">Trọng Tài</option><option value="trong_tai_chinh">Trọng Tài Chính</option>';
          }
      }
      
      document.getElementById("inviteModalTitle").innerText = title;
      inviteModal.style.display = "flex";
      setTimeout(() => inviteSearchInput.focus(), 100);
  };

  // Handle Close
  var closeInviteModalBtn = document.getElementById("closeInviteModal");
  var cancelInviteModalBtn = document.getElementById("cancelInviteModal");
  function closeInvite() { inviteModal.style.display = "none"; }
  if (closeInviteModalBtn) closeInviteModalBtn.addEventListener("click", closeInvite);
  if (cancelInviteModalBtn) cancelInviteModalBtn.addEventListener("click", closeInvite);
  if (inviteModal) inviteModal.addEventListener("click", function(e) { if (e.target === inviteModal) closeInvite(); });

  // Handle Search Input (Debounce)
  if (inviteSearchInput) {
      inviteSearchInput.addEventListener("input", function() {
          var val = this.value.trim();
          currentInviteContext.selectedId = null; // reset selection on typing
          inviteValidationMessage.style.display = "none";

          clearTimeout(inviteDebounceTimer);
          
          if (val.length < 2) {
              inviteAutocompleteDropdown.style.display = "none";
              inviteSearchSpinner.style.display = "none";
              return;
          }

          inviteSearchSpinner.style.display = "block";
          inviteDebounceTimer = setTimeout(function() {
              var url = currentInviteContext.loai === 'doi' 
                  ? "/DoiApi/Search?keyword=" + encodeURIComponent(val)
                  : "/AuthApi/Search?keyword=" + encodeURIComponent(val);

              fetch(url)
                  .then(res => res.json())
                  .then(res => {
                      inviteSearchSpinner.style.display = "none";
                      renderAutocomplete(res.data || res);
                  })
                  .catch(err => {
                      inviteSearchSpinner.style.display = "none";
                      console.error(err);
                  });
          }, 400);
      });
  }

  function renderAutocomplete(data) {
      inviteAutocompleteDropdown.innerHTML = "";
      if (!data || data.length === 0) {
          inviteAutocompleteDropdown.innerHTML = '<div class="autocomplete-empty">Không tìm thấy dữ liệu phù hợp</div>';
          inviteAutocompleteDropdown.style.display = "block";
          return;
      }

      data.forEach(item => {
          var isDoi = currentInviteContext.loai === 'doi';
          var id = isDoi ? item.ma_doi : item.ma_nguoi_dung;
          var name = isDoi ? item.ten_doi : item.ten_dang_nhap;
          var sub = isDoi ? (item.ten_viet_tat || "Không có tag") : (item.email || "Không có email");
          var avatar = isDoi ? item.logo_url : item.avatar_url;
          
          var initial = name ? name.charAt(0).toUpperCase() : '?';
          var avatarHtml = avatar 
              ? `<img src="${esc(avatar)}" alt="Avatar">` 
              : `<span>${esc(initial)}</span>`;

          var div = document.createElement("div");
          div.className = "autocomplete-item";
          div.innerHTML = `
              <div class="autocomplete-avatar">${avatarHtml}</div>
              <div class="autocomplete-info">
                  <span class="autocomplete-name">${esc(name)}</span>
                  <span class="autocomplete-sub">${esc(sub)}</span>
              </div>
          `;
          
          div.addEventListener("click", function() {
              currentInviteContext.selectedId = id;
              inviteSearchInput.value = name;
              inviteAutocompleteDropdown.style.display = "none";
              inviteValidationMessage.style.display = "none";
          });
          
          inviteAutocompleteDropdown.appendChild(div);
      });
      inviteAutocompleteDropdown.style.display = "block";
  }

  // Handle Submit
  var confirmInviteModalBtn = document.getElementById("confirmInviteModal");
  if (confirmInviteModalBtn) {
      confirmInviteModalBtn.addEventListener("click", function() {
          if (!currentInviteContext.selectedId) {
              inviteValidationMessage.style.display = "block";
              return;
          }

          var msg = inviteMessage.value.trim();
          var payload;
          var endpoint;

          if (currentInviteContext.loai === 'doi') {
              endpoint = "/GiaiDauApi/InviteTeam";
              payload = {
                  ma_giai_dau: currentInviteContext.maGiaiDau,
                  ma_doi: currentInviteContext.selectedId,
                  loi_nhan: msg || "Xin mời tham gia giải đấu"
              };
          } else {
              endpoint = "/GiaiDauApi/InviteNhanSu";
              // We need to pass the username or email. But we have ID.
              // Wait, the API requires username_or_email.
              // Let's pass the input value which is the username!
              var username = inviteSearchInput.value.trim();
              payload = {
                  ma_giai_dau: currentInviteContext.maGiaiDau,
                  username_or_email: username,
                  vai_tro: currentInviteContext.loai === 'btc' ? "btc" : inviteRoleSelect.value,
                  loi_nhan: msg || "Mời hợp tác giải đấu"
              };
          }

          // Disable button to prevent double submit
          confirmInviteModalBtn.disabled = true;
          confirmInviteModalBtn.innerHTML = "Đang gửi...";

          postApi(endpoint, payload)
              .then(res => {
                  confirmInviteModalBtn.disabled = false;
                  confirmInviteModalBtn.innerHTML = "Xác nhận";
                  if (res.success) {
                      alert("Đã gửi lời mời thành công!");
                      closeInvite();
                  } else {
                      alert("Lỗi: " + res.message);
                  }
              })
              .catch(err => {
                  confirmInviteModalBtn.disabled = false;
                  confirmInviteModalBtn.innerHTML = "Xác nhận";
                  alert("Đã xảy ra lỗi khi gửi lời mời.");
                  console.error(err);
              });
      });
  }

  // ---- INIT ----
  loadMyTournaments();
  if (document.getElementById("tab-pending")) loadPendingTournaments();
})();



