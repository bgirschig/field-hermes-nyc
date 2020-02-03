using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;

public class SocketService : WebSocketBehavior {
    protected override void OnMessage (MessageEventArgs e) {
        Sessions.Broadcast(e.Data);
    }
}
struct SwingStatusData {
    public float swingPosition;
    public float pathPosition;
    public bool dirty;
}
struct SocketMessage {
    public int swing_id;
    public float swingPosition;
    public float pathPosition;
}

public class ControlRoom : MonoBehaviour {
    private WebSocketServer wssv;
    private WebSocket ws;

    public List<SwingStatus> swings;

    SwingStatusData[] swingStatusDatas;
    float value = 0;

    void Start() {
        swingStatusDatas = new SwingStatusData[swings.Count];

        // socket server
        wssv = new WebSocketServer("ws://0.0.0.0:4649");
        wssv.AddWebSocketService<SocketService>("/");
        wssv.Start();

        // Socket client
        ws = new WebSocket(string.Format("ws://localhost:4649/"));
        ws.OnMessage += (object sender, MessageEventArgs e) => {
            SocketMessage payload = JsonConvert.DeserializeObject<SocketMessage>(e.Data);
            swingStatusDatas[payload.swing_id].swingPosition = payload.swingPosition;
            swingStatusDatas[payload.swing_id].pathPosition = payload.pathPosition;
            swingStatusDatas[payload.swing_id].dirty = true;
        };
        ws.Connect();
    }
 
    void Update() {
        for (int i = 0; i < swingStatusDatas.Length; i++) {
            if (!swingStatusDatas[i].dirty) continue;
            swings[i].pathPosition = swingStatusDatas[i].pathPosition;
            swings[i].swingPosition = swingStatusDatas[i].swingPosition;
            swingStatusDatas[i].dirty = false;            
        }
    }
 
    void OnApplicationQuit() {
        ws.Close();
        wssv.Stop();
    }
}