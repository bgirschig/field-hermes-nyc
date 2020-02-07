import EventHandler from "./EventHandler.js";
import DetectorClient from "./DetectorClient.js";

const eventHandler = new EventHandler();

function init() {
  const buttons = document.querySelectorAll('.actionBtn');
  buttons.forEach(button => {
    button.addEventListener('click', () => {
      let value;
      if (button.dataset.value) {
        value = button.dataset.value
      } else {
        const target = document.querySelector(button.dataset.valuetarget);
        value = target.value || target.innerText;
      }
      eventHandler.invoke(button.dataset.action, value);
      DetectorClient.sendAction(button.dataset.action, value);
    });
  })
}

const out = {
  init,
  eventHandler
}
eventHandler.bind(out);

export default out