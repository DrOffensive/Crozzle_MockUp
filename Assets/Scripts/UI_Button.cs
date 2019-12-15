using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// Simple click/touch button
/// </summary>
public class UI_Button : MonoBehaviour, IPointerClickHandler
{

    [SerializeField]
    bool clicked = false;
    public bool Clicked { get { bool c = clicked; clicked = false; return c; } }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            clicked = true;
        }
    }
}
