using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Letter tile object. Handles:
/// - Scaling
/// - Snapping
/// - Dragging
/// </summary>
public class LetterObject : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    UnityEngine.UI.Text label;
    char letter;

    int homeSlot;
    public int SetHomeSlot { set => homeSlot = value; }
    public Color normalColor = Color.white, gridLockedColor, gridLockedColorDouble;
    public TileState tileState = TileState.Home;
    public float scaleSpeed, floatSpeed, snapSpeed, tintSpeed;
    public float letterSnapDistance = 40, buttomSnapDistance = -550;
    bool dragging = false, locked;
    bool scaleComplete = false;
    public char Letter
    {
        set
        {
            letter = value;
            label.text = value.ToString().ToUpper();
        }
        get => letter;
    }
    public enum TileState
    {
        Home, Floating, GridLocked
    }

    public float homeScale = 1.08f, floatingScale = 1.15f, gridLockedScale = .8f;

    public bool allowMove = false;

    public void OnPointerUp(PointerEventData eventData)
    {
        if (allowMove && dragging)
        {
            TileInfo closestTile = PlayManager.GetManager.GetComponent<Build_Grid>().GetClosestTile(transform.position);
            Vector2 distanceToTile = closestTile.Cell.position - transform.position;
            bool moveHome = false;
            if ((closestTile is LetterTile && distanceToTile.magnitude <= letterSnapDistance))
            {
                LetterObject[] children = closestTile.Cell.GetComponentsInChildren<LetterObject>();

                if (transform.localPosition.y > buttomSnapDistance && children.Length == 0)
                {
                    if (!closestTile.Cell.GetComponent<Tile>().Set)
                    {
                        StartCoroutine(MoveToTile(closestTile));

                        if (((LetterTile)closestTile).isDouble)
                            StartCoroutine(Tint(gridLockedColorDouble));
                        else
                            StartCoroutine(Tint(gridLockedColor));

                        //StartCoroutine(Scale(TileState.Floating, TileState.GridLocked));
                    }
                    else
                    {
                        moveHome = true;
                    }
                }
                else
                {
                    moveHome = true;
                }
            }
            else
                moveHome = true;

            if(moveHome)
            {
                StartCoroutine(MoveHome());
                StartCoroutine(Tint(normalColor));
                StartCoroutine(Scale(TileState.Floating, TileState.Home));
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (allowMove)
        {
            if(tileState==TileState.Home)
            {
                PlayManager.GetManager.EmptySlot(homeSlot);
            }
            tileState = TileState.Floating;
            dragging = true;
            StartCoroutine(Tint(normalColor));
            StartCoroutine(Scale(TileState.Home, TileState.Floating));
        }
    }

    private void Update()
    {
        if (dragging)
        {
            transform.parent = PlayManager.GetManager.activeTilePanel;
            MoveToMouse();
        }
    }

    void MoveToMouse()
    {
        Vector2 direction = Input.mousePosition - GetComponent<RectTransform>().position;
        Vector2 move = direction.normalized * floatSpeed * Time.deltaTime;
        if (move.magnitude > direction.magnitude)
        {
            move = direction;
        }
        GetComponent<RectTransform>().position += new Vector3(move.x, move.y, 0);
    }

    public IEnumerator MoveHome ()
    {
        homeSlot = PlayManager.GetManager.GetFirstEmptySlotIndex();
        RectTransform slot = PlayManager.GetManager.GetSlot(homeSlot);
        allowMove = false;
        dragging = false;
        bool home = false;
        while (!home)
        {
            Vector2 direction = slot.position - GetComponent<RectTransform>().position;
            Vector2 move = direction.normalized * snapSpeed * Time.deltaTime;
            if (move.magnitude > direction.magnitude)
            {
                move = direction;
                home = true;
            }
            GetComponent<RectTransform>().position += new Vector3(move.x, move.y, 0);
            yield return null;
        }
        tileState = TileState.Home;
        PlayManager.GetManager.UseSlot(this, homeSlot);
        if(PlayManager.GetManager.turn == PlayManager.Turn.Player)
            allowMove = true;
    }
    public IEnumerator MoveToSlot(int slotIndex)
    {
        homeSlot = slotIndex;
        RectTransform slot = PlayManager.GetManager.GetSlot(homeSlot);
        allowMove = false;
        dragging = false;
        bool home = false;
        while (!home)
        {
            Vector2 direction = slot.position - GetComponent<RectTransform>().position;
            Vector2 move = direction.normalized * snapSpeed * Time.deltaTime;
            if (move.magnitude > direction.magnitude)
            {
                move = direction;
                home = true;
            }
            GetComponent<RectTransform>().position += new Vector3(move.x, move.y, 0);
            yield return null;
        }
        tileState = TileState.Home;
        PlayManager.GetManager.UseSlot(this, homeSlot);
        if (PlayManager.GetManager.turn == PlayManager.Turn.Player)
            allowMove = true;
    }

    IEnumerator MoveToTile(TileInfo tile)
    {
        allowMove = false;
        dragging = false;
        bool home = false;
        while (!home)
        {
            Vector2 direction = tile.Cell.position - GetComponent<RectTransform>().position;
            Vector2 move = direction.normalized * snapSpeed * Time.deltaTime;
            if (move.magnitude > direction.magnitude)
            {
                move = direction;
                home = true;
            }
            GetComponent<RectTransform>().position += new Vector3(move.x, move.y, 0);
            yield return null;
        }
        scaleComplete = false;
        StartCoroutine(Scale(TileState.Floating, TileState.GridLocked));
        while(!scaleComplete)
        {
            yield return null;
        }
        tileState = TileState.GridLocked;
        transform.parent = tile.Cell;        
        allowMove = true;
    }

    public float StateToScale(TileState state)
    {
        switch (state)
        {
            case TileState.Home: return homeScale;
            case TileState.Floating: return floatingScale;
            case TileState.GridLocked: return gridLockedScale;
            default: return homeScale;
        }
    }

    public IEnumerator Scale(TileState from, TileState to)
    {
        float fromScale = StateToScale(from);
        float toScale = StateToScale(to);
        transform.localScale = Vector3.one * fromScale;
        float difference = Mathf.Abs(fromScale - toScale);
        bool up = toScale > fromScale;
        float scaled = 0;
        while (scaled < 1f)
        {
            if (up)
            {
                scaled += Time.deltaTime * scaleSpeed;
                scaled = Mathf.Clamp01(scaled);
                transform.localScale = Vector3.one * (fromScale + ((difference) * scaled));
            }
            else
            {
                scaled += Time.deltaTime * scaleSpeed;
                scaled = Mathf.Clamp01(scaled);
                transform.localScale = Vector3.one * (fromScale - ((difference) * scaled));
            }

            yield return null;
        }
        transform.localScale = Vector3.one * toScale;
        scaleComplete = true;
    }

    public IEnumerator Tint (Color toColor)
    {
        UnityEngine.UI.Image image = GetComponent<UnityEngine.UI.Image>();
        Color startColor = image.color;
        float tinted = 0;
        while(tinted < 1f)
        {
            tinted = Mathf.Clamp01(tinted + (tintSpeed * Time.deltaTime));
            image.color = Color.Lerp(startColor, toColor, tinted);
            yield return null;
        }
    }
}
