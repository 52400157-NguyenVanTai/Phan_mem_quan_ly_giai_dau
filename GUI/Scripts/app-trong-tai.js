(function () {
    const api = {
        matches: "/TrongTai/Matches",
        updateResult: "/TrongTai/UpdateResult"
    };

    const state = { matches: [], currentMatch: null };
    const $ = id => document.getElementById(id);

    const stateLabels = {
        chua_dau: "Chờ đấu",
        chuan_bi: "Chuẩn bị",
        san_sang: "Sẵn sàng",
        dang_dau: "Đang thi đấu",
        cho_ket_qua: "Chờ xác nhận",
        da_hoan_thanh: "Đã hoàn thành",
        huy_bo: "Đã hủy",
        bye: "BYE"
    };

    function escapeHtml(value) {
        return String(value == null ? "" : value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function post(url, data) {
        return fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data || {})
        }).then(r => r.json());
    }

    function getData(res) {
        return res && res.success ? (res.data || res.Data || []) : [];
    }

    function numberValue(selector, fallback) {
        const input = typeof selector === "string" ? document.querySelector(selector) : selector;
        const value = parseInt(input && input.value, 10);
        return Number.isFinite(value) ? value : (fallback || 0);
    }

    function isBattleRoyale(match) {
        const format = String(match && match.the_thuc_tran || "").toLowerCase();
        return format === "sinhton" || format === "sinh_ton" || format === "battle_royale";
    }

    function loadMatches() {
        fetch(api.matches)
            .then(r => r.json())
            .then(res => {
                state.matches = getData(res);
                renderMatches();
            })
            .catch(() => {
                $("refereeMatchList").innerHTML = '<div class="empty-page-card"><h3>Không thể tải danh sách trận đấu.</h3></div>';
            });
    }

    function renderMatches() {
        const list = $("refereeMatchList");
        if (!list) return;
        if (!state.matches.length) {
            list.innerHTML = '<div class="empty-page-card"><h3>Chưa có trận đấu đã xác nhận.</h3><p class="text-muted">Sau khi bạn xác nhận phân công, các trận đấu sẽ hiển thị tại đây.</p></div>';
            return;
        }

        list.innerHTML = state.matches.map(match => {
            const teams = (match.chi_tiet || []).map(t => escapeHtml(t.ten_doi)).join(" vs ") || "Chưa có đội";
            const canInput = match.trang_thai === "dang_dau";
            const action = canInput
                ? `<button class="btn btn-success" data-result="${match.ma_tran}">Ghi kết quả</button>`
                : '<span class="badge">Trận đấu đã kết thúc</span>';
            return `
                <article class="request-card mb-3 p-3">
                    <div class="d-flex justify-content-between align-items-start gap-3 flex-wrap">
                        <div>
                            <p class="eyebrow">${escapeHtml(match.ten_giai_doan || "Trận đấu")}</p>
                            <h4 class="mb-1">${escapeHtml(match.vong_dau || "Trận #" + match.ma_tran)}</h4>
                            <p class="text-muted mb-2">${teams}</p>
                            <small class="text-secondary">Thể thức: ${escapeHtml(match.the_thuc_tran || "BO1")} • Trạng thái: ${escapeHtml(stateLabels[match.trang_thai] || match.trang_thai)}</small>
                        </div>
                        <div class="d-flex gap-2 align-items-center">
                            <a class="btn btn-outline-info" href="/GiaiDau/ChiTiet/${match.ma_giai_dau}">Xem giải đấu</a>
                            ${action}
                        </div>
                    </div>
                </article>`;
        }).join("");

        list.querySelectorAll("[data-result]").forEach(btn => {
            btn.onclick = () => openResultModal(parseInt(btn.dataset.result, 10));
        });
    }

    function openResultModal(matchId) {
        const match = state.matches.find(x => x.ma_tran === matchId);
        if (!match) return;
        const teams = match.chi_tiet || [];
        if (teams.length < 2) {
            alert("Trận đấu chưa đủ đội để nhập kết quả.");
            return;
        }

        state.currentMatch = match;
        $("refereeResultMatchId").value = matchId;
        $("refereeWinnerTeam").innerHTML = teams.map(t => `<option value="${t.ma_nhom}">${escapeHtml(t.ten_doi)}</option>`).join("");
        $("refereeScoreOne").value = teams[0].so_kill || 0;
        $("refereeScoreTwo").value = teams[1].so_kill || 0;
        $("refereeResultMessage").style.display = "none";

        setBoFieldsVisible(!isBattleRoyale(match));
        if (isBattleRoyale(match)) {
            renderBattleRoyaleFields(match);
        } else {
            renderBoFields(match);
        }

        $("refereeResultModal").style.display = "flex";
    }

    function setBoFieldsVisible(visible) {
        ["refereeWinnerTeam", "refereeScoreOne", "refereeScoreTwo"].forEach(id => {
            const group = $(id) && $(id).closest(".form-group");
            if (group) group.style.display = visible ? "" : "none";
        });
    }

    function renderBoFields(match) {
        const players = match.nguoi_choi || [];
        $("refereePlayerStats").innerHTML = `
            <div class="form-group">
                <label>Game số</label>
                <input id="refereeGameNo" class="form-control" type="number" min="1" max="${match.so_vong || 1}" value="1">
            </div>
            ${renderPlayerStats(players)}`;
    }

    function renderPlayerStats(players) {
        if (!players.length) {
            return '<p class="text-muted mt-3">Chưa có đội hình thi đấu. Bạn vẫn có thể gửi đội thắng và số kills.</p>';
        }

        return `
            <label class="mt-3">Thông số tuyển thủ</label>
            <div class="table-responsive">
                <table class="table">
                    <thead><tr><th>Tuyển thủ</th><th>Đội</th><th>K</th><th>D</th><th>A</th><th>MVP</th></tr></thead>
                    <tbody>${players.map((p, index) => `
                        <tr data-player="${p.ma_nguoi_dung}">
                            <td>${escapeHtml(p.ten_dang_nhap)}</td>
                            <td>${escapeHtml(p.ten_doi || "Chưa cập nhật")}</td>
                            <td><input class="form-control stat-kill" type="number" min="0" value="${p.so_kill || 0}"></td>
                            <td><input class="form-control stat-death" type="number" min="0" value="${p.so_death || 0}"></td>
                            <td><input class="form-control stat-assist" type="number" min="0" value="${p.so_assist || 0}"></td>
                            <td><input name="refereeMvp" type="radio" ${index === 0 ? "checked" : ""}></td>
                        </tr>`).join("")}</tbody>
                </table>
            </div>`;
    }

    function renderBattleRoyaleFields(match) {
        const teams = match.chi_tiet || [];
        $("refereePlayerStats").innerHTML = `
            <div class="form-group">
                <label>Map số</label>
                <input id="refereeGameNo" class="form-control" type="number" min="1" max="${match.so_vong || 1}" value="1">
            </div>
            <div class="table-responsive mt-3">
                <table class="table">
                    <thead><tr><th>Đội</th><th>Hạng</th><th>Kills</th></tr></thead>
                    <tbody>${teams.map(t => `
                        <tr data-br-team="${t.ma_nhom}">
                            <td>${escapeHtml(t.ten_doi)}</td>
                            <td><input class="form-control br-rank" type="number" min="1" max="${teams.length}" value="${t.thu_hang || teams.length}"></td>
                            <td><input class="form-control br-kill" type="number" min="0" value="${t.so_kill || 0}"></td>
                        </tr>`).join("")}</tbody>
                </table>
            </div>`;
    }

    function setupModal() {
        const modal = $("refereeResultModal");
        if (!modal) return;
        const close = () => { modal.style.display = "none"; };
        $("closeRefereeResultModal").onclick = close;
        $("cancelRefereeResultModal").onclick = close;
        modal.addEventListener("click", e => { if (e.target === modal) close(); });
        $("submitRefereeResult").onclick = submitResult;
    }

    function collectPlayerStats() {
        return Array.from(document.querySelectorAll("#refereePlayerStats tr[data-player]")).map(row => ({
            ma_nguoi_dung: parseInt(row.dataset.player, 10),
            so_kill: numberValue(row.querySelector(".stat-kill")),
            so_death: numberValue(row.querySelector(".stat-death")),
            so_assist: numberValue(row.querySelector(".stat-assist")),
            is_mvp_tran: !!row.querySelector('input[name="refereeMvp"]').checked
        }));
    }

    function buildPayload() {
        const match = state.currentMatch;
        const teams = match.chi_tiet || [];
        const gameNo = numberValue("#refereeGameNo", 1);

        if (isBattleRoyale(match)) {
            return {
                ma_tran: match.ma_tran,
                so_van: gameNo,
                ket_qua_br: Array.from(document.querySelectorAll("#refereePlayerStats tr[data-br-team]")).map(row => ({
                    ma_nhom: parseInt(row.dataset.brTeam, 10),
                    thu_hang: numberValue(row.querySelector(".br-rank"), teams.length),
                    so_kill: numberValue(row.querySelector(".br-kill"))
                }))
            };
        }

        return {
            ma_tran: match.ma_tran,
            so_van: gameNo,
            games: [{
                so_van: gameNo,
                ma_doi_1: teams[0].ma_nhom,
                ma_doi_2: teams[1].ma_nhom,
                ma_doi_thang: numberValue("#refereeWinnerTeam"),
                kill_doi_1: numberValue("#refereeScoreOne"),
                kill_doi_2: numberValue("#refereeScoreTwo")
            }],
            nguoi_choi: collectPlayerStats()
        };
    }

    function mightCompleteMatch(match) {
        const gameNo = numberValue("#refereeGameNo", 1);
        if (isBattleRoyale(match)) return gameNo >= (match.so_vong || 1);
        return gameNo >= (match.so_vong || 1) || (match.so_vong || 1) === 1;
    }

    function submitResult() {
        const match = state.currentMatch;
        if (!match) return;

        if (mightCompleteMatch(match) && !confirm("Cảnh báo: Lưu kết quả này có thể kết thúc trận đấu. Bạn có chắc dữ liệu đã chính xác?")) {
            return;
        }

        const button = $("submitRefereeResult");
        button.disabled = true;
        post(api.updateResult, buildPayload()).then(res => {
            button.disabled = false;
            if (!res.success) {
                $("refereeResultMessage").textContent = res.message || "Không thể gửi kết quả.";
                $("refereeResultMessage").style.display = "block";
                return;
            }
            alert(res.message || "Đã lưu kết quả.");
            $("refereeResultModal").style.display = "none";
            loadMatches();
        }).catch(() => {
            button.disabled = false;
            $("refereeResultMessage").textContent = "Không thể gửi kết quả lúc này.";
            $("refereeResultMessage").style.display = "block";
        });
    }

    setupModal();
    loadMatches();
})();
