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
        loai_truc_tiep: "Single Elimination",
        nhanh_thang_nhanh_thua: "Double Elimination",
        vong_tron: "Round Robin",
        thuy_si: "Swiss",
        battle_royale: "Battle Royale",
        champion_rush: "Champion Rush",
    };

    // INIT
    async function init() {
        await loadTournamentDetail();
        setupTabs();
        setupInviteModal();
    }

    async function loadTournamentDetail() {
        try {
            const result = await getApi(`/GiaiDauApi/Detail?maGiaiDau=${maGiaiDau}`);
            if (!isResponseSuccess(result)) {
                document.getElementById("hubTitle").textContent = "LỖI TẢI DỮ LIỆU";
                return;
            }

            tournamentData = result.data || result.Data;
            renderHeader();
            renderPrizePool();
            renderSidebar();
            renderActiveTab();
        } catch (e) {
            console.error("Error loading detail:", e);
        }
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
        const isBTC = gd.is_btc || gd.ma_nguoi_tao === parseInt(hub.dataset.userId); // Assuming we can get userId

        // Note: For now we'll rely on the backend check if the user can perform actions.
        // If ma_nguoi_tao matches Session["UserId"], show BTC actions.
        
        if (tt === "mo_dang_ky") {
            const regBtn = document.createElement("button");
            regBtn.className = "hub-btn-primary";
            regBtn.textContent = "ĐĂNG KÝ NGAY";
            regBtn.onclick = () => openRegisterTeamModal(maGiaiDau);
            container.appendChild(regBtn);
        }

        // BTC Actions (Placeholder check - ideally passed from backend)
        // For demonstration, we'll show them if it's not Draft.
        if (tt !== "nhap" && tt !== "bi_tu_choi") {
            const inviteTeamBtn = document.createElement("button");
            inviteTeamBtn.className = "hub-btn-outline";
            inviteTeamBtn.textContent = "MỜI ĐỘI";
            inviteTeamBtn.onclick = () => window.openInviteModal(maGiaiDau, "doi");
            container.appendChild(inviteTeamBtn);

            const inviteRefBtn = document.createElement("button");
            inviteRefBtn.className = "hub-btn-outline";
            inviteRefBtn.textContent = "MỜI TRỌNG TÀI";
            inviteRefBtn.onclick = () => window.openInviteModal(maGiaiDau, "trong_tai");
            container.appendChild(inviteRefBtn);

            if (tt === "mo_dang_ky") {
                const closeRegBtn = document.createElement("button");
                closeRegBtn.className = "hub-btn-outline";
                closeRegBtn.style.color = "var(--hub-accent-red)";
                closeRegBtn.style.borderColor = "var(--hub-accent-red)";
                closeRegBtn.textContent = "ĐÓNG ĐĂNG KÝ";
                closeRegBtn.onclick = () => handleAction("close-reg", maGiaiDau);
                container.appendChild(closeRegBtn);
            }
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
            <div class="hub-team-card">
                <div class="hub-team-logo">
                    ${t.logo_url ? `<img src="${t.logo_url}" alt="Logo">` : `<div class="logo-placeholder" style="font-size: 1.5rem;"><i class="fas fa-users"></i></div>`}
                </div>
                <span class="hub-team-name">${escapeHtml(t.ten_doi)}</span>
                <span class="hub-team-tag">${escapeHtml(t.ten_viet_tat || "")}</span>
            </div>
        `).join("");
    }

    function renderRulesTab() {
        const gd = tournamentData.giai_dau;
        const container = document.getElementById("rulesContent");
        container.innerHTML = `<div style="white-space: pre-wrap; line-height: 1.6; color: var(--hub-text-muted);">${escapeHtml(gd.mo_ta || "Không có mô tả chi tiết.")}</div>`;
    }

    // ACTIONS HELPERS
    async function handleAction(action, id) {
        const confirmMessages = {
            "close-reg": "Chốt sổ đăng ký ngay bây giờ?",
            "cancel": "Hủy giải đấu? Hành động này không thể hoàn tác.",
        };
        if (confirmMessages[action] && !confirm(confirmMessages[action])) return;

        const endpoints = {
            "close-reg": "/GiaiDauApi/CloseRegistration",
        };

        const result = await postApi(endpoints[action], { ma_giai_dau: id });
        alert(result.message);
        if (result.success) loadTournamentDetail();
    }

    window.openRegisterTeamModal = function(maGiaiDau) {
        const maDoi = prompt("Nhập mã Đội của bạn để đăng ký tham gia giải:");
        if (maDoi) {
            postApi("/GiaiDauApi/RegisterTeam", { ma_giai_dau: maGiaiDau, ma_doi: parseInt(maDoi) })
                .then(res => alert(res.message))
                .then(() => loadTournamentDetail());
        }
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

            const result = await postApi(endpoint, payload);
            alert(result.message);
            if (result.success) closeInvite();
        };
    }

    init();
})();
