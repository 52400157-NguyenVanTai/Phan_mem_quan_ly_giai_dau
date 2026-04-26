// ============================================================
// ESPORT HUB — app.js (Part 1: Core + Auth)
// ============================================================
(function () {
'use strict';

// ---- GAME CONFIG ----
const GAMES = [
  { id: 'game-aov',  name: 'Arena of Valor',    emoji: '⚔️',  color: '#ffa502', maGame: null },
  { id: 'game-lol',  name: 'League of Legends', emoji: '🏆',  color: '#6c63ff', maGame: null },
  { id: 'game-ff',   name: 'Free Fire',          emoji: '🔥',  color: '#ff4757', maGame: null },
  { id: 'game-pubg', name: 'PUBG',               emoji: '🪂',  color: '#ffd700', maGame: null },
  { id: 'game-val',  name: 'Valorant',            emoji: '🎯',  color: '#ff4655', maGame: null },
  { id: 'game-cs2',  name: 'CS2',                emoji: '💣',  color: '#f0a500', maGame: null },
];

// ---- STATE ----
let currentUser = null;
let currentPage = 'home';
let gameList = [];

// ---- API HELPERS ----
async function api(url, method, body) {
  const opts = { method: method || 'GET', headers: {} };
  if (body && method === 'POST') {
    opts.headers['Content-Type'] = 'application/json';
    opts.body = JSON.stringify(body);
  }
  try {
    const res = await fetch(url, opts);
    return await res.json();
  } catch (e) {
    return { Success: false, Message: 'Lỗi kết nối: ' + e.message };
  }
}
async function postForm(url, data) {
  const body = new URLSearchParams(data).toString();
  try {
    const res = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
      body
    });
    return await res.json();
  } catch (e) {
    return { Success: false, Message: 'Lỗi kết nối: ' + e.message };
  }
}

// ---- AUTH ALERTS ----
function showAuthError(msg) {
  const el = document.getElementById('auth-error');
  el.textContent = msg; el.classList.add('show');
  document.getElementById('auth-success').classList.remove('show');
}
function showAuthSuccess(msg) {
  const el = document.getElementById('auth-success');
  el.textContent = msg; el.classList.add('show');
  document.getElementById('auth-error').classList.remove('show');
}
function clearAuthMsg() {
  document.getElementById('auth-error').classList.remove('show');
  document.getElementById('auth-success').classList.remove('show');
}

// ---- TAB SWITCH ----
window.switchTab = function(tab) {
  clearAuthMsg();
  document.getElementById('tab-login').classList.toggle('active', tab === 'login');
  document.getElementById('tab-register').classList.toggle('active', tab === 'register');
  document.getElementById('form-login').style.display = tab === 'login' ? '' : 'none';
  document.getElementById('form-register').style.display = tab === 'register' ? '' : 'none';
};

// ---- REGISTER ----
document.getElementById('form-register').addEventListener('submit', async function(e) {
  e.preventDefault();
  clearAuthMsg();
  const ten    = document.getElementById('reg-ten').value.trim();
  const email  = document.getElementById('reg-email').value.trim();
  const mk     = document.getElementById('reg-mk').value;
  const confirm= document.getElementById('reg-confirm').value;

  if (!ten || ten.length < 3)  { showAuthError('Tên đăng nhập phải có ít nhất 3 ký tự.'); return; }
  if (!email || !email.includes('@')) { showAuthError('Email không hợp lệ.'); return; }
  if (mk.length < 8)           { showAuthError('Mật khẩu phải có ít nhất 8 ký tự.'); return; }
  if (mk !== confirm)          { showAuthError('Mật khẩu không giống nhau.'); return; }

  const btn = this.querySelector('button[type=submit]');
  btn.disabled = true; btn.textContent = 'Đang tạo...';

  // Gửi JSON — AuthApi.DangKy nhận DangKyNguoiDungDTO
  const result = await api('/AuthApi/DangKy', 'POST', {
    TenDangNhap: ten, Email: email, MatKhau: mk
  });

  btn.disabled = false; btn.textContent = 'Tạo tài khoản';

  if (result.Success) {
    showAuthSuccess('✅ Tạo tài khoản thành công! Đang chuyển sang đăng nhập...');
    this.reset();
    setTimeout(() => switchTab('login'), 1600);
  } else {
    const msg = (result.Message || '').toLowerCase();
    if (msg.includes('tên đăng nhập'))   showAuthError('Tên đăng nhập đã được đăng ký.');
    else if (msg.includes('email'))       showAuthError('Email đã được đăng ký.');
    else showAuthError(result.Message || 'Đăng ký thất bại. Vui lòng thử lại.');
  }
});

// ---- LOGIN ----
document.getElementById('form-login').addEventListener('submit', async function(e) {
  e.preventDefault();
  clearAuthMsg();
  const dinhDanh = document.getElementById('login-dinh-danh').value.trim();
  const matKhau  = document.getElementById('login-mat-khau').value;

  if (!dinhDanh) { showAuthError('Vui lòng nhập tên đăng nhập hoặc email.'); return; }
  if (!matKhau)  { showAuthError('Vui lòng nhập mật khẩu.'); return; }

  const btn = this.querySelector('button[type=submit]');
  btn.disabled = true; btn.textContent = 'Đang kiểm tra...';

  // AuthApi.DangNhap nhận DangNhapDTO với DinhDanh + MatKhau
  // Backend dùng BCrypt.Verify để so sánh hash
  const result = await api('/AuthApi/DangNhap', 'POST', {
    DinhDanh: dinhDanh,
    MatKhau: matKhau
  });

  btn.disabled = false; btn.textContent = 'Đăng nhập';

  if (result.Success && result.Data) {
    currentUser = result.Data;
    enterDashboard(currentUser);
  } else {
    // Luôn hiển thị lỗi chung để không tiết lộ thông tin
    showAuthError('Tên đăng nhập, email hoặc mật khẩu sai.');
  }
});

// ---- ENTER DASHBOARD ----
function enterDashboard(user) {
  currentUser = user;
  document.getElementById('landing-page').style.display = 'none';
  document.getElementById('app-shell').classList.add('active');
  updateAvatar(user);
  loadGames();
  loadNotifications();
  navigateTo('home');
}

function updateAvatar(user) {
  if (!user) return;
  const initials = (user.TenDangNhap || user.ten_dang_nhap || '?').charAt(0).toUpperCase();
  const topbarInitials = document.getElementById('avatar-initials');
  topbarInitials.textContent = initials;
  // Profile page
  const un = user.TenDangNhap || user.ten_dang_nhap || '';
  const em = user.Email || user.email || '';
  document.getElementById('profile-username-display').textContent = un;
  document.getElementById('profile-email-display').textContent = em;

  // Avatar big (click-to-upload)
  const bigInitials = document.getElementById('avatar-initials-big');
  const bigImg      = document.getElementById('profile-avatar-img');
  const avatarUrl   = user.AvatarUrl || user.avatar_url || '';
  if (bigInitials) bigInitials.textContent = initials;
  if (bigImg) {
    if (avatarUrl) {
      bigImg.src             = avatarUrl;
      bigImg.style.display   = 'block';
      if (bigInitials) bigInitials.style.display = 'none';
    } else {
      bigImg.style.display   = 'none';
      if (bigInitials) bigInitials.style.display = '';
    }
  }
  // Topbar avatar — if user has real avatar show it
  const avatarBtn = document.getElementById('avatar-btn');
  if (avatarBtn) {
    if (avatarUrl) {
      avatarBtn.style.backgroundImage = 'url(' + avatarUrl + ')';
      avatarBtn.style.backgroundSize  = 'cover';
      avatarBtn.style.backgroundPosition = 'center';
      avatarBtn.style.backgroundRepeat = 'no-repeat';
      topbarInitials.style.display = 'none';
    } else {
      avatarBtn.style.backgroundImage = '';
      avatarBtn.style.backgroundSize = '';
      avatarBtn.style.backgroundPosition = '';
      avatarBtn.style.backgroundRepeat = '';
      topbarInitials.style.display = '';
    }
  }

  document.getElementById('profile-role-display').textContent = user.VaiTroHeThong || user.vai_tro_he_thong || '';
  document.getElementById('pf-username').value = un;
  document.getElementById('pf-email').value = em;
  document.getElementById('pf-bio').value = user.Bio || user.bio || '';

  // Show admin button if admin
  const btnAdmin = document.getElementById('btn-admin-module');
  if (btnAdmin) {
    const isAdmin = String(user.VaiTroHeThong || user.vai_tro_he_thong || '').toLowerCase() === 'admin';
    btnAdmin.classList.toggle('d-none', !isAdmin);
  }

  // Load team name for profile
  loadMyTeamForProfile();
}

