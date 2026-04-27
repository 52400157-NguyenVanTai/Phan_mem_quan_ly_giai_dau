(function () {
  const root = document.getElementById("public-tournament-root");
  if (!root) return;

  const maGiaiDau = Number(root.getAttribute("data-ma-giai-dau") || 0);
  const alertBox = document.getElementById("public-alert");
  const titleEl = document.getElementById("public-title");
  const liveStatusEl = document.getElementById("public-live-status");
  const honoringEl = document.getElementById("honoring-content");
  let currentStage = null;

  if (document.getElementById("public-main-tabs")) {
    const heroEl = document.getElementById("tpub-hero");
    const subtitleEl = document.getElementById("public-subtitle");
    const statusEl = document.getElementById("public-status-label");
    const metaGridEl = document.getElementById("public-meta-grid");
    const timelineEl = document.getElementById("tournament-timeline");
    const stageNowEl = document.getElementById("public-stage-now");
    const stageSelectEl = document.getElementById("public-stage-select");
    const mainTabsEl = document.getElementById("public-main-tabs");
    const sectionOverviewEl = document.getElementById("section-overview");
    const sectionTeamsEl = document.getElementById("section-teams");
    const sectionScheduleEl = document.getElementById("section-schedule");
    const sectionResultsEl = document.getElementById("section-results");
    const sectionRulesEl = document.getElementById("section-rules");
    const sectionMap = {
      overview: sectionOverviewEl,
      teams: sectionTeamsEl,
      schedule: sectionScheduleEl,
      results: sectionResultsEl,
      rules: sectionRulesEl,
    };

    let currentSection = "overview";
    let overviewData = null;

    function fmtDate(value) {
      if (!value) return "-";
      const d = new Date(value);
      return isNaN(d.getTime()) ? "-" : d.toLocaleString("vi-VN");
    }

    function fmtCurrency(value) {
      const n = Number(value || 0);
      return n.toLocaleString("vi-VN") + " VND";
    }

    function mapFormatLabel(format) {
      const m = {
        loai_truc_tiep: "Single Elimination",
        nhanh_thang_nhanh_thua: "Double Elimination",
        vong_tron: "Round Robin",
        thuy_si: "Swiss",
        league_bang_cheo: "League/Bảng chéo",
        champion_rush: "Champion Rush",
      };
      return m[String(format || "").toLowerCase()] || (format || "-");
    }

    function mapTournamentStatus(statusRaw) {
      const status = String(statusRaw || "").toLowerCase();
      const map = {
        cho_xet_duyet: "Chờ duyệt",
        chuan_bi_dien_ra: "Chuẩn bị diễn ra",
        dang_dien_ra: "Đang thi đấu",
        tong_ket: "Tổng kết",
        tam_hoan: "Tạm hoãn",
        khoa: "Đã khóa",
        ket_thuc: "Đã kết thúc",
        cho_phe_duyet: "Chờ duyệt",
        mo_dang_ky: "Chuẩn bị diễn ra",
        sap_dien_ra: "Chuẩn bị diễn ra",
      };
      return map[status] || statusRaw || "-";
    }

    function mapMatchStatus(statusRaw) {
      const status = String(statusRaw || "").toLowerCase();
      if (status === "da_hoan_thanh")
        return { text: "Đã hoàn tất", cls: "done" };
      if (status === "dang_dau") return { text: "Đang đấu", cls: "live" };
      if (status === "chua_dau") return { text: "Sắp đấu", cls: "soon" };
      return { text: statusRaw || "-", cls: "soon" };
    }

    async function getJson(url) {
      const res = await fetch(url, { method: "GET" });
      return res.json();
    }

    function switchSection(section) {
      currentSection = section;
      Object.keys(sectionMap).forEach(function (key) {
        const el = sectionMap[key];
        if (!el) return;
        el.classList.toggle("d-none", key !== section);
      });
      mainTabsEl.querySelectorAll("button[data-section]").forEach(function (btn) {
        btn.classList.toggle("active", btn.getAttribute("data-section") === section);
      });
    }

    function renderMetaCards(data) {
      const cards = [
        { label: "Game", value: data.TenGame || "-" },
        { label: "Ban tổ chức", value: data.TenBanToChuc || "-" },
        {
          label: "Đăng ký",
          value: fmtDate(data.ThoiGianMoDangKy) + " - " + fmtDate(data.ThoiGianDongDangKy),
        },
        {
          label: "Thi đấu",
          value: fmtDate(data.NgayBatDau) + " - " + fmtDate(data.NgayKetThuc),
        },
        {
          label: "Giải thưởng",
          value: fmtCurrency(data.TongGiaiThuong),
        },
        {
          label: "Số đội",
          value: String(data.SoDoiDaDangKy || 0) + " / " + String(data.SoDoiToiDa || 0),
        },
      ];
      metaGridEl.innerHTML = cards
        .map(function (x) {
          return (
            '<div class="tpub-meta-card">' +
            '<span class="tpub-meta-label">' +
            x.label +
            "</span>" +
            '<span class="tpub-meta-value">' +
            x.value +
            "</span>" +
            "</div>"
          );
        })
        .join("");
    }

    function renderTimeline(timeline) {
      if (!Array.isArray(timeline) || !timeline.length) {
        timelineEl.innerHTML = "<div class='text-muted'>Chưa có timeline.</div>";
        return;
      }

      timelineEl.innerHTML = timeline
        .map(function (item) {
          const cls = item.IsCurrent ? "current" : item.IsDone ? "done" : "pending";
          return (
            '<div class="tpub-step ' +
            cls +
            '">' +
            '<div class="tpub-step-dot"></div>' +
            '<div class="tpub-step-label">' +
            (item.Label || "-") +
            "</div>" +
            "</div>"
          );
        })
        .join("");
    }

    function renderOverview(data) {
      sectionOverviewEl.innerHTML =
        '<div class="tpub-overview-grid">' +
        '<article class="tpub-panel"><h5>Mô tả</h5><p>' +
        (data.MoTa || "Chưa có mô tả cho giải đấu này.") +
        "</p></article>" +
        '<article class="tpub-panel"><h5>Thông tin nhanh</h5>' +
        '<ul class="tpub-quick-list">' +
        "<li><span>Trạng thái</span><b>" +
        mapTournamentStatus(data.TrangThai) +
        "</b></li>" +
        "<li><span>Đóng đăng ký</span><b>" +
        fmtDate(data.ThoiGianDongDangKy) +
        "</b></li>" +
        "<li><span>Giai đoạn đang diễn ra</span><b>" +
        (data.GiaiDoanDangDienRa || "Chưa xác định") +
        "</b></li>" +
        "</ul></article>" +
        "</div>";
    }

    function renderTeams(data) {
      const teams = Array.isArray(data.DoiThamGia) ? data.DoiThamGia : [];
      if (!teams.length) {
        sectionTeamsEl.innerHTML = "<div class='text-muted'>Chưa có đội tham gia được duyệt.</div>";
        return;
      }

      sectionTeamsEl.innerHTML =
        '<div class="tpub-table-wrap"><table class="tpub-table">' +
        "<thead><tr><th>#</th><th>Đội</th><th>Nhóm</th><th>Seed</th><th>Trạng thái</th></tr></thead>" +
        "<tbody>" +
        teams
          .map(function (t, idx) {
            return (
              "<tr>" +
              "<td>" +
              (idx + 1) +
              "</td>" +
              "<td>" +
              (t.TenDoi || "-") +
              "</td>" +
              "<td>" +
              (t.TenNhom || "-") +
              "</td>" +
              "<td>" +
              (t.HatGiong == null ? "-" : t.HatGiong) +
              "</td>" +
              "<td><span class='tpub-pill'>" +
              (t.TrangThaiThamGia || "-") +
              "</span></td>" +
              "</tr>"
            );
          })
          .join("") +
        "</tbody></table></div>";
    }

    function renderRules(data) {
      sectionRulesEl.innerHTML =
        '<article class="tpub-panel"><h5>Luật giải</h5><div class="tpub-rules">' +
        (data.LuatGiai || "Chưa cập nhật luật giải.") +
        "</div></article>";
    }

    function renderScheduleBlocked(data) {
      sectionScheduleEl.innerHTML =
        '<div class="tpub-empty">Lịch thi đấu sẽ hiển thị sau khi đóng đăng ký. Hiện tại đang chờ chốt danh sách đội (' +
        String(data.SoDoiDaDangKy || 0) +
        "/" +
        String(data.SoDoiToiDa || 0) +
        ").</div>";
    }

    function renderMatchCard(m) {
      const st = mapMatchStatus(m.TrangThai);
      const score =
        m.KetQuaTomTat ||
        (m.DiemNhomA != null && m.DiemNhomB != null
          ? String(m.DiemNhomA) + " - " + String(m.DiemNhomB)
          : "vs");

      return (
        '<div class="tpub-match-card">' +
        '<div class="tpub-match-head"><b>Trận #' +
        m.MaTran +
        "</b><span class='tpub-pill " +
        st.cls +
        "'>" +
        st.text +
        "</span></div>" +
        '<div class="tpub-match-team"><span>' +
        (m.TenNhomA || "TBD") +
        "</span><strong>" +
        score +
        "</strong><span>" +
        (m.TenNhomB || "TBD") +
        "</span></div>" +
        '<div class="tpub-match-foot">' +
        "Vòng " +
        (m.SoVong || "?") +
        " • " +
        fmtDate(m.ThoiGianBatDau) +
        "</div></div>"
      );
    }

    function getRoundNow(matches) {
      if (!Array.isArray(matches) || !matches.length) {
        return null;
      }

      const dangDau = matches
        .filter(function (m) {
          return String(m.TrangThai || "") === "dang_dau";
        })
        .map(function (m) {
          return Number(m.SoVong || 0);
        })
        .filter(function (v) {
          return v > 0;
        });
      if (dangDau.length) {
        return Math.min.apply(null, dangDau);
      }

      const chuaDau = matches
        .filter(function (m) {
          return String(m.TrangThai || "") === "chua_dau";
        })
        .map(function (m) {
          return Number(m.SoVong || 0);
        })
        .filter(function (v) {
          return v > 0;
        });
      if (chuaDau.length) {
        return Math.min.apply(null, chuaDau);
      }

      const all = matches
        .map(function (m) {
          return Number(m.SoVong || 0);
        })
        .filter(function (v) {
          return v > 0;
        });
      if (all.length) {
        return Math.max.apply(null, all);
      }

      return null;
    }

    function renderSingleBracket(matches) {
      const grouped = {};
      matches.forEach(function (m) {
        const r = Number(m.SoVong || 0);
        if (!grouped[r]) grouped[r] = [];
        grouped[r].push(m);
      });
      const rounds = Object.keys(grouped)
        .map(Number)
        .sort(function (a, b) {
          return a - b;
        });

      return (
        '<div class="tpub-round-columns">' +
        rounds
          .map(function (round) {
            return (
              '<div class="tpub-round-col"><h6>Round ' +
              round +
              "</h6>" +
              grouped[round].map(renderMatchCard).join("") +
              "</div>"
            );
          })
          .join("") +
        "</div>"
      );
    }

    function renderDoubleBracket(matches) {
      const upper = matches.filter(function (x) {
        return String(x.NhanhDau || "") === "upper";
      });
      const lower = matches.filter(function (x) {
        return String(x.NhanhDau || "") === "lower";
      });
      const grand = matches.filter(function (x) {
        return String(x.NhanhDau || "") === "grand_final";
      });

      return (
        '<div class="tpub-dual-bracket">' +
        '<div><h5>Nhánh thắng</h5>' +
        (upper.length ? renderSingleBracket(upper) : "<div class='tpub-empty'>Chưa có dữ liệu.</div>") +
        "</div>" +
        '<div><h5>Nhánh thua</h5>' +
        (lower.length ? renderSingleBracket(lower) : "<div class='tpub-empty'>Chưa có dữ liệu.</div>") +
        "</div>" +
        '<div><h5>Chung kết tổng</h5>' +
        (grand.length ? grand.map(renderMatchCard).join("") : "<div class='tpub-empty'>Chưa có dữ liệu.</div>") +
        "</div></div>"
      );
    }

    function renderRoundList(matches) {
      const grouped = {};
      matches.forEach(function (m) {
        const r = Number(m.SoVong || 0);
        if (!grouped[r]) grouped[r] = [];
        grouped[r].push(m);
      });

      return Object.keys(grouped)
        .map(Number)
        .sort(function (a, b) {
          return a - b;
        })
        .map(function (round) {
          return (
            '<div class="tpub-round-group"><h6>Round ' +
            round +
            "</h6>" +
            grouped[round].map(renderMatchCard).join("") +
            "</div>"
          );
        })
        .join("");
    }

    function renderStandings(rows) {
      if (!Array.isArray(rows) || !rows.length) {
        return "<div class='tpub-empty'>Chưa có bảng xếp hạng.</div>";
      }

      return (
        '<div class="tpub-table-wrap"><table class="tpub-table">' +
        "<thead><tr><th>Hạng</th><th>Đội</th><th>Nhóm</th><th>Trận</th><th>Thắng</th><th>Thua</th><th>Điểm</th></tr></thead>" +
        "<tbody>" +
        rows
          .map(function (r, idx) {
            return (
              "<tr><td>" +
              (r.thu_hang_hien_tai || idx + 1) +
              "</td><td>" +
              (r.ten_doi || "-") +
              "</td><td>" +
              (r.ten_nhom || "-") +
              "</td><td>" +
              (r.so_tran_da_dau || 0) +
              "</td><td>" +
              (r.so_tran_thang || 0) +
              "</td><td>" +
              (r.so_tran_thua || 0) +
              "</td><td>" +
              (r.diem_tong_ket || 0) +
              "</td></tr>"
            );
          })
          .join("") +
        "</tbody></table></div>"
      );
    }

    async function renderScheduleAndResult(stage) {
      if (!stage || !stage.MaGiaiDoan) return;

      const matchRes = await getJson(
        "/MatchmakingApi/DanhSachTran?maGiaiDoan=" + stage.MaGiaiDoan,
      );
      if (!matchRes.Success) {
        sectionScheduleEl.innerHTML =
          "<div class='tpub-empty'>" + (matchRes.Message || "Không tải được lịch thi đấu.") + "</div>";
        return;
      }

      const matches = Array.isArray(matchRes.Data) ? matchRes.Data : [];

      if (overviewData.DangChoChotDanhSach && matches.length === 0) {
        renderScheduleBlocked(overviewData);
        sectionResultsEl.innerHTML =
          "<div class='tpub-empty'>Kết quả/BXH sẽ có sau khi lịch thi đấu được tạo.</div>";
        return;
      }

      if (matches.length === 0) {
        sectionScheduleEl.innerHTML =
          "<div class='tpub-empty'>Chưa sinh lịch thi đấu cho giai đoạn này.</div>";
        sectionResultsEl.innerHTML =
          "<div class='tpub-empty'>Chưa có kết quả cho giai đoạn này.</div>";
        return;
      }

      const roundNow = getRoundNow(matches);
      stageNowEl.textContent =
        "Giai đoạn hiện tại: " +
        (stage.TenGiaiDoan || "-") +
        " • " +
        mapFormatLabel(stage.TheThuc) +
        (roundNow ? " • Round " + roundNow : "");

      const format = String(stage.TheThuc || "").toLowerCase();
      let scheduleHtml = "";
      if (format === "loai_truc_tiep") scheduleHtml = renderSingleBracket(matches);
      else if (format === "nhanh_thang_nhanh_thua")
        scheduleHtml = renderDoubleBracket(matches);
      else scheduleHtml = renderRoundList(matches);

      sectionScheduleEl.innerHTML =
        '<h5 class="mb-3">' +
        mapFormatLabel(stage.TheThuc) +
        "</h5>" +
        (scheduleHtml || "<div class='tpub-empty'>Chưa có lịch thi đấu.</div>");

      const standingsFormats = [
        "vong_tron",
        "league_bang_cheo",
        "thuy_si",
        "champion_rush",
      ];
      if (standingsFormats.indexOf(format) >= 0) {
        const bxhRes = await getJson(
          "/MatchmakingApi/BangXepHang?maGiaiDoan=" + stage.MaGiaiDoan,
        );
        sectionResultsEl.innerHTML = bxhRes.Success
          ? renderStandings(bxhRes.Data || [])
          : "<div class='tpub-empty'>" + (bxhRes.Message || "Không tải được bảng xếp hạng") + "</div>";
      } else {
        const completed = matches.filter(function (m) {
          return String(m.TrangThai || "") === "da_hoan_thanh";
        });
        sectionResultsEl.innerHTML = completed.length
          ? '<div class="tpub-round-group">' + completed.map(renderMatchCard).join("") + "</div>"
          : "<div class='tpub-empty'>Chưa có trận hoàn tất để hiển thị kết quả.</div>";
      }
    }

    function bindMainTabs() {
      mainTabsEl.querySelectorAll("button[data-section]").forEach(function (btn) {
        btn.addEventListener("click", function () {
          switchSection(btn.getAttribute("data-section"));
        });
      });
    }

    function bindStageSelect(stages) {
      stageSelectEl.innerHTML = stages
        .map(function (s) {
          return (
            '<option value="' +
            s.MaGiaiDoan +
            '">' +
            (s.TenGiaiDoan || "Giai đoạn") +
            " • " +
            mapFormatLabel(s.TheThuc) +
            "</option>"
          );
        })
        .join("");

      stageSelectEl.addEventListener("change", async function () {
        const selected = Number(stageSelectEl.value || 0);
        currentStage = stages.find(function (x) {
          return x.MaGiaiDoan === selected;
        });
        if (currentStage) {
          await renderScheduleAndResult(currentStage);
        }
      });
    }

    async function refreshLive() {
      if (!currentStage || !currentStage.MaGiaiDoan) return;
      const live = await getJson(
        "/MatchmakingApi/LiveSnapshot?maGiaiDau=" +
          maGiaiDau +
          "&maGiaiDoan=" +
          currentStage.MaGiaiDoan,
      );
      if (!live.Success) {
        if (liveStatusEl)
          liveStatusEl.textContent =
            "Live Engine tạm gián đoạn: " + (live.Message || "Không rõ lỗi");
        return;
      }

      if (liveStatusEl) {
        liveStatusEl.textContent =
          "Live cập nhật lúc " + fmtDate((live.Data || {}).ServerTime || null);
      }

      if (currentSection === "results") {
        const format = String(currentStage.TheThuc || "").toLowerCase();
        const standingsFormats = [
          "vong_tron",
          "league_bang_cheo",
          "thuy_si",
          "champion_rush",
        ];
        if (standingsFormats.indexOf(format) >= 0) {
          sectionResultsEl.innerHTML = renderStandings((live.Data || {}).BangXepHang || []);
        }
      }
      renderHonoring((live.Data || {}).VinhDanh || null);
    }

    async function initModern() {
      if (maGiaiDau <= 0) {
        showAlert(false, "Mã giải đấu không hợp lệ.");
        return;
      }

      const result = await getJson(
        "/MatchmakingApi/CongKhai?maGiaiDau=" + maGiaiDau,
      );
      if (!result.Success) {
        showAlert(false, result.Message || "Không tải được cổng thông tin giải đấu.");
        return;
      }

      overviewData = result.Data || {};
      titleEl.textContent = overviewData.TenGiaiDau || "Giải đấu";
      subtitleEl.textContent =
        (overviewData.TenGame || "") + " • " + (overviewData.TenBanToChuc || "Ban tổ chức");
      statusEl.textContent = mapTournamentStatus(overviewData.TrangThai);

      if (overviewData.BannerUrl) {
        heroEl.style.backgroundImage =
          "linear-gradient(120deg, rgba(9,11,17,0.68), rgba(14,18,29,0.86)), url('" +
          overviewData.BannerUrl +
          "')";
      }

      renderMetaCards(overviewData);
      renderTimeline(overviewData.Timeline || []);
      renderOverview(overviewData);
      renderTeams(overviewData);
      renderRules(overviewData);
      bindMainTabs();

      const stages = Array.isArray(overviewData.GiaiDoan) ? overviewData.GiaiDoan : [];
      if (!stages.length) {
        sectionScheduleEl.innerHTML =
          "<div class='tpub-empty'>Giải đấu chưa cấu hình giai đoạn thi đấu.</div>";
        sectionResultsEl.innerHTML =
          "<div class='tpub-empty'>Chưa có dữ liệu kết quả.</div>";
        stageNowEl.textContent = "Chưa có giai đoạn hoạt động.";
      } else {
        bindStageSelect(stages);
        currentStage = stages[0];
        stageSelectEl.value = String(currentStage.MaGiaiDoan);
        await renderScheduleAndResult(currentStage);
      }

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

      switchSection("overview");
      setInterval(function () {
        refreshLive().catch(function () {});
      }, 9000);
    }

    initModern().catch(function (e) {
      showAlert(false, "Lỗi tải dữ liệu: " + e.message);
    });
    return;
  }

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

})();
