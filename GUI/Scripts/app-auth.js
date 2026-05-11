function formToObject(form) {
  const data = new FormData(form);
  const object = {};
  data.forEach((value, key) => (object[key] = value));
  return object;
}

function showMessage(elementId, response) {
  const element = document.getElementById(elementId);
  if (!element) return;

  element.textContent = response.message || "";
  element.className = response.success
    ? "form-message success"
    : "form-message error";
}

async function postApi(url, body) {
  const response = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body || {}),
  });

  return parseApiResponse(response);
}

async function postFormApi(url, formData) {
  const response = await fetch(url, {
    method: "POST",
    body: formData,
  });

  return parseApiResponse(response);
}

async function getApi(url) {
  const response = await fetch(url, { method: "GET" });
  return parseApiResponse(response);
}

async function parseApiResponse(response) {
  const contentType = response.headers.get("content-type") || "";
  if (contentType.indexOf("application/json") === -1) {
    return {
      success: false,
      message: "Duong dan API khong hop le hoac server tra ve trang HTML.",
      data: null,
      status: response.status,
    };
  }

  const result = await response.json();
  if (!response.ok && result.success === undefined && result.Success === undefined) {
    result.success = false;
  }
  return result;
}

function buildUrl(url, params) {
  const query = new URLSearchParams();
  Object.keys(params || {}).forEach(function (key) {
    if (params[key] !== undefined && params[key] !== null) {
      query.append(key, params[key]);
    }
  });

  const queryString = query.toString();
  if (!queryString) return url;
  return url + (url.indexOf("?") === -1 ? "?" : "&") + queryString;
}

function setupHomeAuth() {
  const loginForm = document.getElementById("loginForm");
  const registerForm = document.getElementById("registerForm");
  const forgotForm = document.getElementById("forgotPasswordForm");

  if (loginForm) {
    loginForm.addEventListener("submit", async function (event) {
      event.preventDefault();
      const result = await postApi("/AuthApi/Login", formToObject(loginForm));
      showMessage("loginMessage", result);

      if (result.success) {
        window.location.href = "/Dashboard";
      }
    });
  }

  if (registerForm) {
    registerForm.addEventListener("submit", async function (event) {
      event.preventDefault();
      const result = await postApi(
        "/AuthApi/Register",
        formToObject(registerForm),
      );
      showMessage("registerMessage", result);

      if (result.success) {
        registerForm.reset();
        setTimeout(function () {
          window.location.href = "/Home/DangNhap";
        }, 800);
      }
    });
  }

  if (forgotForm) {
    forgotForm.addEventListener("submit", async function (event) {
      event.preventDefault();
      const result = await postApi(
        "/AuthApi/ForgotPassword",
        formToObject(forgotForm),
      );
      showMessage("forgotPasswordMessage", result);

      if (result.success) {
        forgotForm.reset();
        setTimeout(function () {
          window.location.href = "/Home/DangNhap";
        }, 800);
      }
    });
  }
}

async function setupProfile() {
  const profileForm = document.getElementById("profileForm");
  const changePasswordForm = document.getElementById("changePasswordForm");
  const avatarFileInput = document.getElementById("avatarFileInput");
  const profileAvatarPreview = document.getElementById("profileAvatarPreview");

  if (profileForm) {
    const result = await getApi("/AuthApi/CurrentUser");
    if (!result.success) {
      window.location.href = "/";
      return;
    }

    const user = result.data;
    profileForm.ten_dang_nhap.value = user.ten_dang_nhap || "";
    profileForm.email.value = user.email || "";
    profileForm.avatar_url.value = user.avatar_url || "";
    profileForm.bio.value = user.bio || "";
    if (profileAvatarPreview && user.avatar_url) {
      profileAvatarPreview.src = user.avatar_url;
    }

    if (avatarFileInput) {
      avatarFileInput.addEventListener("change", async function () {
        const file = avatarFileInput.files && avatarFileInput.files[0];
        if (!file) return;

        if (profileAvatarPreview) {
          profileAvatarPreview.src = URL.createObjectURL(file);
        }

        const formData = new FormData();
        formData.append("avatar", file);
        const uploadResult = await postFormApi(
          "/AuthApi/UploadAvatar",
          formData,
        );
        showMessage("profileMessage", uploadResult);

        if (uploadResult.success) {
          profileForm.avatar_url.value = uploadResult.data || "";
          if (profileAvatarPreview && uploadResult.data) {
            profileAvatarPreview.src = uploadResult.data;
          }
        }
      });
    }

    profileForm.addEventListener("submit", async function (event) {
      event.preventDefault();
      const updateResult = await postApi(
        "/AuthApi/UpdateProfile",
        formToObject(profileForm),
      );
      showMessage("profileMessage", updateResult);
    });
  }

  if (changePasswordForm) {
    changePasswordForm.addEventListener("submit", async function (event) {
      event.preventDefault();
      const result = await postApi(
        "/AuthApi/ChangePassword",
        formToObject(changePasswordForm),
      );
      showMessage("changePasswordMessage", result);

      if (result.success) {
        changePasswordForm.reset();
      }
    });
  }
}

