using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tile data for tiles in the grid
/// </summary>
public class Tile : MonoBehaviour
{
    Vector2Int index;
    public UnityEngine.UI.Text letterText;
    bool set;


    public Vector2Int Index { get => index; set => index = value; }
    public bool Set { get => set; set { set = value; letterText.gameObject.SetActive(value); } }
}
