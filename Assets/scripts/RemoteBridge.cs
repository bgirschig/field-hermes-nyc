using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using MessageProtos;

public class RemoteBridge : MonoBehaviour
{
    private int port = 4649;
    private float update_rate = 10;

    private string _socketHost = "localhost";
    private WebSocket ws;
    
    private DetectorClient detectorClient;
    private AnimationSwingController swingController;
    private Eureka eurekaController;
    private shootingStarSpawn shootingStarController;

    private string prevUrl;
    private float nextReconnectTime = 0;
    private float nextUpdateTime = 0;

    private Queue<SwingControl> pendingMessages;

    public string socketHost{
        set {
            _socketHost = value;
            tryConnect();
        }
        get { return _socketHost; }
    }
    public int ID = 0;

    void Start() {
        pendingMessages = new Queue<SwingControl>();
        swingController = GameObject.FindObjectOfType<AnimationSwingController>();
        detectorClient = GameObject.FindObjectOfType<DetectorClient>();
        eurekaController = GameObject.FindObjectOfType<Eureka>();
        shootingStarController = GameObject.FindObjectOfType<shootingStarSpawn>();
    }

    // Update is called once per frame
    void Update() {
        if (ws == null) {
            tryConnect();
        } else if (ws.ReadyState == WebSocketSharp.WebSocketState.Open) {
            sendUpdateState();
        } else {
            tryConnect();
        }

        while (pendingMessages.Count > 0) HandleMessage(pendingMessages.Dequeue());
    }

    void sendUpdateState() {
        if (Time.time < nextUpdateTime) return;
        var data = new SwingState();
        data.swing_id = ID;
        data.pathPosition = (float)(swingController.director.time / swingController.director.duration);
        data.swingPosition = detectorClient.normalized_position;
        ws.Send(JsonConvert.SerializeObject(data));
        nextUpdateTime = Time.time + 1 / update_rate;
    }

    void tryConnect() {
        string url = string.Format("ws://{0}:{1}", socketHost, port);

        if (Time.time < nextReconnectTime) return;
        if (ws != null && ws.ReadyState == WebSocketState.Closing) return;
        if (ws != null && prevUrl == url && ws.ReadyState == WebSocketState.Open) return;
        if (ws != null && prevUrl == url && ws.ReadyState == WebSocketState.Connecting) return;

        prevUrl = url;

        if (ws != null) ws.Close();

        Debug.Log(string.Format("connecting to {0}", url));
        ws = new WebSocket(url);
        ws.OnMessage += (object sender, MessageEventArgs e) => {
            Debug.Log(e.Data);
            var message = JsonConvert.DeserializeObject<JObject>(e.Data);
            bool isValidMessage = (string)message["messageType"] == "SwingControl";
            if (isValidMessage) pendingMessages.Enqueue(message.ToObject<SwingControl>());
        };
        ws.Connect();
        nextReconnectTime = Time.time + 2.0f;
    }

    void HandleMessage(SwingControl message) {
        if (message.swing_id != ID) return;
        
        if (message.action == "ShootingStar") shootingStarController.spawn();
        else if (message.action == "eureka") eurekaController.toggle();
        else Debug.Log("Action not found");
    }
}
