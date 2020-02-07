export default class SocketStubClient {
  constructor(socket) {
    if (socket instanceof WebSocket) this.socket = socket;
    else this.socket = new WebSocket(socket);

    this.currentId = 0;
    this.callbacks = {};
    this.readyPromise = new Promise(resolve => this.onReady = resolve);
    this.ready = false;

    this.socket.addEventListener('open', this.onOpen.bind(this));
    this.socket.addEventListener('close', this.onClose.bind(this));
    this.socket.addEventListener('error', this.onError.bind(this));
    this.socket.addEventListener('message', this.onMessage.bind(this));
  }

  async onOpen() {
    this.docs = await this._call({type: 'SocketStubDoc'});
    this.methods = Object.keys(this.docs);

    this.ready = true;
    this.onReady();
  }

  onClose(event) {
    this.ready = false;
    console.log('socket stub closed', event);
  }

  onError(event) {
    console.error(event);
  }

  onMessage(event) {
    const data = JSON.parse(event.data);
    if (data.type === 'SocketStubResponse' && this.callbacks[data.id]) {
      this.callbacks[data.id](data);
      delete this.callbacks[data.id];
    }
  }

  call(method, ...args) {
    return this._call({method, args, type: 'SocketStubCall'});
  }

  _call({method, type, args}) {
    const id = this.currentId;
    this.currentId += 1;
    this.socket.send(JSON.stringify({
      id,
      type: type,
      time: Date.now(),
      method,
      args,
    }));
    return new Promise((resolve, reject) => {
      this.callbacks[id] = (response) => {
        if (response.error) reject(response.error);
        else resolve(response.data);
      };
    });
  }
}
