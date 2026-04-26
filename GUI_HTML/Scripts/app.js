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
  document.getElementById('avatar-initials').textContent = initials;
  // Profile page
  const un = user.TenDangNhap || user.ten_dang_nhap || '';
  const em = user.Email || user.email || '';
  document.getElementById('profile-username-display').textContent = un;
  document.getElementById('profile-email-display').textContent = em;
  document.getElementById('profile-avatar-big').textContent = initials;
  document.getElementById('profile-role-display').textContent = user.VaiTroHeThong || user.vai_tro_he_thong || '';
  document.getElementById('pf-username').value = un;
  document.getElementById('pf-email').value = em;
  document.getElementById('pf-avatar-url').value = user.AvatarUrl || user.avatar_url || '';
  document.getElementById('pf-bio').value = user.Bio || user.bio || '';

  // Show admin button if admin
  const btnAdmin = document.getElementById('btn-admin-module');
  if (btnAdmin) {
    const isAdmin = String(user.VaiTroHeThong || user.vai_tro_he_thong || '').toLowerCase() === 'admin';
    btnAdmin.classList.toggle('d-none', !isAdmin);
  }
}

// ---- LOGOUT ----
window.doLogout = async function() {
  await api('/AuthApi/DangXuat', 'POST', {});
  currentUser = null;
  document.getElementById('app-shell').classList.remove('active');
  document.getElementById('landing-page').style.display = '';
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
    if (page === 'manage-tournament') loadManageTournament();
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
    AvatarUrl: document.getElementById('pf-avatar-url').value.trim(),
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
  const posOptions = positions.map(p => '<option value="' + p.MaViTri + '"' + (existing && existing.ma_vi_tri_so_truong == p.MaViTri ? ' selected' : '') + '>' + p.TenViTri + '</option>').join('');

  body.innerHTML = '<div class="form-group-dark"><label>ID trong game</label><input class="form-control-dark" id="gp-id" value="' + (existing ? existing.in_game_id || '' : '') + '" placeholder="ID trong game" /></div>' +
    '<div class="form-group-dark"><label>Tên hiển thị trong game</label><input class="form-control-dark" id="gp-name" value="' + (existing ? existing.in_game_name || '' : '') + '" placeholder="Nickname" /></div>' +
    '<div class="form-group-dark"><label>Vị trí sở trường</label><select class="select-dark" id="gp-vitri"><option value="">-- Chọn vị trí --</option>' + posOptions + '</select></div>' +
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

// ---- INIT ----
restoreSession();

})();
