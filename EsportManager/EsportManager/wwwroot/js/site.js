// Tab switching
function switchTab(id, el) {
    document.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    const pane = document.getElementById(id);
    if (pane) pane.classList.add('active');
    if (el) el.classList.add('active');
}

// Mobile nav
function toggleMobileNav() {
    const n = document.getElementById('mobileNav');
    if (n) n.style.display = n.style.display === 'block' ? 'none' : 'block';
}

// Countdown
function initCountdowns() {
    document.querySelectorAll('[data-countdown]').forEach(el => {
        const end = new Date(el.dataset.countdown);
        const tick = () => {
            const diff = end - new Date();
            if (diff <= 0) { el.textContent = 'Đã bắt đầu'; return; }
            const d = Math.floor(diff / 86400000);
            const h = Math.floor((diff % 86400000) / 3600000);
            const m = Math.floor((diff % 3600000) / 60000);
            const s = Math.floor((diff % 60000) / 1000);
            el.textContent = `${d > 0 ? d + 'n ' : ''}${h}g ${m}p ${s}s`;
        };
        tick(); setInterval(tick, 1000);
    });
}

// Animate numbers
function animateNumbers() {
    document.querySelectorAll('[data-count]').forEach(el => {
        const target = parseInt(el.dataset.count);
        if (isNaN(target)) return;
        let current = 0;
        const step = Math.max(1, Math.ceil(target / 40));
        const t = setInterval(() => {
            current = Math.min(current + step, target);
            el.textContent = current.toLocaleString();
            if (current >= target) clearInterval(t);
        }, 30);
    });
}

// Auto-dismiss toast
function autoDismissToast() {
    const t = document.querySelector('.toast-bar');
    if (t) setTimeout(() => { t.style.opacity = '0'; t.style.transition = 'opacity 0.5s'; setTimeout(() => t.remove(), 500); }, 4000);
}

document.addEventListener('DOMContentLoaded', () => {
    initCountdowns();
    animateNumbers();
    autoDismissToast();
});
