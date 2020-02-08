using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MessageProtos {
  // Used to communicate with the detector
  // value and arrayValue are separate because I can't figure out how to parse tvarying data types
  // with newtonsofts JSON lib
  public class DetectorMessage {
    public string type;
    public string action;
    public string value;
    public string[] arrayValue;
    public float timeout;
  }

  // used for sending "actions" to the detector
  public class DetectorAction<T> {
    public string type = "action";
    public string action;
    public T value;
    public float timeout;
  }
}