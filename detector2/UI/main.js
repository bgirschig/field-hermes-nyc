import ActionButtons from "./ActionButtons.js";
import DetectorClient from "./DetectorClient.js";
import * as MaskEditor from "./mask.js";

const marker = document.querySelector('.marker');
const debugImg = document.querySelector('#debugImg');

ActionButtons.init();
MaskEditor.init();

const ipInput = document.querySelector("#targetIP");
ipInput.addEventListener("blur", onNewIp);
ipInput.addEventListener("keyup", e => { if (e.keyCode === 13) onNewIp(e) });
function onNewIp(e) {
  DetectorClient.setHost(e.target.value);
}

ActionButtons.addListener('setDebug', value => {
  debugImg.style.visibility = value == "true" ? "" : "hidden";
});

DetectorClient.addListener('detectorValue', ([value, time]) => {
  marker.style.left = `${50 + 50 * value}%`;
});

DetectorClient.addListener('detectorDisplay', value => {
  debugImg.src = value;
});