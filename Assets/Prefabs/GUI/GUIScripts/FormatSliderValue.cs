using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormatSliderValue : MonoBehaviour
{
    public float value; // Core Value in slider
    public string format = "F2";
    public string units = ""; // String for units (if applicable)

    private Text label;

    // Start is called before the first frame update
    void Start()
    {
        label = GetComponent<Text>();
    }

    public void OnSliderUpdate(float newVal)
    {
        label.text = string.Format("{0}{1}{2}", newVal < 0 ? "-" : "+", newVal.ToString(format), units);
    }
}
