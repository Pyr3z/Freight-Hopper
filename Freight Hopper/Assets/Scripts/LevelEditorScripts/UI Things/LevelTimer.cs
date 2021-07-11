using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Source: https://www.youtube.com/watch?v=x-C95TuQtf0
public class LevelTimer : MonoBehaviour
{
    public Text textTimer;
    private float startTime;
    
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float actualTime = Time.time - startTime;

        string minutes = ((int) actualTime / 60).ToString();
        string seconds = (actualTime % 60).ToString("f2");

        textTimer.text = minutes + ":" + seconds;
    }
}