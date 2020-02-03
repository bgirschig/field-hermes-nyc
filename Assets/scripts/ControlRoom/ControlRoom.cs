using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class SocketService : WebSocketBehavior {
    protected override void OnMessage (MessageEventArgs e) {
        Sessions.Broadcast(e.Data);
    }
}

public class ControlRoom : MonoBehaviour {
    private WebSocketServer wssv;

    public List<SwingStatus> swings;
    WebSocket ws;

    float value = 0;

    void Start() {
        // socket server
        wssv = new WebSocketServer("ws://0.0.0.0:4649");
        wssv.AddWebSocketService<SocketService>("/");
        wssv.Start();

        // Socket client
        ws = new WebSocket(string.Format("ws://localhost:4649/"));
        ws.OnMessage += (object sender, MessageEventArgs e) => {
            value += 1;
        };
        ws.Connect();
    }
 
    void Update() {
        Debug.Log(value);
        swings.ForEach(swing => {
            swing.swingPosition = Mathf.Sin(Time.time * 0.8f);
            swing.pathPosition = (swing.pathPosition + 0.001f) % 1;
        });
    }
 
    void OnApplicationQuit() {
        ws.Close();
        wssv.Stop();
    }
}