function setupLogout() {
  const logoutButton = document.getElementById("logoutButton");
  if (!logoutButton) return;

  logoutButton.addEventListener("click", async function () {
    const result = await postApi("/AuthApi/Logout", {});
    if (result.success) {
      window.location.href = "/";
    }
  });
}

function fillSelect(select, items, valueField, textField, placeholder) {
  if (!select) return;
  if (!Array.isArray(items)) return;
  select.innerHTML = "";
  const defaultOption = document.createElement("option");
  defaultOption.value = "";
  defaultOption.textContent = placeholder;
  select.appendChild(defaultOption);

  items.forEach(function (item) {
    const option = document.createElement("option");
    option.value = item[valueField];
    option.textContent = item[textField];
    select.appendChild(option);
  });
}

function hasSelectOptions(select) {
  return select && select.options && select.options.length > 1;
}

function getResponseData(response) {
  if (!response) return null;
  return response.data || response.Data || null;
}

function isResponseSuccess(response) {
  return !!(response && (response.success || response.Success));
}

function renderHoSoThiDauPreview(hoSo) {
  const preview = document.getElementById("hoSoThiDauPreview");
  if (!preview) return;

  if (!hoSo) {
    preview.innerHTML = '<p class="text-muted">Bạn chưa có hồ sơ thi đấu.</p>';
    return;
  }

  preview.innerHTML =
    "<dl>" +
    "<dt>Trò chơi</dt><dd>" +
    (hoSo.ten_game || "") +
    "</dd>" +
    "<dt>ID trong game</dt><dd>" +
    (hoSo.in_game_id || "") +
    "</dd>" +
    "<dt>Tên trong game</dt><dd>" +
    (hoSo.in_game_name || "") +
    "</dd>" +
    "<dt>Loại vị trí</dt><dd>" +
    (hoSo.loai_vi_tri === "HuanLuyen" ? "Huấn luyện / quản lý" : "Tuyển thủ") +
    "</dd>" +
    "<dt>Vị trí</dt><dd>" +
    (hoSo.ten_vi_tri || "") +
    "</dd>" +
    "<dt>Thành tích</dt><dd>" +
    (hoSo.thanh_tich || "Chưa nhập") +
    "</dd>" +
    "</dl>";
}

