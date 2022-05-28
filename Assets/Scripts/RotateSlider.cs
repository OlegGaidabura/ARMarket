using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateSlider : MonoBehaviour
{

    private Slider slider;
    public float minValue;
    public float maxValue;
    public bool vertical;

    // Start is called before the first frame update
    void Start()
    {
        slider = GameObject.Find("RotateSlider").GetComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;

        slider.onValueChanged.AddListener(SliderUpdate);
    }

    void SliderUpdate(float value)
    {
        if (!vertical)
            transform.localEulerAngles = new Vector3(transform.rotation.x, value, transform.rotation.z);
        else
            transform.localEulerAngles = new Vector3(value, transform.rotation.y - 90, transform.rotation.z + 90);
    }
}