// ---- LOGOUT ----
window.doLogout = async function() {
  await api('/AuthApi/DangXuat', 'POST', {});
  currentUser = null;
  document.getElementById('app-shell').classList.remove('active');
  document.getElementById('landing-page').style.display = '';
  const avatarBtn = document.getElementById('avatar-btn');
  const avatarInitials = document.getElementById('avatar-initials');
  const bigImg = document.getElementById('profile-avatar-img');
  const bigInitials = document.getElementById('avatar-initials-big');
  if (avatarBtn) {
    avatarBtn.style.backgroundImage = '';
    avatarBtn.style.backgroundSize = '';
    avatarBtn.style.backgroundPosition = '';
    avatarBtn.style.backgroundRepeat = '';
  }
  if (avatarInitials) {
    avatarInitials.textContent = '?';
    avatarInitials.style.display = '';
  }
  if (bigImg) {
    bigImg.src = '';
    bigImg.style.display = 'none';
  }
  if (bigInitials) {
    bigInitials.textContent = '?';
    bigInitials.style.display = '';
  }
  switchTab('login');
  closeHamburger();
};

// ---- RESTORE SESSION ----
async function restoreSession() {
  const result = await api('/AuthApi/Me');
  if (result.Success && result.Data) {
    enterDashboard(result.Data);
  }
}

// ---- SIDEBAR / HAMBURGER ----
window.toggleSidebar = function() {
  document.getElementById('sidebar').classList.toggle('open');
};
window.toggleHamburger = function() {
  document.getElementById('hamburger-menu').classList.toggle('open');
};
function closeHamburger() {
  document.getElementById('hamburger-menu').classList.remove('open');
}
document.addEventListener('click', function(e) {
  const wrap = document.querySelector('.hamburger-menu-wrap');
  if (wrap && !wrap.contains(e.target)) closeHamburger();
});

// ---- NAVIGATION ----
window.navigateTo = function(page) {
  currentPage = page;
  closeHamburger();
  // Hide all pages
  document.querySelectorAll('.page-section').forEach(p => p.style.display = 'none');
  // Update sidebar active
  document.querySelectorAll('.sidebar-item').forEach(b => b.classList.remove('active'));
  const activeBtn = document.querySelector('[data-page="' + page + '"]');
  if (activeBtn) activeBtn.classList.add('active');

  // Map page id to game page
  const gameMatch = GAMES.find(g => g.id === page);
  if (gameMatch) {
    showGamePage(gameMatch);
    return;
  }

  const pageMap = {
    'home': 'page-home',
    'follow': 'page-follow',
    'notifications': 'page-notifications',
    'my-tournaments': 'page-my-tournaments',
    'my-teams': 'page-my-teams',
    'organize': 'page-organize',
    'create-team': 'page-create-team',
    'profile': 'page-profile',
    'player-profile': 'page-player-profile',
    'manage-tournament': 'page-manage-tournament'
  };
  const target = pageMap[page];
  if (target) {
    const el = document.getElementById(target);
    if (el) el.style.display = '';
    if (page === 'home') loadHomePage();
    if (page === 'notifications') loadNotifications();
    if (page === 'player-profile') loadPlayerProfileTabs();
    if (page === 'my-tournaments') loadMyTournaments();
<<<<<<< HEAD
    if (page === 'manage-tournament') loadManageTournament();
=======
    if (page === 'my-teams') loadMyTeams();
>>>>>>> 7df51e49ef4801811f08d15a2fcf400287dcc536
  }
};

// ---- LOAD GAMES LIST ----
async function loadGames() {
  const result = await api('/AdminApi/DanhSachGame');
  if (!result.Success || !Array.isArray(result.Data)) return;
  gameList = result.Data;

  // Map tên game -> maGame
  GAMES.forEach(g => {
    const found = gameList.find(d => {
      const name = (d.ten_game || '').toLowerCase();
      const id = g.id;
      if (id === 'game-aov') return name.includes('valor') || name.includes('aov') || name.includes('liên quân');
      if (id === 'game-lol') return name.includes('liên minh') || name.includes('league');
      if (id === 'game-ff') return name.includes('free fire');
      if (id === 'game-pubg') return name.includes('pubg');
      if (id === 'game-val') return name.includes('valorant');
      if (id === 'game-cs2') return name.includes('cs') || name.includes('counter');
      return false;
    });
    if (found) g.maGame = found.ma_tro_choi;
  });

  // Populate organize/create-team selects
  populateGameSelects();
  loadPlayerProfileTabs();
}

function populateGameSelects() {
  ['org-game', 'team-game'].forEach(id => {
    const sel = document.getElementById(id);
    if (!sel) return;
    sel.innerHTML = '<option value="">-- Chọn game --</option>';
    gameList.forEach(g => {
      const opt = document.createElement('option');
      opt.value = g.ma_tro_choi;
      opt.textContent = g.ten_game;
      sel.appendChild(opt);
    });
  });
}

// ---- HOME PAGE ----
function loadHomePage() {
  loadFeaturedTournaments();
}

function buildTournamentCard(t, opts) {
  opts = opts || {};
  const gameInfo = GAMES.find(g => g.maGame == t.ma_tro_choi) || { emoji: '🎮', color: '#6c63ff', name: '' };
  const status = t.trang_thai || '';
  let statusHtml = '';
  if (status === 'dang_dien_ra') statusHtml = "<span class='tc-status live'>🔴 Live</span>";
  else if (status === 'mo_dang_ky' || status === 'sap_dien_ra') statusHtml = "<span class='tc-status upcoming'>🔵 Sắp diễn ra</span>";
  else statusHtml = "<span class='tc-status finished'>✅ Kết thúc</span>";

  const prize    = t.tong_giai_thuong ? Number(t.tong_giai_thuong).toLocaleString('vi-VN') + '₫' : 'N/A';
  const maGiai   = t.ma_giai_dau;
  const likeAct  = opts.da_like       ? 'tc-btn-like active'   : 'tc-btn-like';
  const followAct= opts.dang_theo_doi ? 'tc-btn-follow active' : 'tc-btn-follow';
  const tLike    = opts.tong_like    || 0;
  const tFollow  = opts.tong_theo_doi || 0;

  return '<div class="tournament-card" id="tc-' + maGiai + '">' +
    '<div class="tc-banner" style="background:linear-gradient(135deg,#1a1f36,' + gameInfo.color + '33)" onclick="openTournament(' + maGiai + ')" style="cursor:pointer">' +
    '<span style="font-size:2.5rem">' + gameInfo.emoji + '</span></div>' +
    '<div class="tc-body">' +
    '<div class="tc-game-badge">' + (t.ten_game || gameInfo.name) + '</div>' +
    '<div class="tc-name" onclick="openTournament(' + maGiai + ')" style="cursor:pointer">' + (t.ten_giai_dau || 'Giải đấu') + '</div>' +
    '<div class="tc-meta">' + statusHtml + '<span>💰 ' + prize + '</span></div>' +
    '<div class="tc-actions">' +
      '<button class="' + likeAct + '" onclick="toggleLike(' + maGiai + ',this)" title="Thích">' +
        '<span class="tc-icon">❤️</span> <span class="tc-like-count" id="like-count-' + maGiai + '">' + tLike + '</span>' +
      '</button>' +
      '<button class="' + followAct + '" onclick="toggleFollow(' + maGiai + ',this)" title="Theo dõi">' +
        '<span class="tc-icon">🔔</span> <span id="follow-label-' + maGiai + '">' + (opts.dang_theo_doi ? 'Đang theo dõi' : 'Theo dõi') + '</span>' +
        ' <span class="tc-follow-count" id="follow-count-' + maGiai + '">' + tFollow + '</span>' +
      '</button>' +
    '</div>' +
    '</div></div>';
}

// ---- Toggle Like ----
window.toggleLike = async function(maGiaiDau, btn) {
  if (!currentUser) { showToast('Vui lòng đăng nhập để thích giải đấu.'); return; }
  btn.disabled = true;
  const res = await fetch('/TuongTacApi/Like?maGiaiDau=' + maGiaiDau, { method: 'POST' });
  const result = await res.json();
  btn.disabled = false;
  if (result.Success && result.Data) {
    const d = result.Data;
    btn.classList.toggle('active', d.DaLike);
    const cnt = document.getElementById('like-count-' + maGiaiDau);
    if (cnt) cnt.textContent = d.TongLike || 0;
    // Sync tất cả card cùng giải trên trang
    document.querySelectorAll('#tc-' + maGiaiDau + ' .tc-btn-like').forEach(b => b.classList.toggle('active', d.DaLike));
  } else {
    showToast(result.Message || 'Lỗi.');
  }
};