async function setupHoSoThiDau() {
  const form = document.getElementById("hoSoThiDauForm");
  if (!form) return;

  const maTroChoiSelect = document.getElementById("maTroChoiSelect");
  const loaiViTriSelect = document.getElementById("loaiViTriSelect");
  const maViTriSelect = document.getElementById("maViTriSelect");
  const deleteButton = document.getElementById("xoaHoSoThiDauButton");
  const urls = {
    troChoi: form.dataset.troChoiUrl,
    viTri: form.dataset.viTriUrl,
    current: form.dataset.currentUrl,
    save: form.dataset.saveUrl,
    delete: form.dataset.deleteUrl,
  };

  async function loadViTri(selectedValue) {
    const maTroChoi = maTroChoiSelect.value;
    const loaiViTri = loaiViTriSelect.value;
    if (!maTroChoi || !loaiViTri) {
      fillSelect(maViTriSelect, [], "ma_vi_tri", "ten_vi_tri", "Chá»n vá»‹ trÃ­");
      return;
    }
    const result = await getApi(
      buildUrl(urls.viTri, {
        maTroChoi: maTroChoi,
        loaiViTri: loaiViTri,
      }),
    );

    const viTriItems = getResponseData(result);

    if (isResponseSuccess(result)) {
      fillSelect(
        maViTriSelect,
        viTriItems,
        "ma_vi_tri",
        "ten_vi_tri",
        "Chọn vị trí",
      );
      if (selectedValue) {
        maViTriSelect.value = selectedValue;
      }
    } else {
      fillSelect(maViTriSelect, [], "ma_vi_tri", "ten_vi_tri", "Chá»n vá»‹ trÃ­");
      showMessage("hoSoThiDauMessage", result);
    }
  }

  const troChoiResult = await getApi(urls.troChoi);
  const troChoiItems = getResponseData(troChoiResult);
  troChoiResult.data = troChoiItems;
  if (!isResponseSuccess(troChoiResult) || !Array.isArray(troChoiItems) || troChoiItems.length === 0) {
    if (!hasSelectOptions(maTroChoiSelect)) {
      showMessage("hoSoThiDauMessage", {
        success: false,
        message: troChoiResult.message || "Khong tai duoc danh sach tro choi.",
      });
    }
  } else {
  fillSelect(maTroChoiSelect, troChoiResult.data, "ma_tro_choi", "ten_game", "Chọn trò chơi");
  }

  const currentResult = await getApi(urls.current);
  const currentData = getResponseData(currentResult);
  if (isResponseSuccess(currentResult) && currentData) {
    const hoSo = currentData;
    form.ma_tro_choi.value = hoSo.ma_tro_choi || "";
    form.in_game_id.value = hoSo.in_game_id || "";
    form.in_game_name.value = hoSo.in_game_name || "";
    form.loai_vi_tri.value = hoSo.loai_vi_tri || "";
    form.thanh_tich.value = hoSo.thanh_tich || "";
    await loadViTri(hoSo.ma_vi_tri_so_truong || "");
    renderHoSoThiDauPreview(hoSo);
  } else {
    renderHoSoThiDauPreview(null);
  }

  maTroChoiSelect.addEventListener("change", function () {
    loadViTri();
  });

  loaiViTriSelect.addEventListener("change", function () {
    loadViTri();
  });

  form.addEventListener("submit", async function (event) {
    event.preventDefault();
    const result = await postApi(urls.save, formToObject(form));
    showMessage("hoSoThiDauMessage", result);
    if (result.success) {
      renderHoSoThiDauPreview(getResponseData(result));
    }
  });

  if (deleteButton) {
    deleteButton.addEventListener("click", async function () {
      if (!confirm("Bạn có chắc muốn xóa hồ sơ thi đấu?")) return;
      const result = await postApi(urls.delete, {
        ma_tro_choi: maTroChoiSelect.value,
      });
      showMessage("hoSoThiDauMessage", result);
      if (result.success) {
        form.reset();
        fillSelect(maViTriSelect, [], "ma_vi_tri", "ten_vi_tri", "Chọn vị trí");
        renderHoSoThiDauPreview(null);
      }
    });
  }
}

function escapeHtml(value) {
  const div = document.createElement("div");
  div.textContent = value == null ? "" : String(value);
  return div.innerHTML;
}

function getHoSoItems(response) {
  const data = getResponseData(response);
  if (Array.isArray(data)) return data;
  return data ? [data] : [];
}

function renderHoSoThiDauPreview(hoSoItems) {
  const preview = document.getElementById("hoSoThiDauPreview");
  if (!preview) return;

  const items = Array.isArray(hoSoItems) ? hoSoItems : (hoSoItems ? [hoSoItems] : []);
  if (items.length === 0) {
    preview.innerHTML = '<p class="text-muted">Ban chua co ho so thi dau.</p>';
    return;
  }

  preview.innerHTML = items.map(function (hoSo) {
    const loaiViTri = hoSo.loai_vi_tri === "HuanLuyen" ? "Huan luyen / quan ly" : "Tuyen thu";
    return (
      '<article class="match-profile-item">' +
        '<div class="match-profile-heading">' +
          '<h4>' + escapeHtml(hoSo.ten_game || "") + '</h4>' +
          '<span>' + escapeHtml(loaiViTri) + '</span>' +
        '</div>' +
        '<dl>' +
          '<dt>ID trong game</dt><dd>' + escapeHtml(hoSo.in_game_id || "") + '</dd>' +
          '<dt>Ten trong game</dt><dd>' + escapeHtml(hoSo.in_game_name || "") + '</dd>' +
          '<dt>Vi tri</dt><dd>' + escapeHtml(hoSo.ten_vi_tri || "") + '</dd>' +
          '<dt>Thanh tich</dt><dd>' + escapeHtml(hoSo.thanh_tich || "Chua nhap") + '</dd>' +
        '</dl>' +
        '<button class="btn btn-outline-danger btn-sm delete-ho-so-button" type="button" data-game-id="' +
          escapeHtml(hoSo.ma_tro_choi || "") +
        '">Xoa ho so</button>' +
      '</article>'
    );
  }).join("");
}

