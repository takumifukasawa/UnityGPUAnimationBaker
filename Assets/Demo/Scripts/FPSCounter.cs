using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Demo
{
    // ref base
    // https://sharpcoderblog.com/blog/unity-fps-counter
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _fpsText;

        /* Assign this script to any object in the Scene to display frames per second */

        public float updateInterval = 0.5f; //How often should the number update

        float accum = 0.0f;
        int frames = 0;
        float timeleft;
        float fps;

        // Use this for initialization
        void Start()
        {
            timeleft = updateInterval;
        }

        // Update is called once per frame
        void Update()
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            // Interval ended - update GUI text and start new interval
            if (timeleft <= 0.0)
            {
                // display two fractional digits (f2 format)
                fps = (accum / frames);
                timeleft = updateInterval;
                accum = 0.0f;
                frames = 0;
            }

            _fpsText.text = $"FPS:\n{Mathf.FloorToInt(fps).ToString()}";
        }
    }
}