// ---- Toggle Follow ----
window.toggleFollow = async function(maGiaiDau, btn) {
  if (!currentUser) { showToast('Vui lòng đăng nhập để theo dõi giải đấu.'); return; }
  btn.disabled = true;
  const res = await fetch('/TuongTacApi/Follow?maGiaiDau=' + maGiaiDau, { method: 'POST' });
  const result = await res.json();
  btn.disabled = false;
  if (result.Success && result.Data) {
    const d = result.Data;
    btn.classList.toggle('active', d.DangTheoDoi);
    const lbl = document.getElementById('follow-label-' + maGiaiDau);
    if (lbl) lbl.textContent = d.DangTheoDoi ? 'Đang theo dõi' : 'Theo dõi';
    const cnt = document.getElementById('follow-count-' + maGiaiDau);
    if (cnt) cnt.textContent = d.TongTheoDoi || 0;
    // Cập nhật sidebar nếu đang theo dõi
    if (d.DangTheoDoi) showSidebarItem('side-my-follow');
  } else {
    showToast(result.Message || 'Lỗi.');
  }
};

// ---- Load like/follow state cho các card sau khi render ----
async function loadInteractionStates(maGiaiDauList) {
  if (!currentUser || !Array.isArray(maGiaiDauList)) return;
  maGiaiDauList.forEach(async function(id) {
    const res = await fetch('/TuongTacApi/TrangThai?maGiaiDau=' + id);
    const result = await res.json();
    if (!result.Success || !result.Data) return;
    const d = result.Data;
    // Like
    document.querySelectorAll('#tc-' + id + ' .tc-btn-like').forEach(b => b.classList.toggle('active', !!(d.caNhan && d.caNhan.da_like)));
    const lc = document.getElementById('like-count-' + id);
    if (lc) lc.textContent = d.TongLike || 0;
    // Follow
    const following = !!(d.caNhan && d.caNhan.dang_theo_doi);
    document.querySelectorAll('#tc-' + id + ' .tc-btn-follow').forEach(b => b.classList.toggle('active', following));
    const lbl = document.getElementById('follow-label-' + id);
    if (lbl) lbl.textContent = following ? 'Đang theo dõi' : 'Theo dõi';
    const fc = document.getElementById('follow-count-' + id);
    if (fc) fc.textContent = d.TongTheoDoi || 0;
  });
}

// ---- Toast notification ----
function showToast(msg) {
  let t = document.getElementById('app-toast');
  if (!t) {
    t = document.createElement('div');
    t.id = 'app-toast';
    t.style.cssText = 'position:fixed;bottom:24px;right:24px;background:var(--bg-card);border:1px solid var(--border);border-radius:10px;padding:12px 20px;color:var(--text-primary);font-size:.88rem;z-index:9999;box-shadow:0 8px 24px rgba(0,0,0,.4);transition:opacity .3s;';
    document.body.appendChild(t);
  }
  t.textContent = msg;
  t.style.opacity = '1';
  clearTimeout(t._timer);
  t._timer = setTimeout(() => { t.style.opacity = '0'; }, 3000);
}

async function loadFeaturedTournaments() {
  const grid = document.getElementById('featured-grid');
  const upGrid = document.getElementById('upcoming-grid');
  if (!grid) return;
  grid.innerHTML = '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';

  const result = await api('/MatchmakingApi/DanhSachGiaiCongKhai');
  if (!result.Success || !Array.isArray(result.Data) || result.Data.length === 0) {
    grid.innerHTML = '<div class="empty-state"><div class="empty-state-icon">🏆</div><h4>Chưa có giải đấu nào</h4><p>Hãy là người đầu tiên tổ chức giải!</p></div>';
    if (upGrid) upGrid.innerHTML = '';
    return;
  }

  const active   = result.Data.filter(t => t.trang_thai === 'dang_dien_ra');
  const upcoming = result.Data.filter(t => t.trang_thai === 'mo_dang_ky' || t.trang_thai === 'sap_dien_ra');

  const activeList = active.slice(0, 6);
  const upList     = upcoming.slice(0, 6);

  grid.innerHTML = activeList.length ? activeList.map(t => buildTournamentCard(t)).join('') :
    '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">🏆</div><h4>Không có giải đang diễn ra</h4></div>';

  if (upGrid) upGrid.innerHTML = upList.length ? upList.map(t => buildTournamentCard(t)).join('') :
    '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">📅</div><h4>Không có giải sắp diễn ra</h4></div>';

  // Load trạng thái like/follow sau khi render
  const allIds = [...activeList, ...upList].map(t => t.ma_giai_dau);
  loadInteractionStates(allIds);
}

window.openTournament = function(maGiaiDau) {
  window.open('/giai/' + maGiaiDau, '_blank');
};

// ---- GAME PAGE ----
function showGamePage(game) {
  document.getElementById('page-game').style.display = '';
  document.getElementById('game-page-title').textContent = game.emoji + ' ' + game.name;
  document.getElementById('game-page-sub').textContent = 'Các giải đấu nổi bật · ' + game.name;
  loadGameTournaments(game);
}

async function loadGameTournaments(game) {
  const grid = document.getElementById('game-grid');
  grid.innerHTML = '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';
  let url = '/MatchmakingApi/DanhSachGiaiCongKhai';
  if (game.maGame) url += '?maTroChoi=' + game.maGame;
  const result = await api(url);
  if (!result.Success || !Array.isArray(result.Data) || result.Data.length === 0) {
    grid.innerHTML = '<div class="empty-state"><div class="empty-state-icon">' + game.emoji + '</div><h4>Chưa có giải đấu ' + game.name + '</h4></div>';
    return;
  }
  const list = result.Data.slice(0, 12);
  grid.innerHTML = list.map(t => buildTournamentCard(t)).join('');
  loadInteractionStates(list.map(t => t.ma_giai_dau));
}

// ---- NOTIFICATIONS ----
async function loadNotifications() {
  // Fallback to empty if no endpoint
  const list = document.getElementById('notif-list');
  if (!list) return;
  // You can call an API here if you have one
  // For now we show empty state
}

// ---- SEARCH ----
document.getElementById('global-search').addEventListener('keydown', function(e) {
  if (e.key === 'Enter' && this.value.trim()) {
    alert('Chức năng tìm kiếm đang phát triển: ' + this.value.trim());
  }
});

// Expose restoreSession
window._restoreSession = restoreSession;

// ============================================================
// PART 2: Profile, Player Profile, Organize, Create Team
// ============================================================

// ---- SAVE PROFILE ----
window.saveProfile = async function() {
  const msg = document.getElementById('pf-msg');
  const result = await api('/AuthApi/CapNhatThongTin', 'POST', {
    Bio: document.getElementById('pf-bio').value.trim()
  });
  msg.style.color = result.Success ? '#2ed573' : '#ff4757';
  msg.textContent = result.Success ? '✅ Đã lưu!' : (result.Message || 'Lỗi lưu.');
};

// ---- CHANGE PASSWORD ----
window.changePassword = async function() {
  const msg = document.getElementById('pw-msg');
  const old = document.getElementById('pw-old').value;
  const nw = document.getElementById('pw-new').value;
  const conf = document.getElementById('pw-confirm').value;
  if (nw !== conf) { msg.style.color='#ff4757'; msg.textContent='Mật khẩu mới không khớp.'; return; }
  if (nw.length < 8) { msg.style.color='#ff4757'; msg.textContent='Mật khẩu phải ≥ 8 ký tự.'; return; }
  const result = await api('/AuthApi/DoiMatKhau', 'POST', { MatKhauCu: old, MatKhauMoi: nw });
  msg.style.color = result.Success ? '#2ed573' : '#ff4757';
  msg.textContent = result.Success ? '✅ Đổi mật khẩu thành công!' : (result.Message || 'Sai mật khẩu cũ.');
  if (result.Success) { document.getElementById('pw-old').value=''; document.getElementById('pw-new').value=''; document.getElementById('pw-confirm').value=''; }
};

// ---- PLAYER PROFILE TABS ----
const GAME_NAMES = ['Arena of Valor','League of Legends','Free Fire','PUBG','Valorant','CS2'];
let activeGameTab = null;

