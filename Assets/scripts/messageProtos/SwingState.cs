// used by a swing instance to send its status "up" to the control room
namespace MessageProtos {
  public class SwingState {
    public string messageType = "SwingState";
    public float fps;
    public int swing_id;
    public float swingPosition;
    public float swingSpeed;
    public float pathPosition;
  }
}