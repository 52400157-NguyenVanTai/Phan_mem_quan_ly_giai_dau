(function () {
    const api = {
        matches: "/TrongTai/Matches",
        updateResult: "/TrongTai/UpdateResult"
    };

    const state = { matches: [] };
    const $ = id => document.getElementById(id);

    const stateLabels = {
        chua_dau: "Chưa đấu",
        san_sang: "Sẵn sàng",
        dang_dau: "Đang thi đấu",
        cho_ket_qua: "Chờ Ban tổ chức xác nhận",
        da_hoan_thanh: "Đã hoàn thành",
        huy_bo: "Đã hủy",
        bye: "BYE"
    };

    function text(value) {
        return value || "Chưa cập nhật";
    }

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
            list.innerHTML = '<div class="empty-page-card"><h3>Chưa có trận đấu đã xác nhận.</h3><p class="text-muted">Sau khi bạn bấm “Đã nhận” trong trang Yêu cầu, trận đấu được phân công sẽ hiển thị tại đây.</p></div>';
            return;
        }

        list.innerHTML = state.matches.map(match => {
            const teams = (match.chi_tiet || []).map(t => escapeHtml(t.ten_doi)).join(" vs ") || "Chưa có đội";
            const canInput = match.trang_thai !== "da_hoan_thanh" && match.trang_thai !== "huy_bo" && match.trang_thai !== "bye";
            return `
                <article class="request-card mb-3 p-3" style="background-color: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-md);">
                    <div class="d-flex justify-content-between align-items-start gap-3 flex-wrap">
                        <div>
                            <p class="eyebrow">${escapeHtml(match.ten_giai_doan || "Trận đấu")}</p>
                            <h4 class="mb-1">${escapeHtml(match.vong_dau || "Trận #" + match.ma_tran)}</h4>
                            <p class="text-muted mb-2">${teams}</p>
                            <small class="text-secondary">Thể thức: ${escapeHtml(match.the_thuc_tran)} • Trạng thái: ${escapeHtml(stateLabels[match.trang_thai] || match.trang_thai)}</small>
                        </div>
                        <div class="d-flex gap-2">
                            <a class="btn btn-outline-info" href="/GiaiDau/ChiTiet/${match.ma_giai_dau}">Xem giải đấu</a>
                            ${canInput ? `<button class="btn btn-success" data-result="${match.ma_tran}">Nhập kết quả</button>` : ""}
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

        $("refereeResultMatchId").value = matchId;
        $("refereeWinnerTeam").innerHTML = teams.map(t => `<option value="${t.ma_nhom}">${escapeHtml(t.ten_doi)}</option>`).join("");
        $("refereeScoreOne").value = teams[0].so_kill || 0;
        $("refereeScoreTwo").value = teams[1].so_kill || 0;
        renderPlayerStats(match.nguoi_choi || []);
        $("refereeResultMessage").style.display = "none";
        $("refereeResultModal").style.display = "flex";
    }

    function renderPlayerStats(players) {
        const box = $("refereePlayerStats");
        if (!players.length) {
            box.innerHTML = '<p class="text-muted">Chưa có đội hình thi đấu. Bạn vẫn có thể gửi đội thắng và tỉ số.</p>';
            return;
        }
        box.innerHTML = `
            <label>Thông số tuyển thủ</label>
            <div class="table-responsive">
                <table class="table">
                    <thead><tr><th>Tuyển thủ</th><th>Đội</th><th>K</th><th>D</th><th>A</th><th>MVP</th></tr></thead>
                    <tbody>${players.map((p, index) => `
                        <tr data-player="${p.ma_nguoi_dung}">
                            <td>${escapeHtml(p.ten_dang_nhap)}</td>
                            <td>${escapeHtml(text(p.ten_doi))}</td>
                            <td><input class="form-control stat-kill" type="number" min="0" value="${p.so_kill || 0}"></td>
                            <td><input class="form-control stat-death" type="number" min="0" value="${p.so_death || 0}"></td>
                            <td><input class="form-control stat-assist" type="number" min="0" value="${p.so_assist || 0}"></td>
                            <td><input name="refereeMvp" type="radio" ${index === 0 ? "checked" : ""}></td>
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
            so_kill: parseInt(row.querySelector(".stat-kill").value, 10) || 0,
            so_death: parseInt(row.querySelector(".stat-death").value, 10) || 0,
            so_assist: parseInt(row.querySelector(".stat-assist").value, 10) || 0,
            is_mvp_tran: !!row.querySelector('input[name="refereeMvp"]').checked
        }));
    }

    function submitResult() {
        const matchId = parseInt($("refereeResultMatchId").value, 10);
        const winner = parseInt($("refereeWinnerTeam").value, 10);
        const payload = {
            ma_tran: matchId,
            ma_doi_thang: winner,
            ty_so_doi_1: parseInt($("refereeScoreOne").value, 10) || 0,
            ty_so_doi_2: parseInt($("refereeScoreTwo").value, 10) || 0,
            nguoi_choi: collectPlayerStats()
        };

        $("submitRefereeResult").disabled = true;
        post(api.updateResult, payload).then(res => {
            $("submitRefereeResult").disabled = false;
            if (!res.success) {
                $("refereeResultMessage").textContent = res.message || "Không thể gửi kết quả.";
                $("refereeResultMessage").style.display = "block";
                return;
            }
            alert(res.message || "Đã gửi kết quả.");
            $("refereeResultModal").style.display = "none";
            loadMatches();
        }).catch(() => {
            $("submitRefereeResult").disabled = false;
            $("refereeResultMessage").textContent = "Không thể gửi kết quả lúc này.";
            $("refereeResultMessage").style.display = "block";
        });
    }

    setupModal();
    loadMatches();
})();
