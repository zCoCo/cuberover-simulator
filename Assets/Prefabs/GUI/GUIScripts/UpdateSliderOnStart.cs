/*
 * Trigger an update call (On Value Changed) to the slider on start
 * (eg. to update slider text automatically and set controlled value to the
 * default).
 *
 * Author: Connor W. Colombo (CMU)
 * Last Update: 6/2/2020, Colombo (CMU)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSliderOnStart : MonoBehaviour
{
    public bool update_on_start = true;

    private bool started = false; // whether the application has started running yet

    void Update()
    {
        // Do this on first Update instead of Start so everything (incl. the
        // value text field) has been initialzed
        if (!started && update_on_start)
        {
            GetComponent<Slider>().onValueChanged.Invoke(GetComponent<Slider>().value);
            started = true;
        }
    }
}
