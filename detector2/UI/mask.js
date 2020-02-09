import DetectorClient from "./DetectorClient.js";

// references
/** @type {HTMLCanvasElement} */
let mainCanvas;
/** @type {HTMLCanvasElement} */
let maskCanvas;
/** @type {CanvasRenderingContext2D} */
let maskCtx;
/** @type {CanvasRenderingContext2D} */
let mainCtx;
/** @type {HTMLDivElement} */
let cursor;

// config
let brushRadius = 30;

// state
let drawing = false;
let scalingBrush = false;
let canvasBounds = null;
let isAdding = false;
let maskOnly = false;
let mouseDownPos = null;
let canvasScale = 1;
let cursorPos = {x: 0, y: 0, screenX: 0, screenY: 0};
let lastCaptureTime = 0;
let lastCapturedFrame = 0;

export function init() {
  mainCanvas = document.querySelector('#maskEditor canvas');
  maskCanvas = document.createElement('canvas');
  cursor = document.querySelector('#maskEditor .cursor');

  // initialize canvases
  mainCtx = mainCanvas.getContext('2d');
  maskCtx = maskCanvas.getContext('2d');
  updateCameraInfo([500, 300])

  DetectorClient.addListener('cameraInfo', updateCameraInfo);
  DetectorClient.addListener('detectorMask', updateMask);
  DetectorClient.addListener('cameraFrame', updateFrame);
  DetectorClient.sendAction('getMask');
  DetectorClient.sendAction('getCameraInfo');

  // initialize controls
  document.querySelectorAll('#maskEditor .controls button').forEach(control => {
    control.addEventListener('click', () => onControl(control.dataset.action));
  });

  // initialize event
  window.addEventListener('mousedown', onMouseDown);
  window.addEventListener('mouseup', onMouseUp);
  window.addEventListener('mousemove', onMouseMove);
  window.addEventListener('keyup', onKey);

  // start
  loop();
}

function updateCameraInfo([cameraHeight, cameraWidth]) {
  mainCanvas.width = cameraWidth;
  mainCanvas.height = cameraHeight;

  maskCanvas.width = cameraWidth;
  maskCanvas.height = cameraHeight;

  // various
  canvasBounds = mainCanvas.getBoundingClientRect();
  canvasScale = maskCanvas.width / canvasBounds.width;
  updateBrush();
}

// mouse events
function onMouseDown(e) {
  mouseDownPos = screenToCanvas(e.clientX, e.clientY);
  if (e.altKey) {
    scalingBrush = true;
    cursorPos = mouseDownPos;
  } else {
    drawing = true;
    onMouseMove(e);
  }
}
function onMouseUp(e) {
  drawing = false;
  scalingBrush = false;
}
function onMouseMove(e) {
  const position = screenToCanvas(e.clientX, e.clientY);
  if (drawing) {
    maskCtx.beginPath();
    maskCtx.ellipse(position.x, position.y, brushRadius, brushRadius, 0, 0, Math.PI*2);
    maskCtx.fillStyle = isAdding ? 'white' : 'black';
    maskCtx.fill();
  }

  if (scalingBrush) {
    const dist = Math.sqrt((position.x - mouseDownPos.x) ** 2 + (position.y - mouseDownPos.y) ** 2);
    brushRadius = dist;
  } else {
    cursorPos = position;
  }
  updateBrush();
}
function onKey(e) {
  if (e.key === 'x') isAdding = !isAdding;
  if (e.key === 'm') maskOnly = !maskOnly;
}
function screenToCanvas(x, y) {
  return {
    x: (x - canvasBounds.x) * canvasScale,
    y: (y - canvasBounds.y) * canvasScale,
    screenX: x,
    screenY: y,
  };
}

function updateBrush() {
  cursor.style.width = `${brushRadius*2/canvasScale}px`;
  cursor.style.height = `${brushRadius*2/canvasScale}px`;
  cursor.style.transform = `
    translate(${cursorPos.screenX}px, ${cursorPos.screenY}px)
    translate(-50%, -50%)`;
}

function updateMask(dataURL) {
  if (dataURL) {
    const img = new Image;
    img.onload = () => {
      maskCtx.drawImage(img, 0, 0);
    };
    img.src = dataURL;
  } else {
    maskCtx.fillStyle = 'white';
    maskCtx.fillRect(0, 0, maskCanvas.width, maskCanvas.height);
  }
}

function onControl(action) {
  switch (action) {
    case 'save':
      const dataURL = maskCanvas.toDataURL('image/png');
      DetectorClient.sendAction('setMask', dataURL);
      break;
    default:
      console.log('action not implemented:', action);
      break;
  }
}

async function updateFrame(dataURL) {
  lastCapturedFrame = await makeImage(dataURL);
}

function loop() {
  requestAnimationFrame(loop);

  const now = performance.now();
  if (now - lastCaptureTime > 1000) {
    lastCaptureTime = now;    
    DetectorClient.sendAction('getFrame');
  }

  if (mainCanvas.width === 0) return;
  mainCtx.clearRect(0, 0, mainCanvas.width, mainCanvas.height);
  if (maskOnly) {
    mainCtx.globalAlpha = 1;
    mainCtx.globalCompositeOperation = 'source-over';
  } else if (lastCapturedFrame) {
    mainCtx.globalAlpha = 1;
    mainCtx.drawImage(lastCapturedFrame, 0, 0);
    mainCtx.globalCompositeOperation = 'multiply';
    mainCtx.globalAlpha = 0.7;
  }
  if (mainCanvas.width === 0) return
  mainCtx.drawImage(maskCanvas, 0, 0);
}

function makeImage(src) {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve(img);
    img.onerror = reject;
    img.src = src;
  });
}
