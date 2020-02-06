﻿using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using MessageProtos;

public class SocketService : WebSocketBehavior {
    protected override void OnMessage (MessageEventArgs e) {
        Sessions.Broadcast(e.Data);
    }
}

public class ControlRoom : MonoBehaviour {
    private WebSocketServer wssv;
    private WebSocket ws;

    SwingStatus[] swings;
    private Queue<MessageProtos.SwingState> pendingMessages;

    void Start() {
        swings = FindObjectsOfType<SwingStatus>();
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
        swings[message.swing_id].pathPosition = message.pathPosition;
        swings[message.swing_id].swingPosition = message.swingPosition;
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