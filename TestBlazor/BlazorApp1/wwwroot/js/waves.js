export function initCards() {
  const cards = Array.from(document.querySelectorAll('.proc-card'));
  if (!cards.length) return;

  const animators = [];

  cards.forEach(card => {
    // Avoid duplicating canvas
    if (card.querySelector('.__waves_canvas')) return;

    const canvas = document.createElement('canvas');
    canvas.className = '__waves_canvas';
    canvas.style.position = 'absolute';
    canvas.style.left = '0';
    canvas.style.top = '0';
    canvas.style.width = '100%';
    canvas.style.height = '100%';
    canvas.style.pointerEvents = 'none';
    canvas.style.zIndex = '0';
    canvas.style.borderRadius = getComputedStyle(card).borderRadius || '12px';

    // ensure card is positioned
    const cs = getComputedStyle(card);
    if (cs.position === 'static') card.style.position = 'relative';

    card.insertBefore(canvas, card.firstChild);

    const ctx = canvas.getContext('2d');

    function fit() {
      const rect = card.getBoundingClientRect();
      canvas.width = Math.max(1, Math.floor(rect.width * devicePixelRatio));
      canvas.height = Math.max(1, Math.floor(rect.height * devicePixelRatio));
      canvas.style.width = rect.width + 'px';
      canvas.style.height = rect.height + 'px';
      ctx.setTransform(devicePixelRatio, 0, 0, devicePixelRatio, 0, 0);
    }

    fit();

    const config = {
      layers: [
        { amp: rectHeightFraction(0.06), freq: 0.8, speed: 0.6, color: 'rgba(111,215,255,0.06)' },
        { amp: rectHeightFraction(0.04), freq: 1.25, speed: 0.9, color: 'rgba(10,88,255,0.045)' }
      ]
    };

    // helper for amplitude as fraction of card height
    function rectHeightFraction(frac) {
      const rect = card.getBoundingClientRect();
      return Math.max(6, rect.height * frac);
    }

    let last = performance.now();
    let t = 0;

    function draw(now) {
      const dt = Math.max(0, (now - last) / 1000);
      last = now;
      t += dt;

      fit();
      const w = canvas.width / devicePixelRatio;
      const h = canvas.height / devicePixelRatio;

      ctx.clearRect(0, 0, w, h);

      // draw each layer
      config.layers.forEach((layer, idx) => {
        const amp = rectHeightFraction(idx === 0 ? 0.06 : 0.04);
        const freq = layer.freq;
        const speed = layer.speed;
        ctx.fillStyle = layer.color;
        ctx.beginPath();
        ctx.moveTo(0, h);

        const step = Math.max(12, Math.floor(w / 60));
        for (let x = 0; x <= w + step; x += step) {
          const nx = x / w;
          const y = h / 2 + Math.sin((nx * freq + t * speed) * Math.PI * 2) * amp * (1 + 0.3 * Math.sin(t * 0.7 + idx));
          ctx.lineTo(x, y);
        }

        ctx.lineTo(w, h);
        ctx.closePath();
        ctx.fill();
      });

      animFrame = requestAnimationFrame(draw);
    }

    let animFrame = requestAnimationFrame(draw);

    // Resize observer to refit the canvas
    const ro = new ResizeObserver(() => fit());
    ro.observe(card);

    animators.push({ cancel() { cancelAnimationFrame(animFrame); ro.disconnect(); } });
  });

  // return a cleanup handle on window so Blazor dev hot reload can call if needed
  window.__waves_animators = animators;
}
