using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Moves newly created letters into place in the drawer
/// </summary>
public class ReadyLetters : MonoBehaviour
{ 
    bool ready = false;

    public bool IsReady { get => ready; }
    public float letterSpeed, letterWait;
    
    public void ReadyUpLetters(LetterObject[] letters, RectTransform[] slots)
    {
        ready = false;
        StartCoroutine(ReadyUp(letters, slots));
    }

    IEnumerator ReadyUp (LetterObject[] letters, RectTransform[] slots)
    {
        int activeLetters = 1;
        float activeNext = Time.time + letterWait;
        if (letters.Length > 0)
        {
            while (!ready)
            {
                if (Time.time >= activeNext && activeLetters < letters.Length)
                {
                    activeNext = Time.time + letterWait;
                    activeLetters = activeLetters + 1;
                }
                bool complete = false;
                for (int i = 0, completed = 0; i < activeLetters; i++)
                {
                    Vector3 slotPosition = slots[i].position;
                    Vector3 letterPosition = letters[i].GetComponent<RectTransform>().position;
                    Vector3 dir = slotPosition - letterPosition;
                    //Vector3 dir = slots[i].position - letters[i].GetComponent<RectTransform>().position;
                    float move = letterSpeed * Time.deltaTime;
                    if (move > dir.magnitude)
                    {
                        move = dir.magnitude;
                        completed += 1;
                    }

                    letters[i].GetComponent<RectTransform>().position += dir.normalized * move;
                    if (completed == letters.Length)
                        complete = true;
                }
                if (complete)
                {
                    for (int i = 0; i < activeLetters; i++)
                    {
                        PlayManager.GetManager.UseSlot(letters[i], i);
                    }

                    ready = true;
                }
                yield return null;
            }
        } else
        {
            ready = true;
        }
    }
}
