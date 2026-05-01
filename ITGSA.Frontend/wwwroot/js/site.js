document.addEventListener('DOMContentLoaded', () => {
    initFileInputs();
    markNav();

    document.getElementById('btnResetOpen')?.addEventListener('click', () => {
        document.getElementById('resetOverlay').classList.add('show');
    });

    document.getElementById('btnResetCancel')?.addEventListener('click', () => {
        document.getElementById('resetOverlay').classList.remove('show');
    });
});

function initFileInputs() {
    document.querySelectorAll('.file-wrap').forEach(wrap => {
        const input = wrap.querySelector('input[type="file"]');
        const name  = wrap.querySelector('.file-name');
        if (!input || !name) return;

        input.addEventListener('change', () => {
            name.textContent = input.files[0]?.name ?? '';
        });

        wrap.addEventListener('dragover', e => { e.preventDefault(); wrap.style.borderColor = '#444'; });
        wrap.addEventListener('dragleave', () => { wrap.style.borderColor = ''; });
        wrap.addEventListener('drop', e => {
            e.preventDefault();
            wrap.style.borderColor = '';
            if (e.dataTransfer.files[0]) {
                const dt = new DataTransfer();
                dt.items.add(e.dataTransfer.files[0]);
                input.files = dt.files;
                name.textContent = e.dataTransfer.files[0].name;
            }
        });
    });
}

function markNav() {
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('nav a[href]').forEach(a => {
        const href = a.getAttribute('href').toLowerCase();
        if (href === '/' ? path === '/' : path.startsWith(href)) {
            a.classList.add('active');
        }
    });
}