function loadPlayerProfileTabs() {
  const tabs = document.getElementById('game-profile-tabs');
  if (!tabs) return;
  tabs.innerHTML = '';
  const games = gameList.length ? gameList : GAME_NAMES.map((n,i)=>({ma_tro_choi:i+1,ten_game:n}));
  games.forEach(g => {
    const btn = document.createElement('button');
    btn.className = 'game-tab-btn' + (activeGameTab === g.ma_tro_choi ? ' active' : '');
    btn.textContent = g.ten_game;
    btn.onclick = () => loadGameProfile(g);
    tabs.appendChild(btn);
  });
}

async function loadGameProfile(game) {
  activeGameTab = game.ma_tro_choi;
  document.querySelectorAll('.game-tab-btn').forEach(b => b.classList.toggle('active', b.textContent === game.ten_game));
  const title = document.getElementById('game-profile-title');
  const body = document.getElementById('game-profile-body');
  title.textContent = '🎮 ' + game.ten_game;

  // Load existing profile
  const result = await api('/ProfileApi/LayHoSo?maTroChoi=' + game.ma_tro_choi);
  const existing = result.Success && result.Data ? result.Data : null;

  // Load positions
  const posResult = await api('/ProfileApi/ViTri?maTroChoi=' + game.ma_tro_choi);
  const positions = posResult.Success && Array.isArray(posResult.Data) ? posResult.Data : [];

  // Nhóm vị trí theo loai_vi_tri
  const groups = {};
  positions.forEach(p => {
    const grp = p.LoaiViTri || 'Khác';
    if (!groups[grp]) groups[grp] = [];
    groups[grp].push(p);
  });

  const loaiLabel = { 'ChuyenMon': '⚔️ Chuyên môn thi đấu', 'BanHuanLuyen': '🎓 Ban huấn luyện' };

  let posOptions = '<option value="">-- Chọn vị trí --</option>';
  Object.keys(groups).forEach(loai => {
    const label = loaiLabel[loai] || loai;
    posOptions += '<optgroup label="' + label + '">';
    groups[loai].forEach(p => {
      const sel = existing && existing.ma_vi_tri_so_truong == p.MaViTri ? ' selected' : '';
      const ky = p.KyHieu ? ' [' + p.KyHieu + ']' : '';
      posOptions += '<option value="' + p.MaViTri + '"' + sel + '>' + p.TenViTri + ky + '</option>';
    });
    posOptions += '</optgroup>';
  });

  body.innerHTML = '<div class="form-group-dark"><label>ID trong game</label><input class="form-control-dark" id="gp-id" value="' + (existing ? existing.in_game_id || '' : '') + '" placeholder="ID trong game" /></div>' +
    '<div class="form-group-dark"><label>Tên hiển thị trong game</label><input class="form-control-dark" id="gp-name" value="' + (existing ? existing.in_game_name || '' : '') + '" placeholder="Nickname" /></div>' +
    '<div class="form-group-dark"><label>Vị trí sở trường</label><select class="select-dark" id="gp-vitri">' + posOptions + '</select></div>' +
    '<button class="btn-save" onclick="saveGameProfile(' + game.ma_tro_choi + ')">Lưu hồ sơ</button>' +
    '<span id="gp-msg" style="margin-left:12px;font-size:.85rem"></span>';
}

window.saveGameProfile = async function(maTroChoi) {
  const msg = document.getElementById('gp-msg');
  const result = await api('/ProfileApi/TaoHoSo', 'POST', {
    MaTroChoi: maTroChoi,
    InGameId: document.getElementById('gp-id').value.trim(),
    InGameName: document.getElementById('gp-name').value.trim(),
    MaViTriSoTruong: Number(document.getElementById('gp-vitri').value) || null
  });
  msg.style.color = result.Success ? '#2ed573' : '#ff4757';
  msg.textContent = result.Success ? '✅ Đã lưu hồ sơ!' : (result.Message || 'Lỗi lưu.');
};

// ---- ORGANIZE TOURNAMENT ----
window.submitOrganize = async function() {
  const msg = document.getElementById('org-msg');
  const payload = {
    TenGiaiDau: document.getElementById('org-ten-giai').value.trim(),
    MaTroChoi: Number(document.getElementById('org-game').value) || null,
    TongGiaiThuong: Number(document.getElementById('org-giai-thuong').value) || 0,
    NgayBatDau: document.getElementById('org-start').value ? new Date(document.getElementById('org-start').value).toISOString() : null,
    NgayKetThuc: document.getElementById('org-end').value ? new Date(document.getElementById('org-end').value).toISOString() : null
  };
  if (!payload.TenGiaiDau) { msg.style.color='#ff4757'; msg.textContent='Nhập tên giải đấu.'; return; }
  const result = await api('/TournamentBuilderApi/TaoBanNhap', 'POST', payload);
  msg.style.color = result.Success ? '#2ed573' : '#ff4757';
  if (result.Success) {
    msg.textContent = '✅ Đã tạo! Mã giải: #' + (result.Data && result.Data.MaGiaiDau ? result.Data.MaGiaiDau : '');
    showSidebarItem('side-my-tournaments');
    setTimeout(() => navigateTo('my-tournaments'), 1000);
  } else {
    msg.textContent = result.Message || 'Tạo giải thất bại.';
  }
};

let myTournamentsData = [];

async function loadMyTournaments() {
  const grid = document.getElementById('my-tournaments-grid');
  if (!grid) return;
  grid.innerHTML = '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';

  const result = await api('/TournamentBuilderApi/DanhSachCuaToi');
  if (!result.Success || !Array.isArray(result.Data) || result.Data.length === 0) {
    grid.innerHTML = '<div class="empty-state"><div class="empty-state-icon">🏆</div><h4>Chưa có giải đấu nào</h4><p>Bạn chưa tổ chức giải đấu nào.</p></div>';
    return;
  }

  myTournamentsData = result.Data;

  grid.innerHTML = result.Data.map(t => {
    const gameInfo = GAMES.find(g => g.maGame == t.ma_tro_choi) || { emoji: '🎮', color: '#6c63ff', name: '' };
    const status = t.trang_thai || '';
    let statusHtml = '';
    if (status === 'ban_nhap') statusHtml = "<span class='tc-status' style='color:#a4b0be'>📝 Bản nháp</span>";
    else if (status === 'cho_phe_duyet') statusHtml = "<span class='tc-status' style='color:#eccc68'>⏳ Chờ duyệt</span>";
    else if (status === 'dang_dien_ra') statusHtml = "<span class='tc-status live'>🔴 Live</span>";
    else statusHtml = "<span class='tc-status upcoming'>🔵 Đã duyệt (" + status + ")</span>";

    const prize = t.tong_giai_thuong ? Number(t.tong_giai_thuong).toLocaleString('vi-VN') + '₫' : 'N/A';
    
    return '<div class="tournament-card">' +
      '<div class="tc-banner" style="background:linear-gradient(135deg,#1a1f36,' + gameInfo.color + '33); cursor:default">' +
      '<span style="font-size:2.5rem">' + gameInfo.emoji + '</span></div>' +
      '<div class="tc-body">' +
      '<div class="tc-game-badge">' + (t.ten_game || gameInfo.name) + '</div>' +
      '<div class="tc-name">' + (t.ten_giai_dau || 'Giải đấu') + '</div>' +
      '<div class="tc-meta">' + statusHtml + '<span>💰 ' + prize + '</span></div>' +
      '<div style="margin-top:12px; display:flex; gap:8px;">' +
        '<button class="btn-primary-glow" style="padding:4px 8px; font-size:0.8rem; flex:1" onclick="manageTournament(' + t.ma_giai_dau + ')">Quản lý</button>' +
        '<button class="btn-outline-glow" style="padding:4px 8px; font-size:0.8rem; flex:1" onclick="openTournament(' + t.ma_giai_dau + ')">Xem public</button>' +
      '</div>' +
      '</div></div>';
  }).join('');
}

let currentManageId = 0;
let currentManageStatus = '';

window.manageTournament = function(maGiaiDau) {
  currentManageId = maGiaiDau;
  navigateTo('manage-tournament');
};

