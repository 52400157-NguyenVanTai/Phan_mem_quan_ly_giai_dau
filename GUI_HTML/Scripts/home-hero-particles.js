(function () {
  "use strict";

  var hero = document.querySelector(".public-hero");
  var canvas = document.getElementById("hero-particles-canvas");
  if (!hero || !canvas) {
    return;
  }

  var ctx = canvas.getContext("2d", { alpha: true });
  if (!ctx) {
    return;
  }

  var reducedMotionQuery = window.matchMedia("(prefers-reduced-motion: reduce)");
  var coarsePointerQuery = window.matchMedia("(pointer: coarse)");

  var prefersReducedMotion = reducedMotionQuery.matches;
  var coarsePointer = coarsePointerQuery.matches;
  var dpr = Math.min(window.devicePixelRatio || 1, 2);

  var particles = [];
  var frameId = 0;
  var paused = false;

  var mouse = {
    x: 0,
    y: 0,
    active: false,
  };

  function clamp(value, min, max) {
    return Math.max(min, Math.min(max, value));
  }

  function randomFrom(list) {
    return list[Math.floor(Math.random() * list.length)];
  }

  function pickColor() {
    return randomFrom([
      "rgba(118, 210, 255, 0.36)",
      "rgba(87, 185, 255, 0.3)",
      "rgba(146, 136, 255, 0.26)",
      "rgba(193, 230, 255, 0.22)",
    ]);
  }

  function getParticleCount() {
    var area = hero.clientWidth * hero.clientHeight;
    var density = coarsePointer ? 18000 : 12000;

    if (prefersReducedMotion) {
      density = 26000;
    }

    return clamp(Math.floor(area / density), coarsePointer ? 28 : 42, coarsePointer ? 72 : 118);
  }

  function resizeCanvas() {
    var rect = hero.getBoundingClientRect();

    dpr = Math.min(window.devicePixelRatio || 1, 2);
    canvas.width = Math.max(1, Math.floor(rect.width * dpr));
    canvas.height = Math.max(1, Math.floor(rect.height * dpr));
    canvas.style.width = rect.width + "px";
    canvas.style.height = rect.height + "px";

    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

    buildParticles();

    if (prefersReducedMotion) {
      drawStatic();
    }
  }

  function buildParticles() {
    var count = getParticleCount();
    particles = [];

    var width = hero.clientWidth;
    var height = hero.clientHeight;

    for (var i = 0; i < count; i += 1) {
      var baseX = Math.random() * width;
      var baseY = Math.random() * height;
      var type = Math.random() < 0.18 ? "dash" : "dot";

      particles.push({
        baseX: baseX,
        baseY: baseY,
        x: baseX,
        y: baseY,
        vx: 0,
        vy: 0,
        radius: Math.random() < 0.72 ? 0.75 + Math.random() * 1.25 : 1.5 + Math.random() * 1.1,
        dashLength: 2.2 + Math.random() * 3,
        angle: Math.random() * Math.PI * 2,
        color: pickColor(),
        type: type,
      });
    }
  }

  function drawParticle(p) {
    ctx.save();
    ctx.fillStyle = p.color;
    ctx.strokeStyle = p.color;

    if (p.type === "dash") {
      ctx.translate(p.x, p.y);
      ctx.rotate(p.angle);
      ctx.lineWidth = Math.max(0.8, p.radius * 0.82);
      ctx.beginPath();
      ctx.moveTo(-p.dashLength * 0.5, 0);
      ctx.lineTo(p.dashLength * 0.5, 0);
      ctx.stroke();
    } else {
      ctx.beginPath();
      ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
      ctx.fill();
    }

    ctx.restore();
  }

  function step() {
    if (paused) {
      return;
    }

    var width = hero.clientWidth;
    var height = hero.clientHeight;

    ctx.clearRect(0, 0, width, height);

    var repelRadius = coarsePointer ? 72 : 108;
    var repelStrength = coarsePointer ? 0.34 : 0.52;
    var spring = coarsePointer ? 0.016 : 0.022;
    var damping = 0.9;

    for (var i = 0; i < particles.length; i += 1) {
      var p = particles[i];

      if (mouse.active) {
        var dx = p.x - mouse.x;
        var dy = p.y - mouse.y;
        var distSq = dx * dx + dy * dy;
        var radiusSq = repelRadius * repelRadius;

        if (distSq < radiusSq && distSq > 0.001) {
          var dist = Math.sqrt(distSq);
          var force = (1 - dist / repelRadius) * repelStrength;
          var inv = 1 / dist;
          p.vx += dx * inv * force;
          p.vy += dy * inv * force;
        }
      }

      p.vx += (p.baseX - p.x) * spring;
      p.vy += (p.baseY - p.y) * spring;
      p.vx *= damping;
      p.vy *= damping;
      p.x += p.vx;
      p.y += p.vy;

      drawParticle(p);
    }

    frameId = window.requestAnimationFrame(step);
  }

  function drawStatic() {
    var width = hero.clientWidth;
    var height = hero.clientHeight;

    ctx.clearRect(0, 0, width, height);
    for (var i = 0; i < particles.length; i += 1) {
      drawParticle(particles[i]);
    }
  }

  function getLocalPoint(evt) {
    var rect = canvas.getBoundingClientRect();
    return {
      x: evt.clientX - rect.left,
      y: evt.clientY - rect.top,
    };
  }

  function onPointerMove(evt) {
    var point = getLocalPoint(evt);
    mouse.x = point.x;
    mouse.y = point.y;
    mouse.active = true;
  }

  function onPointerLeave() {
    mouse.active = false;
  }

  function onVisibilityChange() {
    paused = document.hidden;

    if (!paused && !prefersReducedMotion && frameId === 0) {
      frameId = window.requestAnimationFrame(step);
    }

    if (paused && frameId) {
      window.cancelAnimationFrame(frameId);
      frameId = 0;
    }
  }

  function handleMotionPreferenceChange() {
    prefersReducedMotion = reducedMotionQuery.matches;
    resizeCanvas();

    if (prefersReducedMotion) {
      if (frameId) {
        window.cancelAnimationFrame(frameId);
        frameId = 0;
      }
      drawStatic();
      return;
    }

    if (!frameId) {
      frameId = window.requestAnimationFrame(step);
    }
  }

  function handlePointerPreferenceChange() {
    coarsePointer = coarsePointerQuery.matches;
    resizeCanvas();
  }

  window.addEventListener("resize", resizeCanvas);
  hero.addEventListener("pointermove", onPointerMove);
  hero.addEventListener("pointerleave", onPointerLeave);
  hero.addEventListener("pointercancel", onPointerLeave);
  document.addEventListener("visibilitychange", onVisibilityChange);

  if (typeof reducedMotionQuery.addEventListener === "function") {
    reducedMotionQuery.addEventListener("change", handleMotionPreferenceChange);
    coarsePointerQuery.addEventListener("change", handlePointerPreferenceChange);
  } else {
    reducedMotionQuery.addListener(handleMotionPreferenceChange);
    coarsePointerQuery.addListener(handlePointerPreferenceChange);
  }

  resizeCanvas();

  if (!prefersReducedMotion) {
    frameId = window.requestAnimationFrame(step);
  }
})();
