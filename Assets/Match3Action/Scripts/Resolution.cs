using UnityEngine;
using System.Collections;

public class Resolution : MonoBehaviour {

	void Start () {
        //Application.targetFrameRate = 60;
        Screen.SetResolution(480, 800, false);
        Destroy(this);
    }
	
}
