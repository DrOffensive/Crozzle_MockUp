using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public Turn turn;

    private static PlayManager getManager;

    [SerializeField]
    PlayerInfo playerInfo, aiInfo;
    public float scoreTransitionWait = .5f, aiTurnTime = 2f;
    public Move_grid grid;
    public RectTransform activeTilePanel, scoringPanel;

    public RectTransform letterSpawn;
    public RectTransform[] letterSlots;
    public UI_ScoreLabel scoreLabelPrefab;
    LetterObject[] slotsUsed = new LetterObject[5];

    public GameObject letterPrefab;
    public UI_Button startBtn, okBtn, skipBtn;
    Count_down countDown;
    bool ready, skipCalculation;
    public UI_ScoreText playerScoreText, aiScoreText;

    [Header("Letter score colors")]
    public Color correctColor;
    public Color correctDoubleColor, wrongColor;

    [Header("Word score colors")]
    public Color wordColor;
    public Color wordDoubleColor;
    /// <summary>
    /// Use a drawer tile slot
    /// </summary>
    /// <param name="letter"></param>
    /// <param name="slot"></param>
    public void UseSlot(LetterObject letter, int slot)
    {
        if (slot < slotsUsed.Length)
        {
            if (slotsUsed[slot] == null)
            {
                slotsUsed[slot] = letter;
            }
        }
    }

    /// <summary>
    /// Returns the index of the first empty drawer tile slot
    /// </summary>
    /// <returns>int</returns>
    public int GetFirstEmptySlotIndex()
    {
        for (int i = 0; i < slotsUsed.Length; i++)
        {
            if (slotsUsed[i] == null)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Returns a list of all the empty drawer tile slots
    /// </summary>
    /// <returns>List of ints</returns>
    public List<int> GetAllEmptySlots ()
    {
        List<int> slots = new List<int>();
        for (int i = 0; i < slotsUsed.Length; i++)
        {
            if (slotsUsed[i] == null)
                slots.Add(i);
        }

        return slots;
    }

    /// <summary>
    /// Get drawer tile slot with index $slot
    /// </summary>
    /// <param name="slot"></param>
    /// <returns>RectTransform</returns>
    public RectTransform GetSlot(int slot)
    {
        if (slot < letterSlots.Length)
        {
            return letterSlots[slot];
        }

        return null;
    }

    /// <summary>
    /// Empties the slot with index $slot
    /// </summary>
    /// <param name="slot"></param>
    public void EmptySlot(int slot)
    {
        if (slot < slotsUsed.Length)
        {
            slotsUsed[slot] = null;
        }
    }

    /// <summary>
    /// Get an instance of the playmanager in the scene
    /// </summary>
    public static PlayManager GetManager { get => getManager; }

    void Start()
    {
        getManager = this;
        countDown = GetComponent<Count_down>();
        StartCoroutine(Setup());
    }


    /// <summary>
    /// Sets up the game, and ensures that the player can't start before everythin is ready
    /// </summary>
    IEnumerator Setup()
    {
        okBtn.gameObject.SetActive(false);
        Build_Grid gridBuilder = GetComponent<Build_Grid>();
        playerInfo = new PlayerInfo();
        aiInfo = new PlayerInfo();

        while (!gridBuilder.IsBuilt)
        {
            yield return null;
        }
        
        startBtn.gameObject.SetActive(true);
    }

    /// <summary>
    /// Starts the player round. 
    /// </summary>
    IEnumerator StartRound()
    {
        //Move the grid
        grid.Move();

        while (!grid.IsReady)
        {
            yield return null;
        }

        //Create a list of the letters the player have from last round, if any
        List<int> openSlots = GetAllEmptySlots();
        List<LetterObject> remainingLetters = new List<LetterObject>();
        foreach(LetterObject tile in slotsUsed)
        {
            if (tile != null)
            {
                remainingLetters.Add(tile);
            }
        }

        //Request new letters
        char[] newLetters = new char[0];
        RequestRemainingLetters(ref playerInfo.letters, ref newLetters);
        //Create new letter tiles for the new letters just requested, and add them to the list of remaining letters and to the $playerInfo
        List<LetterObject> letters = new List<LetterObject>();
        for (int i = 0; i < newLetters.Length; i++)
        {
            if (newLetters[i] != '!')
            {
                LetterObject letter = Instantiate(letterPrefab, activeTilePanel).GetComponent<LetterObject>();
                letter.GetComponent<RectTransform>().position = letterSpawn.position;
                letter.SetHomeSlot = openSlots[i];
                letter.Letter = newLetters[i];
                remainingLetters.Add(letter);
                letters.Add(letter);
                playerInfo.tiles[i] = letter;
            }
        }
        playerInfo.tiles = remainingLetters.ToArray();

        //Allocate slots to the new letter tiles and start moving them towards their slots
        RectTransform[] slotRects = new RectTransform[openSlots.Count];
        for(int i = 0; i < openSlots.Count; i++)
        {
            slotRects[i] = letterSlots[openSlots[i]];
        }
        ReadyLetters readyLetters = GetComponent<ReadyLetters>();
        readyLetters.ReadyUpLetters(letters.ToArray(), slotRects);
        while (!readyLetters.IsReady)
        {
            yield return null;
        }

        //Unlock letters and start the round
        foreach (LetterObject letter in playerInfo.tiles)
        {
            letter.allowMove = true;
        }
        okBtn.gameObject.SetActive(true);
        countDown.Set(60);
    }
    /// <summary>
    /// Plays the scoring animation for the player after a round and awards it to the player
    /// </summary>
    /// <param name="scores"></param>
    IEnumerator ScoringAnimation (CalculatedScore scores)
    {
        bool done = false;
        //Award Letter scores
        float start = Time.time;
        for (int i = 0; i < scores.LetterScores.Length; i++)
        {
            if (scores.LetterScores[i].ScoreType == LetterScore.LetterScoreType.WrongLetter)
            {
                StartCoroutine(scores.LetterScores[i].Letter.Tint(wrongColor));
            }
            else if (scores.LetterScores[i].ScoreType == LetterScore.LetterScoreType.CorrectLetter)
            {
                StartCoroutine(scores.LetterScores[i].Letter.Tint(correctColor));
                UI_ScoreLabel scoreLabel = Instantiate(scoreLabelPrefab, scoringPanel);
                scoreLabel.transform.position = scores.LetterScores[i].Letter.transform.position;
                scoreLabel.SetLabel = 1;
            }
            else
            {
                StartCoroutine(scores.LetterScores[i].Letter.Tint(correctDoubleColor));
                UI_ScoreLabel scoreLabel = Instantiate(scoreLabelPrefab, scoringPanel);
                scoreLabel.transform.position = scores.LetterScores[i].Letter.transform.position;
                scoreLabel.SetLabel = 2;
            }
        }

        //Wait a bit before moving on, unless skip was pressed
        while (!done && !skipCalculation)
        {
            if (start + scoreTransitionWait <= Time.time)
                done = true;
            yield return null;
        }
        playerInfo.score += scores.ScoreFromLetters;

        //Destroy correctly placed tiles and send wrongly placed tiles home
        List<int> openSlots = GetAllEmptySlots();
        int currentSlot = -1;
        foreach(LetterScore letter in scores.LetterScores)
        {
            if(letter.ScoreType != LetterScore.LetterScoreType.WrongLetter)
            {
                for(int i =0; i < playerInfo.letters.Length; i++)
                {
                    if(playerInfo.letters[i] == letter.Letter.Letter)
                    {
                        playerInfo.letters[i] = '!';
                        break;
                    }
                }
                Destroy(letter.Letter.gameObject);
            } else
            {
                currentSlot++;
                letter.Letter.transform.parent = activeTilePanel;
                letter.Letter.tileState = LetterObject.TileState.Floating;
                StartCoroutine(letter.Letter.Scale(LetterObject.TileState.GridLocked, LetterObject.TileState.Home));
                StartCoroutine(letter.Letter.Tint(Color.white));
                StartCoroutine(letter.Letter.MoveToSlot(openSlots[currentSlot]));
            }
        }
        //If not skip was pressed, continue with going through each word
        if (!skipCalculation)
        {
            UpdatePlayerScore();
            Build_Grid buildGrid = PlayManager.GetManager.GetComponent<Build_Grid>();
            int words = 0;
            if (scores.WordScores.Length > 0)
            {
                Vector2Int[] highlitLetters = new Vector2Int[0];
                yield return new WaitForSeconds(scoreTransitionWait);
                while (words < scores.WordScores.Length && !skipCalculation)
                {

                    foreach(Vector2Int letterIndex in highlitLetters)
                    {
                        buildGrid.GetTile(letterIndex).Cell.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    }

                    highlitLetters = scores.WordScores[words].Tiles;

                    foreach (Vector2Int letterIndex in highlitLetters)
                    {
                        LetterTile tile = (LetterTile)buildGrid.GetTile(letterIndex);
                        tile.Cell.GetComponent<UnityEngine.UI.Image>().color = tile.isDouble ? wordDoubleColor : wordColor;
                    }
                    UI_ScoreLabel scoreLabel = Instantiate(scoreLabelPrefab, scoringPanel);
                    scoreLabel.transform.position = scores.WordScores[words].GetCenterPosition;
                    scoreLabel.SetLabel = scores.WordScores[words].Tiles.Length;
                    words++;
                    yield return new WaitForSeconds(scoreTransitionWait);
                }

                foreach (Vector2Int letterIndex in highlitLetters)
                {
                    buildGrid.GetTile(letterIndex).Cell.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                }
            }
            yield return new WaitForSeconds(scoreTransitionWait);
        }
        playerInfo.score += scores.ScoreFromWords;

        //Gather the remaining letters and set the $playerInfo correctly
        List<char> remainingLetters = new List<char>();
        for(int i = 0; i < slotsUsed.Length; i++)
        {
            if(slotsUsed[i]!=null)
            {
                remainingLetters.Add(slotsUsed[i].Letter);
            }
        }
        if(remainingLetters.Count<slotsUsed.Length)
        {
            for(int i = remainingLetters.Count-1; i < slotsUsed.Length; i++)
            {
                remainingLetters.Add('!');
            }
        }
        playerInfo.letters = remainingLetters.ToArray();

        //Move grid back down and end the turn
        countDown.timertext.SetText = "You Scored " + scores.FinalScore;
        UpdatePlayerScore();
        skipBtn.gameObject.SetActive(false);
        grid.Move();
        while(!grid.IsDown)
        {
            yield return null;
        }
        skipCalculation = false;
        if (CheckFullBoard())
        {
            turn = Turn.Finished;
            EndGame();
        }
        else
        {
            turn = Turn.Opponent;
            StartCoroutine(OpponentTurn());
        }
    }

    /// <summary>
    /// The AI opponents turn
    /// </summary>
    /// <returns></returns>
    IEnumerator OpponentTurn()
    {
        //Request new letters
        char[] newChars = new char[0];
        RequestRemainingLetters(ref aiInfo.letters, ref newChars);
        Build_Grid buildGrid = GetComponent<Build_Grid>();
        yield return new WaitForSeconds(scoreTransitionWait);

        //Determine how many letters AI can use
        countDown.timertext.SetText = "Opponent is playing";
        List<int> usedLetters = new List<int>();
        int usableLetters = 0;
        foreach(char c in aiInfo.letters)
        {
            if(c!='!')
            {
                usableLetters++;
            }
        }

        //Determine how many letters AI is going to use
        int placedLetters = Random.Range(Mathf.Clamp(3, 0, usableLetters), usableLetters);
        if (buildGrid.GetComponent<WordManager>().LettersLeft <= 10)
        {
            placedLetters = usableLetters;
        }

        //Determine where AI is going to place tiles
        int attempts = 0, selectedLetters = 0;
        while (selectedLetters <= placedLetters && attempts < 1000)
        {
            int selection = Random.Range(0, 5);
            if (!usedLetters.Contains(selection) && aiInfo.letters[selection] != '!')
            {
                usedLetters.Add(selection);
                selectedLetters++;
            }

            //With really bad RNG this can take forever, so to avoid that, drop out of the loop after 1000 failed attempts
            attempts++; 
            yield return null;
        }

        //Calculate the letter scores
        int letterScores = 0;
        List<LetterTile> usedTiles = new List<LetterTile>();
        if (usedLetters.Count > 0)
        {
            for (int i = 0; i < usedLetters.Count; i++)
            {
                char letter = aiInfo.letters[usedLetters[i]];
                aiInfo.letters[usedLetters[i]] = '!';
                List<LetterTile> letterTiles = buildGrid.GetAllUnsetTilesOfType(letter);
                int selectedTile = Random.Range(0, letterTiles.Count);
                    letterTiles[selectedTile].Cell.GetComponent<Tile>().Set = true;
                    usedTiles.Add(letterTiles[selectedTile]);

                    if (letterTiles[selectedTile].isDouble)
                        letterScores += 2;
                    else
                        letterScores++;
            }
        }

        //Waiting just so it doesn't happen instantly, this could be done anywhere. I decided here
        yield return new WaitForSeconds(aiTurnTime);

        //Calculate word scores
        WordManager wordManager = PlayManager.GetManager.GetComponent<WordManager>();
        int wordScores = 0;
        List<WordManager.Word> usedWords = new List<WordManager.Word>();
        foreach (WordManager.Word word in wordManager.words)
        {
            foreach (LetterTile tile in usedTiles)
            {
                if (word.GetLetterPositions.Contains(tile.Index))
                {
                    List<Vector2Int> letterIndicies = word.GetLetterPositions;
                    bool completedWord = true;
                    foreach (Vector2Int index in letterIndicies)
                    {
                        if (!buildGrid.GetTile(index).Cell.GetComponent<Tile>().Set)
                        {
                            completedWord = false;
                            break;
                        }
                    }
                    if (completedWord)
                    {
                        if (!usedWords.Contains(word))
                        {
                            usedWords.Add(word);
                            wordScores += letterIndicies.Count;
                        }
                    }
                }
            }
        }

        //Gather up the scores and end the turn
        aiInfo.score += wordScores + letterScores;
        countDown.timertext.SetText = "Opponent scored " + (letterScores + wordScores);
        UpdateOpponentScore();
        yield return new WaitForSeconds(scoreTransitionWait);

        //If the board is full end the game otherwise give the turn back to player
        if (CheckFullBoard())
        {
            turn = Turn.Finished;
            EndGame();
        }
        else
        {
            countDown.timertext.SetText = "Your turn";
            turn = Turn.Player;
            startBtn.gameObject.SetActive(true);
        }
    }

    //Updates the players score UI text
    void UpdatePlayerScore ()
    {
        playerScoreText.SetScore((int)playerInfo.score);
    }

    //Updates AIs score UI text
    void UpdateOpponentScore()
    {
        aiScoreText.SetScore((int)aiInfo.score);
    }
    
    bool CheckFullBoard ()
    {
        Build_Grid buildGrid = GetComponent<Build_Grid>();
        foreach(TileInfo tile in buildGrid.tiles)
        {
            if (tile is LetterTile && !tile.Cell.GetComponent<Tile>().Set)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Updates $currentLetters array with new letters where needed and also repopulates $newLetters array with the newly added letters.
    /// </summary>
    /// <param name="currentLetters"></param>
    /// <param name="newLetters"></param>
    void RequestRemainingLetters (ref char[] currentLetters, ref char[] newLetters)
    {
        int neededLetters = 0;
        List<char> remainingLetters = new List<char>();
        for (int i = 0; i < 5; i++)
        {
            if (currentLetters[i] == '!')
            {
                neededLetters++;
            }
            else
            {
                remainingLetters.Add(currentLetters[i]);
            }
        }

        newLetters = GetComponent<WordManager>().RequestLetters(neededLetters);
        foreach (char letter in newLetters)
        {
            remainingLetters.Add(letter);
        }
        currentLetters = remainingLetters.ToArray();
    }


    /// <summary>
    /// Calculates scores based on placed tiles on the board
    /// </summary>
    public CalculatedScore CalculateScore ()
    {
        Build_Grid buildGrid = PlayManager.GetManager.GetComponent<Build_Grid>();
        WordManager wordManager = PlayManager.GetManager.GetComponent<WordManager>();
        int wordScore = 0;
        int letterScore = 0;
        List<LetterScore> letterScores = new List<LetterScore>();
        List<WordScore> wordScores = new List<WordScore>();

        //Calculate letters
        foreach(LetterObject letter in playerInfo.tiles)
        {
            if(letter.tileState==LetterObject.TileState.GridLocked)
            {
                Vector2Int index = letter.GetComponentInParent<Tile>().Index;
                LetterTile tile = (LetterTile)buildGrid.GetTile(index);
                if(tile.letter==letter.Letter)
                {
                    if(tile.isDouble)
                    {
                        letterScore += 2;
                        buildGrid.GetTile(index).Cell.GetComponent<Tile>().Set = true;
                        letterScores.Add(new LetterScore(letter, index, LetterScore.LetterScoreType.CorrectDoubleLetter));
                    } else
                    {
                        letterScore += 1;
                        buildGrid.GetTile(index).Cell.GetComponent<Tile>().Set = true;
                        letterScores.Add(new LetterScore(letter, index, LetterScore.LetterScoreType.CorrectLetter));
                    }
                } else
                {
                    letterScores.Add(new LetterScore(letter, index, LetterScore.LetterScoreType.WrongLetter));
                }
            }
        }

        //Takes calculates letters and match them with the words in the word list
        List<WordManager.Word> usedWords = new List<WordManager.Word>();
        foreach (WordManager.Word word in wordManager.words)
        {
            foreach(LetterScore score in letterScores)
            {
                if(word.GetLetterPositions.Contains(score.Tile))
                {
                    List<Vector2Int> letterIndicies = word.GetLetterPositions;
                    bool completedWord = true;
                    foreach (Vector2Int index in letterIndicies)
                    {
                        if (!buildGrid.GetTile(index).Cell.GetComponent<Tile>().Set)
                        {
                            completedWord = false;
                            break;
                        }
                    }
                    if(completedWord)
                    {
                        if (!usedWords.Contains(word))
                        {
                            usedWords.Add(word);
                            wordScore += letterIndicies.Count;
                            wordScores.Add(new WordScore(letterIndicies.ToArray()));
                        }
                    }
                }
            }
        }

        return new CalculatedScore(letterScores.ToArray(), wordScores.ToArray(), letterScore, wordScore);
    }

    // Update is called once per frame
    void Update()
    {
        if (turn == Turn.Player)
        {
            //Starts your round and hides the start button
            if (startBtn.Clicked)
            {
                startBtn.gameObject.SetActive(false);
                StartCoroutine(StartRound());
            }

            //Ends your round and hides the OK button
            if (okBtn.Clicked)
            {
                okBtn.gameObject.SetActive(false);
                EndTurn();
            }

            //If timer hits 0 end the turn
            if (countDown.Alarm)
            {
                EndTurn();

                //Ensure any floating lettertiles get returned home
                foreach (LetterObject letter in playerInfo.tiles)
                {
                    if (letter.tileState == LetterObject.TileState.Floating)
                    {
                        StartCoroutine(letter.MoveHome());
                        StartCoroutine(letter.Scale(LetterObject.TileState.Floating, LetterObject.TileState.Home));
                    }
                }
            }
        } else if(turn == Turn.Intermission)
        {
            //Skip the scoring animation and hide the Skip button
            if(skipBtn.Clicked)
            {
                skipBtn.gameObject.SetActive(false);
                skipCalculation = true;
            }
        }
    }

    /// <summary>
    /// An object that stores the calculated score data for easy retrieval during scoring animation
    /// </summary>
    public struct CalculatedScore
    {
        LetterScore[] letterScores;
        WordScore[] wordScores;
        float scoreFromLetters, scoreFromWords;

        public CalculatedScore(LetterScore[] letterScores, WordScore[] wordScores, float scoreFromLetters, float scoreFromWords)
        {
            this.letterScores = letterScores;
            this.wordScores = wordScores;
            this.scoreFromLetters = scoreFromLetters;
            this.scoreFromWords = scoreFromWords;
        }

        public float FinalScore { get => scoreFromLetters + scoreFromWords; }
        public LetterScore[] LetterScores { get => letterScores;}
        public WordScore[] WordScores { get => wordScores; }
        public float ScoreFromLetters { get => scoreFromLetters; }
        public float ScoreFromWords { get => scoreFromWords; }
    }

    /// <summary>
    /// Scores from letters
    /// </summary>
    public struct LetterScore
    {
        LetterObject letter;
        Vector2Int tile;
        LetterScoreType scoreType;

        public LetterScore(LetterObject letter, Vector2Int tile, LetterScoreType scoreType)
        {
            this.letter = letter;
            this.tile = tile;
            this.scoreType = scoreType;
        }

        public Vector2Int Tile { get => tile; }
        public LetterScoreType ScoreType { get => scoreType; }
        public LetterObject Letter { get => letter; }

        public enum LetterScoreType
        {
            WrongLetter, CorrectLetter, CorrectDoubleLetter
        }
    }

    /// <summary>
    /// Scores from words
    /// </summary>
    public struct WordScore
    {
        Vector2Int[] tiles;

        public WordScore(Vector2Int[] tiles)
        {
            this.tiles = tiles;
        }

        public Vector2 GetCenterPosition
        {
            get {
                Build_Grid board = PlayManager.GetManager.GetComponent<Build_Grid>();
                if (tiles.Length > 1)
                {
                    Vector2 firstLetterPosition = board.GetTile(tiles[0]).Cell.position;
                    Vector2 lastLetterPosition = board.GetTile(tiles[tiles.Length-1]).Cell.position;
                    Vector2 line = firstLetterPosition - lastLetterPosition;
                    return lastLetterPosition + (line.normalized * (line.magnitude * .5f));
                }
                else
                    return board.GetTile(tiles[0]).Cell.position;
            }
        }

        public Vector2Int[] Tiles { get => tiles; }
    }

    /// <summary>
    /// Ends the current player turn
    /// Stops the timer, locks the tiles, starts the scoring animation
    /// </summary>
    void EndTurn ()
    {
        countDown.Stop();
        foreach(LetterObject tile in playerInfo.tiles)
        {
            tile.allowMove = false;
        }

        CalculatedScore score = CalculateScore();
        turn = Turn.Intermission;
        skipBtn.gameObject.SetActive(true);
        StartCoroutine(ScoringAnimation(score));
    }

    /// <summary>
    /// End the game and tally up the score
    /// </summary>
    void EndGame()
    {
        if (playerInfo.score > aiInfo.score)
        {
            countDown.timertext.SetText = "You Win";
            countDown.timertext.SetFaceColor = Color.green;
        }
        else if (aiInfo.score > playerInfo.score)
        {
            countDown.timertext.SetText = "Opponent Wins";
            countDown.timertext.SetFaceColor = Color.red;
        } else
        {
            countDown.timertext.SetText = "Draw";
            countDown.timertext.SetFaceColor = Color.black;
        }
    }

    public enum Turn { Opponent, Player, Intermission, Finished }
}

/// <summary>
/// Stored data for each player
/// </summary>
[System.Serializable]
public class PlayerInfo
{
    public float score;
    public char[] letters; 
    public LetterObject[] tiles;

    public PlayerInfo()
    {
        score = 0;
        letters = new char[] { '!', '!', '!', '!', '!' };
        tiles = new LetterObject[5];
    }
}