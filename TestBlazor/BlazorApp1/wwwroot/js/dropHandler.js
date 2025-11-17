export function enableDrop(dropSelector, inputId) {
  const dropEl = document.querySelector(dropSelector);
  const input = document.getElementById(inputId);
  if (!dropEl || !input) return;

  // Global per-input file store so selections survive dialog/blur cycles
  window.__bfStoredFiles = window.__bfStoredFiles || {};
  const store = window.__bfStoredFiles;

  // Ensure the underlying input accepts multiple files
  try { input.multiple = true; } catch (_) {}

  function prevent(e) { e.preventDefault(); e.stopPropagation(); }

  // Helper: merge arrays of File objects, dedupe by name/size/lastModified
  function mergeFiles(a, b) {
    const combined = (a || []).concat(b || []);
    const seen = new Map();
    const out = [];
    for (const f of combined) {
      const key = `${f.name}::${f.size}::${f.lastModified}`;
      if (!seen.has(key)) { seen.set(key, true); out.push(f); }
    }
    return out;
  }

  // Capture-phase change listener on the input so we can merge before Blazor sees it
  input.addEventListener('change', (e) => {
    try {
      const newFiles = input.files ? Array.from(input.files) : [];
      const existing = store[inputId] || [];
      const merged = mergeFiles(existing, newFiles);
      // If merged differs from what Blazor would see, assign merged list so later handlers (Blazor) get full list
      if (merged.length !== newFiles.length || merged.length !== existing.length) {
        const dt = new DataTransfer();
        for (const f of merged) dt.items.add(f);
        input.files = dt.files;
      }
      store[inputId] = merged;
      // allow event to propagate to Blazor with updated input.files
    } catch (err) {
      console.warn('merge on input change failed', err);
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

    // Merge dropped files with stored input selection, then assign
    try {
      const existing = store[inputId] || [];
      const merged = mergeFiles(existing, files);
      const newDt = new DataTransfer();
      for (const f of merged) newDt.items.add(f);
      input.files = newDt.files;
      store[inputId] = merged;

      // Dispatch change event so Blazor's InputFile processes all files
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
