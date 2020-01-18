using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

// TODO [STABILITY] https://github.com/Marfusios/websocket-client

public class DetectorClient : MonoBehaviour
{
    WebSocket ws = new WebSocket("ws://localhost:8080");
    // Start is called before the first frame update
    void Start() {
        ws.OnMessage += handleMessage;
        ws.Connect();
    }

    // Update is called once per frame
    void Update() {
        if (Time.frameCount % 30 == 0) ws.Send("hi there "+Time.frameCount);
    }

    void handleMessage(object sender, MessageEventArgs e) {
        Debug.Log(e.Data);
    }
}
