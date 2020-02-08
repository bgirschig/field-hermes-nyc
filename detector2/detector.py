import numpy as np
import cv2
from scipy.signal import find_peaks
import base64
import math
import urllib.request
import os
import utils

maskPath = os.path.join(os.path.join(os.path.expanduser('~')), 'Desktop', 'mask.png')
zone = {
  'minX': 0,
  'maxX': 1,
  'y': 0.5,
  'height': 10,
}

debug = False
active = True
cap = None
currentMask = None
camera_info = [0,0]
current_camera_id = -1

def init():
  set_camera(0)
  loadMask()

def detect():
  if not active:
    return None, None

  frame = getFrame(masked=True)
  if frame is None:
    return None, None
  height, width, _ = frame.shape

  focus = frame
  height, width, _ = frame.shape
  
  # 'optimised' version of: cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
  frame = frame[:,:,0]

  # We need a 1d array of values for processing. The focus area is a rectangle,
  # so we average the values on the vertical axis.
  # This reduces the effect of camera noise on the detection, giving a more
  # stable output
  baseData = np.average(frame, axis=0)

  # Compute some metrics for 'auto-calibration'
  average = np.average(baseData)
  maxValue = np.max(baseData)
  distToAvg = np.abs(baseData-average)
  avgDistToAvg = np.average(distToAvg)

  # Find the peak
  treshold = (maxValue + average) / 2
  clean = np.zeros(width, frame.dtype)
  clean[baseData > treshold] = 255
  # 'find_peaks' fails when the peak is on the edge. Fix that by setting the edge
  # values to 0
  clean[[0,-1]] = 0
  peaks, _ = find_peaks(clean, height=avgDistToAvg, distance=1000, width=10)

  outputPeak = None
  display = None
  # Only send if the peak count is 1
  if len(peaks == 1):
    outputPeak = peaks[0] / float(width) - 0.5
  if debug:
    # create the display image
    display = np.zeros((100, width, 3), frame.dtype)
    display[0:50,:] = baseData[:,np.newaxis]
    display[50:100, baseData > treshold] = 255
    for peak in peaks:
      cv2.line(display, (peak, 0), (peak, 100), (0,0,255), 3)
  return outputPeak, display

def getFrame(masked=True):
  """  Get a (cropped and masked) frame from the current video source (camera/video). """
  ret, frame = cap.read()

  if (frame is None):
    # loop if cap is a video
    cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
    ret, frame = cap.read()
  if (frame is None): raise Exception("could not capture a frame")

  if masked and currentMask is not None and currentMask.shape[:2] == frame.shape[:2]:
    frame = frame * currentMask

  return frame

def loadMask():
  global currentMask

  """ loads the currently saved mask """
  mask = cv2.imread(maskPath)
  if mask is not None: currentMask = mask / 255
  else: currentMask = None
  
def set_camera(camera_id):
  print("set camera", camera_id)
  global cap, camera_info
  cap = cv2.VideoCapture(camera_id)
  current_camera_id = camera_id

  # Get first frame so that we know the capture width and height
  ret, frame = cap.read()
  camera_info = frame.shape
  return camera_info

def getCameraInfo():
  return camera_info

def getCurrentMask():
  if currentMask is None: return None
  if currentMask.shape[:2] != camera_info[:2]: return None
  return currentMask * 255

def set_mask(dataUrl):
  """ sets and saves a mask image: white pixels will be taken into
  consideration black pixels will not """
  response = urllib.request.urlopen(dataUrl)
  with open(maskPath, 'wb') as f:
    f.write(response.file.read())
  loadMask()

def stop():
  cap.release()
  cv2.destroyAllWindows()