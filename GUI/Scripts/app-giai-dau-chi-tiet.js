/**
 * app-giai-dau-chi-tiet.js
 * Interactive logic for the Tournament Hub Page
 */

(function () {
    const hub = document.getElementById("tournamentHub");
    if (!hub) return;

    const maGiaiDau = parseInt(hub.getAttribute("data-id"));
    let tournamentData = null;

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
        dang_thi_dau: "Đang thi đấu",
        cho_ket_qua: "Chờ kết quả",
        da_hoan_thanh: "Đã hoàn thành",
        huy_bo: "Hủy bỏ",
        bye: "BYE"
    };

    // INIT
    async function init() {
        ensureOperationTabs();
        await loadTournamentDetail();
        setupTabs();
        setupInviteModal();
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
        const approvedCount = gd.so_doi_da_duyet || 0;
        const maxTeams = gd.so_doi_toi_da || 16;
        document.getElementById("slotCount").textContent = `${approvedCount}/${maxTeams}`;
        const percent = Math.min(100, (approvedCount / maxTeams) * 100);
        document.getElementById("slotBar").style.width = `${percent}%`;

        // Stats
        document.getElementById("statTeams").textContent = approvedCount;
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
        if (tt === "mo_dang_ky" || gd.dang_mo_dang_ky) {
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
            if (tt !== "nhap" && tt !== "bi_tu_choi" && tt !== "da_huy" && tt !== "ket_thuc") {
                const toggleRegBtn = document.createElement("button");
                if (gd.dang_mo_dang_ky) {
                    toggleRegBtn.className = "hub-btn-warning";
                    toggleRegBtn.textContent = "ĐÓNG ĐĂNG KÝ / CHỐT SỔ";
                    toggleRegBtn.textContent = "DUNG DANG KY";
                    toggleRegBtn.onclick = () => handleAction("toggle-reg", maGiaiDau, { mo_dang_ky: false });
                } else {
                    toggleRegBtn.className = "hub-btn-primary";
                    toggleRegBtn.textContent = "MỞ ĐĂNG KÝ";
                    toggleRegBtn.textContent = "MO DANG KY";
                    toggleRegBtn.onclick = () => handleAction("toggle-reg", maGiaiDau, { mo_dang_ky: true });
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
        if (tab === "bracket") renderScheduleTab();
        if (tab === "rules") renderRulesTab();
    }

    function renderInfoTab() {
        const gd = tournamentData.giai_dau;
        const stages = tournamentData.giai_doan || [];
        const container = document.getElementById("infoList");
        
        const rows = [
            { label: "Tựa Game", value: gd.ten_game || "Chưa chọn" },
            { label: "Thể thức", value: FORMAT_LABELS[gd.the_thuc] || gd.the_thuc },
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

    function renderPeopleList(title, items) {
        const body = items.length ? items.map(p => `
            <div class="operator-row">
                <img class="operator-avatar" src="${p.avatar_url || "/Content/images/default-avatar.png"}" onerror="this.style.display='none'" alt="">
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
        const matches = tournamentData.tran_dau || [];
        const container = document.getElementById("matchList") || document.querySelector("#pane-bracket");
        if (!container) return;
        if (!matches.length) {
            container.innerHTML = '<div class="empty-state">Lịch thi đấu sẽ hiển thị khi giải đấu bắt đầu.</div>';
            return;
        }
        const isBTC = tournamentData.giai_dau.is_btc || tournamentData.giai_dau.ma_nguoi_tao === parseInt(hub.dataset.userId);
        container.innerHTML = matches.map(m => {
            const teams = (m.chi_tiet || []).map(c => escapeHtml(c.ten_doi)).join(" vs ");
            const canStart = isBTC && m.trang_thai === "san_sang";
            const canComplete = isBTC && (m.trang_thai === "dang_thi_dau" || m.trang_thai === "cho_ket_qua");
            return `
                <article class="match-card" data-match-id="${m.ma_tran}">
                    <div>
                        <div class="match-title">${escapeHtml(m.vong_dau || m.ten_giai_doan || "Trận đấu")}</div>
                        <div class="match-teams">${teams || "Đang chờ đội"}</div>
                        <div class="muted">Trọng tài: ${escapeHtml(m.ten_trong_tai || "Chưa chọn")} · ${MATCH_STATE_LABELS[m.trang_thai] || m.trang_thai}</div>
                    </div>
                    <div class="match-actions">
                        ${isBTC ? `<button class="hub-btn-outline js-setup-match" data-id="${m.ma_tran}">Chuẩn bị</button>` : ""}
                        ${canStart ? `<button class="hub-btn-primary js-start-match" data-id="${m.ma_tran}">Bắt đầu trận</button>` : ""}
                        ${m.trang_thai === "dang_thi_dau" ? `<button class="hub-btn-outline js-stats-match" data-id="${m.ma_tran}">Ghi kết quả</button>` : ""}
                        ${canComplete ? `<button class="hub-btn-warning js-complete-match" data-id="${m.ma_tran}">Xác nhận kết thúc</button>` : ""}
                    </div>
                </article>`;
        }).join("");

        container.querySelectorAll(".js-setup-match").forEach(btn => btn.onclick = () => openSetupMatchModal(parseInt(btn.dataset.id)));
        container.querySelectorAll(".js-start-match").forEach(btn => btn.onclick = () => postMatchAction("/GiaiDauApi/StartMatch", parseInt(btn.dataset.id)));
        container.querySelectorAll(".js-complete-match").forEach(btn => btn.onclick = () => postMatchAction("/GiaiDauApi/CompleteMatch", parseInt(btn.dataset.id)));
        container.querySelectorAll(".js-stats-match").forEach(btn => btn.onclick = () => openStatsModal(parseInt(btn.dataset.id)));
    }

    async function postMatchAction(url, maTran) {
        const result = await postApi(url, { ma_giai_dau: maTran });
        notify(result.message || "Đã xử lý.");
        if (result.success) await loadTournamentDetail();
    }

    async function openSetupMatchModal(maTran) {
        const refs = (tournamentData.nhan_su || []).filter(x => x.vai_tro_giai === "trong_tai");
        if (!refs.length) {
            notify("Chưa có trọng tài trong giải.");
            return;
        }
        const refText = refs.map(r => `${r.ma_nguoi_dung}: ${r.ten_dang_nhap}`).join("\n");
        const maTrongTai = parseInt(prompt("Chọn trọng tài bằng ID:\n" + refText, refs[0].ma_nguoi_dung));
        if (!maTrongTai) return;
        const gameName = (tournamentData.giai_dau.ten_game || "").toLowerCase();
        const isBR = gameName.indexOf("pubg") >= 0 || gameName.indexOf("free fire") >= 0;
        const format = isBR ? "SinhTon" : (prompt("Thể thức trận: BO1, BO3, BO5, BO7", "BO1") || "BO1");
        const rounds = isBR ? parseInt(prompt("Số lượng game/map", "5")) : null;
        const result = await postApi("/GiaiDauApi/SetupMatch", {
            ma_tran: maTran,
            ma_trong_tai: maTrongTai,
            the_thuc_tran: format,
            so_vong: rounds
        });
        notify(result.message);
        if (result.success) await loadTournamentDetail();
    }

    async function openStatsModal(maTran) {
        const match = (tournamentData.tran_dau || []).find(x => x.ma_tran === maTran);
        if (!match) return;
        const players = match.nguoi_choi || [];
        if (!players.length) {
            notify("Chưa có đội hình thi đấu để nhập KDA.");
            return;
        }
        const teamText = (match.chi_tiet || []).map(t => `${t.ma_nhom}: ${t.ten_doi}`).join("\n");
        const winner = teamText ? parseInt(prompt("Nhập ID đội thắng:\n" + teamText, match.chi_tiet[0].ma_nhom)) : null;
        const payload = {
            ma_tran: maTran,
            ma_doi_thang: winner,
            nguoi_choi: players.map(p => {
                const raw = prompt(`K/D/A cho ${p.ten_dang_nhap} (${p.ten_vi_tri || ""})`, `${p.so_kill || 0}/${p.so_death || 0}/${p.so_assist || 0}`);
                const parts = String(raw || "0/0/0").split(/[\\/,-]/).map(x => parseInt(x, 10) || 0);
                return {
                    ma_nguoi_dung: p.ma_nguoi_dung,
                    so_kill: parts[0] || 0,
                    so_death: parts[1] || 0,
                    so_assist: parts[2] || 0,
                    is_mvp_tran: false
                };
            })
        };
        const mvp = parseInt(prompt("ID người chơi MVP:\n" + players.map(p => `${p.ma_nguoi_dung}: ${p.ten_dang_nhap}`).join("\n"), players[0].ma_nguoi_dung));
        payload.nguoi_choi.forEach(p => p.is_mvp_tran = p.ma_nguoi_dung === mvp);
        const result = await postApi("/GiaiDauApi/UpdateMatchStats", payload);
        notify(result.message);
        if (result.success) await loadTournamentDetail();
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
        const confirmMessages = {
            "close-reg": "Chốt sổ đăng ký ngay bây giờ?",
            "open-reg": "Bạn muốn mở đăng ký cho giải đấu này?",
            "cancel": "Hủy giải đấu? Hành động này không thể hoàn tác.",
        };
        if (action === "cancel" && tournamentData && tournamentData.giai_dau && tournamentData.giai_dau.trang_thai === "sap_dien_ra") {
            confirmMessages.cancel = "Hủy giải đấu sắp diễn ra? Giải đấu sẽ bị xóa hoàn toàn khỏi database.";
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

        const result = await postApi(endpoints[action], Object.assign({ ma_giai_dau: id }, extraPayload || {}));
        notify(result.message);
        
        if (result.success) {
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
                tournamentData.giai_dau.dang_mo_dang_ky = !!(extraPayload && extraPayload.mo_dang_ky);
                tournamentData.giai_dau.trang_thai = tournamentData.giai_dau.dang_mo_dang_ky ? "mo_dang_ky" : "khoa_dang_ky";
            }
            if (action === "start") {
                await loadTournamentDetail();
                return;
            }
            renderHeader(); // Cập nhật badge trạng thái
            renderActions(); // Re-render các nút
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
