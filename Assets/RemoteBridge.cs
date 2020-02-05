using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;

public class RemoteBridge : MonoBehaviour
{
    private int port = 4649;
    private string _socketHost;
    private WebSocket ws;
    private DetectorClient detectorClient;
    private AnimationSwingController swingController;
    private string prevUrl;
    private float nextReconnectTime = 0;

    public string socketHost{
        set {
            _socketHost = value;
            connect();
        }
        get { return _socketHost; }
    }
    public int ID = 0;

    void Start() {
        swingController = GameObject.FindObjectOfType<AnimationSwingController>();
        detectorClient = GameObject.FindObjectOfType<DetectorClient>();
    }

    // Update is called once per frame
    void Update() {
        if (ws.ReadyState == WebSocketSharp.WebSocketState.Open) {
            var data = new SocketMessage();
            data.swing_id = ID;
            data.pathPosition = (float)(swingController.director.time / swingController.director.duration);
            data.swingPosition = detectorClient.normalized_position;
            Debug.Log(data.swingPosition);
            ws.Send(JsonConvert.SerializeObject(data));
        } else if (ws.ReadyState == WebSocketState.Connecting || ws.ReadyState == WebSocketState.Closing) {
            // pass
        } else {
            connect();
        }
    }

    void connect() {
        if (Time.time < nextReconnectTime) return;
        string url = string.Format("ws://{0}:{1}", socketHost, port);
        if (prevUrl == url && (ws.ReadyState == WebSocketState.Open || ws.ReadyState == WebSocketState.Connecting)) {
            return;
        }
        prevUrl = url;

        if (ws != null) ws.Close();
        
        Debug.Log(string.Format("connecting to {0}", url));
        ws = new WebSocket(url);
        ws.Connect();
        nextReconnectTime = Time.time + 2.0f;
    }
}
