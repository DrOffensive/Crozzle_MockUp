using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animated word ans letter scores, self destructible after animation ends.
/// </summary>
public class UI_ScoreLabel : MonoBehaviour
{
    [SerializeField]
    UnityEngine.UI.Text label;
    public int SetLabel { set { label.text = "+" + value; } }

    public float floatTime, scaleTime, waitTime, upSpeedMod;
    public AnimationCurve upSpeedOverTime, alphaOverTime, scaleOverTime;


    void Start()
    {
        StartCoroutine(Scale());
    }

    IEnumerator Scale()
    {
        float t = 0;
        float delta = 1f / scaleTime;

        while (t < 1f)
        {
            t = Mathf.Clamp01(t + (delta * Time.deltaTime));
            transform.localScale = Vector3.one * scaleOverTime.Evaluate(t);
            yield return null;
        }
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(Float());
    }


    IEnumerator Float ()
    {
        float t = 0;
        float delta = 1f / floatTime;
        while (t < 1f)
        {
            t = Mathf.Clamp01(t + (delta * Time.deltaTime));

            label.color = new Color(label.color.r,label.color.g, label.color.b, alphaOverTime.Evaluate(t));
            transform.position += Vector3.up * (upSpeedOverTime.Evaluate(t) * upSpeedMod * Time.deltaTime); 
            yield return null;
        }

        Destroy(gameObject);
    }
}
