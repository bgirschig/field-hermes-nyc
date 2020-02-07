using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using MessageProtos;
using UnityEngine.UI;

public class SocketService : WebSocketBehavior {
    protected override void OnMessage (MessageEventArgs e) {
        Sessions.Broadcast(e.Data);
    }
}

public class ControlRoom : MonoBehaviour {
    public HorizontalLayoutGroup swingGroup;

    private WebSocketServer wssv;
    private WebSocket ws;

    SwingStatus[] swings;
    private Queue<MessageProtos.SwingState> pendingMessages;

    void Start() {
        swings = swingGroup.GetComponentsInChildren<SwingStatus>();
        for (int i = 0; i < swings.Length; i++) swings[i].swing_id = i;
        
        pendingMessages = new Queue<SwingState>();

        // socket server
        wssv = new WebSocketServer("ws://0.0.0.0:4649");
        wssv.AddWebSocketService<SocketService>("/");
        wssv.Start();

        // Socket client
        ws = new WebSocket(string.Format("ws://localhost:4649/"));
        ws.OnMessage += (object sender, MessageEventArgs e) => {
            var message = JsonConvert.DeserializeObject<JObject>(e.Data);
            bool isValidMessage = (string)message["messageType"] == "SwingState";
            if (isValidMessage) pendingMessages.Enqueue(message.ToObject<SwingState>());
        };
        ws.Connect();
    }
 
    void Update() {
        if (Input.GetKeyDown(KeyCode.C)) SceneManager.LoadScene("main");
        while (pendingMessages.Count > 0) handleMessage(pendingMessages.Dequeue());
    }

    void handleMessage(SwingState message) {
        swings[message.swing_id].updateState(message);
    }
 
    public void send(string message) {
        ws.Send(message);
    }

    void OnApplicationQuit() {
        ws.Close();
        wssv.Stop();
    }

    void OnDestroy() {
        ws.Close();
        wssv.Stop();
    }
}