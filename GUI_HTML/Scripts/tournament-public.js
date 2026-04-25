(function () {
  const root = document.getElementById("public-tournament-root");
  if (!root) return;

  const maGiaiDau = Number(root.getAttribute("data-ma-giai-dau") || 0);
  const alertBox = document.getElementById("public-alert");
  const titleEl = document.getElementById("public-title");
  const metaEl = document.getElementById("public-meta");
  const liveStatusEl = document.getElementById("public-live-status");
  const tabsEl = document.getElementById("stage-tabs");
  const contentEl = document.getElementById("stage-content");
  const honoringEl = document.getElementById("honoring-content");
  let currentStage = null;

  function showAlert(success, message) {
    if (!alertBox) return;
    alertBox.classList.remove("d-none", "alert-success", "alert-danger");
    alertBox.classList.add(success ? "alert-success" : "alert-danger");
    alertBox.textContent = message;
  }

  function renderHonoring(data) {
    if (!honoringEl) return;
    if (!data) {
      honoringEl.innerHTML =
        "<span class='text-muted'>Chưa có dữ liệu vinh danh.</span>";
      return;
    }

    const mvp = data.MvpGiai || null;
    const lineup = Array.isArray(data.DoiHinhTieuBieu)
      ? data.DoiHinhTieuBieu
      : [];
    const coach = Array.isArray(data.BanHuanLuyenVoDich)
      ? data.BanHuanLuyenVoDich
      : [];

    const lineupHtml =
      lineup.length === 0
        ? "<div class='text-muted'>Chưa có đội hình tiêu biểu.</div>"
        : lineup
            .map(function (p) {
              return (
                "<div class='border rounded p-2 mb-1'>" +
                "<b>" +
                (p.ten_vi_tri || "Vị trí") +
                ":</b> " +
                (p.ten_hien_thi || p.ten_dang_nhap || "-") +
                " <span class='text-muted'>(KDA tổng: " +
                Number(p.kda_tong || 0).toFixed(2) +
                ")</span>" +
                "</div>"
              );
            })
            .join("");

    const coachHtml =
      coach.length === 0
        ? "<div class='text-muted'>Chưa có dữ liệu ban huấn luyện vô địch.</div>"
        : coach
            .map(function (c) {
              return (
                "<div class='border rounded p-2 mb-1'>" +
                (c.ten_hien_thi || c.ten_dang_nhap || "-") +
                " - <span class='text-muted'>" +
                (c.vai_tro_noi_bo || c.phan_he || "-") +
                "</span></div>"
              );
            })
            .join("");

    honoringEl.innerHTML =
      "<div class='mb-3'><h6>MVP Giải</h6>" +
      (mvp
        ? "<div class='border rounded p-2'>" +
          "<b>" +
          (mvp.ten_hien_thi || mvp.ten_dang_nhap || "-") +
          "</b> | KDA TB: " +
          Number(mvp.diem_kda_trung_binh || 0).toFixed(2) +
          " | MVP trận: " +
          (mvp.so_lan_dat_mvp_tran || 0) +
          "</div>"
        : "<div class='text-muted'>Chưa xác định MVP giải.</div>") +
      "</div>" +
      "<div class='mb-3'><h6>Đội hình tiêu biểu</h6>" +
      lineupHtml +
      "</div>" +
      "<div><h6>Vinh danh Ban huấn luyện</h6>" +
      coachHtml +
      "</div>";
  }

  function fmtDate(value) {
    if (!value) return "-";
    const d = new Date(value);
    return isNaN(d.getTime()) ? "-" : d.toLocaleString("vi-VN");
  }

  async function getJson(url) {
    const res = await fetch(url, { method: "GET" });
    return res.json();
  }

  function renderStandingTable(rows) {
    if (!rows || rows.length === 0) {
      return "<p class='text-muted mb-0'>Chưa có dữ liệu bảng xếp hạng.</p>";
    }

    const body = rows
      .map(function (r, idx) {
        const isMatchPoint = Number(r.is_match_point || 0) === 1;
        const trClass = isMatchPoint ? " class='table-warning fw-bold'" : "";
        const badge = isMatchPoint
          ? " <span class='badge bg-danger'>MATCH POINT</span>"
          : "";
        return (
          "<tr" +
          trClass +
          ">" +
          "<td>" +
          (r.thu_hang_hien_tai || idx + 1) +
          "</td>" +
          "<td>" +
          (r.ten_doi || "") +
          "</td>" +
          "<td>" +
          (r.ten_nhom || "") +
          badge +
          "</td>" +
          "<td>" +
          (r.so_tran_da_dau || 0) +
          "</td>" +
          "<td>" +
          (r.so_tran_thang || 0) +
          "</td>" +
          "<td>" +
          (r.so_tran_thua || 0) +
          "</td>" +
          "<td>" +
          (r.diem_tong_ket || 0) +
          "</td>" +
          "</tr>"
        );
      })
      .join("");

    return (
      "<div class='table-responsive'><table class='table table-striped'>" +
      "<thead><tr><th>Hạng</th><th>Đội</th><th>Nhóm</th><th>Trận</th><th>Thắng</th><th>Thua</th><th>Điểm</th></tr></thead>" +
      "<tbody>" +
      body +
      "</tbody></table></div>"
    );
  }

  function renderMatchList(matches) {
    if (!matches || matches.length === 0) {
      return "<p class='text-muted mb-0'>Chưa có lịch thi đấu.</p>";
    }

    const items = matches
      .map(function (m) {
        const badge =
          m.TrangThai === "da_hoan_thanh"
            ? "<span class='badge bg-success'>Đã đấu</span>"
            : "<span class='badge bg-secondary'>Sắp đấu</span>";

        return (
          "<div class='border rounded p-2 mb-2'>" +
          "<div class='d-flex justify-content-between align-items-center'>" +
          "<strong>Trận #" +
          m.MaTran +
          " - Vòng " +
          (m.SoVong || "?") +
          "</strong>" +
          badge +
          "</div>" +
          "<small class='text-muted'>Nhánh: " +
          (m.NhanhDau || "-") +
          " | Bắt đầu: " +
          fmtDate(m.ThoiGianBatDau) +
          "</small>" +
          "</div>"
        );
      })
      .join("");

    return items;
  }

  function renderBracket(matches) {
    if (!matches || matches.length === 0) {
      return "<p class='text-muted mb-0'>Chưa có nhánh đấu.</p>";
    }

    const grouped = {};
    matches.forEach(function (m) {
      const round = Number(m.SoVong || 0);
      if (!grouped[round]) grouped[round] = [];
      grouped[round].push(m);
    });

    const rounds = Object.keys(grouped)
      .map(function (x) {
        return Number(x);
      })
      .sort(function (a, b) {
        return a - b;
      });

    const columns = rounds
      .map(function (round) {
        const cards = grouped[round]
          .map(function (m) {
            const a = m.TenNhomA || "TBD";
            const b = m.TenNhomB || "TBD";
            return (
              "<div class='card mb-2 border-secondary'>" +
              "<div class='card-body p-2'>" +
              "<div class='small text-muted mb-1'>Trận #" +
              m.MaTran +
              "</div>" +
              "<div><strong>" +
              a +
              "</strong> vs <strong>" +
              b +
              "</strong></div>" +
              "<div class='small text-muted'>" +
              (m.NhanhDau || "-") +
              " | " +
              (m.TrangThai || "-") +
              "</div>" +
              "</div>" +
              "</div>"
            );
          })
          .join("");

        return (
          "<div class='col-md-3 col-sm-6'>" +
          "<div class='p-2 bg-light border rounded h-100'>" +
          "<div class='fw-bold mb-2'>Vòng " +
          round +
          "</div>" +
          cards +
          "</div>" +
          "</div>"
        );
      })
      .join("");

    return "<div class='row g-2'>" + columns + "</div>";
  }

  async function renderStage(stage) {
    currentStage = stage;
    const isStandings =
      stage.TheThuc === "vong_tron" ||
      stage.TheThuc === "league_bang_cheo" ||
      stage.TheThuc === "thuy_si" ||
      stage.TheThuc === "champion_rush";
    const isBracket =
      stage.TheThuc === "loai_truc_tiep" ||
      stage.TheThuc === "nhanh_thang_nhanh_thua";

    if (isStandings) {
      const result = await getJson(
        "/MatchmakingApi/BangXepHang?maGiaiDoan=" + stage.MaGiaiDoan,
      );
      if (!result.Success) {
        contentEl.innerHTML =
          "<p class='text-danger mb-0'>" + result.Message + "</p>";
        return;
      }
      contentEl.innerHTML = renderStandingTable(result.Data);
      return;
    }

    const result = await getJson(
      "/MatchmakingApi/DanhSachTran?maGiaiDoan=" + stage.MaGiaiDoan,
    );
    if (!result.Success) {
      contentEl.innerHTML =
        "<p class='text-danger mb-0'>" + result.Message + "</p>";
      return;
    }
    contentEl.innerHTML = isBracket
      ? renderBracket(result.Data)
      : renderMatchList(result.Data);
  }

  async function refreshLiveStage() {
    if (!currentStage || !currentStage.MaGiaiDoan) return;

    const live = await getJson(
      "/MatchmakingApi/LiveSnapshot?maGiaiDau=" +
        maGiaiDau +
        "&maGiaiDoan=" +
        currentStage.MaGiaiDoan,
    );

    if (!live.Success) {
      if (liveStatusEl) {
        liveStatusEl.textContent =
          "Live Engine tạm gián đoạn: " + (live.Message || "Không rõ lỗi");
        liveStatusEl.classList.remove("text-success");
        liveStatusEl.classList.add("text-danger");
      }
      return;
    }

    if (liveStatusEl) {
      liveStatusEl.textContent =
        "Live cập nhật lúc " + fmtDate((live.Data || {}).ServerTime || null);
      liveStatusEl.classList.remove("text-danger");
      liveStatusEl.classList.add("text-success");
    }

    const data = live.Data || {};
    if (
      currentStage.TheThuc === "vong_tron" ||
      currentStage.TheThuc === "league_bang_cheo" ||
      currentStage.TheThuc === "thuy_si" ||
      currentStage.TheThuc === "champion_rush"
    ) {
      contentEl.innerHTML = renderStandingTable(data.BangXepHang || []);
    }
    renderHonoring(data.VinhDanh || null);
  }

  function renderTabs(stages) {
    tabsEl.innerHTML = "";

    stages.forEach(function (s, idx) {
      const li = document.createElement("li");
      li.className = "nav-item";

      const btn = document.createElement("button");
      btn.type = "button";
      btn.className = "nav-link" + (idx === 0 ? " active" : "");
      btn.textContent = s.TenGiaiDoan;
      btn.addEventListener("click", async function () {
        tabsEl.querySelectorAll(".nav-link").forEach(function (x) {
          x.classList.remove("active");
        });
        btn.classList.add("active");
        await renderStage(s);
      });

      li.appendChild(btn);
      tabsEl.appendChild(li);
    });
  }

  async function init() {
    if (maGiaiDau <= 0) {
      showAlert(false, "Mã giải đấu không hợp lệ.");
      return;
    }

    const result = await getJson(
      "/MatchmakingApi/CongKhai?maGiaiDau=" + maGiaiDau,
    );
    if (!result.Success) {
      showAlert(
        false,
        result.Message || "Không tải được cổng thông tin giải đấu.",
      );
      return;
    }

    const data = result.Data;
    titleEl.textContent = data.TenGiaiDau || "Giải đấu";
    metaEl.textContent =
      "Trạng thái: " +
      (data.TrangThai || "-") +
      " | " +
      fmtDate(data.NgayBatDau) +
      " - " +
      fmtDate(data.NgayKetThuc);

    const stages = data.GiaiDoan || [];
    if (stages.length === 0) {
      contentEl.innerHTML =
        "<p class='text-muted mb-0'>Giải đấu chưa có giai đoạn.</p>";
      return;
    }

    renderTabs(stages);
    await renderStage(stages[0]);

    try {
      const honoring = await getJson(
        "/MatchmakingApi/VinhDanh?maGiaiDau=" + maGiaiDau,
      );
      if (honoring.Success) {
        renderHonoring(honoring.Data || null);
      }
    } catch (e) {
      renderHonoring(null);
    }

    setInterval(function () {
      refreshLiveStage().catch(function () {});
    }, 8000);
  }

  init().catch(function (e) {
    showAlert(false, "Lỗi tải dữ liệu: " + e.message);
  });
})();
