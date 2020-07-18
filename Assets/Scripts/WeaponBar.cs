using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponBar : MonoBehaviour
{
    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Text timeText;

    [SerializeField]
    private int time = 5;

    private float timeRemaining;

    private void Start()
    {
        timeText.text = time.ToString();
        timeRemaining = time;
        slider.maxValue = time;
        slider.value = time;
    }

    // Update is called once per frame
    void Update()
    {
        var oldTime = timeRemaining;
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0) timeRemaining = time;

        if (Mathf.Floor(oldTime) != Mathf.Floor(timeRemaining)
            && timeRemaining != time) timeText.text = Mathf.Floor(oldTime).ToString();

        slider.value = timeRemaining;
    }
}