async function loadManageTournament() {
  if (!currentManageId) return;
  document.getElementById('manage-tour-name').textContent = 'Quản lý Giải đấu #' + currentManageId;
  document.getElementById('manage-tour-msg').textContent = 'Đang tải dữ liệu...';
  
  // 1. Fetch stage list
  const stagesRes = await api('/TournamentBuilderApi/DanhSachGiaiDoan?maGiaiDau=' + currentManageId);
  
  // To get status, we look up myTournamentsData
  const t = myTournamentsData.find(x => x.ma_giai_dau === currentManageId);
  document.getElementById('manage-tour-msg').textContent = '';
  
  const actionsEl = document.getElementById('manage-tour-actions');
  actionsEl.innerHTML = '';
  
  if (t) {
    const status = t.trang_thai || '';
    currentManageStatus = status;
    
    let statusText = status;
    if (status === 'ban_nhap') statusText = 'Bản nháp';
    if (status === 'cho_phe_duyet') statusText = 'Đang chờ Admin duyệt';
    if (status === 'sap_dien_ra' || status === 'mo_dang_ky') statusText = 'Sắp diễn ra';
    
    document.getElementById('manage-tour-status').innerHTML = 'Trạng thái hiện tại: <strong style="color:var(--primary)">' + statusText + '</strong>';
    
    // Action buttons based on status
    if (status === 'ban_nhap') {
      actionsEl.innerHTML = '<button class="btn-primary-glow" onclick="submitGuiDuyet()">Gửi yêu cầu xét duyệt</button>' +
                            '<p style="margin-top:10px; font-size: 0.9em; color:var(--text-muted)">Sau khi thêm xong các giai đoạn, hãy bấm Gửi yêu cầu để admin duyệt giải.</p>';
    } else if (status === 'mo_dang_ky' || status === 'sap_dien_ra') {
      actionsEl.innerHTML = '<button class="btn-primary-glow" onclick="submitBatDauGiai()">Bắt đầu giải ngay</button>' +
                            '<p style="margin-top:10px; font-size: 0.9em; color:var(--text-muted)">Khóa danh sách đăng ký và bắt đầu thi đấu.</p>';
    } else if (status === 'dang_dien_ra') {
      actionsEl.innerHTML = '<span style="color:#2ed573">Giải đang diễn ra live!</span>';
    }
  }
  
  // 2. Render stages
  const listEl = document.getElementById('manage-stage-list');
  if (stagesRes.Success && stagesRes.Data && stagesRes.Data.length > 0) {
    listEl.innerHTML = stagesRes.Data.map(s => {
      return '<div style="border:1px solid var(--border); border-radius: var(--radius); padding: 10px; margin-bottom: 5px; display:flex; justify-content:space-between">' +
        '<div><strong>' + s.TenGiaiDoan + '</strong> <span style="color:var(--text-muted);font-size:0.9em">(' + s.TheThuc + ')</span></div>' +
        '<button class="btn-outline-glow" style="padding: 2px 8px; border-color: #ff4757; color:#ff4757" onclick="submitXoaGiaiDoan(' + s.MaGiaiDoan + ')">Xóa</button>' +
        '</div>';
    }).join('');
  } else {
    listEl.innerHTML = '<div class="text-muted">Chưa có giai đoạn thi đấu nào.</div>';
  }
}

window.submitAddStage = async function() {
  const name = document.getElementById('manage-stage-name').value.trim();
  const format = document.getElementById('manage-stage-format').value;
  const teams = Number(document.getElementById('manage-stage-teams').value) || 2;
  const msg = document.getElementById('manage-tour-msg');
  
  if (!name) { msg.textContent = 'Vui lòng nhập tên giai đoạn.'; msg.style.color='#ff4757'; return; }
  
  const payload = {
    MaGiaiDau: currentManageId,
    TenGiaiDoan: name,
    TheThuc: format,
    SoDoiDiTiep: teams,
    DiemNguongMatchPoint: 50 // default if champion_rush
  };
  
  const res = await api('/TournamentBuilderApi/ThemGiaiDoan', 'POST', payload);
  if (res.Success) {
    document.getElementById('manage-stage-name').value = '';
    loadManageTournament();
  } else {
    msg.textContent = res.Message;
    msg.style.color='#ff4757';
  }
};

window.submitXoaGiaiDoan = async function(maGiaiDoan) {
  if (!confirm('Bạn có chắc muốn xóa giai đoạn này?')) return;
  const res = await api('/TournamentBuilderApi/XoaGiaiDoan', 'POST', { maGiaiDau: currentManageId, maGiaiDoan: maGiaiDoan });
  if (res.Success) loadManageTournament();
  else alert(res.Message);
};

window.submitGuiDuyet = async function() {
  const msg = document.getElementById('manage-tour-msg');
  msg.textContent = 'Đang gửi...';
  msg.style.color = 'var(--text-muted)';
  
  const res = await api('/TournamentBuilderApi/GuiXetDuyet', 'POST', { maGiaiDau: currentManageId });
  if (res.Success) {
    // Reload my tournaments so the status updates in the array
    await loadMyTournaments();
    loadManageTournament();
  } else {
    msg.textContent = res.Message;
    msg.style.color = '#ff4757';
  }
};

window.submitBatDauGiai = async function() {
  const msg = document.getElementById('manage-tour-msg');
  if (!confirm('Sau khi bắt đầu, bạn không thể thay đổi danh sách đội tham gia. Xác nhận bắt đầu giải?')) return;
  
  const res = await api('/TournamentBuilderApi/BatDauGiai', 'POST', { maGiaiDau: currentManageId });
  if (res.Success) {
    await loadMyTournaments();
    loadManageTournament();
  } else {
    msg.textContent = res.Message;
    msg.style.color = '#ff4757';
  }
};

// ---- CREATE TEAM ----
window.submitCreateTeam = async function() {
  const msg = document.getElementById('team-msg');
  const payload = {
    TenDoi: document.getElementById('team-ten').value.trim(),
    Slogan: document.getElementById('team-slogan').value.trim(),
    LogoUrl: '',
    maTroChoiMacDinh: Number(document.getElementById('team-game').value) || 0,
    tenNhomMacDinh: document.getElementById('team-nhom').value.trim()
  };
  if (!payload.TenDoi) { msg.style.color='#ff4757'; msg.textContent='Nhập tên đội.'; return; }
  if (!payload.maTroChoiMacDinh) { msg.style.color='#ff4757'; msg.textContent='Chọn game cho đội.'; return; }
  if (!payload.tenNhomMacDinh) { msg.style.color='#ff4757'; msg.textContent='Nhập tên nhóm.'; return; }

  const fd = new URLSearchParams();
  Object.entries(payload).forEach(([k,v]) => fd.append(k, v));
  const res = await fetch('/TeamApi/TaoDoi', { method:'POST', headers:{'Content-Type':'application/x-www-form-urlencoded'}, body: fd.toString() });
  const result = await res.json();
  msg.style.color = result.Success ? '#2ed573' : '#ff4757';
  if (result.Success) {
    msg.textContent = '✅ Đã tạo đội thành công!';
    showSidebarItem('side-my-teams');
  } else {
    msg.textContent = result.Message || 'Tạo đội thất bại.';
  }
};

// ---- SIDEBAR CONDITIONAL ITEMS ----
function showSidebarItem(id) {
  const el = document.getElementById(id);
  if (el) el.classList.remove('d-none');
}

// ---- AVATAR UPLOAD ----
window.triggerAvatarUpload = function() {
  const input = document.getElementById('avatar-file-input');
  if (input) input.click();
};

(function initAvatarUpload() {
  const input = document.getElementById('avatar-file-input');
  if (!input) return;
  input.addEventListener('change', async function() {
    if (!this.files || !this.files[0]) return;
    const file   = this.files[0];
    const formData = new FormData();
    formData.append('avatar', file);

    const uploadMsg = document.getElementById('pf-msg');
    if (uploadMsg) { uploadMsg.style.color = '#aaa'; uploadMsg.textContent = 'Đang tải ảnh...'; }

    try {
      const res    = await fetch('/UploadApi/UploadAvatar', { method: 'POST', body: formData });
      const result = await res.json();
      if (result.Success && result.Data) {
        const url = result.Data.AvatarUrl;
        // Cập nhật avatar hiển thị
        const bigImg      = document.getElementById('profile-avatar-img');
        const bigInitials = document.getElementById('avatar-initials-big');
        if (bigImg) { bigImg.src = url; bigImg.style.display = 'block'; }
        if (bigInitials) bigInitials.style.display = 'none';
        // Cập nhật topbar avatar
        const avatarBtn = document.getElementById('avatar-btn');
        if (avatarBtn) {
          avatarBtn.style.backgroundImage = 'url(' + url + ')';
          avatarBtn.style.backgroundSize  = 'cover';
          document.getElementById('avatar-initials').style.display = 'none';
        }
        if (currentUser) currentUser.AvatarUrl = url;
        if (uploadMsg) { uploadMsg.style.color = '#2ed573'; uploadMsg.textContent = '✅ Đã cập nhật ảnh đại diện!'; }
      } else {
        if (uploadMsg) { uploadMsg.style.color = '#ff4757'; uploadMsg.textContent = result.Message || 'Upload thất bại.'; }
      }
    } catch(e) {
      if (uploadMsg) { uploadMsg.style.color = '#ff4757'; uploadMsg.textContent = 'Lỗi kết nối.'; }
    }
    this.value = ''; // reset input
  });
})();

