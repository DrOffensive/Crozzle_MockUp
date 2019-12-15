using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages letters and words.
/// This is where players request new letters and placed letters are checked against words on the board
/// </summary>


public class WordManager : MonoBehaviour
{
    List<char> letters = new List<char>();

    public int LettersLeft { get => letters.Count; }

    public Word[] words;

    [System.Serializable]
    public struct Word
    {
        public string name;
        public Vector2Int firstLetterIndex;
        public int wordLength;
        public WordAlignment alignment;

        public List<Vector2Int> GetLetterPositions
        {
            get
            {
                List<Vector2Int> indicies = new List<Vector2Int>();
                for (int i = 0; i < wordLength; i++)
                {
                    indicies.Add(firstLetterIndex + (alignment == WordAlignment.Horizontal ? new Vector2Int(i, 0) : new Vector2Int(0, i)));
                }
                return indicies;
            }
        }
    }

    public void AddLetter (char letter)
    {
        if (letters == null)
            letters = new List<char>();

        letters.Add(letter);
    }

    public void ShuffleLetters ()
    {
        for(int i = letters.Count-1; i >= 0; i--)
        {
            int swap = Random.Range(0, i);
            char temp = letters[i];
            letters[i] = letters[swap];
            letters[swap] = temp;
        }
    }

    /// <summary>
    /// Requests $amount new letter(s).
    /// If there are no letters to give, but the game is still not over it will return what is still open on the board.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>Array of chars</returns>
    public char[] RequestLetters(int amount)
    {
        char[] selectedLetters = new char[amount];
        if (letters.Count > 0)
        {
            int toRemove = 0;
            for (int i = 0; i < amount; i++)
            {
                if (i < letters.Count)
                {
                    selectedLetters[i] = letters[i];
                    toRemove++;
                } else
                {
                    selectedLetters[i] = '!';
                }
            }
            letters.RemoveRange(0, toRemove);
        } else {
            List<LetterTile> tilesLeft = GetComponent<Build_Grid>().GetAllUnsetTiles();
            Debug.Log(tilesLeft.Count);
            for (int i = 0; i < amount; i++)
            {
                if (i < tilesLeft.Count)
                {
                    selectedLetters[i] = tilesLeft[i].letter;
                    Debug.Log(tilesLeft[i].letter);
                }
                else
                {
                    selectedLetters[i] = '!';
                }
            }
        }
        return selectedLetters;
    }

    public enum WordAlignment
    {
        Horizontal, Vertical
    }
}
