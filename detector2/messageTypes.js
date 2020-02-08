// example messages between app and detector, for all message types

var messages = [
  {type: "action", action: "setDebug", value: true},
  {type: "action", action: "setActive", value: true},
  {type: "action", action: "setCamera", value: 0},
  {type: "action", action: "setCamera", value: "/user/blafos/Desktop/plop.mp4"},
  {type: "action", action: "setCamera", value: "/user/blafos/Desktop/plop.mp4"},
  {type: "action", action: "setMask", value: "data:image/png;base64,iVBORw0KGgoAAA..."},
  {type: "action", action: "getMask"},
  {type: "action", action: "getCameraInfo"},
  {type: "action", action: "getFrame"},

  {type: "cameraInfo", arrayValue: [1920, 1080]},
  {type: "detectorMask", value: "data:image/png;base64,iVBORw0KGgoAAA..."},
  {type: "detectorValue", value: 0.4},
  {type: "detectorDisplay", value: "data:image/png;base64,iVBORw0KGgoAAA..."},
]