// ---- MY TOURNAMENTS ----
async function loadMyTournaments() {
  const grid = document.getElementById('my-tournaments-grid');
  if (!grid || !currentUser) return;
  grid.innerHTML = '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';

  const result = await api('/TournamentBuilderApi/GiaiCuaToi');
  if (!result.Success || !Array.isArray(result.Data) || result.Data.length === 0) {
    grid.innerHTML = '<div class="empty-state"><div class="empty-state-icon">🏆</div><h4>Chưa có giải đấu nào</h4><p>Hãy tổ chức giải đầu tiên của bạn!</p></div>';
    return;
  }

  showSidebarItem('side-my-tournaments');
  grid.innerHTML = result.Data.map(function(t) {
    const statusMap = {
      'ban_nhap':      { label: 'Bản nháp',      cls: 'tc-status' },
      'cho_phe_duyet': { label: 'Chờ duyệt',    cls: 'tc-status' },
      'mo_dang_ky':    { label: 'Mở đăng ký',    cls: 'tc-status upcoming' },
      'sap_dien_ra':   { label: 'Sắp diễn ra',   cls: 'tc-status upcoming' },
      'dang_dien_ra':  { label: '🔴 Live',          cls: 'tc-status live' },
      'ket_thuc':      { label: '✅ Kết thúc',      cls: 'tc-status finished' },
      'khoa':          { label: '🔒 Khóa',           cls: 'tc-status' }
    };
    const s = statusMap[t.trang_thai] || { label: t.trang_thai, cls: 'tc-status' };
    const prize = t.tong_giai_thuong ? Number(t.tong_giai_thuong).toLocaleString('vi-VN') + '₫' : 'N/A';
    return '<div class="tournament-card" id="tc-' + t.ma_giai_dau + '">' +
      '<div class="tc-banner" onclick="openTournament(' + t.ma_giai_dau + ')" style="cursor:pointer;background:linear-gradient(135deg,#1a1f36,#6c63ff33)">' +
        '<span style="font-size:2.5rem">🏆</span></div>' +
      '<div class="tc-body">' +
        '<div class="tc-game-badge">' + (t.ten_game || 'Giải hỗn hợp') + '</div>' +
        '<div class="tc-name" onclick="openTournament(' + t.ma_giai_dau + ')" style="cursor:pointer">' + t.ten_giai_dau + '</div>' +
        '<div class="tc-meta"><span class="' + s.cls + '">' + s.label + '</span><span>💰 ' + prize + '</span></div>' +
      '</div></div>';
  }).join('');
}

// ---- MY TEAMS ----
async function loadMyTeams() {
  const list = document.getElementById('my-teams-list');
  if (!list || !currentUser) return;
  list.innerHTML = '<div style="color:var(--text-muted);padding:20px">Đang tải...</div>';

  const result = await api('/TeamApi/DoiCuaToi');
  if (!result.Success || !result.Data) {
    list.innerHTML = '<div class="empty-state"><div class="empty-state-icon">👥</div><h4>Chưa thuộc đội nào</h4><p>Hãy tạo hoặc tham gia một đội!</p></div>';
    return;
  }

  showSidebarItem('side-my-teams');
  const d = result.Data;
  const roleMap = { 'leader': '👑 Leader', 'captain': '⚔️ Captain', 'member': '👤 Thành viên' };
  list.innerHTML =
    '<div class="profile-card" style="display:flex;align-items:center;gap:20px">' +
      '<div style="font-size:3rem">👥</div>' +
      '<div>' +
        '<div style="font-size:1.2rem;font-weight:700;color:var(--text-primary)">' + d.ten_doi + '</div>' +
        '<div style="color:var(--text-muted);font-size:.85rem;margin-top:4px">' + (d.slogan || '') + '</div>' +
        '<div style="margin-top:8px;display:flex;gap:12px;flex-wrap:wrap">' +
          '<span style="background:var(--bg-sidebar);border-radius:6px;padding:4px 10px;font-size:.8rem">🎮 ' + d.ten_game + '</span>' +
          '<span style="background:var(--bg-sidebar);border-radius:6px;padding:4px 10px;font-size:.8rem">' + (roleMap[d.vai_tro] || d.vai_tro) + '</span>' +
          '<span style="background:var(--bg-sidebar);border-radius:6px;padding:4px 10px;font-size:.8rem">📌 ' + d.ten_nhom + '</span>' +
        '</div>' +
      '</div>' +
    '</div>';
}

// ---- LOAD TEAM FOR PROFILE PAGE ----
async function loadMyTeamForProfile() {
  const pfTeam = document.getElementById('pf-team');
  if (!pfTeam) return;
  const result = await api('/TeamApi/DoiCuaToi');
  if (result.Success && result.Data) {
    pfTeam.value = result.Data.ten_doi;
  } else {
    pfTeam.value = '';
    pfTeam.placeholder = 'Chưa tham gia đội nào';
  }
}

function getProfileValue(source, upperKey, lowerKey) {
  if (!source) return '';
  if (source[upperKey] !== undefined && source[upperKey] !== null) return source[upperKey];
  if (source[lowerKey] !== undefined && source[lowerKey] !== null) return source[lowerKey];
  return '';
}

updateAvatar = function(user) {
  if (!user) return;

  const initials = (getProfileValue(user, 'TenDangNhap', 'ten_dang_nhap') || '?').charAt(0).toUpperCase();
  const topbarInitials = document.getElementById('avatar-initials');
  const profileUserName = document.getElementById('profile-username-display');
  const profileEmail = document.getElementById('profile-email-display');
  const bigInitials = document.getElementById('avatar-initials-big');
  const bigImg = document.getElementById('profile-avatar-img');
  const avatarBtn = document.getElementById('avatar-btn');
  const avatarUrl = getProfileValue(user, 'AvatarUrl', 'avatar_url');

  if (topbarInitials) {
    topbarInitials.textContent = initials;
  }
  if (profileUserName) {
    profileUserName.textContent = getProfileValue(user, 'TenDangNhap', 'ten_dang_nhap') || '';
  }
  if (profileEmail) {
    profileEmail.textContent = getProfileValue(user, 'Email', 'email') || '';
  }
  if (bigInitials) {
    bigInitials.textContent = initials;
  }

  if (bigImg) {
    if (avatarUrl) {
      bigImg.src = avatarUrl;
      bigImg.style.display = 'block';
      if (bigInitials) bigInitials.style.display = 'none';
    } else {
      bigImg.src = '';
      bigImg.style.display = 'none';
      if (bigInitials) bigInitials.style.display = '';
    }
  }

  if (avatarBtn) {
    if (avatarUrl) {
      avatarBtn.style.backgroundImage = 'url(' + avatarUrl + ')';
      avatarBtn.style.backgroundSize = 'cover';
      avatarBtn.style.backgroundPosition = 'center';
      avatarBtn.style.backgroundRepeat = 'no-repeat';
      if (topbarInitials) topbarInitials.style.display = 'none';
    } else {
      avatarBtn.style.backgroundImage = '';
      avatarBtn.style.backgroundSize = '';
      avatarBtn.style.backgroundPosition = '';
      avatarBtn.style.backgroundRepeat = '';
      if (topbarInitials) topbarInitials.style.display = '';
    }
  }

  const roleEl = document.getElementById('profile-role-display');
  const usernameInput = document.getElementById('pf-username');
  const emailInput = document.getElementById('pf-email');
  const bioInput = document.getElementById('pf-bio');

  if (roleEl) roleEl.textContent = getProfileValue(user, 'VaiTroHeThong', 'vai_tro_he_thong') || '';
  if (usernameInput) usernameInput.value = getProfileValue(user, 'TenDangNhap', 'ten_dang_nhap') || '';
  if (emailInput) emailInput.value = getProfileValue(user, 'Email', 'email') || '';
  if (bioInput) bioInput.value = getProfileValue(user, 'Bio', 'bio') || '';

  const btnAdmin = document.getElementById('btn-admin-module');
  if (btnAdmin) {
    const isAdmin = String(getProfileValue(user, 'VaiTroHeThong', 'vai_tro_he_thong') || '').toLowerCase() === 'admin';
    btnAdmin.classList.toggle('d-none', !isAdmin);
  }

  loadMyTeamForProfile();
};

