(function () {
  "use strict";

  function fmtNum(value) {
    return Number(value || 0).toLocaleString("vi-VN");
  }

  function fmtMoney(value) {
    if (!value || Number(value) <= 0) return "Chưa công bố";
    return Number(value).toLocaleString("vi-VN") + "đ";
  }

  function fmtDate(value) {
    if (!value) return "Chưa có lịch";
    var d = new Date(value);
    if (isNaN(d.getTime())) return "Chưa có lịch";
    return d.toLocaleString("vi-VN", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  }

  function statusLabel(status) {
    var map = {
      chuan_bi_dien_ra: "Chuẩn bị diễn ra",
      dang_dien_ra: "Đang diễn ra",
      tong_ket: "Tổng kết",
      tam_hoan: "Tạm hoãn",
      khoa: "Đã khóa",
      ket_thuc: "Kết thúc",
      mo_dang_ky: "Chuẩn bị diễn ra",
      sap_dien_ra: "Chuẩn bị diễn ra",
      chua_dau: "Sắp đấu",
      dang_dau: "Đang đấu",
      da_hoan_thanh: "Đã hoàn thành",
    };
    return map[status] || (status || "Không xác định");
  }

  function setText(id, value) {
    var el = document.getElementById(id);
    if (el) el.textContent = value;
  }

  function renderHeroStats(stats) {
    setText("hero-stat-active-tournaments", fmtNum(stats.TongGiaiDangHoatDong));
    setText("hero-stat-active-teams", fmtNum(stats.TongDoiTuyenThamGia));
    setText("hero-stat-supported-games", fmtNum(stats.TongGameHoTro));
  }

  function tournamentCardHtml(t) {
    var progress =
      t.SoLuongDoiToiDa > 0
        ? Math.min(100, Math.round((t.SoDoiDaDangKy * 100) / t.SoLuongDoiToiDa))
        : null;

    return (
      '<article class="public-card tournament-card">' +
      '<div class="card-chip chip-purple">' +
      (t.TenGame || "Giải hỗn hợp") +
      "</div>" +
      "<h3>" +
      (t.TenGiaiDau || "Giải đấu") +
      "</h3>" +
      '<p class="public-line-2">Trạng thái: ' +
      statusLabel(t.TrangThai) +
      "</p>" +
      '<div class="card-meta-row"><span>📅 ' +
      fmtDate(t.NgayBatDau) +
      "</span><span>💰 " +
      fmtMoney(t.TongGiaiThuong) +
      "</span></div>" +
      '<div class="card-meta-row"><span>👥 ' +
      fmtNum(t.SoDoiDaDangKy) +
      " đội đã duyệt</span><span>🏁 " +
      (t.SoLuongDoiToiDa > 0
        ? fmtNum(t.SoLuongDoiToiDa) + " đội tối đa"
        : "Không giới hạn") +
      "</span></div>" +
      (progress !== null
        ? '<div class="slot-progress"><div class="slot-progress-fill" style="width:' +
          progress +
          '%"></div></div>'
        : "") +
      "</article>"
    );
  }

  function renderTournamentGrid(id, list, emptyText) {
    var el = document.getElementById(id);
    if (!el) return;

    if (!list || list.length === 0) {
      el.innerHTML =
        '<article class="public-card empty-card">' + emptyText + "</article>";
      return;
    }

    el.innerHTML = list.map(tournamentCardHtml).join("");
  }

  function gameCardHtml(g) {
    return (
      '<article class="public-card game-card">' +
      '<span class="game-name">' +
      (g.TenGame || "Game") +
      "</span>" +
      '<small class="game-meta">' +
      (g.TheLoai || "-") +
      " • " +
      fmtNum(g.SoGiaiDangVanHanh) +
      " giải vận hành</small>" +
      "</article>"
    );
  }

  function renderGameGrid(list) {
    var el = document.getElementById("supported-games-list");
    if (!el) return;

    if (!list || list.length === 0) {
      el.innerHTML =
        '<article class="public-card empty-card">Chưa có game active để hiển thị.</article>';
      return;
    }

    el.innerHTML = list.map(gameCardHtml).join("");
  }

  function teamCardHtml(t) {
    return (
      '<article class="public-card team-card">' +
      '<div class="team-header"><span class="team-tag">' +
      (t.DangTuyen ? "Đang tuyển" : "Đang hoạt động") +
      "</span><span class="team-points">" +
      fmtNum(t.SoGiaiDaThamGia) +
      " giải</span></div>" +
      "<h3>" +
      (t.TenDoi || "Đội tuyển") +
      "</h3>" +
      '<p class="public-line-2">' +
      (t.Slogan || "Đội tuyển đang vận hành trong hệ thống.") +
      "</p>" +
      '<div class="card-meta-row"><span>👤 ' +
      fmtNum(t.SoThanhVienActive) +
      " thành viên active</span><span>🎮 " +
      fmtNum(t.SoGiaiDaThamGia) +
      " giải đã tham gia</span></div>" +
      "</article>"
    );
  }

  function renderTeamGrid(list) {
    var el = document.getElementById("featured-teams-list");
    if (!el) return;

    if (!list || list.length === 0) {
      el.innerHTML =
        '<article class="public-card empty-card">Chưa có dữ liệu đội đủ điều kiện xếp nổi bật.</article>';
      return;
    }

    el.innerHTML = list.map(teamCardHtml).join("");
  }

  function stackedTournamentHtml(t) {
    var slotText =
      t.SoLuongDoiToiDa > 0
        ? fmtNum(t.SoDoiDaDangKy) + "/" + fmtNum(t.SoLuongDoiToiDa) + " đội"
        : fmtNum(t.SoDoiDaDangKy) + " đội";

    return (
      '<div class="stacked-item">' +
      '<div class="stacked-title">' +
      (t.TenGiaiDau || "Giải đấu") +
      "</div>" +
      '<div class="stacked-meta">' +
      (t.TenGame || "Giải hỗn hợp") +
      " • " +
      fmtDate(t.NgayBatDau) +
      " • " +
      slotText +
      "</div>" +
      "</div>"
    );
  }

  function renderStackedList(id, list, emptyText) {
    var el = document.getElementById(id);
    if (!el) return;

    if (!list || list.length === 0) {
      el.innerHTML = '<div class="empty-line">' + emptyText + "</div>";
      return;
    }

    el.innerHTML = list.map(stackedTournamentHtml).join("");
  }

  function matchCardHtml(m) {
    return (
      '<article class="public-card match-card">' +
      '<div class="card-meta-row"><span class="card-chip chip-cyan">' +
      statusLabel(m.TrangThai) +
      "</span><span>" +
      fmtDate(m.ThoiGianBatDau || m.ThoiGianKetThuc) +
      "</span></div>" +
      "<h3>" +
      (m.TenGiaiDau || "Giải đấu") +
      "</h3>" +
      '<p class="public-line-2">' +
      (m.TenGiaiDoan ? "Giai đoạn: " + m.TenGiaiDoan : "Chưa có giai đoạn") +
      "</p>" +
      '<div class="card-meta-row"><span>🅰 ' +
      (m.TenDoiA || "TBD") +
      "</span><span>🅱 " +
      (m.TenDoiB || "TBD") +
      "</span></div>" +
      "</article>"
    );
  }

  function renderMatches(list) {
    var el = document.getElementById("live-matches-list");
    if (!el) return;

    if (!list || list.length === 0) {
      el.innerHTML =
        '<article class="public-card empty-card">Hiện chưa có dữ liệu trận phù hợp để hiển thị công khai.</article>';
      return;
    }

    el.innerHTML = list.map(matchCardHtml).join("");
  }

  function fillHeroSpotlight(openList, upcomingList, matches, stats) {
    var open = openList && openList.length > 0 ? openList[0] : null;
    var upcoming = upcomingList && upcomingList.length > 0 ? upcomingList[0] : null;
    var match = matches && matches.length > 0 ? matches[0] : null;

    setText("hero-open-title", open ? open.TenGiaiDau || "Giải mở đăng ký" : "Chưa có giải mở đăng ký");
    setText(
      "hero-open-meta",
      open
        ? (open.TenGame || "Giải hỗn hợp") +
            " • " +
            fmtNum(open.SoDoiDaDangKy) +
            (open.SoLuongDoiToiDa > 0 ? "/" + fmtNum(open.SoLuongDoiToiDa) : "") +
            " đội"
        : "Hệ thống chưa có giải ở trạng thái mở đăng ký"
    );

    setText("hero-upcoming-title", upcoming ? upcoming.TenGiaiDau || "Giải sắp diễn ra" : "Chưa có giải sắp diễn ra");
    setText(
      "hero-upcoming-meta",
      upcoming
        ? (upcoming.TenGame || "Giải hỗn hợp") + " • " + fmtDate(upcoming.NgayBatDau)
        : "Hệ thống chưa có lịch sắp diễn ra"
    );

    if (match) {
      setText("hero-match-title", match.TenGiaiDau || "Trận đấu công khai");
      setText("hero-match-time", fmtDate(match.ThoiGianBatDau || match.ThoiGianKetThuc));
      setText("hero-match-team-a-name", match.TenDoiA || "TBD");
      setText("hero-match-team-b-name", match.TenDoiB || "TBD");
      setText("hero-match-team-a-mark", (match.TenDoiA || "T").substring(0, 2).toUpperCase());
      setText("hero-match-team-b-mark", (match.TenDoiB || "T").substring(0, 2).toUpperCase());
      setText("hero-match-status", statusLabel(match.TrangThai));
      setText("hero-match-meta-left", match.TenGiaiDoan ? "Giai đoạn: " + match.TenGiaiDoan : "Chưa rõ giai đoạn");
      setText("hero-match-meta-right", "👁 " + fmtNum(stats.TongLuotTheoDoi) + " lượt theo dõi giải");
    }
  }

  async function loadHomepageData() {
    var res = await fetch("/HomepageApi/PublicData", { method: "GET" });
    var result = await res.json();

    if (!result || !result.Success || !result.Data) {
      return;
    }

    var data = result.Data;
    var stats = data.HeroStats || {};
    var featuredTournaments = data.FeaturedTournaments || [];
    var supportedGames = data.SupportedGames || [];
    var featuredTeams = data.FeaturedTeams || [];
    var openRegistration = data.OpenRegistrationTournaments || [];
    var upcoming = data.UpcomingTournaments || [];
    var matches = data.RecentOrLiveMatches || [];

    renderHeroStats(stats);
    renderTournamentGrid(
      "featured-tournaments-list",
      featuredTournaments,
      "Hiện chưa có giải đáp ứng rule nổi bật (mở đăng ký/sắp diễn ra/đang diễn ra)."
    );
    renderGameGrid(supportedGames);
    renderTeamGrid(featuredTeams);
    renderStackedList(
      "open-registration-list",
      openRegistration,
      "Hiện chưa có giải mở đăng ký."
    );
    renderStackedList(
      "upcoming-tournaments-list",
      upcoming,
      "Hiện chưa có giải sắp diễn ra."
    );
    renderMatches(matches);
    fillHeroSpotlight(openRegistration, upcoming, matches, stats);
  }

  loadHomepageData().catch(function () {});
})();
