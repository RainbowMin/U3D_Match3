using UnityEngine;
using System.Collections;

public class ClickMessage : MonoBehaviour {
    public GameObject target;
    public string message = "OnClick";

    public void OnMouseDown()
    {
        if (target) target.SendMessage(message, SendMessageOptions.DontRequireReceiver);
    }
}
