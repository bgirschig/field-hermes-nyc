# Inspired by https://www.smashingmagazine.com/2016/02/simple-augmented-reality-with-opencv-a-three-js/

from SimpleWebSocketServer import WebSocket, SimpleWebSocketServer
import json
import detector
import traceback
import threading
import cv2
import time
from utils import image2dataUrl
import queue

# global state
server = None
clients = []

received_messages = queue.Queue()
sent_messages = queue.Queue()

"""
Basic websocket handler. Does nothing itself, apart from parsing the messages
and keeping track of the clients. Sends parsed messages to the receive function
"""
class DetectorHandler(WebSocket):
  def handleMessage(self):
    try:
      received_messages.put(json.loads(self.data))
    except Exception as e:
      traceback.print_stack()
      print(e)
  def handleConnected(self):
    clients.append(self)
  def handleClose(self):
     clients.remove(self)

"""
Handle incoming messages
"""
def handleMessage(data):
  if data["type"] == "action":
    if data["action"] == "setDebug":
      detector.debug = data["value"]
    elif data["action"] == "setActive": detector.active = data["value"]
    elif data["action"] == "setCamera":
      info = detector.set_camera(data["value"])
      print (info)
      send({'type': 'cameraInfo', 'value': info })
    elif data["action"] == "setMask": detector.set_mask(data["value"])
    elif data["action"] == "getMask":
      mask = detector.getCurrentMask()
      if (mask is not None): send({'type': 'detectorMask', 'value': image2dataUrl(mask) })
    elif data["action"] == "getCameraInfo":
      info = detector.getCameraInfo()
      print(info)
      send({'type': 'cameraInfo', 'value': info })
    elif data["action"] == "getFrame":
      frame = detector.getFrame(masked=False)
      send({'type': 'cameraFrame', 'value': image2dataUrl(frame) })

"""
Send data to all clients
"""
def send(data):
  msg = str(json.dumps(data))
  for client in clients:
    client.sendMessage(msg)

"""
Method used to run the socket server inside a thread, allowing the openCV loop to
be uninterrupted
"""
def run_server():
  global server
  server = SimpleWebSocketServer('', 9000, DetectorHandler, selectInterval=(1000.0 / 15) / 1000)
  server.serveforever()

# Start the socket server thread
socket_thread = threading.Thread(target=run_server)
socket_thread.daemon = True
socket_thread.start()

detector.init()

# OpenCV loop
last_detection_time = 0
while True:
  try:
    message = received_messages.get_nowait()
    while message:
      handleMessage(message)
      message = received_messages.get_nowait()
  except queue.Empty:
    pass

  try:
    value, display = detector.detect()
    if value:
      send({'type': 'detectorValue', 'value': value })
    if display is not None:
      send({'type': 'detectorDisplay', 'value': image2dataUrl(display) })
  except KeyboardInterrupt:
    detector.stop()
    break
  except Exception as e:
    print(e)
    pass
  time.sleep(20 / 1000.0)

detector.stop()
