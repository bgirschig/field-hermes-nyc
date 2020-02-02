/*
 * Allows calling methods on any program that implements the SocketStubServer
 * logic. Can be used eg. for "calling" python methods inside from a unity project
 * The important method here is
 * 'call<T>(string methodname, * arg1, * arg2, ...)'
 * where T is the expected return type of the method, methodname is the name of
 * the method, and arg1, arg2, etc... are the method's arguments
 */

using WebSocketSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

public class SocketStubClient {
  string host;
  public WebSocket ws;
  Dictionary<int, TaskCompletionSource<JObject>> tasks;
  DateTime lastConnectionAttempt;

  public SocketStubClient(string host) {
    // Connect to the socket server
    // TODO [STABILITY] Auto-reconnect (check out https://github.com/Marfusios/websocket-client)
    this.host = host;
    ws = new WebSocket(string.Format("ws://{0}", host));
    ws.OnMessage += handleMessage;

    // Initialize the dictionnary that will hold the pending tasks
    tasks = new Dictionary<int, TaskCompletionSource<JObject>>();

    lastConnectionAttempt = DateTime.Now;
    // Start up
    ws.Connect();
  }

  void handleMessage(object sender, MessageEventArgs e) {
    JObject response = JsonConvert.DeserializeObject<JObject>(e.Data);
    tasks[(int)response["id"]].SetResult(response);
  }

  public async Task<T> call<T>(string methodName, params object[] args) {
    TaskCompletionSource<JObject> tcs = new TaskCompletionSource<JObject>();

    SocketStubRequest request = new SocketStubRequest();
    request.type = CallType.SocketStubCall;
    request.method = methodName;
    request.args = args;

    string s_request = JsonConvert.SerializeObject(request);

    tasks.Add(request.id, tcs);

    if (
      ws.ReadyState != WebSocketSharp.WebSocketState.Open &&
      ws.ReadyState != WebSocketSharp.WebSocketState.Connecting &&
      (DateTime.Now - lastConnectionAttempt).TotalSeconds > 1) {
        lastConnectionAttempt = DateTime.Now;
        ws.Connect();
    }
    if (ws.ReadyState != WebSocketSharp.WebSocketState.Open) throw new StubException("socket not connected (yet)");
    ws.Send(s_request);
    
    JObject json_response = await tcs.Task;
    SocketStubResponse<T> response = json_response.ToObject<SocketStubResponse<T>>();
    
    return response.data;
  }
}

enum CallType {
  SocketStubCall, SocketStubDoc
};

class SocketStubRequest {
  static int currentID = 0;
  
  public int id;
  [JsonConverter(typeof(StringEnumConverter))]
  public CallType type;
  public string method;
  public object[] args;

  public SocketStubRequest() {
    id = currentID;
    currentID += 1;
  }
}

class SocketStubResponse<T> {
  public int id;
  public int time;
  public string type;
  public T data;
}

public class StubException : Exception
{
    public StubException() {
    }

    public StubException(string message) : base(message){
    }

    public StubException(string message, Exception inner) : base(message, inner) {
    }
}
