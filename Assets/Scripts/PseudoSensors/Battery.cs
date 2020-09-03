using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Battery : MonoBehaviour
{

    private float startTime;
    public Transform bar;
    public float battery = 100;
    public float rate = 10;
    public int seconds;
    private bool wait = false;
    public Text batteryData;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //battery drains 1% every (rate) seconds
        bar.localScale = new Vector3((battery / 100), 1f);
        if (rate == 1)//catch for decreasing 1 per frame
        {
            float t = Time.time - startTime;
            battery = 100- (int)t;
        }
        else
        {
            float t = Time.time - startTime;
            seconds = (int)t % (int)rate;
            seconds++;

            if (seconds == 1)
                wait = false;

            if (seconds == rate)
            {
                if (wait == false)
                    battery--;
                wait = true;
            }
        }
        if (battery < 0)
            battery = 0;

        batteryData.text = "Battery: " + battery + "%";
    }
}
