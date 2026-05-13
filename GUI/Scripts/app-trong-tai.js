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
        return format === "sinhton" || format === "sinh_ton" || format === "battle_royale" || format === "battleroyale";
    }

    function parseBoCount(format) {
        const found = String(format || "BO1").match(/BO(\d+)/i);
        return found ? parseInt(found[1], 10) : 1;
    }

    function rankPoint(rank, teamCount) {
        if (rank === 1) return 10;
        if (rank === 2) return 6;
        if (rank === 3) return 5;
        if (rank === 4) return 4;
        if (rank === 5) return 3;
        return rank > 0 && rank <= teamCount ? 1 : 0;
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
            const canInput = ["da_hoan_thanh", "huy_bo", "bye"].indexOf(match.trang_thai) < 0 && (match.chi_tiet || []).length >= 2;
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
        $("refereeResultMessage").style.display = "none";

        setBoFieldsVisible(false);
        if (isBattleRoyale(match)) {
            configureBoControls(match, false);
            renderBattleRoyaleFields(match);
        } else {
            configureBoControls(match, true);
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

    function configureBoControls(match, visible) {
        const wrap = $("refereeBoSettings");
        const formatSelect = $("refereeResultFormat");
        const gameCount = $("refereeResultGameCount");
        if (!wrap || !formatSelect || !gameCount) return;
        wrap.style.display = visible ? "grid" : "none";
        if (!visible) return;
        const format = /^BO[1357]$/i.test(match.the_thuc_tran || "") ? String(match.the_thuc_tran).toUpperCase() : "BO" + parseBoCount(match.the_thuc_tran || ("BO" + (match.so_vong || 1)));
        formatSelect.value = ["BO1", "BO3", "BO5", "BO7"].indexOf(format) >= 0 ? format : "BO1";
        gameCount.value = parseBoCount(formatSelect.value);
        formatSelect.onchange = () => {
            gameCount.value = parseBoCount(formatSelect.value);
            renderBoFields(state.currentMatch);
        };
        gameCount.onchange = () => {
            const count = Math.max(1, Math.min(7, numberValue(gameCount, 1)));
            gameCount.value = count % 2 === 0 ? count + 1 : count;
            renderBoFields(state.currentMatch);
        };
    }

    function renderBoFields(match) {
        const teams = (match.chi_tiet || []).slice(0, 2);
        const selectedFormat = $("refereeResultFormat") ? $("refereeResultFormat").value : match.the_thuc_tran;
        const totalGames = Math.max(1, Math.min(7, numberValue("#refereeResultGameCount", parseBoCount(selectedFormat || ("BO" + (match.so_vong || 1))))));
        const blocks = [];
        for (let i = 1; i <= totalGames; i++) {
            blocks.push(`
                <section class="result-game-card mb-3 referee-bo-game" data-game="${i}">
                    <div class="result-game-head">
                        <h4 class="result-game-title">Game ${i}</h4>
                        <span class="badge referee-game-state">Dang nhap</span>
                    </div>
                    <div class="result-game-grid">
                        <div class="form-group">
                            <label>Doi thang game nay</label>
                            <select class="form-control referee-game-winner">
                                <option value="">Chon doi thang</option>
                                ${teams.map(t => `<option value="${t.ma_nhom}">${escapeHtml(t.ten_doi)}</option>`).join("")}
                            </select>
                        </div>
                        <div class="form-group">
                            <label>${escapeHtml(teams[0] && teams[0].ten_doi || "Doi 1")} kills</label>
                            <input class="form-control referee-kill-one" type="number" min="0" value="0">
                        </div>
                        <div class="form-group">
                            <label>${escapeHtml(teams[1] && teams[1].ten_doi || "Doi 2")} kills</label>
                            <input class="form-control referee-kill-two" type="number" min="0" value="0">
                        </div>
                    </div>
                </section>`);
        }
        $("refereePlayerStats").innerHTML = `
            <div class="alert alert-info py-2">Can ${Math.floor(totalGames / 2) + 1} game thang de ket thuc ${escapeHtml(selectedFormat || ("BO" + totalGames))}.</div>
            <div id="refereeBoGames">${blocks.join("")}</div>
            <div id="refereeBoPreview" class="result-preview"></div>`;
        document.querySelectorAll("#refereeBoGames select").forEach(el => el.onchange = updateBoPreview);
        updateBoPreview();
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
                <label>So map can nhap</label>
                <input id="soTranBR" class="form-control" type="number" min="1" value="${match.so_vong || 1}">
            </div>
            <div id="refereeBrGames" class="mt-3"></div>
            <div id="refereeBrPreview" class="table-responsive mt-3"></div>`;
        $("soTranBR").onchange = renderBrGameBlocks;
        renderBrGameBlocks();
    }

    function renderBrGameBlocks() {
        const match = state.currentMatch;
        const teams = match.chi_tiet || [];
        const count = Math.max(1, numberValue("#soTranBR", match.so_vong || 1));
        const blocks = [];
        for (let i = 1; i <= count; i++) {
            blocks.push(`
                <section class="request-card p-3 mb-3 referee-br-game" data-game="${i}">
                    <strong>Map ${i}</strong>
                    <div class="table-responsive mt-3">
                        <table class="table">
                            <thead><tr><th>Doi</th><th>Hang</th><th>Kills</th><th>Diem</th></tr></thead>
                            <tbody>${teams.map(t => `
                                <tr data-br-team="${t.ma_nhom}">
                                    <td>${escapeHtml(t.ten_doi)}</td>
                                    <td><input class="form-control br-rank" type="number" min="1" max="${teams.length}" value="${t.thu_hang || teams.length}"></td>
                                    <td><input class="form-control br-kill" type="number" min="0" value="${t.so_kill || 0}"></td>
                                    <td class="br-row-total">0</td>
                                </tr>`).join("")}</tbody>
                        </table>
                    </div>
                </section>`);
        }
        $("refereeBrGames").innerHTML = blocks.join("");
        document.querySelectorAll("#refereeBrGames input").forEach(el => el.oninput = updateBrPreview);
        updateBrPreview();
    }

    function updateBoPreview() {
        const match = state.currentMatch;
        if (!match) return;
        const teams = (match.chi_tiet || []).slice(0, 2);
        const score = {};
        teams.forEach(t => { score[t.ma_nhom] = 0; });
        const target = Math.floor(numberValue("#refereeResultGameCount", parseBoCount(match.the_thuc_tran)) / 2) + 1;
        let finished = false;
        document.querySelectorAll(".referee-bo-game").forEach(block => {
            const winner = numberValue(block.querySelector(".referee-game-winner"), 0);
            const stateBadge = block.querySelector(".referee-game-state");
            if (!finished && winner) {
                score[winner] = (score[winner] || 0) + 1;
                stateBadge.textContent = "Da tinh";
                block.style.display = "";
                if (score[winner] >= target) finished = true;
            } else if (finished) {
                block.style.display = "none";
                block.querySelector(".referee-game-winner").value = "";
            } else {
                stateBadge.textContent = "Dang nhap";
                block.style.display = "";
            }
        });
        const scoreText = `${score[teams[0].ma_nhom] || 0}-${score[teams[1].ma_nhom] || 0}`;
        $("refereeBoPreview").textContent = "Ty so tam tinh: " + scoreText;
        $("submitRefereeResult").textContent = "Luu ket qua (Ty so chung cuoc " + scoreText + ")";
    }

    function updateBrPreview() {
        const match = state.currentMatch;
        const teams = match.chi_tiet || [];
        const totals = {};
        teams.forEach(t => { totals[t.ma_nhom] = { name: t.ten_doi, points: 0, kills: 0 }; });
        document.querySelectorAll(".referee-br-game tr[data-br-team]").forEach(row => {
            const id = parseInt(row.dataset.brTeam, 10);
            const rank = numberValue(row.querySelector(".br-rank"), teams.length);
            const kills = numberValue(row.querySelector(".br-kill"), 0);
            const points = rankPoint(rank, teams.length) + kills;
            row.querySelector(".br-row-total").textContent = points;
            totals[id].points += points;
            totals[id].kills += kills;
        });
        $("refereeBrPreview").innerHTML = `
            <table class="table">
                <thead><tr><th>Doi</th><th>Tong diem</th><th>Tong kills</th></tr></thead>
                <tbody>${Object.keys(totals).map(id => totals[id]).sort((a, b) => b.points - a.points || b.kills - a.kills).map(t =>
                    `<tr><td>${escapeHtml(t.name)}</td><td>${t.points}</td><td>${t.kills}</td></tr>`).join("")}</tbody>
            </table>`;
        $("submitRefereeResult").textContent = "Luu ket qua BR";
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

        if (isBattleRoyale(match)) {
            return {
                ma_tran: match.ma_tran,
                so_van: numberValue("#soTranBR", 1),
                br_games: Array.from(document.querySelectorAll(".referee-br-game")).map(block => ({
                    so_van: parseInt(block.dataset.game, 10),
                    ket_qua: Array.from(block.querySelectorAll("tr[data-br-team]")).map(row => ({
                        ma_nhom: parseInt(row.dataset.brTeam, 10),
                        thu_hang: numberValue(row.querySelector(".br-rank"), teams.length),
                        so_kill: numberValue(row.querySelector(".br-kill"))
                    }))
                }))
            };
        }

        return {
            ma_tran: match.ma_tran,
            so_van: 1,
            the_thuc_tran: $("refereeResultFormat") ? $("refereeResultFormat").value : match.the_thuc_tran,
            games: Array.from(document.querySelectorAll(".referee-bo-game"))
                .filter(block => block.style.display !== "none" && numberValue(block.querySelector(".referee-game-winner"), 0))
                .map(block => ({
                so_van: parseInt(block.dataset.game, 10),
                ma_doi_1: teams[0].ma_nhom,
                ma_doi_2: teams[1].ma_nhom,
                ma_doi_thang: numberValue(block.querySelector(".referee-game-winner")),
                kill_doi_1: numberValue(block.querySelector(".referee-kill-one")),
                kill_doi_2: numberValue(block.querySelector(".referee-kill-two"))
            })),
            nguoi_choi: []
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
        const payload = buildPayload();
        if (!isBattleRoyale(match) && (!payload.games || !payload.games.length)) {
            $("refereeResultMessage").textContent = "Vui long chon it nhat mot game thang.";
            $("refereeResultMessage").style.display = "block";
            return;
        }

        if (!confirm("Luu ket qua nay? Ban co chac du lieu da chinh xac?")) {
            return;
        }

        const button = $("submitRefereeResult");
        button.disabled = true;
        post(api.updateResult, payload).then(res => {
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
