// used by the control room to send commands "down" to a swing instance
namespace MessageProtos {
  class SwingControl {
    public string messageType = "SwingControl";
    public int swing_id;
    public string action;
  }
}