async function setupHoSoThiDau() {
  const form = document.getElementById("hoSoThiDauForm");
  if (!form) return;

  const maTroChoiSelect = document.getElementById("maTroChoiSelect");
  const loaiViTriSelect = document.getElementById("loaiViTriSelect");
  const maViTriSelect = document.getElementById("maViTriSelect");
  const preview = document.getElementById("hoSoThiDauPreview");
  const urls = {
    troChoi: form.dataset.troChoiUrl,
    viTri: form.dataset.viTriUrl,
    current: form.dataset.currentUrl,
    all: form.dataset.allUrl,
    save: form.dataset.saveUrl,
    delete: form.dataset.deleteUrl,
  };

  async function loadViTri(selectedValue) {
    const maTroChoi = maTroChoiSelect.value;
    const loaiViTri = loaiViTriSelect.value;
    if (!maTroChoi || !loaiViTri) {
      fillSelect(maViTriSelect, [], "ma_vi_tri", "ten_vi_tri", "Chon vi tri");
      return;
    }

    const result = await getApi(buildUrl(urls.viTri, {
      maTroChoi: maTroChoi,
      loaiViTri: loaiViTri,
    }));
    const items = getResponseData(result);

    if (isResponseSuccess(result)) {
      fillSelect(maViTriSelect, items, "ma_vi_tri", "ten_vi_tri", "Chon vi tri");
      if (selectedValue) {
        maViTriSelect.value = selectedValue;
      }
    } else {
      fillSelect(maViTriSelect, [], "ma_vi_tri", "ten_vi_tri", "Chon vi tri");
      showMessage("hoSoThiDauMessage", result);
    }
  }

  async function loadDanhSachHoSo() {
    const result = await getApi(urls.all || urls.current);
    if (isResponseSuccess(result)) {
      renderHoSoThiDauPreview(getHoSoItems(result));
    } else {
      renderHoSoThiDauPreview([]);
      showMessage("hoSoThiDauMessage", result);
    }
  }

  const troChoiResult = await getApi(urls.troChoi);
  const troChoiItems = getResponseData(troChoiResult);
  if (isResponseSuccess(troChoiResult) && Array.isArray(troChoiItems) && troChoiItems.length > 0) {
    fillSelect(maTroChoiSelect, troChoiItems, "ma_tro_choi", "ten_game", "Chon tro choi");
  } else if (!hasSelectOptions(maTroChoiSelect)) {
    showMessage("hoSoThiDauMessage", troChoiResult);
  }

  const currentResult = await getApi(urls.current);
  const currentData = getResponseData(currentResult);
  if (isResponseSuccess(currentResult) && currentData) {
    form.ma_tro_choi.value = currentData.ma_tro_choi || "";
    form.in_game_id.value = currentData.in_game_id || "";
    form.in_game_name.value = currentData.in_game_name || "";
    form.loai_vi_tri.value = currentData.loai_vi_tri || "";
    form.thanh_tich.value = currentData.thanh_tich || "";
    await loadViTri(currentData.ma_vi_tri_so_truong || "");
  } else {
    fillSelect(maViTriSelect, [], "ma_vi_tri", "ten_vi_tri", "Chon vi tri");
  }
  await loadDanhSachHoSo();

  maTroChoiSelect.addEventListener("change", function () {
    loadViTri();
  });

  loaiViTriSelect.addEventListener("change", function () {
    loadViTri();
  });

  form.addEventListener("submit", async function (event) {
    event.preventDefault();
    const result = await postApi(urls.save, formToObject(form));
    showMessage("hoSoThiDauMessage", result);
    if (isResponseSuccess(result)) {
      await loadDanhSachHoSo();
    }
  });

  if (preview) {
    preview.addEventListener("click", async function (event) {
      const button = event.target.closest(".delete-ho-so-button");
      if (!button) return;

      if (!confirm("Ban co chac muon xoa ho so thi dau cua game nay?")) return;
      const result = await postApi(urls.delete, {
        ma_tro_choi: button.dataset.gameId,
      });
      showMessage("hoSoThiDauMessage", result);
      if (isResponseSuccess(result)) {
        if (String(maTroChoiSelect.value) === String(button.dataset.gameId)) {
          form.reset();
          fillSelect(maViTriSelect, [], "ma_vi_tri", "ten_vi_tri", "Chon vi tri");
        }
        await loadDanhSachHoSo();
      }
    });
  }
}

document.addEventListener("DOMContentLoaded", function () {
  setupHomeAuth();
  setupProfile();
  setupLogout();
  setupHoSoThiDau();
});
