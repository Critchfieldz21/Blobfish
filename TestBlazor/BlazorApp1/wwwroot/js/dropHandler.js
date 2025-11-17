export function enableDrop(dropSelector, inputId) {
    const dropEl = document.querySelector(dropSelector);
    const input = document.getElementById(inputId);
    if (!dropEl || !input) return;

    // Global per-input file store so selections survive dialog/blur cycles
    window.__bfStoredFiles = window.__bfStoredFiles || {};
    const store = window.__bfStoredFiles;

    // Ensure the underlying input accepts multiple files
    try { input.multiple = true; } catch (_) { }

    function prevent(e) { e.preventDefault(); e.stopPropagation(); }

    // Capture-phase change listener on the input so we can update the store before Blazor sees it
    input.addEventListener('change', (e) => {
        try {
            const newFiles = input.files ? Array.from(input.files) : [];
            // Replace previous selection
            store[inputId] = newFiles;
            // allow event to propagate to Blazor with input.files already set by the browser
        } catch (err) {
            console.warn('storing input change failed', err);
        }
    }, true);

    dropEl.addEventListener('dragover', (e) => {
        prevent(e);
        dropEl.classList.add('drag-over');
    });
    dropEl.addEventListener('dragenter', (e) => { prevent(e); dropEl.classList.add('drag-over'); });
    dropEl.addEventListener('dragleave', (e) => { prevent(e); dropEl.classList.remove('drag-over'); });

    dropEl.addEventListener('drop', async (e) => {
        prevent(e);
        dropEl.classList.remove('drag-over');
        const dt = e.dataTransfer;
        if (!dt) return;

        // Collect files via dataTransfer.items (better for some browsers) or dataTransfer.files
        let collected = [];
        try {
            if (dt.items && dt.items.length > 0) {
                for (let i = 0; i < dt.items.length; i++) {
                    const item = dt.items[i];
                    if (item.kind === 'file') {
                        const file = item.getAsFile();
                        if (file) collected.push(file);
                    }
                }
            } else if (dt.files && dt.files.length > 0) {
                collected = Array.from(dt.files);
            }
        } catch (ex) {
            // fallback to files list
            collected = dt.files ? Array.from(dt.files) : [];
        }

        const files = collected.filter(f => {
            const name = (f.name || '').toLowerCase();
            return (f.type === 'application/pdf') || name.endsWith('.pdf');
        });

        if (files.length === 0) {
            dropEl.classList.add('drop-error');
            setTimeout(() => dropEl.classList.remove('drop-error'), 1200);
            return;
        }

        // Replace stored input selection with the dropped files
        try {
            const newDt = new DataTransfer();
            for (const f of files) newDt.items.add(f);
            input.files = newDt.files;
            store[inputId] = files;

            // Dispatch change event so Blazor's InputFile processes the new selection
            const ev = new Event('change', { bubbles: true });
            input.dispatchEvent(ev);
        } catch (err) {
            console.warn('Creating DataTransfer failed, attempting fallback', err);
            try {
                const fallbackEvent = new Event('drop', { bubbles: true });
                input.dispatchEvent(fallbackEvent);
            } catch (e) {
                console.error('Drop fallback also failed', e);
            }
        }
    });
}