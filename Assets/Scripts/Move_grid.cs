using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move_grid : MonoBehaviour
{
    float gridStart;
    public float playPosition;
    public float speed;
    public AnimationCurve speedOverTime;
    bool ready = false, down = true;
    float distance;
    public bool IsReady { get => ready;  }
    public bool IsDown { get => down; }

    RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        gridStart = rect.localPosition.y;
        distance = Mathf.Abs(playPosition - gridStart);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Moves the grid up or down, based on where it currently is
    /// </summary>
    public void Move ()
    {
        if (ready)
            StartCoroutine(Move(false));
        else if (IsDown)
            StartCoroutine(Move(true));

    }

    IEnumerator Move (bool up)
    {
        ready = false;
        down = false;
        float traveled = 0;
        while (traveled < distance)
        {
            float t = 1f / distance * traveled;

            float move = (speedOverTime.Evaluate(t) * speed) * Time.deltaTime;
            if (move > distance - traveled)
                move = distance - traveled;
            rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y + (up ? move : (-move)), 0f);
            traveled += move;
            yield return null;
        }
        if (up)
            ready = true;
        else
            down = true;
    }
}
