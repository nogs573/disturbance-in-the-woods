using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutImage : MonoBehaviour
{
    public float fadeDuration = 2f;
    public float waitDuration = 2f;

    private Image image;
    private float fadeTimer;
    private float waitTimer;
    private bool isWaiting;
    private bool isFading;

    void Start()
    {
        image = GetComponent<Image>();
        fadeTimer = 0f;
        waitTimer = 0f;
        isWaiting = true;
        isFading = false;        
    }

    // Update is called once per frame
    void Update()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitDuration)
            {
                isWaiting = false;
                isFading = true;
            }
        }
        else if (isFading)
        {
            fadeTimer += Time.deltaTime;

            float alpha = 1f - Mathf.Clamp01(fadeTimer / fadeDuration);

            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;

            if (fadeTimer >= fadeDuration)
            {
                isFading = false;

                gameObject.SetActive(false);
            }
        }
    }
}
