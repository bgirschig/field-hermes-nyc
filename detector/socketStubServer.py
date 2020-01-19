import websockets
import json
from inspect import ismethod

class SocketStubServer():
  def __init__(self, stub):
    self.stub = stub
    self.methodNames = [key for key in dir(stub) if not key.startswith('_') and ismethod(getattr(stub, key))]
    self.methods = {
      key: getattr(stub, key)
      for key in dir(stub)
      if not key.startswith('_')
      and ismethod(getattr(stub, key))
    }
    self.docs = {key: self.methods[key].__doc__ for key in self.methods}

  async def main(self, websocket, path):
    while True:
      try:
        data = await websocket.recv()
        event = json.loads(data)
      
        if event['type'] == 'SocketStubCall':
          callId = event['id']
          response = self.callMethod(event['method'], event['args'])
          response['id'] = event['id']
          response['type'] = 'SocketStubResponse'
          await websocket.send(json.dumps(response))
        if event['type'] == 'SocketStubDoc':
          await websocket.send(json.dumps({
            'id': event['id'],
            'type': 'SocketStubResponse',
            'data': self.docs,
          }))
      except websockets.ConnectionClosed:
        print(f"Connection terminated")
        break
      except Exception as e:
        # Catch all exceptions so that we don't get out of the main loop
        print("unexpected error:", type(e), e)
        continue

  def callMethod(self, method, args):
    if (not hasattr(self.stub, method)):
      return {'error': f'there is no method named "{method}"'}
    try:
      data = self.methods[method](*args)
      return {'data': data}
    except Exception as e:
      return {'error': str(e)}
