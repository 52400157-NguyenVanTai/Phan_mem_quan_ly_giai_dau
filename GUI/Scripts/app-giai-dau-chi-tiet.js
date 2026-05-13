/**
 * app-giai-dau-chi-tiet.js
 * Interactive logic for the Tournament Hub Page
 */

(function () {
    const hub = document.getElementById("tournamentHub");
    if (!hub) return;

    const maGiaiDau = parseInt(hub.getAttribute("data-id"));
    let tournamentData = null;
    let currentResultMatch = null;
    const pendingActions = new Set();

    const STATE_LABELS = {
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

    const FORMAT_LABELS = {
        loai_truc_tiep: "Loại trực tiếp",
        nhanh_thang_nhanh_thua: "Nhánh thắng nhánh thua",
        vong_tron: "Vòng tròn tính điểm",
        vong_tron_tinh_diem: "Vòng tròn tính điểm",
        thuy_si: "Thụy Sĩ",
        battle_royale: "Sinh tồn",
        champion_rush: "Champion Rush",
        league_bang_cheo: "League bảng chéo",
    };

    const MATCH_STATE_LABELS = {
        chua_dau: "Chưa đấu",
        chuan_bi: "Chuẩn bị",
        san_sang: "Sẵn sàng",
        dang_dau: "Đang thi đấu",
        cho_ket_qua: "Chờ kết quả",
        da_hoan_thanh: "Đã hoàn thành",
        huy_bo: "Hủy bỏ",
        bye: "BYE"
    };

    const TEAM_APPROVAL_LABELS = {
        cho_duyet: "Cho duyet",
        da_duyet: "Da duyet",
        bi_tu_choi: "Bi tu choi"
    };

    // INIT
    async function init() {
        ensureOperationTabs();
        await loadTournamentDetail();
        setupTabs();
        setupInviteModal();
        setupQuickResultModal();
    }

    function ensureOperationTabs() {
        const tabs = document.querySelector(".hub-tabs");
        const wrapper = document.querySelector(".hub-tab-content-wrapper");
        if (!tabs || !wrapper || document.querySelector('[data-tab="operators"]')) return;

        const scheduleBtn = document.querySelector('[data-tab="schedule"]');
        const insertBefore = scheduleBtn || document.querySelector('[data-tab="rules"]');
        [
            { tab: "operators", text: "BAN ĐIỀU HÀNH" },
            { tab: "standings", text: "BẢNG XẾP HẠNG", live: true },
        ].forEach(item => {
            const btn = document.createElement("button");
            btn.className = "hub-tab-btn" + (item.live ? " live-only" : "");
            btn.dataset.tab = item.tab;
            btn.textContent = item.text;
            if (item.live) btn.style.display = "none";
            tabs.insertBefore(btn, insertBefore);
        });
        if (scheduleBtn) {
            scheduleBtn.classList.add("live-only");
            scheduleBtn.style.display = "none";
        }

        const operatorsPane = document.createElement("div");
        operatorsPane.className = "hub-tab-pane";
        operatorsPane.id = "pane-operators";
        operatorsPane.innerHTML = '<div class="operators-grid" id="operatorsGrid"></div>';
        wrapper.insertBefore(operatorsPane, document.getElementById("pane-schedule"));

        const standingsPane = document.createElement("div");
        standingsPane.className = "hub-tab-pane";
        standingsPane.id = "pane-standings";
        standingsPane.innerHTML = '<div class="standings-table" id="standingsTable"></div>';
        wrapper.insertBefore(standingsPane, document.getElementById("pane-schedule"));

        const schedulePane = document.getElementById("pane-schedule");
        if (schedulePane && !document.getElementById("matchList")) {
            schedulePane.innerHTML = '<div class="match-list" id="matchList"></div>';
        }
    }

    async function loadTournamentDetail() {
        try {
            const result = await getApi(`/GiaiDauApi/Detail?maGiaiDau=${maGiaiDau}`);
            if (!isResponseSuccess(result)) {
                showLoadError(result && result.message);
                return;
            }

            tournamentData = result.data || result.Data;
            renderHeader();
            renderPrizePool();
            renderSidebar();
            renderActiveTab();
        } catch (e) {
            console.error("Error loading detail:", e);
            showLoadError();
        }
    }

    function showLoadError(message) {
        const title = document.getElementById("hubTitle");
        const actions = document.getElementById("hubActions");
        const info = document.getElementById("infoList");
        const teams = document.getElementById("teamsGrid");
        const text = message || "Không thể tải dữ liệu giải đấu lúc này, vui lòng thử lại sau!";
        if (title) title.textContent = "Không thể tải giải đấu";
        if (actions) {
            actions.innerHTML = `<button class="hub-btn-outline" type="button" id="retryLoadTournament">Thử lại</button>`;
            const retry = document.getElementById("retryLoadTournament");
            if (retry) retry.onclick = loadTournamentDetail;
        }
        if (info) info.innerHTML = `<div class="empty-state">${escapeHtml(text)}</div>`;
        if (teams) teams.innerHTML = "";
    }

    function renderHeader() {
        const gd = tournamentData.giai_dau;
        
        // Banner
        if (gd.banner_url) {
            document.getElementById("hubBanner").style.backgroundImage = `url(${gd.banner_url})`;
        }

        // Logo
        const logoImg = document.getElementById("hubLogoImg");
        const placeholder = document.querySelector(".logo-placeholder");
        if (gd.banner_url) { // Using banner as logo if no specific logo
             logoImg.src = gd.banner_url;
             logoImg.style.display = "block";
             placeholder.style.display = "none";
        }

        // Title
        document.getElementById("hubTitle").textContent = gd.ten_giai_dau;

        // Badges
        const badges = document.getElementById("hubBadges");
        badges.innerHTML = `
            <span class="hub-badge badge-game">${gd.ten_game || "Game"}</span>
            <span class="hub-badge badge-format">${FORMAT_LABELS[gd.the_thuc] || "Tournament"}</span>
            <span class="hub-badge badge-status">${STATE_LABELS[gd.trang_thai] || gd.trang_thai}</span>
        `;

        const isLive = gd.trang_thai === "dang_dien_ra" || gd.trang_thai === "ket_thuc";
        document.querySelectorAll(".live-only").forEach(el => {
            el.style.display = isLive ? "" : "none";
        });

        // Meta
        document.getElementById("hubViews").innerHTML = `<i class="far fa-eye"></i> ${Math.floor(Math.random() * 1000) + 100} lượt xem`;
    }

    function renderPrizePool() {
        const gd = tournamentData.giai_dau;
        const prizes = tournamentData.danh_sach_giai_thuong || [];
        
        function formatPrice(val) {
            return val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        }

        document.getElementById("prizeTotal").textContent = formatPrice(gd.tong_giai_thuong || 0) + " VNĐ";
        
        const prizeListContainer = document.getElementById("prizeList");
        if (!prizeListContainer) return;
        prizeListContainer.innerHTML = "";
        
        prizes.forEach((prize, index) => {
            const card = document.createElement("div");
            card.className = "prize-card rank-" + (index + 1);
            card.innerHTML = `
                <span class="rank-label">${prize.ten_giai}</span>
                <span class="rank-value">${formatPrice(prize.gia_tri)} VNĐ</span>
            `;
            prizeListContainer.appendChild(card);
        });
    }

    function renderSidebar() {
        const gd = tournamentData.giai_dau;
        const stages = tournamentData.giai_doan || [];
        const teams = tournamentData.doi_tham_gia || [];

        // Slot progress
        const registeredCount = gd.so_doi_dang_ky || teams.length || 0;
        const approvedCount = gd.so_doi_da_duyet || teams.filter(t => t.trang_thai_duyet === "da_duyet").length || 0;
        const maxTeams = gd.so_doi_toi_da || 16;
        document.getElementById("slotCount").textContent = `${approvedCount}/${maxTeams}`;
        const percent = Math.min(100, (approvedCount / maxTeams) * 100);
        document.getElementById("slotBar").style.width = `${percent}%`;

        // Stats
        document.getElementById("statTeams").textContent = registeredCount;
        document.getElementById("statSlots").textContent = maxTeams;
        document.getElementById("statStages").textContent = stages.length;

        // Actions
        renderActions();
    }

    function renderActions() {
        const gd = tournamentData.giai_dau;
        const container = document.getElementById("hubActions");
        container.innerHTML = "";

        const tt = gd.trang_thai;
        const isBTC = gd.is_btc || gd.ma_nguoi_tao === parseInt(hub.dataset.userId);

        // 1. KHU VỰC ĐẠI CHÚNG (Public Action)
        if (tt === "mo_dang_ky") {
            const regBtn = document.createElement("button");
            regBtn.className = "hub-btn-primary";
            regBtn.textContent = "ĐĂNG KÝ THAM GIA";
            regBtn.onclick = () => window.openRegisterTeamModal(maGiaiDau);
            container.appendChild(regBtn);
        }

        // 2. KHU VỰC BẢNG ĐIỀU KHIỂN BTC (Admin Panel)
        if (isBTC) {
            const adminBox = document.createElement("div");
            adminBox.className = "admin-panel-box";
            adminBox.innerHTML = `<h4 class="block-title">BẢNG ĐIỀU KHIỂN BTC</h4>`;

            // Mở/Đóng Đăng Ký Toggle
            if (tt === "sap_dien_ra" || tt === "mo_dang_ky" || tt === "khoa_dang_ky") {
                const toggleRegBtn = document.createElement("button");
                if (tt === "mo_dang_ky") {
                    toggleRegBtn.className = "hub-btn-warning";
                    toggleRegBtn.textContent = "DUNG DANG KY";
                    toggleRegBtn.onclick = () => handleAction("toggle-reg", maGiaiDau, { mo_dang_ky: false });
                } else if (tt === "sap_dien_ra") {
                    toggleRegBtn.className = "hub-btn-primary";
                    toggleRegBtn.textContent = "MO DANG KY";
                    toggleRegBtn.onclick = () => handleAction("toggle-reg", maGiaiDau, { mo_dang_ky: true });
                } else {
                    toggleRegBtn.className = "hub-btn-outline";
                    toggleRegBtn.textContent = "DA KHOA DANG KY";
                    toggleRegBtn.disabled = true;
                }
                adminBox.appendChild(toggleRegBtn);
            }

            // Mời BTC
            const inviteBtcBtn = document.createElement("button");
            inviteBtcBtn.className = "hub-btn-outline";
            inviteBtcBtn.textContent = "MỜI BAN TỔ CHỨC";
            inviteBtcBtn.onclick = () => window.openInviteModal(maGiaiDau, "btc");
            adminBox.appendChild(inviteBtcBtn);

            // Mời Trọng Tài
            const inviteRefBtn = document.createElement("button");
            inviteRefBtn.className = "hub-btn-outline";
            inviteRefBtn.textContent = "MỜI TRỌNG TÀI";
            inviteRefBtn.onclick = () => window.openInviteModal(maGiaiDau, "trong_tai");
            adminBox.appendChild(inviteRefBtn);

            // Mời Đội
            const inviteTeamBtn = document.createElement("button");
            inviteTeamBtn.className = "hub-btn-outline";
            inviteTeamBtn.textContent = "MỜI ĐỘI";
            inviteTeamBtn.onclick = () => window.openInviteModal(maGiaiDau, "doi");
            adminBox.appendChild(inviteTeamBtn);

            // Khởi Tranh (Placeholder)
            if (tt === "sap_dien_ra" || tt === "mo_dang_ky" || tt === "khoa_dang_ky") {
                const startBtn = document.createElement("button");
                startBtn.className = "hub-btn-outline";
                startBtn.textContent = "KHOI TRANH GIAI DAU";
                startBtn.textContent = "KHỞI TRANH GIẢI ĐẤU";
                startBtn.onclick = () => alert("Chức năng Khởi Tranh đang được hoàn thiện.");
                startBtn.onclick = () => handleAction("start", maGiaiDau);
                adminBox.appendChild(startBtn);
            }

            // Hủy Giải Đấu
            if (tt !== "ket_thuc" && tt !== "da_huy") {
                const cancelBtn = document.createElement("button");
                cancelBtn.className = "hub-btn-danger";
                cancelBtn.textContent = "HỦY GIẢI ĐẤU";
                cancelBtn.onclick = () => handleAction("cancel", maGiaiDau);
                adminBox.appendChild(cancelBtn);
            }

            container.appendChild(adminBox);
        }
    }

    function setupTabs() {
        const btns = document.querySelectorAll(".hub-tab-btn");
        btns.forEach(btn => {
            btn.addEventListener("click", () => {
                btns.forEach(b => b.classList.remove("active"));
                btn.classList.add("active");
                
                const tab = btn.dataset.tab;
                document.querySelectorAll(".hub-tab-pane").forEach(p => p.classList.remove("active"));
                document.getElementById(`pane-${tab}`).classList.add("active");
                
                renderActiveTab();
            });
        });
    }

    function renderActiveTab() {
        const activeBtn = document.querySelector(".hub-tab-btn.active");
        const tab = activeBtn.dataset.tab;

        if (tab === "info") renderInfoTab();
        if (tab === "teams") renderTeamsTab();
        if (tab === "operators") renderOperatorsTab();
        if (tab === "standings") renderStandingsTab();
        if (tab === "schedule") renderScheduleTab();
        if (tab === "bracket") renderBracketTab();
        if (tab === "rules") renderRulesTab();
    }

    function renderInfoTab() {
        const gd = tournamentData.giai_dau;
        const stages = tournamentData.giai_doan || [];
        const container = document.getElementById("infoList");
        
        const rows = [
            { label: "Tựa Game", value: gd.ten_game || "Chưa chọn" },
            { label: "Thể thức", value: formatStageFormats(stages, gd.the_thuc) },
            { label: "Số đội tối đa", value: gd.so_doi_toi_da || "Không giới hạn" },
            { label: "Đơn vị tổ chức", value: gd.ten_nguoi_tao || "Esport Manager" },
            { label: "Các giai đoạn", value: stages.map(s => s.ten_giai_doan).join(" → ") || "N/A" }
        ];

        container.innerHTML = rows.map(r => `
            <div class="info-row">
                <span class="info-label">${r.label}</span>
                <span class="info-value">${r.value}</span>
            </div>
        `).join("");
    }


    function formatStageFormats(stages, fallbackFormat) {
        if (!stages || !stages.length) return FORMAT_LABELS[fallbackFormat] || fallbackFormat || "N/A";
        return stages
            .slice()
            .sort((a, b) => (a.so_thu_tu || 0) - (b.so_thu_tu || 0))
            .map(stage => {
                const name = stage.ten_giai_doan || ("Vong " + (stage.so_thu_tu || ""));
                const format = FORMAT_LABELS[stage.the_thuc] || stage.the_thuc || "N/A";
                return name + ": " + format;
            })
            .join(" → ");
    }

    function renderTeamsTab() {
        const teams = tournamentData.doi_tham_gia || [];
        const container = document.getElementById("teamsGrid");
        
        if (teams.length === 0) {
            container.innerHTML = '<div class="empty-state">Chưa có đội nào tham gia.</div>';
            return;
        }

        container.innerHTML = teams.map(t => `
            <div class="hub-team-card" role="button" tabindex="0" onclick="window.location.href='/Doi/ChiTiet/${t.ma_nhom}'">
                <div class="hub-team-logo">
                    ${t.logo_url ? `<img src="${t.logo_url}" alt="Logo">` : `<div class="logo-placeholder" style="font-size: 1.5rem;"><i class="fas fa-users"></i></div>`}
                </div>
                <span class="hub-team-name">${escapeHtml(t.ten_doi)}</span>
                <span class="hub-team-tag">${escapeHtml(t.ten_viet_tat || "")}</span>
                <span class="hub-team-tag">${escapeHtml(TEAM_APPROVAL_LABELS[t.trang_thai_duyet] || t.trang_thai_duyet || "")}</span>
            </div>
        `).join("");
    }

    function renderOperatorsTab() {
        const people = tournamentData.nhan_su || [];
        const container = document.getElementById("operatorsGrid");
        if (!container) return;
        const btc = people.filter(x => x.vai_tro_giai === "ban_to_chuc");
        const refs = people.filter(x => x.vai_tro_giai === "trong_tai");
        container.innerHTML = [renderPeopleList("Ban Tổ Chức (BTC)", btc), renderPeopleList("Trọng Tài", refs)].join("");
    }

    function avatarFallbackHtml() {
        return '<span class="operator-avatar operator-avatar-fallback"><i class="fas fa-user"></i></span>';
    }

    function createAvatarFallback() {
        const span = document.createElement("span");
        span.className = "operator-avatar operator-avatar-fallback";
        span.innerHTML = '<i class="fas fa-user"></i>';
        return span;
    }

    function renderPeopleList(title, items) {
        const body = items.length ? items.map(p => `
            <div class="operator-row">
                ${p.avatar_url ? `<img class="operator-avatar" src="${p.avatar_url}" onerror="this.replaceWith(createAvatarFallback())" alt="">` : avatarFallbackHtml()}
                <div>
                    <strong>${escapeHtml(p.ten_dang_nhap)}</strong>
                    <div class="muted">${escapeHtml(p.email || "")}</div>
                </div>
            </div>
        `).join("") : '<div class="empty-state">Chưa có nhân sự.</div>';
        return `<section class="operator-section"><h3 class="block-title">${title}</h3>${body}</section>`;
    }

    function renderStandingsTab() {
        const rows = tournamentData.bang_xep_hang || [];
        const container = document.getElementById("standingsTable");
        if (!container) return;
        if (!rows.length) {
            container.innerHTML = '<div class="empty-state">Bảng xếp hạng sẽ xuất hiện sau khi khởi tranh.</div>';
            return;
        }
        container.innerHTML = `
            <table class="table">
                <thead><tr><th>#</th><th>Đội</th><th>Trận</th><th>Thắng</th><th>Thua</th><th>Điểm</th></tr></thead>
                <tbody>${rows.map((r, i) => `
                    <tr>
                        <td>${r.thu_hang_hien_tai || i + 1}</td>
                        <td>${escapeHtml(r.ten_doi)}</td>
                        <td>${r.so_tran_da_dau}</td>
                        <td>${r.so_tran_thang}</td>
                        <td>${r.so_tran_thua}</td>
                        <td>${r.diem_tong_ket}</td>
                    </tr>
                `).join("")}</tbody>
            </table>`;
    }

    function renderScheduleTab() {
        const matches = uniqueMatches(tournamentData.tran_dau || []);
        const container = document.getElementById("matchList");
        if (!container) return;
        if (!matches.length) {
            container.innerHTML = '<div class="empty-state">Lich thi dau se hien thi khi giai dau bat dau.</div>';
            return;
        }
        renderMatchList(container, matches);
    }

    function renderBracketTab() {
        const matches = uniqueMatches(tournamentData.tran_dau || []);
        const container = document.getElementById("pane-bracket");
        if (!container) return;
        if (!matches.length) {
            container.innerHTML = '<div class="empty-state">Cay dau se hien thi khi giai dau bat dau.</div>';
            return;
        }

        const stages = groupBy(matches, m => m.ten_giai_doan || "Giai doan");
        container.innerHTML = '<div class="bracket-stage-list">' + Object.keys(stages).map(stageName => {
            const stageMatches = stages[stageName] || [];
            const hasSwiss = stageMatches.some(m => String(m.nhanh_dau || "").toLowerCase() === "swiss");
            const hasDoubleElim = stageMatches.some(m => ["winners", "losers", "grand_final"].indexOf(String(m.nhanh_dau || "").toLowerCase()) >= 0);
            const body = hasDoubleElim
                ? renderDoubleElimBracketHtml(stageMatches)
                : (hasSwiss ? renderSwissRoundsHtml(stageMatches) : renderKnockoutBracketHtml(stageMatches));
            return '<section class="bracket-stage-section"><h3 class="block-title">' + escapeHtml(stageName) + '</h3>' + body + '</section>';
        }).join("") + '</div>';
        bindMatchButtons(container);
    }

    function uniqueMatches(matches) {
        const byId = new Map();
        (matches || []).forEach(m => { if (m && !byId.has(m.ma_tran)) byId.set(m.ma_tran, m); });
        return Array.from(byId.values());
    }

    function renderMatchList(container, matches) {
        const isBTC = tournamentData.giai_dau.is_btc || tournamentData.giai_dau.ma_nguoi_tao === parseInt(hub.dataset.userId);
        container.innerHTML = matches.map(m => renderMatchCard(m, isBTC)).join("");
        bindMatchButtons(container);
    }

    function renderSwissRounds(container, matches) {
        container.innerHTML = renderSwissRoundsHtml(matches);
        bindMatchButtons(container);
    }

    function renderSwissRoundsHtml(matches) {
        const rounds = groupBy(matches, m => m.vong_dau || "Vong chua xep");
        const isBTC = tournamentData.giai_dau.is_btc || tournamentData.giai_dau.ma_nguoi_tao === parseInt(hub.dataset.userId);
        return '<div class="round-list">' + Object.keys(rounds).map(round =>
            '<section class="round-section"><h3>' + escapeHtml(round) + '</h3><div class="match-list compact">' +
            rounds[round].map(m => renderMatchCard(m, isBTC)).join("") + '</div></section>'
        ).join("") + '</div>';
    }

    function renderKnockoutBracket(container, matches) {
        container.innerHTML = renderKnockoutBracketHtml(matches);
        bindMatchButtons(container);
    }

    function renderKnockoutBracketHtml(matches) {
        const roundNames = [];
        matches.forEach(m => { const name = m.vong_dau || "Vong dau"; if (roundNames.indexOf(name) < 0) roundNames.push(name); });
        const rounds = groupBy(matches, m => m.vong_dau || "Vong dau");
        const isBTC = tournamentData.giai_dau.is_btc || tournamentData.giai_dau.ma_nguoi_tao === parseInt(hub.dataset.userId);
        return '<div class="bracket-board">' + roundNames.map(name =>
            '<section class="bracket-round"><h3>' + escapeHtml(name) + '</h3><div class="bracket-stack">' +
            (rounds[name] || []).map(m => renderBracketMatch(m, isBTC)).join("") + '</div></section>'
        ).join("") + '</div>';
    }

    function renderDoubleElimBracketHtml(matches) {
        const lanes = [
            { key: "winners", title: "Nhanh thang" },
            { key: "losers", title: "Nhanh thua" },
            { key: "grand_final", title: "Chung ket tong" }
        ];
        const isBTC = tournamentData.giai_dau.is_btc || tournamentData.giai_dau.ma_nguoi_tao === parseInt(hub.dataset.userId);
        return '<div class="double-elim-board">' + lanes.map(lane => {
            const laneMatches = matches.filter(m => String(m.nhanh_dau || "").toLowerCase() === lane.key);
            if (!laneMatches.length) return "";
            return '<section class="double-elim-lane"><h3>' + lane.title + '</h3><div class="bracket-stack">' +
                laneMatches.map(m => renderBracketMatch(m, isBTC)).join("") +
                '</div></section>';
        }).join("") + '</div>';
    }

    function renderBracketMatch(m, isBTC) {
        const teams = m.chi_tiet || [];
        const canScore = canWriteResult(m, isBTC);
        const next = m.ma_tran_tiep_theo_thang ? '<div class="bracket-next">Thang -> #' + m.ma_tran_tiep_theo_thang + '</div>' : "";
        return '<article class="bracket-match" data-match-id="' + m.ma_tran + '">' +
            '<div class="bracket-match-code">#' + m.ma_tran + '</div>' +
            '<div class="bracket-team">' + escapeHtml(teams[0] && teams[0].ten_doi || "Cho doi thang") + '</div>' +
            '<div class="bracket-team">' + escapeHtml(teams[1] && teams[1].ten_doi || "Cho doi thang") + '</div>' +
            '<div class="muted">' + escapeHtml(MATCH_STATE_LABELS[m.trang_thai] || m.trang_thai) + '</div>' + next +
            '<div class="match-actions bracket-actions">' + (canScore ? '<button class="hub-btn-outline js-stats-match" data-id="' + m.ma_tran + '">Ghi ket qua</button>' : "") + '</div>' +
            '</article>';
    }

    function renderMatchCard(m, isBTC) {
        const teams = (m.chi_tiet || []).map(c => escapeHtml(c.ten_doi)).join(" vs ");
        const canScore = canWriteResult(m, isBTC);
        return '<article class="match-card" data-match-id="' + m.ma_tran + '"><div>' +
            '<div class="match-title">' + escapeHtml(m.vong_dau || m.ten_giai_doan || "Tran dau") + '</div>' +
            '<div class="match-teams">' + (teams || "Dang cho doi") + '</div>' +
            '<div class="muted">Trong tai: ' + escapeHtml(m.ten_trong_tai || "Chua chon") + ' - ' + escapeHtml(MATCH_STATE_LABELS[m.trang_thai] || m.trang_thai) + '</div></div>' +
            '<div class="match-actions">' +
            (canScore ? '<button class="hub-btn-outline js-stats-match" data-id="' + m.ma_tran + '">Ghi ket qua</button>' : "") +
            '</div></article>';
    }

    function bindMatchButtons(container) {
        container.querySelectorAll(".js-stats-match").forEach(btn => btn.onclick = () => openStatsModal(parseInt(btn.dataset.id)));
    }

    function canWriteResult(match, isBTC) {
        const currentUserId = parseInt(hub.dataset.userId || "0", 10);
        const isReferee = !!match.ma_trong_tai && match.ma_trong_tai === currentUserId;
        const blocked = ["da_hoan_thanh", "huy_bo", "bye"].indexOf(match.trang_thai) >= 0;
        return (isBTC || isReferee) && !blocked && (match.chi_tiet || []).length >= 2;
    }

    function groupBy(items, selector) {
        return (items || []).reduce((acc, item) => {
            const key = selector(item);
            if (!acc[key]) acc[key] = [];
            acc[key].push(item);
            return acc;
        }, {});
    }
    async function postMatchAction(url, maTran) {
        const result = await postApi(url, { ma_tran: maTran });
        notify(result.message || "Đã xử lý.");
        if (result.success) await loadTournamentDetail();
    }

    function openSetupMatchModal(maTran) {
        const modal = document.getElementById("setupMatchModal");
        const matchId = document.getElementById("setupMatchId");
        const refereeSelect = document.getElementById("setupMatchReferee");
        const formatSelect = document.getElementById("setupMatchFormat");
        const roundsGroup = document.getElementById("setupMatchRoundsGroup");
        const roundsInput = document.getElementById("setupMatchRounds");
        const validation = document.getElementById("setupMatchValidation");
        const refs = (tournamentData.nhan_su || []).filter(x => x.vai_tro_giai === "trong_tai");
        if (!refs.length) {
            notify("Chưa có trọng tài trong giải.");
            return;
        }

        const match = (tournamentData.tran_dau || []).find(x => x.ma_tran === maTran);
        const gameName = (tournamentData.giai_dau.ten_game || "").toLowerCase();
        const isBR = gameName.indexOf("pubg") >= 0 || gameName.indexOf("free fire") >= 0;
        const currentFormat = match && match.the_thuc_tran ? match.the_thuc_tran : (isBR ? "SinhTon" : "BO1");

        matchId.value = maTran;
        refereeSelect.innerHTML = refs.map(r =>
            `<option value="${r.ma_nguoi_dung}">${escapeHtml(r.ten_dang_nhap)}${r.email ? " - " + escapeHtml(r.email) : ""}</option>`
        ).join("");
        if (match && match.ma_trong_tai) refereeSelect.value = String(match.ma_trong_tai);
        formatSelect.value = currentFormat;
        roundsInput.value = match && match.so_vong ? match.so_vong : (isBR ? 5 : "");
        roundsGroup.style.display = formatSelect.value === "SinhTon" ? "block" : "none";
        validation.style.display = "none";
        validation.textContent = "";
        modal.style.display = "flex";
    }

    function setupMatchSetupModal() {
        const modal = document.getElementById("setupMatchModal");
        if (!modal) return;
        const closeBtn = document.getElementById("closeSetupMatchModal");
        const cancelBtn = document.getElementById("cancelSetupMatchModal");
        const confirmBtn = document.getElementById("confirmSetupMatchModal");
        const formatSelect = document.getElementById("setupMatchFormat");
        const roundsGroup = document.getElementById("setupMatchRoundsGroup");
        const validation = document.getElementById("setupMatchValidation");

        const close = () => { modal.style.display = "none"; };
        closeBtn.onclick = close;
        cancelBtn.onclick = close;
        modal.addEventListener("click", e => { if (e.target === modal) close(); });
        formatSelect.onchange = () => {
            roundsGroup.style.display = formatSelect.value === "SinhTon" ? "block" : "none";
        };
        confirmBtn.onclick = async () => {
            const maTran = parseInt(document.getElementById("setupMatchId").value, 10);
            const maTrongTai = parseInt(document.getElementById("setupMatchReferee").value, 10);
            const format = formatSelect.value;
            const rounds = format === "SinhTon" ? parseInt(document.getElementById("setupMatchRounds").value, 10) : null;
            if (!maTran || !maTrongTai || !format || (format === "SinhTon" && (!rounds || rounds < 1))) {
                validation.textContent = "Vui lòng điền đầy đủ thông tin hợp lệ.";
                validation.style.display = "block";
                return;
            }
            confirmBtn.disabled = true;
            const result = await postApi("/GiaiDauApi/SetupMatch", {
                ma_tran: maTran,
                ma_trong_tai: maTrongTai,
                the_thuc_tran: format,
                so_vong: rounds
            });
            confirmBtn.disabled = false;
            notify(result.message);
            if (result.success) {
                close();
                await loadTournamentDetail();
            }
        };
    }
    function setupQuickResultModal() {
        const modal = document.getElementById("quickResultModal");
        if (!modal) return;
        const close = () => { modal.style.display = "none"; currentResultMatch = null; };
        document.getElementById("closeQuickResultModal").onclick = close;
        document.getElementById("cancelQuickResultModal").onclick = close;
        modal.addEventListener("click", e => { if (e.target === modal) close(); });
        document.getElementById("quickResultSave").onclick = submitQuickResult;
    }

    function openStatsModal(maTran) {
        const match = (tournamentData.tran_dau || []).find(x => x.ma_tran === maTran);
        if (!match) return;
        if (!canWriteResult(match, tournamentData.giai_dau.is_btc || tournamentData.giai_dau.ma_nguoi_tao === parseInt(hub.dataset.userId || "0", 10))) {
            notify("Ban khong co quyen hoac tran dau chua san sang de ghi ket qua.");
            return;
        }

        currentResultMatch = match;
        const teams = match.chi_tiet || [];
        const isBR = match.the_thuc_tran === "SinhTon";
        document.getElementById("quickResultMatchId").value = match.ma_tran;
        document.getElementById("quickResultTitle").textContent = "Ghi ket qua tran #" + match.ma_tran;
        document.getElementById("quickResultMessage").style.display = "none";
        document.getElementById("quickGameNo").closest(".form-group").style.display = "none";
        document.getElementById("quickResultBoFields").style.display = isBR ? "none" : "block";
        document.getElementById("quickResultBrFields").style.display = isBR ? "block" : "none";

        if (isBR) {
            configureQuickBoControls(match, false);
            renderQuickBattleRoyaleRows(teams);
        } else {
            configureQuickBoControls(match, true);
            renderQuickBoFields(teams);
        }

        document.getElementById("quickResultModal").style.display = "flex";
    }

    function configureQuickBoControls(match, visible) {
        const wrap = document.getElementById("quickBoSettings");
        const formatSelect = document.getElementById("quickResultFormat");
        const gameCount = document.getElementById("quickResultGameCount");
        if (!wrap || !formatSelect || !gameCount) return;
        wrap.style.display = visible ? "grid" : "none";
        if (!visible) return;
        const format = /^BO[1357]$/i.test(match.the_thuc_tran || "") ? String(match.the_thuc_tran).toUpperCase() : "BO" + parseBoCount(match.the_thuc_tran || ("BO" + (match.so_vong || 1)));
        formatSelect.value = ["BO1", "BO3", "BO5", "BO7"].indexOf(format) >= 0 ? format : "BO1";
        gameCount.value = parseBoCount(formatSelect.value);
        formatSelect.onchange = () => {
            gameCount.value = parseBoCount(formatSelect.value);
            renderQuickBoFields(currentResultMatch.chi_tiet || []);
        };
        gameCount.onchange = () => {
            let count = Math.max(1, Math.min(7, parseInt(gameCount.value, 10) || 1));
            if (count % 2 === 0) count += 1;
            gameCount.value = count;
            renderQuickBoFields(currentResultMatch.chi_tiet || []);
        };
    }

    function renderQuickBoFields(teams) {
        teams = teams.slice(0, 2);
        const selectedFormat = document.getElementById("quickResultFormat") ? document.getElementById("quickResultFormat").value : currentResultMatch.the_thuc_tran;
        const totalGames = Math.max(1, Math.min(7, parseInt(document.getElementById("quickResultGameCount").value, 10) || parseBoCount(selectedFormat || ("BO" + (currentResultMatch.so_vong || 1)))));
        const blocks = [];
        for (let i = 1; i <= totalGames; i++) {
            blocks.push('<section class="result-game-card mb-3 quick-bo-game" data-game="' + i + '">' +
                '<div class="result-game-head"><h4 class="result-game-title">Game ' + i + '</h4><span class="badge quick-game-state">Dang nhap</span></div>' +
                '<div class="result-game-grid">' +
                '<div class="form-group"><label>Doi thang game nay</label><select class="form-control quick-game-winner"><option value="">Chon doi thang</option>' +
                teams.map(t => '<option value="' + t.ma_nhom + '">' + escapeHtml(t.ten_doi) + '</option>').join("") +
                '</select></div>' +
                '<div class="form-group"><label>' + escapeHtml(teams[0] && teams[0].ten_doi || "Doi 1") + ' kills</label><input class="form-control quick-kill-one" type="number" min="0" value="0"></div>' +
                '<div class="form-group"><label>' + escapeHtml(teams[1] && teams[1].ten_doi || "Doi 2") + ' kills</label><input class="form-control quick-kill-two" type="number" min="0" value="0"></div>' +
                '</div></section>');
        }
        document.getElementById("quickResultBoFields").innerHTML = '<div class="alert alert-info py-2">Can ' + (Math.floor(totalGames / 2) + 1) + ' game thang de ket thuc ' + escapeHtml(selectedFormat || ("BO" + totalGames)) + '.</div><div id="quickBoGames">' + blocks.join("") + '</div><div id="quickBoPreview" class="result-preview"></div>';
        document.querySelectorAll("#quickBoGames select").forEach(el => el.onchange = updateQuickBoPreview);
        updateQuickBoPreview();
    }

    function renderQuickBattleRoyaleRows(teams) {
        document.getElementById("quickResultBrFields").innerHTML = '<div class="form-group"><label>So map can nhap</label><input id="soTranBRQuick" class="form-control" type="number" min="1" value="' + (currentResultMatch.so_vong || 1) + '"></div><div id="quickBrGames" style="margin-top: 16px;"></div><div id="quickBrPreview" class="table-responsive" style="margin-top: 16px;"></div>';
        document.getElementById("soTranBRQuick").onchange = renderQuickBrGameBlocks;
        renderQuickBrGameBlocks();
    }

    function brRankPoint(rank, teamCount) {
        if (rank === 1) return 10;
        if (rank === 2) return 6;
        if (rank === 3) return 5;
        if (rank === 4) return 4;
        if (rank === 5) return 3;
        return rank > 0 && rank <= teamCount ? 1 : 0;
    }

    function renderQuickBrGameBlocks() {
        const teams = currentResultMatch.chi_tiet || [];
        const count = Math.max(1, parseInt(document.getElementById("soTranBRQuick").value, 10) || 1);
        const blocks = [];
        for (let i = 1; i <= count; i++) {
            blocks.push('<section class="request-card p-3 mb-3 quick-br-game" data-game="' + i + '"><strong>Map ' + i + '</strong><div class="table-responsive mt-3"><table class="table"><thead><tr><th>Doi</th><th>Hang</th><th>Kills</th><th>Diem</th></tr></thead><tbody>' +
                teams.map(t => '<tr data-br-team="' + t.ma_nhom + '"><td>' + escapeHtml(t.ten_doi) + '</td><td><input class="form-control quick-br-rank" type="number" min="1" max="' + teams.length + '" value="' + (t.thu_hang || teams.length) + '"></td><td><input class="form-control quick-br-kill" type="number" min="0" value="' + (t.so_kill || 0) + '"></td><td class="quick-br-total">0</td></tr>').join("") +
                '</tbody></table></div></section>');
        }
        document.getElementById("quickBrGames").innerHTML = blocks.join("");
        document.querySelectorAll("#quickBrGames input").forEach(el => el.oninput = updateQuickBrPreview);
        updateQuickBrPreview();
    }

    function updateQuickBoPreview() {
        const teams = (currentResultMatch.chi_tiet || []).slice(0, 2);
        const score = {};
        teams.forEach(t => { score[t.ma_nhom] = 0; });
        const target = Math.floor((parseInt(document.getElementById("quickResultGameCount").value, 10) || parseBoCount(currentResultMatch.the_thuc_tran)) / 2) + 1;
        let finished = false;
        document.querySelectorAll(".quick-bo-game").forEach(block => {
            const winner = parseInt(block.querySelector(".quick-game-winner").value, 10) || 0;
            if (!finished && winner) {
                score[winner] = (score[winner] || 0) + 1;
                block.querySelector(".quick-game-state").textContent = "Da tinh";
                block.style.display = "";
                if (score[winner] >= target) finished = true;
            } else if (finished) {
                block.style.display = "none";
                block.querySelector(".quick-game-winner").value = "";
            } else {
                block.querySelector(".quick-game-state").textContent = "Dang nhap";
                block.style.display = "";
            }
        });
        const scoreText = (score[teams[0].ma_nhom] || 0) + "-" + (score[teams[1].ma_nhom] || 0);
        document.getElementById("quickBoPreview").textContent = "Ty so tam tinh: " + scoreText;
        document.getElementById("quickResultSave").textContent = "Luu ket qua (Ty so chung cuoc " + scoreText + ")";
    }

    function updateQuickBrPreview() {
        const teams = currentResultMatch.chi_tiet || [];
        const totals = {};
        teams.forEach(t => { totals[t.ma_nhom] = { name: t.ten_doi, points: 0, kills: 0 }; });
        document.querySelectorAll(".quick-br-game tr[data-br-team]").forEach(row => {
            const id = parseInt(row.dataset.brTeam, 10);
            const rank = parseInt(row.querySelector(".quick-br-rank").value, 10) || teams.length;
            const kills = parseInt(row.querySelector(".quick-br-kill").value, 10) || 0;
            const points = brRankPoint(rank, teams.length) + kills;
            row.querySelector(".quick-br-total").textContent = points;
            totals[id].points += points;
            totals[id].kills += kills;
        });
        document.getElementById("quickBrPreview").innerHTML = '<table class="table"><thead><tr><th>Doi</th><th>Tong diem</th><th>Tong kills</th></tr></thead><tbody>' +
            Object.keys(totals).map(id => totals[id]).sort((a, b) => b.points - a.points || b.kills - a.kills).map(t => '<tr><td>' + escapeHtml(t.name) + '</td><td>' + t.points + '</td><td>' + t.kills + '</td></tr>').join("") +
            '</tbody></table>';
        document.getElementById("quickResultSave").textContent = "Luu ket qua BR";
    }

    function buildQuickResultPayload() {
        const match = currentResultMatch;
        const teams = match.chi_tiet || [];
        if (match.the_thuc_tran === "SinhTon") {
            return {
                ma_tran: match.ma_tran,
                so_van: parseInt(document.getElementById("soTranBRQuick").value, 10) || 1,
                br_games: Array.from(document.querySelectorAll(".quick-br-game")).map(block => ({
                    so_van: parseInt(block.dataset.game, 10),
                    ket_qua: Array.from(block.querySelectorAll("tr[data-br-team]")).map(row => ({
                        ma_nhom: parseInt(row.dataset.brTeam, 10),
                        thu_hang: parseInt(row.querySelector(".quick-br-rank").value, 10),
                        so_kill: parseInt(row.querySelector(".quick-br-kill").value, 10) || 0
                    }))
                }))
            };
        }

        return {
            ma_tran: match.ma_tran,
            so_van: 1,
            the_thuc_tran: document.getElementById("quickResultFormat") ? document.getElementById("quickResultFormat").value : match.the_thuc_tran,
            games: Array.from(document.querySelectorAll(".quick-bo-game"))
                .filter(block => block.style.display !== "none" && (parseInt(block.querySelector(".quick-game-winner").value, 10) || 0))
                .map(block => ({
                so_van: parseInt(block.dataset.game, 10),
                ma_doi_1: teams[0].ma_nhom,
                ma_doi_2: teams[1].ma_nhom,
                ma_doi_thang: parseInt(block.querySelector(".quick-game-winner").value, 10),
                kill_doi_1: parseInt(block.querySelector(".quick-kill-one").value, 10) || 0,
                kill_doi_2: parseInt(block.querySelector(".quick-kill-two").value, 10) || 0
            }))
        };
    }

    async function submitQuickResult() {
        if (!currentResultMatch) return;
        const payload = buildQuickResultPayload();
        if (currentResultMatch.the_thuc_tran !== "SinhTon" && (!payload.games || !payload.games.length)) {
            const message = document.getElementById("quickResultMessage");
            message.textContent = "Vui long chon it nhat mot game thang.";
            message.style.display = "block";
            return;
        }
        if (!confirm("Luu ket qua nay? Du lieu da chinh xac?")) return;
        const button = document.getElementById("quickResultSave");
        const message = document.getElementById("quickResultMessage");
        button.disabled = true;
        const result = await postApi("/GiaiDauApi/SaveMatchResults", payload);
        button.disabled = false;
        if (!result.success) {
            message.textContent = result.message || "Khong the luu ket qua.";
            message.style.display = "block";
            return;
        }
        notify(result.message || "Da luu ket qua.");
        document.getElementById("quickResultModal").style.display = "none";
        currentResultMatch = null;
        await loadTournamentDetail();
    }

    function parseBoCount(format) {

        const found = String(format || "BO1").match(/BO(\d+)/i);
        return found ? parseInt(found[1], 10) : 1;
    }

    function willCompleteMatch(match, payload) {
        if (match.the_thuc_tran === "SinhTon") return payload.so_van >= (match.so_vong || 1);
        const target = Math.floor(parseBoCount(match.the_thuc_tran) / 2) + 1;
        const wins = {};
        (payload.games || []).forEach(g => { wins[g.ma_doi_thang] = (wins[g.ma_doi_thang] || 0) + 1; });
        return Object.keys(wins).some(k => wins[k] >= target);
    }

    function notify(message) {
        alert(message || "Da xu ly.");
    }

    function renderRulesTab() {
        const gd = tournamentData.giai_dau;
        const container = document.getElementById("rulesContent");
        container.innerHTML = `<div style="white-space: pre-wrap; line-height: 1.6; color: var(--hub-text-muted);">${escapeHtml(gd.mo_ta || "Không có mô tả chi tiết.")}</div>`;
    }

    // ACTIONS HELPERS
    async function handleAction(action, id, extraPayload) {
        const actionKey = action + ":" + id;
        if (pendingActions.has(actionKey)) return;
        const confirmMessages = {
            "close-reg": "Chot so dang ky ngay bay gio?",
            "open-reg": "Ban muon mo dang ky cho giai dau nay?",
            "cancel": "Huy giai dau? Hanh dong nay khong the hoan tac.",
        };
        if (action === "cancel" && tournamentData && tournamentData.giai_dau && tournamentData.giai_dau.trang_thai === "sap_dien_ra") {
            confirmMessages.cancel = "Huy giai dau sap dien ra? Giai dau se bi xoa hoan toan khoi database.";
        }
        confirmMessages["toggle-reg"] = extraPayload && extraPayload.mo_dang_ky ? "Mo dang ky cho giai dau nay?" : "Dung dang ky giai dau nay?";
        confirmMessages["start"] = "Khoi tranh giai dau va sinh lich thi dau tu dong?";
        if (confirmMessages[action] && !confirm(confirmMessages[action])) return;

        const endpoints = {
            "close-reg": "/GiaiDauApi/CloseRegistration",
            "open-reg": "/GiaiDauApi/OpenRegistration",
            "toggle-reg": "/GiaiDauApi/ToggleRegistration",
            "start": "/GiaiDauApi/Start",
            "cancel": "/GiaiDauApi/Cancel",
        };
        if (!endpoints[action]) return;

        const activeButton = document.activeElement && document.activeElement.tagName === "BUTTON" ? document.activeElement : null;
        pendingActions.add(actionKey);
        if (activeButton) {
            activeButton.disabled = true;
            activeButton.classList.add("notion-is-loading");
        }

        try {
            const result = await postApi(endpoints[action], Object.assign({ ma_giai_dau: id }, extraPayload || {}));
            notify(result.message);
            if (!result.success) return;

            if (action === "cancel") {
                window.location.href = "/GiaiDau";
                return;
            }
            if (action === "open-reg") {
                tournamentData.giai_dau.dang_mo_dang_ky = true;
                tournamentData.giai_dau.trang_thai = "mo_dang_ky";
            }
            if (action === "close-reg") {
                tournamentData.giai_dau.dang_mo_dang_ky = false;
                tournamentData.giai_dau.trang_thai = "khoa_dang_ky";
            }
            if (action === "toggle-reg") {
                const fresh = getResponseData(result);
                if (fresh) tournamentData.giai_dau = Object.assign(tournamentData.giai_dau, fresh);
                else {
                    tournamentData.giai_dau.trang_thai = extraPayload && extraPayload.mo_dang_ky ? "mo_dang_ky" : "khoa_dang_ky";
                    tournamentData.giai_dau.dang_mo_dang_ky = tournamentData.giai_dau.trang_thai === "mo_dang_ky";
                }
                await loadTournamentDetail();
                return;
            }
            if (action === "start") {
                await loadTournamentDetail();
                return;
            }
            renderHeader();
            renderActions();
        } finally {
            pendingActions.delete(actionKey);
            if (activeButton) {
                activeButton.disabled = false;
                activeButton.classList.remove("notion-is-loading");
            }
        }
    }

    window.openRegisterTeamModal = function(maGiaiDau) {
        postApi("/GiaiDauApi/RegisterTeam", { ma_giai_dau: maGiaiDau, ma_doi: 0 })
            .then(res => {
                alert(res.message);
                if (res.success) loadTournamentDetail();
            });
    };

    // INVITATION MODAL LOGIC (ADAPTED)
    function setupInviteModal() {
        const inviteModal = document.getElementById("inviteModal");
        const inviteSearchInput = document.getElementById("inviteSearchInput");
        const inviteSearchSpinner = document.getElementById("inviteSearchSpinner");
        const inviteAutocompleteDropdown = document.getElementById("inviteAutocompleteDropdown");
        const inviteValidationMessage = document.getElementById("inviteValidationMessage");
        const inviteMessage = document.getElementById("inviteMessage");
        const inviteRoleGroup = document.getElementById("inviteRoleGroup");
        const inviteRoleSelect = document.getElementById("inviteRoleSelect");
        
        let currentInviteContext = { maGiaiDau: null, loai: null, selectedId: null };
        let inviteDebounceTimer = null;

        window.openInviteModal = function(maGiaiDau, loai) {
            currentInviteContext = { maGiaiDau, loai, selectedId: null };
            
            // Reset UI
            inviteSearchInput.value = "";
            inviteAutocompleteDropdown.style.display = "none";
            inviteValidationMessage.style.display = "none";
            inviteMessage.value = "";

            let title = loai === "doi" ? "Mời Đội tham gia" : (loai === "btc" ? "Mời Ban Tổ Chức" : "Mời Trọng Tài");
            document.getElementById("inviteModalTitle").innerText = title;
            document.getElementById("inviteInputLabel").innerText = loai === "doi" ? "Tìm kiếm Đội" : "Tìm kiếm Người dùng";
            
            inviteRoleGroup.style.display = loai === "doi" ? "none" : "block";
            if (loai !== "doi") {
                inviteRoleSelect.innerHTML = loai === "btc" 
                    ? '<option value="btc">Ban Tổ Chức</option>'
                    : '<option value="trong_tai">Trọng Tài</option><option value="trong_tai_chinh">Trọng Tài Chính</option>';
            }

            inviteModal.style.display = "flex";
        };

        const closeInvite = () => { inviteModal.style.display = "none"; };
        document.getElementById("closeInviteModal").onclick = closeInvite;
        document.getElementById("cancelInviteModal").onclick = closeInvite;

        inviteSearchInput.oninput = function() {
            const val = this.value.trim();
            currentInviteContext.selectedId = null;
            clearTimeout(inviteDebounceTimer);
            if (val.length < 2) {
                inviteAutocompleteDropdown.style.display = "none";
                return;
            }

            inviteSearchSpinner.style.display = "block";
            inviteDebounceTimer = setTimeout(() => {
                const url = currentInviteContext.loai === "doi" 
                    ? `/DoiApi/Search?keyword=${encodeURIComponent(val)}`
                    : `/AuthApi/Search?keyword=${encodeURIComponent(val)}`;

                fetch(url).then(res => res.json()).then(res => {
                    inviteSearchSpinner.style.display = "none";
                    renderAutocomplete(res.data || res);
                }).catch(err => {
                    inviteSearchSpinner.style.display = "none";
                    console.error("Invite search failed:", err);
                    inviteAutocompleteDropdown.innerHTML = '<div class="autocomplete-empty">Không thể tìm kiếm lúc này.</div>';
                    inviteAutocompleteDropdown.style.display = "block";
                });
            }, 400);
        };

        function renderAutocomplete(data) {
            inviteAutocompleteDropdown.innerHTML = "";
            if (!data || data.length === 0) {
                inviteAutocompleteDropdown.innerHTML = '<div class="autocomplete-empty">Không tìm thấy</div>';
                inviteAutocompleteDropdown.style.display = "block";
                return;
            }

            data.forEach(item => {
                const isDoi = currentInviteContext.loai === "doi";
                const id = isDoi ? item.ma_doi : item.ma_nguoi_dung;
                const name = isDoi ? item.ten_doi : item.ten_dang_nhap;
                
                const div = document.createElement("div");
                div.className = "autocomplete-item";
                div.innerHTML = `<div>${escapeHtml(name)}</div><div style="font-size: 0.75rem; color: #94a3b8;">${escapeHtml(isDoi ? item.ten_viet_tat : item.email)}</div>`;
                div.onclick = () => {
                    currentInviteContext.selectedId = id;
                    inviteSearchInput.value = name;
                    inviteAutocompleteDropdown.style.display = "none";
                };
                inviteAutocompleteDropdown.appendChild(div);
            });
            inviteAutocompleteDropdown.style.display = "block";
        }

        document.getElementById("confirmInviteModal").onclick = async function() {
            if (!currentInviteContext.selectedId) {
                inviteValidationMessage.style.display = "block";
                return;
            }

            const isDoi = currentInviteContext.loai === "doi";
            const endpoint = isDoi ? "/GiaiDauApi/InviteTeam" : "/GiaiDauApi/InviteNhanSu";
            const payload = isDoi ? {
                ma_giai_dau: maGiaiDau,
                ma_doi: currentInviteContext.selectedId,
                loi_nhan: inviteMessage.value || "Mời tham gia giải"
            } : {
                ma_giai_dau: maGiaiDau,
                username_or_email: inviteSearchInput.value,
                vai_tro: currentInviteContext.loai === "btc" ? "btc" : inviteRoleSelect.value,
                loi_nhan: inviteMessage.value || "Mời hợp tác"
            };

            try {
                const result = await postApi(endpoint, payload);
                alert(result.message || "Đã xử lý.");
                if (result.success) closeInvite();
            } catch (err) {
                console.error("Invite failed:", err);
                alert("Không thể gửi lời mời lúc này, vui lòng thử lại sau!");
            }
        };
    }

    init();
})();
