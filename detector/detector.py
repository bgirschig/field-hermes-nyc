import numpy as np
import cv2
from scipy.signal import find_peaks
import base64
import math
import urllib.request
import os

# TODO: [STABILITY] Select color channel for detection + have physical color on strings
# TODO: [STABILITY] Detect lines, assuming we know (roughly) the line's origin (probably needs an editor)
# TODO: [PERFORMANCE] Don't recompute "calibration metrics" on every frame

class Detector:
  def __init__(self):
    self.minX = None
    self.minY = None
    self.maxX = None
    self.maxY = None
    self.cap = None
    self.mask = None
    self.shape = None
    self.cropParams = [0, 1, 1, 0.5]
    self._loadMask()

  def __del__(self):
    print("closing capture", flush=True)
    if (self.cap): self.cap.release()

  def setCamera(self, camera_id):
    """ select which camera to use """
    print('setCamera', camera_id, flush=True)
    
    if (self.cap): self.cap.release()
    
    if camera_id == 'auto':
      try:
        self.cap = cv2.VideoCapture(0)
      except expression as identifier:
        self.cap = cv2.VideoCapture('recording.mov')
    elif camera_id == 'emulator':
      video_path = os.path.expanduser('~/Desktop/recording.mov')
      print('trying to load video from', video_path)
      self.cap = cv2.VideoCapture(video_path)
    else:
      self.cap = cv2.VideoCapture(camera_id)

    # Get first frame so that we know the capture width and height
    ret, frame = self.cap.read()
    self.capHeight, self.capWidth, self.capChannels = frame.shape

    # setup default crop values
    self._updateCrop()
    self.shape = (self.capWidth, self.capHeight, self.capChannels)
    return self.shape

  def getShape(self):
    return self.shape or (None, None)

  def setMask(self, dataUrl):
    """ sets and saves a mask image: white pixels will be taken into
    consideration black pixels will not """
    response = urllib.request.urlopen(dataUrl)
    with open('mask.png', 'wb') as f:
      f.write(response.file.read())
    self._loadMask()
  
  def getMask(self):
    """ returns the currently saved mask """
    if self.mask is None: return None
    else: return self._image2dataUrl(self.mask * 255, 'png')

  def _loadMask(self):
    """ loads the currently saved mask """
    mask = cv2.imread('mask.png')
    if mask is not None: self.mask = mask / 255

  def setCrop(self, minX, maxX, height, y):
    """ update the crop region: which part of the image to look at for processing """
    self.cropParams = [minX, maxX, height, y]
    self._updateCrop()

  def _updateCrop(self):
    """ updates the crop region (setCrop uses easy to use params, but we need
    more masic ones during detection: min/max for x/y) """
    if (not self.cap): return

    minX, maxX, height, y = self.cropParams
    self.minX = int(minX * self.capWidth)
    self.maxX = int(maxX * self.capWidth)
    
    focusHeight =  self.capHeight * height
    self.minY = int(y * (self.capHeight - focusHeight))
    self.maxY = int(self.minY + focusHeight)

  def getFrameAsDataUrl(self):
    """ Captures a frame and returns it as a data url """
    frame = self._getFrame(cropped=False, masked=False)
    return self._image2dataUrl(frame)

  def _getFrame(self, cropped=True, masked=True):
    """  Get a (cropped and masked) frame from the current video source (camera/video). """
    ret, frame = self.cap.read()

    if (frame is None):
      # loop if cap is a video
      cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
      ret, frame = self.cap.read()
    if (frame is None): raise Exception("could not capture a frame")

    if masked:
      frame = frame * self.mask

    if cropped:
      return frame[self.minY:self.maxY,self.minX:self.maxX]
    else:
      return frame

  def _image2dataUrl(self, image, format='jpeg'):
    retval, buffer = cv2.imencode(f'.{format}', image)
    outputImg = base64.b64encode(buffer).decode()
    outputImg = f'data:image/{format};base64,{outputImg}'
    return outputImg

  def detect(self, debug=False, chanel=0):
    """ Captures an image and runs the swing detector on it """
    if (not self.cap): return None, None
    frame = self._getFrame()
    height, width, _ = frame.shape

    # 'optimised' version of: cv2.cvtColor(focus, cv2.COLOR_BGR2GRAY)
    # Only keep one chanel
    frame = frame[:,:,chanel]

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
    # 'find_peaks' fails when the peak is on the edge. Fix that by setting the
    # edge values to 0
    clean[[0,-1]] = 0
    peaks, _ = find_peaks(clean, height=avgDistToAvg, distance=1000, width=10)

    outputPeak = None
    outputImg = None
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
      outputImg = self._image2dataUrl(display)
    return outputPeak, outputImg
