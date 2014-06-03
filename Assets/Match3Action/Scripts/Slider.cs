using UnityEngine;
using System.Collections;

public class Slider : MonoBehaviour {
    public Transform bar;
    float _sliderValue = 1f;
    Vector3 savedScale;

    public float sliderValue
    {
        get {
            return _sliderValue;
        }
        set {
            _sliderValue = Mathf.Clamp(value, 0f, 1f);
            if (bar) bar.transform.localScale = new Vector3(savedScale.x * _sliderValue, savedScale.y, savedScale.z);
        }
    }

	void Start () {
        if (bar) savedScale = bar.localScale;
	}
}
