// used by a swing instance to send its status "up" to the control room
namespace MessageProtos {
  class SwingState {
    public string messageType = "SwingState";
    public int swing_id;
    public float swingPosition;
    public float pathPosition;
  }
}