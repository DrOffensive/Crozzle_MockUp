using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Text script that can:
/// - Drop a shadow
/// - Change face & shadow color
/// - Blink between 2 colors
/// </summary>

[RequireComponent(typeof(Text))]
public class Text_Shadow : MonoBehaviour
{
    public Text faceText;

    bool blinking = false;
    public bool IsBlinking { get => blinking; }


    public string SetText
    {
        set
        {
            GetComponent<Text>().text = value;
            faceText.text = value;
        }
    }

    public Color SetFaceColor
    {
        set => faceText.color = value;        
    }

    public Color SetShadowColor
    {
        set => GetComponent<Text>().color = value;
    }

    /// <summary>
    /// Blinks from the current color to $color at $interval in seconds
    /// </summary>
    /// <param name="color"></param>
    /// <param name="interval"></param>
    public void StartBlinking (Color color, float interval)
    {
        StartCoroutine(ColorBlink(color, interval));
    }

    public void StopBlinking ()
    {
        blinking = false;
    }

    IEnumerator ColorBlink (Color blinkColor, float blinkInterval)
    {
        Color defaultColor = faceText.color;
        blinking = true;
        float next = Time.time + blinkInterval;
        bool blink = false;
        while (blinking)
        {
            if(Time.time > next)
            {
                faceText.color = !blink ? blinkColor : defaultColor;
                blink = !blink;
                next = Time.time + blinkInterval;
            }

            yield return null;
        }
        faceText.color = defaultColor;
    }
}
