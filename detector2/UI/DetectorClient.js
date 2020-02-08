import EventHandler from "./EventHandler.js";

const ACTION_TIMEOUT_SECONDS = 2;

let socket = new WebSocket("ws://localhost:9000");
const eventHandler = new EventHandler();
const pendingMessages = [];

socket.onmessage = e => {
  const data = JSON.parse(e.data);
  eventHandler.invoke(data.type, data.value || data.arrayValue);
};
socket.onopen = () => {
  let message;
  while (message = pendingMessages.pop()) {
    if (performance.now() > message.timeout) return;
    
    socket.send(JSON.stringify(message));
  };
};

function sendAction(action, value="") {
  try {
    value = JSON.parse(value);
  } catch (error) {}

  const message = {
    type: 'action',
    action: action,
    value,
    timeout: performance.now() + ACTION_TIMEOUT_SECONDS * 1000,
  }
  if (socket.readyState == socket.OPEN) {
    socket.send(JSON.stringify(message));
  } else {
    pendingMessages.push(message);
  }
}

const out = {
  socket,
  sendAction,
  eventHandler,
};
eventHandler.bind(out);

export default out;