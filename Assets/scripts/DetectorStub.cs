/** 
 * Stub for the python swing detector, implemented using SocketStubClient
 * Defines all the script's public methods signatures, and implements some
 * conversions (eg. data-uri to unity textures)
*/

using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEngine;

public class DetectorStub {
  SocketStubClient stubClient;

  public DetectorStub(string host) {
    stubClient = new SocketStubClient(host);
  }

  public async Task<int[]> setCamera(string camera_id) {
    return await stubClient.call<int[]>("setCamera", camera_id);
  }

  public async Task<int[]> setCamera(int camera_id) {
    return await stubClient.call<int[]>("setCamera", camera_id);
  }

  public async Task<int[]> getShape() {
    return await stubClient.call<int[]>("getShape");
  }

  public async void setMask(Texture2D maskImage) {
    string dataUri = textureToDataUri(maskImage, "png");
    await stubClient.call<object>("setMask", dataUri);
  }

  public async Task<Texture2D> getMask() {
    Texture2D tex = new Texture2D(1,1);
    return await getMask(tex);
  }

  public async Task<Texture2D> getMask(Texture2D target) {
    string dataUri = await stubClient.call<string>("getMask");
    dataUriToTexture(dataUri, target);
    return target;
  }

  public async void setCrop(int minX, int maxX, int height, int y) {
    await stubClient.call<object>("setCrop", minX, maxX, height, y);
  }

  public async Task<Texture2D> getFrame() {
    string dataUri = await stubClient.call<string>("getFrameAsDataUrl");
    return dataUriToTexture(dataUri);
  }

  public async Task<float> detect() {
    string[] response = await stubClient.call<string[]>("detect");
    float value = float.Parse(response[0]);
    string debugImage = response[1];
    return value;
  }

  public async Task<float> detectWithDebug(Texture2D target) {
    string[] response = await stubClient.call<string[]>("detect", true);
    float value = float.Parse(response[0]);
    
    if (!target) target = new Texture2D(1,1);
    dataUriToTexture(response[1], target);
    return value;
  }
  public async Task<(float, Texture2D)> detectWithDebug() {
    Texture2D target = new Texture2D(1,1);
    float value = await detectWithDebug(target);
    return (value, target);
  }


  static string textureToDataUri(Texture2D tex, string type = "png") {
    byte[] bytes;

    switch (type) {
      case "jpeg":
        bytes = tex.EncodeToJPG();
        break;
      case "png":
      default:
        bytes = tex.EncodeToPNG();
        break;
    }
    
    string dataUri = "data:image/png;base64,";
    dataUri += System.Convert.ToBase64String(bytes);

    return dataUri;
  }

  static Texture2D dataUriToTexture(string dataUri) {
    Texture2D tex = new Texture2D(1,1);
    dataUriToTexture(dataUri, tex);
    return tex;
  }

  static void dataUriToTexture(string dataUri, Texture2D target) {
    GroupCollection groups = Regex.Match(dataUri, @"data:image/(?<type>\w+);base64,(?<data>.+)").Groups;
    string b64String = groups["data"].Value;

    byte[] decodedBytes = System.Convert.FromBase64String(b64String);

    target.LoadImage(decodedBytes);
  }
}