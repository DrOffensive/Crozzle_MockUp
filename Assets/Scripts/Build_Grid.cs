using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Builds and maintains the grid
/// </summary>
public class Build_Grid : MonoBehaviour
{
    public Move_grid grid;
    public Vector2Int gridSize = new Vector2Int(8, 12);
    public float cellSize = 80f;
    public RectTransform cellPrefab;
    public RectTransform framesParent, cellsParent;
    public Sprite frameSprite;

    public Color doubleTileColor = Color.blue;

    public int doubleTiles = 3;

    public List<TileBuilder> tileSetup;

    public List<TileInfo> tiles;

    bool built;
    public bool IsBuilt { get => built; }

    [System.Serializable]
    public struct TileBuilder
    {
        public char letter;
        public Sprite sprite;
        public TileType type;
    }

    void Start()
    {
        Build();
        SetDoubleTiles();
        GetComponent<WordManager>().ShuffleLetters();
    }

    public TileInfo GetClosestTile (Vector2 point)
    {
        float distance = Mathf.Infinity;
        int selection = -1;
        for(int i = 0; i < tiles.Count; i++)
        {
            Vector2 dist = point - (Vector2)tiles[i].Cell.position;
            if(dist.magnitude < distance)
            {
                selection = i;
                distance = dist.magnitude;
            }
        }

        return tiles[selection];
    }

    public List<LetterTile> GetAllUnsetTiles ()
    {
        List<LetterTile> unsetTiles = new List<LetterTile>();
        foreach (TileInfo tile in tiles)
        {
            if (tile is LetterTile && !tile.Cell.GetComponent<Tile>().Set)
                tiles.Add((LetterTile)tile);
        }
        return unsetTiles;
    }

    public List<LetterTile> GetAllUnsetTilesOfType (char type)
    {
        List<LetterTile> tilesOfType = new List<LetterTile>();
        foreach(TileInfo tile in tiles)
        {
            if (tile is LetterTile && ((LetterTile)tile).letter == type && !tile.Cell.GetComponent<Tile>().Set)
                tilesOfType.Add((LetterTile)tile);
        }
        return tilesOfType;
    }

    public TileInfo GetTile (Vector2Int index)
    {
        int i = (index.y * gridSize.x) + index.x;
        return tiles[i];
    }

    void Build ()
    {
        tiles = new List<TileInfo>();
        Vector2 offset = new Vector2((((gridSize.x -1) * cellSize) / 2) * -1, (((gridSize.y - 1) * cellSize) / 2));
        for(int i = 0; i < gridSize.x * gridSize.y; i++)
        {
            int y = (int)(i/gridSize.x);
            int x = i - (y * gridSize.x);

            TileBuilder builder = tileSetup[i];
            Vector2 position = offset + new Vector2(x * cellSize, (y * cellSize) * -1);

            //Creating the Tile frame
            Image frame = Instantiate(cellPrefab, framesParent).GetComponent<Image>();
            frame.raycastTarget = false;
            frame.sprite = frameSprite;
            frame.rectTransform.localPosition = position;
            Destroy(frame.gameObject.GetComponent<Tile>().letterText);
            Destroy(frame.gameObject.GetComponent<Tile>());

            //Creating the tile
            Image cell = Instantiate(cellPrefab, cellsParent).GetComponent<Image>();
            cell.gameObject.name = "Tile - " + x + ", " + y + " - " + (builder.type == TileType.Helper ? "Question" : ""+builder.letter);
            cell.rectTransform.localPosition = position;

            if(builder.type == TileType.Helper)
            {
                //If it is a helper tile, remove all the letter data from the tile and set the proper sprite 
                cell.gameObject.GetComponent<Tile>().letterText.text ="";
                Destroy(cell.gameObject.GetComponent<Tile>());
                cell.sprite = builder.sprite;
            } else
            {
                //Else set the appropriate letter and add the letter to list of letters
                cell.gameObject.GetComponent<Tile>().Index = new Vector2Int(x, y);
                cell.gameObject.GetComponent<Tile>().letterText.text = (""+builder.letter).ToUpper();
                GetComponent<WordManager>().AddLetter(builder.letter);
            }

            //Adding tile to tile list
            tiles.Add(builder.type==TileType.Helper ? new TileInfo(new Vector2Int(x, y), cell.rectTransform) : new LetterTile(new Vector2Int(x, y), cell.rectTransform, builder.letter));
        }

        //Telling the game that the grid is ready
        built = true;
    }


    public void SetDoubleTiles()
    {
        // Picks $doubleTiles tiles at random and sets them to double score
        int dTiles = doubleTiles;
        while (dTiles > 0)
        {
            Vector2Int selection = new Vector2Int(Random.Range(0, gridSize.x), Random.Range(0, gridSize.y));
            TileInfo tile = tiles[(selection.y * gridSize.x) + selection.x];
            if (tile is LetterTile)
            {
                LetterTile letterTile = (LetterTile)tile;
                if (!letterTile.isDouble)
                {
                    letterTile.isDouble = true;
                    letterTile.Cell.GetComponent<Image>().color = doubleTileColor;
                    dTiles--;
                }
            }
        }
    }
}

public enum TileType
{
    Letter, Helper
}

public class TileInfo
{
    Vector2Int index;
    RectTransform cell;

    public TileInfo(Vector2Int index, RectTransform cell)
    {
        this.index = index;
        this.cell = cell;
    }

    public Vector2Int Index { get => index; /*set => index = value;*/ }
    public RectTransform Cell { get => cell; /*set => cell = value;*/ }
}

public class LetterTile : TileInfo
{
    public char letter;
    public bool isDouble;

    public LetterTile(Vector2Int index, RectTransform cell, char letter) : base(index, cell)
    {
        this.letter = letter;
        this.isDouble = false;
    }
}
