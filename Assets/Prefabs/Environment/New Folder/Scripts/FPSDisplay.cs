using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BinaryLunar
{
    public class FPSDisplay : MonoBehaviour
    {
        public TextMeshProUGUI fpsText;
        private float deltaTime = 0.0f;

        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsText.text = string.Format("{0:0.} fps", fps);
        }
    }
}

