using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle: MonoBehaviour {

    public Light directionalLight;
    public float cycleLength;
    public AnimationCurve lightIntensityOverTime;

    void Start () {

    }

    void Update () {
        float percent = Mathf.PingPong (Time.time, cycleLength) / cycleLength;
        directionalLight.intensity = lightIntensityOverTime.Evaluate (percent);
    }

}