loadGameProfile = async function(game) {
  activeGameTab = game.ma_tro_choi;
  document.querySelectorAll('.game-tab-btn').forEach(b => b.classList.toggle('active', b.textContent === game.ten_game));

  const title = document.getElementById('game-profile-title');
  const body = document.getElementById('game-profile-body');
  if (title) {
    title.textContent = game.ten_game;
  }
  if (!body) return;

  const [profileResult, positionResult] = await Promise.all([
    api('/ProfileApi/LayHoSo?maTroChoi=' + game.ma_tro_choi),
    api('/ProfileApi/ViTri?maTroChoi=' + game.ma_tro_choi)
  ]);

  const existing = profileResult.Success && profileResult.Data ? profileResult.Data : null;
  const positions = positionResult.Success && Array.isArray(positionResult.Data) ? positionResult.Data : [];
  const existingPositionId = getProfileValue(existing, 'MaViTriSoTruong', 'ma_vi_tri_so_truong');
  const existingGameId = getProfileValue(existing, 'InGameId', 'in_game_id');
  const existingGameName = getProfileValue(existing, 'InGameName', 'in_game_name');

  const groups = {};
  positions.forEach(p => {
    const grp = p.LoaiViTri || p.loai_vi_tri || 'Khac';
    if (!groups[grp]) groups[grp] = [];
    groups[grp].push(p);
  });

  const labels = {
    ChuyenMon: 'Chuyen mon thi dau',
    BanHuanLuyen: 'Ban huan luyen'
  };

  let posOptions = '<option value="">-- Chon vi tri --</option>';
  Object.keys(groups).forEach(groupName => {
    posOptions += '<optgroup label="' + (labels[groupName] || groupName) + '">';
    groups[groupName].forEach(p => {
      const maViTri = p.MaViTri || p.ma_vi_tri;
      const tenViTri = p.TenViTri || p.ten_vi_tri || '';
      const kyHieu = p.KyHieu || p.ky_hieu || '';
      const selected = String(existingPositionId) === String(maViTri) ? ' selected' : '';
      posOptions += '<option value="' + maViTri + '"' + selected + '>' + tenViTri + (kyHieu ? ' [' + kyHieu + ']' : '') + '</option>';
    });
    posOptions += '</optgroup>';
  });

  if (!positions.length) {
    posOptions = '<option value="">-- Chua co vi tri --</option>';
  }

  body.innerHTML =
    '<div class="form-group-dark"><label>ID trong game</label><input class="form-control-dark" id="gp-id" value="' + existingGameId + '" placeholder="ID trong game" /></div>' +
    '<div class="form-group-dark"><label>Ten hien thi trong game</label><input class="form-control-dark" id="gp-name" value="' + existingGameName + '" placeholder="Nickname" /></div>' +
    '<div class="form-group-dark"><label>Vi tri so truong</label><select class="select-dark" id="gp-vitri"' + (positions.length ? '' : ' disabled') + '>' + posOptions + '</select></div>' +
    '<button class="btn-save" onclick="saveGameProfile(' + game.ma_tro_choi + ')">Luu ho so</button>' +
    '<span id="gp-msg" style="margin-left:12px;font-size:.85rem"></span>';
};

window.saveGameProfile = async function(maTroChoi) {
  const msg = document.getElementById('gp-msg');
  const maViTri = Number(document.getElementById('gp-vitri').value) || 0;

  if (!maViTri) {
    if (msg) {
      msg.style.color = '#ff4757';
      msg.textContent = 'Vui long chon vi tri so truong.';
    }
    return;
  }

  const result = await api('/ProfileApi/TaoHoSo', 'POST', {
    MaTroChoi: maTroChoi,
    InGameId: document.getElementById('gp-id').value.trim(),
    InGameName: document.getElementById('gp-name').value.trim(),
    MaViTriSoTruong: maViTri
  });

  if (msg) {
    msg.style.color = result.Success ? '#2ed573' : '#ff4757';
    msg.textContent = result.Success ? 'Da luu ho so thanh cong.' : (result.Message || 'Loi luu ho so.');
  }
};

(function setupAvatarUploadOverride() {
  const oldInput = document.getElementById('avatar-file-input');
  if (!oldInput) return;

  const newInput = oldInput.cloneNode(true);
  oldInput.parentNode.replaceChild(newInput, oldInput);

  window.triggerAvatarUpload = function() {
    newInput.click();
  };

  newInput.addEventListener('change', async function() {
    if (!this.files || !this.files.length) return;

    const file = this.files[0];
    const uploadMsg = document.getElementById('pf-msg');
    const formData = new FormData();
    formData.append('avatar', file);

    if (uploadMsg) {
      uploadMsg.style.color = '#aaa';
      uploadMsg.textContent = 'Dang tai anh...';
    }

    try {
      const res = await fetch('/UploadApi/UploadAvatar', { method: 'POST', body: formData });
      const result = await res.json();

      if (result.Success && result.Data && result.Data.AvatarUrl) {
        const url = result.Data.AvatarUrl + '?v=' + Date.now();
        if (currentUser) {
          currentUser.AvatarUrl = url;
          currentUser.avatar_url = url;
        }
        updateAvatar(currentUser || { AvatarUrl: url });
        if (uploadMsg) {
          uploadMsg.style.color = '#2ed573';
          uploadMsg.textContent = 'Cap nhat anh dai dien thanh cong.';
        }
      } else if (uploadMsg) {
        uploadMsg.style.color = '#ff4757';
        uploadMsg.textContent = result.Message || 'Upload that bai.';
      }
    } catch (e) {
      if (uploadMsg) {
        uploadMsg.style.color = '#ff4757';
        uploadMsg.textContent = 'Khong the tai anh len luc nay.';
      }
    }

    this.value = '';
  });
})();

function formatTournamentStatus(status) {
  const map = {
    ban_nhap: 'Ban nhap',
    cho_phe_duyet: 'Cho phe duyet',
    mo_dang_ky: 'Mo dang ky',
    sap_dien_ra: 'Sap dien ra',
    dang_dien_ra: 'Dang dien ra',
    ket_thuc: 'Ket thuc',
    khoa: 'Khoa'
  };
  return map[status] || status || '';
}

window.navigateTo = function(page) {
  currentPage = page;
  closeHamburger();
  document.querySelectorAll('.page-section').forEach(p => p.style.display = 'none');
  document.querySelectorAll('.sidebar-item').forEach(b => b.classList.remove('active'));
  const activeBtn = document.querySelector('[data-page="' + page + '"]');
  if (activeBtn) activeBtn.classList.add('active');

  const gameMatch = GAMES.find(g => g.id === page);
  if (gameMatch) {
    showGamePage(gameMatch);
    return;
  }

  const pageMap = {
    home: 'page-home',
    follow: 'page-follow',
    notifications: 'page-notifications',
    'my-tournaments': 'page-my-tournaments',
    'my-teams': 'page-my-teams',
    organize: 'page-organize',
    'create-team': 'page-create-team',
    profile: 'page-profile',
    'player-profile': 'page-player-profile',
    'admin-requests': 'page-admin-requests',
    'manage-tournament': 'page-manage-tournament'
  };
  const target = pageMap[page];
  if (!target) return;

  const el = document.getElementById(target);
  if (el) el.style.display = '';
  if (page === 'home') loadHomePage();
  if (page === 'notifications') loadNotifications();
  if (page === 'player-profile') loadPlayerProfileTabs();
  if (page === 'my-tournaments') loadMyTournaments();
  if (page === 'my-teams') loadMyTeams();
  if (page === 'admin-requests') loadAdminRequests();
};

loadPlayerProfileTabs = function() {
  const tabs = document.getElementById('game-profile-tabs');
  if (!tabs) return;
  tabs.innerHTML = '';
  const games = gameList.length ? gameList : GAME_NAMES.map((n, i) => ({ ma_tro_choi: i + 1, ten_game: n }));
  games.forEach(g => {
    const btn = document.createElement('button');
    btn.className = 'game-tab-btn' + (activeGameTab === g.ma_tro_choi ? ' active' : '');
    btn.textContent = g.ten_game;
    btn.onclick = () => loadGameProfile(g);
    tabs.appendChild(btn);
  });

  if (games.length && !activeGameTab) {
    loadGameProfile(games[0]);
  }
};

