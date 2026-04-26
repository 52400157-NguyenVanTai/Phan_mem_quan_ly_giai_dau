// ============================================================
// ESPORT HUB — auth.js (Login / Register page only)
// ============================================================
(function () {
  "use strict";

  // ---- API HELPER ----
  async function api(url, method, body) {
    const opts = { method: method || "GET", headers: {} };
    if (body && method === "POST") {
      opts.headers["Content-Type"] = "application/json";
      opts.body = JSON.stringify(body);
    }
    try {
      const res = await fetch(url, opts);
      return await res.json();
    } catch (e) {
      return { Success: false, Message: "Lỗi kết nối: " + e.message };
    }
  }

  // ---- AUTH ALERTS ----
  function showAuthError(msg) {
    var el = document.getElementById("auth-error");
    el.textContent = msg;
    el.classList.add("show");
    document.getElementById("auth-success").classList.remove("show");
  }
  function showAuthSuccess(msg) {
    var el = document.getElementById("auth-success");
    el.textContent = msg;
    el.classList.add("show");
    document.getElementById("auth-error").classList.remove("show");
  }
  function clearAuthMsg() {
    document.getElementById("auth-error").classList.remove("show");
    document.getElementById("auth-success").classList.remove("show");
  }

  // ---- TAB SWITCH ----
  window.switchTab = function (tab) {
    clearAuthMsg();
    document.getElementById("tab-login").classList.toggle("active", tab === "login");
    document.getElementById("tab-register").classList.toggle("active", tab === "register");
    document.getElementById("form-login").style.display = tab === "login" ? "" : "none";
    document.getElementById("form-register").style.display = tab === "register" ? "" : "none";
  };

  // ---- INITIAL TAB (check ?tab=register) ----
  (function applyInitialTab() {
    var params = new URLSearchParams(window.location.search);
    var tab = (params.get("tab") || "").toLowerCase();
    switchTab(tab === "register" ? "register" : "login");
  })();

  // ---- CHECK IF ALREADY LOGGED IN ----
  (async function checkSession() {
    var result = await api("/AuthApi/Me");
    if (result.Success && result.Data) {
      window.location.href = "/dashboard";
    }
  })();

  // ---- REGISTER ----
  document.getElementById("form-register").addEventListener("submit", async function (e) {
    e.preventDefault();
    clearAuthMsg();
    var ten = document.getElementById("reg-ten").value.trim();
    var email = document.getElementById("reg-email").value.trim();
    var mk = document.getElementById("reg-mk").value;
    var confirm = document.getElementById("reg-confirm").value;

    if (!ten || ten.length < 3) { showAuthError("Tên đăng nhập phải có ít nhất 3 ký tự."); return; }
    if (!email || !email.includes("@")) { showAuthError("Email không hợp lệ."); return; }
    if (mk.length < 8) { showAuthError("Mật khẩu phải có ít nhất 8 ký tự."); return; }
    if (mk !== confirm) { showAuthError("Mật khẩu không giống nhau."); return; }

    var btn = this.querySelector("button[type=submit]");
    btn.disabled = true;
    btn.textContent = "Đang tạo...";

    var result = await api("/AuthApi/DangKy", "POST", {
      TenDangNhap: ten,
      Email: email,
      MatKhau: mk
    });

    btn.disabled = false;
    btn.textContent = "Tạo tài khoản";

    if (result.Success) {
      showAuthSuccess("Tạo tài khoản thành công! Đang chuyển sang đăng nhập...");
      this.reset();
      setTimeout(function () { switchTab("login"); }, 1600);
    } else {
      var msg = (result.Message || "").toLowerCase();
      if (msg.includes("tên đăng nhập")) showAuthError("Tên đăng nhập đã được đăng ký.");
      else if (msg.includes("email")) showAuthError("Email đã được đăng ký.");
      else showAuthError(result.Message || "Đăng ký thất bại. Vui lòng thử lại.");
    }
  });

  // ---- LOGIN ----
  document.getElementById("form-login").addEventListener("submit", async function (e) {
    e.preventDefault();
    clearAuthMsg();
    var dinhDanh = document.getElementById("login-dinh-danh").value.trim();
    var matKhau = document.getElementById("login-mat-khau").value;

    if (!dinhDanh) { showAuthError("Vui lòng nhập tên đăng nhập hoặc email."); return; }
    if (!matKhau) { showAuthError("Vui lòng nhập mật khẩu."); return; }

    var btn = this.querySelector("button[type=submit]");
    btn.disabled = true;
    btn.textContent = "Đang kiểm tra...";

    var result = await api("/AuthApi/DangNhap", "POST", {
      DinhDanh: dinhDanh,
      MatKhau: matKhau
    });

    btn.disabled = false;
    btn.textContent = "Đăng nhập";

    if (result.Success && result.Data) {
      // Login success — redirect to dashboard
      window.location.href = "/dashboard";
    } else {
      showAuthError("Tên đăng nhập, email hoặc mật khẩu sai.");
    }
  });
})();
