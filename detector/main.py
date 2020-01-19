import os
import sys
import asyncio
import websockets
from socketStubServer import SocketStubServer
from detector import Detector
import json

script_path = os.path.abspath(sys.argv[0])
script_dir = os.path.dirname(script_path)
print('setting service working directory to:', script_dir)
os.chdir(script_dir)

stub = SocketStubServer(Detector())

print('starting detector server on localhost:8765')
start_server = websockets.serve(stub.main, "localhost", 8765)
asyncio.get_event_loop().run_until_complete(start_server)
asyncio.get_event_loop().run_forever()