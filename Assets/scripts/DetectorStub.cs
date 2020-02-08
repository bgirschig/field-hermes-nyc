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
  WebSocket ws;
  string host;

  Queue<DetectorMessage> pendingMessages;
  Queue<DetectorMessage> pendingActions;
  DateTime lastConnectionAttempt;

  public event EventHandler<OnValueEvent> onValue;

  public DetectorStub(string host="localhost:9000") {
    this.host = host;

    pendingMessages = new Queue<DetectorMessage>();
    ws = new WebSocket(string.Format("ws://{0}", host));

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

    Debug.Log(string.Format("detectro stub conneting to {0}", ws.Url));
    ws.Connect();
  }

  // to be called periodically
  public void update() {
    if (
      ws.ReadyState == WebSocketSharp.WebSocketState.Closed &&
      (DateTime.Now - lastConnectionAttempt).TotalSeconds > 2) {
        lastConnectionAttempt = DateTime.Now;
        ws.Connect();
    }

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
