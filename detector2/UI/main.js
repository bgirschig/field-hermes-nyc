import ActionButtons from "./ActionButtons.js";
import DetectorClient from "./DetectorClient.js";
import * as MaskEditor from "./mask.js";

const marker = document.querySelector('.marker');
const debugImg = document.querySelector('#debugImg');

ActionButtons.init();
MaskEditor.init();

ActionButtons.addListener('setDebug', value => {
  debugImg.style.visibility = value == "true" ? "" : "hidden";
});

DetectorClient.addListener('detectorValue', value => {
  marker.style.left = `${50 + 50 * value}%`;
});

DetectorClient.addListener('detectorDisplay', value => {
  debugImg.src = value;
});