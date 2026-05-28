/* Notaría Parroquial — site.js */
'use strict';

// ── Theme ──────────────────────────────────────
const html = document.documentElement;
const themeToggle = document.getElementById('themeToggle');
const themeIcon   = document.getElementById('themeIcon');
const themeLabel  = document.getElementById('themeLabel');

function applyTheme(t) {
  html.setAttribute('data-theme', t);
  localStorage.setItem('theme', t);
  if (themeIcon && themeLabel) {
    if (t === 'dark') {
      themeIcon.className  = 'bi bi-sun-fill';
      themeLabel.textContent = 'Modo claro';
    } else {
      themeIcon.className  = 'bi bi-moon-fill';
      themeLabel.textContent = 'Modo oscuro';
    }
  }
}

// Apply saved theme immediately
applyTheme(localStorage.getItem('theme') || 'light');

if (themeToggle) {
  themeToggle.addEventListener('click', () => {
    applyTheme(html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark');
  });
}

// ── Sidebar mobile ────────────────────────────
const sidebar  = document.getElementById('sidebar');
const sidebarToggle = document.getElementById('sidebarToggle');
const overlay  = document.getElementById('sidebarOverlay');

function openSidebar() {
  sidebar?.classList.add('open');
  overlay?.classList.add('show');
  document.body.style.overflow = 'hidden';
}

function closeSidebar() {
  sidebar?.classList.remove('open');
  overlay?.classList.remove('show');
  document.body.style.overflow = '';
}

sidebarToggle?.addEventListener('click', () => {
  sidebar?.classList.contains('open') ? closeSidebar() : openSidebar();
});

overlay?.addEventListener('click', closeSidebar);

// ── Toast auto-dismiss ────────────────────────
function initToasts() {
  document.querySelectorAll('.toast-item').forEach(toast => {
    // Auto-dismiss after 4.5s
    const timer = setTimeout(() => dismissToast(toast), 4500);

    toast.querySelector('.toast-close')?.addEventListener('click', () => {
      clearTimeout(timer);
      dismissToast(toast);
    });
  });
}

function dismissToast(toast) {
  toast.classList.add('removing');
  toast.addEventListener('animationend', () => toast.remove(), { once: true });
}

// ── Utility: show programmatic toast ─────────
window.showToast = function(message, type = 'info') {
  const icons = {
    success: 'bi-check-circle-fill',
    warning: 'bi-exclamation-triangle-fill',
    danger:  'bi-x-circle-fill',
    info:    'bi-info-circle-fill'
  };
  const stack = document.getElementById('toastStack');
  if (!stack) return;

  const el = document.createElement('div');
  el.className = `toast-item toast-${type}`;
  el.setAttribute('role', 'alert');
  el.innerHTML = `
    <i class="bi ${icons[type] || icons.info}"></i>
    <span>${message}</span>
    <button class="toast-close" aria-label="Cerrar"><i class="bi bi-x"></i></button>
  `;
  stack.appendChild(el);

  const timer = setTimeout(() => dismissToast(el), 4500);
  el.querySelector('.toast-close').addEventListener('click', () => {
    clearTimeout(timer);
    dismissToast(el);
  });
};

// ── Password toggle (login) ───────────────────
// Handled inline in Login.cshtml

// ── Init ─────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
  initToasts();

  // Active nav link animation
  document.querySelectorAll('.nav-link.active').forEach(link => {
    link.scrollIntoView({ block: 'nearest' });
  });

  // Auto-uppercase CURP input
  document.querySelectorAll('input[data-curp]').forEach(el => {
    el.addEventListener('input', () => { el.value = el.value.toUpperCase(); });
  });

  // Confirm delete buttons (inline)
  // Handled per-view

  // Table row click → navigate to detail link (optional UX)
  document.querySelectorAll('.data-table tbody tr[data-href]').forEach(row => {
    row.style.cursor = 'pointer';
    row.addEventListener('click', (e) => {
      if (!e.target.closest('a, button, form')) {
        window.location.href = row.dataset.href;
      }
    });
  });
});
