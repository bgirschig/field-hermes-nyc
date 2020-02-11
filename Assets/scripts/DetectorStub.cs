using System;
using System.Collections.Generic;
using WebSocketSharp;
using MessageProtos;
using Newtonsoft.Json;
using UnityEngine;

public class OnValueEvent {
  public float value;
  public float time;
}

public class DetectorStub {
  WebSocket ws = null;
  string host;

  float delayBetweenReconnects = 1f;
  // After trying to connect a few times unsuccessfuly (10 times), websocketSharp throws an error
  // on every attempt and "gives up" connecting. to avoid that, we create a new websocket instance
  // when we reach that treshold
  int reconnecCount = 0; // number of connection attempts on the current websocket instance
  int maxReconnectOnInstance = 10; // empirically chosen number

  Queue<DetectorMessage> pendingMessages;
  Queue<DetectorMessage> pendingActions;
  DateTime lastConnectionAttempt;

  public event EventHandler<OnValueEvent> onValue;

  public DetectorStub(string host="localhost:9000") {
    this.host = host;
    pendingMessages = new Queue<DetectorMessage>();
    tryConnect();
  }

  // to be called periodically
  public void update() {
    if (ws.ReadyState == WebSocketSharp.WebSocketState.Closed) tryConnect();

    float? latestValue = null;
    float? latestTime = null;
    while (pendingMessages.Count > 0) {
      DetectorMessage message = pendingMessages.Dequeue();
      switch (message.type) {
          case "detectorValue":
            float time = float.Parse(message.arrayValue[1]);
            if (latestTime == null || latestTime < time) {
              latestValue = float.Parse(message.arrayValue[0]);
              latestTime = float.Parse(message.arrayValue[1]);
            }
            break;
          default:
            break;
      }
    }

    if (latestValue != null) {
      OnValueEvent evt = new OnValueEvent();
      evt.time = (float)latestTime;
      evt.value = (float)latestValue;
      onValue?.Invoke(this, evt);
    }
  }

  // Try connecting to the detector server, and setup event listeners
  // handles creating and renewing websocket instances when needed
  public void tryConnect() {
    if ((DateTime.Now - lastConnectionAttempt).TotalSeconds < delayBetweenReconnects) return;
    lastConnectionAttempt = DateTime.Now;

    if (ws != null) ws.Close();
    if (ws == null || reconnecCount >= maxReconnectOnInstance) {
      Debug.Log("cerating a new websocket instance");
      ws = new WebSocket(string.Format("ws://{0}", host));
      reconnecCount = 0;
    
      ws.OnOpen += (sender, e) => {
        Debug.Log("connected to detector server");
      };
      ws.OnMessage += (sender, e) => {
        try {
          var message = JsonConvert.DeserializeObject<DetectorMessage>(e.Data);
          pendingMessages.Enqueue(message);  
        } catch (JsonReaderException) {
          Debug.Log(e.Data);
          throw;
        }
      };
      ws.OnError += (sender, e) => {
        Debug.LogException(e.Exception);
      };
      ws.OnClose += (sender, e) => {
      };
    }

    ws.Connect();
    reconnecCount += 1;
  }

  public void sendAction<T>(string actionName, T value) {
    if (ws.ReadyState != WebSocketState.Open) return;

    var message = new DetectorAction<T>();
    message.value = value;
    message.action = actionName;

    ws.Send(JsonConvert.SerializeObject(message));
  }

  public void destroy() {
    ws.Close();
  }
}