loadMyTournaments = async function() {
  const grid = document.getElementById('my-tournaments-grid');
  if (!grid || !currentUser) return;
  grid.innerHTML = '<div style="color:var(--text-muted);padding:20px">Dang tai...</div>';

  const result = await api('/TournamentBuilderApi/GiaiCuaToi');
  if (!result.Success || !Array.isArray(result.Data) || result.Data.length === 0) {
    grid.innerHTML = '<div class="empty-state"><div class="empty-state-icon">T</div><h4>Chua co giai dau nao</h4><p>Ban chua tao hoac tham gia giai dau nao.</p></div>';
    return;
  }

  grid.innerHTML = result.Data.map(function(t) {
    const gameName = t.ten_game || 'Giai hon hop';
    const prize = t.tong_giai_thuong ? Number(t.tong_giai_thuong).toLocaleString('vi-VN') + ' VND' : 'N/A';
    const ownerActions = t.is_owner ? (
      '<div class="tc-actions" style="margin-top:12px">' +
        '<button class="btn-outline-glow" onclick="openManageTournament(' + t.ma_giai_dau + ', \'' + String((t.ten_giai_dau || '')).replace(/'/g, "\\'") + '\')">Duyet doi</button>' +
        (t.trang_thai === 'ban_nhap'
          ? '<button class="btn-primary-glow" onclick="submitTournamentApproval(' + t.ma_giai_dau + ')">Gui admin</button>'
          : '') +
      '</div>'
    ) : '<div style="margin-top:12px;color:var(--text-muted);font-size:.85rem">Ban dang tham gia giai nay.</div>';

    return '<div class="tournament-card">' +
      '<div class="tc-banner" onclick="openTournament(' + t.ma_giai_dau + ')" style="cursor:pointer;background:linear-gradient(135deg,#1a1f36,#18a0fb33)"><span style="font-size:2.5rem">T</span></div>' +
      '<div class="tc-body">' +
        '<div class="tc-game-badge">' + gameName + '</div>' +
        '<div class="tc-name" onclick="openTournament(' + t.ma_giai_dau + ')" style="cursor:pointer">' + t.ten_giai_dau + '</div>' +
        '<div class="tc-meta"><span class="tc-status">' + formatTournamentStatus(t.trang_thai) + '</span><span>' + prize + '</span></div>' +
        ownerActions +
      '</div>' +
    '</div>';
  }).join('');
};

window.submitTournamentApproval = async function(maGiaiDau) {
  const result = await api('/TournamentBuilderApi/GuiXetDuyet?maGiaiDau=' + maGiaiDau, 'POST', {});
  showToast(result.Success ? 'Da gui yeu cau len admin.' : (result.Message || 'Khong the gui yeu cau.'));
  if (result.Success) loadMyTournaments();
};

window.openManageTournament = async function(maGiaiDau, tenGiaiDau) {
  navigateTo('manage-tournament');
  const sub = document.getElementById('manage-tournament-sub');
  const list = document.getElementById('manage-tournament-list');
  if (sub) sub.textContent = 'Duyet doi tham gia - ' + (tenGiaiDau || ('Giai #' + maGiaiDau));
  if (!list) return;
  list.innerHTML = '<div class="profile-card">Dang tai danh sach dang ky...</div>';

  const result = await api('/TournamentBuilderApi/DanhSachDangKyDoi?maGiaiDau=' + maGiaiDau);
  if (!result.Success || !Array.isArray(result.Data)) {
    list.innerHTML = '<div class="profile-card">' + (result.Message || 'Khong the tai danh sach dang ky doi.') + '</div>';
    return;
  }

  if (!result.Data.length) {
    list.innerHTML = '<div class="empty-state"><div class="empty-state-icon">N</div><h4>Chua co doi dang ky</h4></div>';
    return;
  }

  list.innerHTML = result.Data.map(function(item) {
    const canReview = item.trang_thai_duyet === 'cho_duyet';
    return '<div class="profile-card" style="margin-bottom:16px">' +
      '<div style="display:flex;justify-content:space-between;gap:12px;flex-wrap:wrap;align-items:flex-start">' +
        '<div>' +
          '<h5 style="margin-bottom:6px">' + item.ten_doi + ' - ' + item.ten_nhom + '</h5>' +
          '<div style="color:var(--text-muted);font-size:.9rem">' + (item.ten_game || '') + '</div>' +
          '<div style="color:var(--text-muted);font-size:.9rem;margin-top:4px">' + (item.slogan || '') + '</div>' +
        '</div>' +
        '<div style="text-align:right">' +
          '<div class="tc-status" style="display:inline-flex">' + item.trang_thai_duyet + '</div>' +
          (canReview
            ? '<div style="display:flex;gap:8px;margin-top:12px;justify-content:flex-end">' +
                '<button class="btn-primary-glow" onclick="reviewTeamRegistration(' + maGiaiDau + ',' + item.ma_nhom + ',true)">Duyet</button>' +
                '<button class="btn-outline-glow" onclick="reviewTeamRegistration(' + maGiaiDau + ',' + item.ma_nhom + ',false)">Tu choi</button>' +
              '</div>'
            : '') +
        '</div>' +
      '</div>' +
    '</div>';
  }).join('');
};

window.reviewTeamRegistration = async function(maGiaiDau, maNhom, chapNhan) {
  const result = await api('/TournamentBuilderApi/DuyetDangKyDoi', 'POST', {
    MaGiaiDau: maGiaiDau,
    MaNhom: maNhom,
    ChapNhan: !!chapNhan
  });
  showToast(result.Success ? (chapNhan ? 'Da duyet doi tham gia.' : 'Da tu choi doi tham gia.') : (result.Message || 'Khong the cap nhat dang ky.'));
  if (result.Success) openManageTournament(maGiaiDau, '');
};

async function loadAdminRequests() {
  const wrap = document.getElementById('admin-request-list');
  if (!wrap) return;
  wrap.innerHTML = '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">!</div><h4>Dang tai yeu cau...</h4></div>';

  const result = await api('/AdminApi/Dashboard');
  const requests = result.Success && result.Data && result.Data.ActionRequired ? result.Data.ActionRequired.GiaiChoXetDuyet : [];
  if (!Array.isArray(requests) || !requests.length) {
    wrap.innerHTML = '<div class="empty-state" style="padding:30px"><div class="empty-state-icon">OK</div><h4>Khong co yeu cau nao</h4></div>';
    return;
  }

  wrap.innerHTML = requests.map(function(item) {
    const ma = item.ma_giai_dau;
    const ten = item.ten_giai_dau || ('Giai #' + ma);
    return '<div class="profile-card" style="margin-bottom:16px">' +
      '<div style="display:flex;justify-content:space-between;gap:12px;flex-wrap:wrap">' +
        '<div>' +
          '<h5 style="margin-bottom:6px">' + ten + '</h5>' +
          '<div style="color:var(--text-muted);font-size:.9rem">Nguoi tao: ' + (item.ten_nguoi_tao || 'Khong ro') + '</div>' +
          '<div style="color:var(--text-muted);font-size:.9rem">Game: ' + (item.ten_game || 'Khong ro') + '</div>' +
        '</div>' +
        '<div style="display:flex;gap:8px;align-items:center">' +
          '<button class="btn-primary-glow" onclick="approveTournamentRequest(' + ma + ')">Duyet</button>' +
          '<button class="btn-outline-glow" onclick="rejectTournamentRequest(' + ma + ')">Huy</button>' +
        '</div>' +
      '</div>' +
    '</div>';
  }).join('');
};

window.approveTournamentRequest = async function(maGiaiDau) {
  const result = await api('/TournamentBuilderApi/PheDuyet?maGiaiDau=' + maGiaiDau, 'POST', {});
  showToast(result.Success ? 'Admin da phe duyet giai dau.' : (result.Message || 'Khong the phe duyet.'));
  if (result.Success) loadAdminRequests();
};

window.rejectTournamentRequest = async function(maGiaiDau) {
  const result = await api('/TournamentBuilderApi/TuChoi?maGiaiDau=' + maGiaiDau, 'POST', {});
  showToast(result.Success ? 'Admin da tu choi yeu cau tao giai.' : (result.Message || 'Khong the tu choi.'));
  if (result.Success) loadAdminRequests();
};

// ---- INIT ----
restoreSession();

})();
