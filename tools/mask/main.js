import SocketStubClient from './socketStubClient.js';

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
/** @type {SocketStubClient} */
let detectorStub;

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
let isDetecting = false;

async function init() {
  mainCanvas = document.querySelector('canvas');
  maskCanvas = document.createElement('canvas');
  cursor = document.querySelector('.cursor');

  detectorStub = new SocketStubClient('ws://localhost:8765');
  await detectorStub.readyPromise;
  let [cameraWidth, cameraHeight] = await detectorStub.call('getShape');

  const search = new URLSearchParams(window.location.search);
  const camId = parseInt(search.get('camera'));
  if (camId) {
    await detectorStub.call('setCamera', camId);
  } else if (cameraWidth === null || cameraHeight === null) {
    [cameraWidth, cameraHeight] = await detectorStub.call('setCamera', 0);
  }

  // initialize main canvas
  mainCanvas.width = cameraWidth;
  mainCanvas.height = cameraHeight;
  mainCtx = mainCanvas.getContext('2d');
  // initialize mask canvas
  maskCanvas.width = cameraWidth;
  maskCanvas.height = cameraHeight;
  maskCtx = maskCanvas.getContext('2d');
  loadSaved();
  // initialize controls
  document.querySelectorAll('.controls button').forEach(control => {
    control.addEventListener('click', () => onControl(control.dataset.action));
  });
  // various
  canvasBounds = mainCanvas.getBoundingClientRect();
  canvasScale = maskCanvas.width / canvasBounds.width;
  updateBrush();

  // initialize event
  window.addEventListener('mousedown', onMouseDown);
  window.addEventListener('mouseup', onMouseUp);
  window.addEventListener('mousemove', onMouseMove);
  window.addEventListener('keyup', onKey);

  // start
  loop();
}

async function loadSaved() {
  const dataURI = await detectorStub.call('getMask');
  if (dataURI) {
    const img = new Image;
    img.onload = () => {
      maskCtx.drawImage(img, 0, 0);
    };
    img.src = dataURI;
  } else {
    maskCtx.fillStyle = 'white';
    maskCtx.fillRect(0, 0, maskCanvas.width, maskCanvas.height);
  }
}

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

function updateBrush() {
  cursor.style.width = `${brushRadius*2/canvasScale}px`;
  cursor.style.height = `${brushRadius*2/canvasScale}px`;
  cursor.style.transform = `
    translate(${cursorPos.screenX}px, ${cursorPos.screenY}px)
    translate(-50%, -50%)`;
}

function onKey(e) {
  if (e.key === 'x') isAdding = !isAdding;
  if (e.key === 'm') maskOnly = !maskOnly;
}

function onControl(action) {
  switch (action) {
    case 'save':
      const dataURL = maskCanvas.toDataURL('image/png');
      detectorStub.call('setMask', dataURL);
      break;
    default:
      console.log('action not implemented:', action);
      break;
  }
}

function loop() {
  requestAnimationFrame(loop);

  captureFrame();

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
  mainCtx.drawImage(maskCanvas, 0, 0);
}

function screenToCanvas(x, y) {
  return {
    x: (x - canvasBounds.x) * canvasScale,
    y: (y - canvasBounds.y) * canvasScale,
    screenX: x,
    screenY: y,
  };
}

async function captureFrame() {
  // limit the framerate for the capture
  const now = performance.now();
  if (now - lastCaptureTime < 500) return;
  lastCaptureTime = now;

  // don't allow concurrent detections
  if (isDetecting) return;
  isDetecting = true;

  // actually update lastCapturedFrame
  const dataUrl = await detectorStub.call('getFrameAsDataUrl');
  lastCapturedFrame = await makeImage(dataUrl);

  isDetecting = false;
}

function makeImage(src) {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve(img);
    img.onerror = reject;
    img.src = src;
  });
}

init();

