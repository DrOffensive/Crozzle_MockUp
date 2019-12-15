using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Placer score UI text animation, counts up to set amount over time 
/// </summary>
[RequireComponent(typeof(UnityEngine.UI.Text))]
public class UI_ScoreText : MonoBehaviour
{
    UnityEngine.UI.Text label;
    int score, labelScore;
    public float countInterval = 0.1f;
    float next = 0;

    private void Start()
    {
        label = GetComponent<UnityEngine.UI.Text>();
    }

    public void SetScore (int amount)
    {
        score = amount;
    }


    private void Update()
    {
        if(score > labelScore)
        {
            if(Time.time >= next)
            {
                labelScore++;
                UpdateLabelScore();
                next = Time.time + countInterval;
            }
        }
    }

    void UpdateLabelScore ()
    {
        label.text = labelScore.ToString();
    }
}
