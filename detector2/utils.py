import cv2
import base64

def image2dataUrl(image, format='jpeg'):
  retval, buffer = cv2.imencode(f'.{format}', image)
  outputImg = base64.b64encode(buffer).decode()
  outputImg = f'data:image/{format};base64,{outputImg}'
  return outputImg