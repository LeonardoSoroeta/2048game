using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public enum GameState { AwaitingMove, ProcessingMove, MoveProcessed, WaitAndSpawn, GameOverCheck, Paused }
public enum Player { Human, AI }
public enum Direction { Up, Down, Left, Right }

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public static GameState gameState;
    public static Player player;
    public static int linesDone;
    public static bool boardHasChanged;
    public Text gameOverScoreText;

    List<BoardState> boardStateStack;

    public Cell[] allCells;

    Direction moveDirection;

    public static Action<Direction> newMoveAction;

    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject gameOverMenu;
    [SerializeField] GameObject newGameMenu;
    [SerializeField] GameObject resetScoreMenu;
    [SerializeField] GameObject startGamePanel;
    [SerializeField] GameObject toggleAIButton;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        boardStateStack = new List<BoardState>();

        SpawnRandomTile();
        SpawnRandomTile();

        boardStateStack.Add(new BoardState());

        gameState = GameState.AwaitingMove;
        player = Player.Human;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == GameState.AwaitingMove)
        {
            if (GetMove())
            {
                if (newMoveAction != null)
                {
                    boardHasChanged = false;
                    linesDone = 0;

                    gameState = GameState.ProcessingMove;
                    newMoveAction(moveDirection);
                }
            }
        }
        else if (gameState == GameState.MoveProcessed)
        {
            if (boardHasChanged)
            {
                gameState = GameState.WaitAndSpawn;
                StartCoroutine(WaitAndSpawnTile());
            }
            else
            {
                gameState = GameState.AwaitingMove;
            }
        }
        else if (gameState == GameState.GameOverCheck)
        {
            if (GameOverCheck() == true)
            {
                gameState = GameState.Paused;
                ShowGameOverMenu();
            }
            else
                gameState = GameState.AwaitingMove;
        }

    }

    IEnumerator WaitAndSpawnTile()
    {
        yield return new WaitForSeconds(0.18f);

        SpawnRandomTile();

        boardStateStack.Add(new BoardState());

        if (gameState != GameState.Paused)
            gameState = GameState.GameOverCheck;

    }

    public void SpawnRandomTile()
    {
        int randomIndex = UnityEngine.Random.Range(0, allCells.Length);
        float random = UnityEngine.Random.Range(0f, 1f);
        int tileValue = 2;
        bool foundEmptyCell = false;

        // 10% chance of a 4 instead of a 2
        if (random < 0.1f)
            tileValue = 4;

        if (BoardIsFull())
            return;

        // Find a random empty cell
        do
        {
            if (allCells[randomIndex].tile == null)
                foundEmptyCell = true;
            else
                randomIndex = (randomIndex + 1) % allCells.Length;

        } while (!foundEmptyCell);

        // Place a tile inside the cell
        GameObject tileObject = Instantiate(tilePrefab, allCells[randomIndex].transform);
        Tile tileComponent = tileObject.GetComponent<Tile>();
        allCells[randomIndex].transform.GetComponent<Cell>().tile = tileComponent;
        tileComponent.UpdateTileValue(tileValue);
        tileComponent.PlaySpawnAnimation();

    }

    public bool GameOverCheck()
    {
        foreach (Cell cell in allCells)
            if (cell.MovesAvailable() == true)
                return false;

        return true;
    }

    public BoardState CurrentBoardState()
    {
        return boardStateStack[boardStateStack.Count - 1];
    }

    public void UndoMove()
    {
        if (player == Player.Human && (gameState == GameState.AwaitingMove || gameState == GameState.Paused))
        {
            if (boardStateStack.Count == 1)
                return;

            gameState = GameState.Paused;

            boardStateStack.RemoveAt(boardStateStack.Count - 1);

            LoadBoardState(CurrentBoardState());

            gameState = GameState.AwaitingMove;
        }
    }

    public void LoadBoardState(BoardState boardState)
    {
        foreach (Cell cell in allCells)
            if (cell.tile != null)
                Destroy(cell.transform.GetChild(0).gameObject);

        foreach (Cell cell in allCells)
            if (boardState.board[cell.row, cell.column] != 0)
            {
                GameObject tileObject = Instantiate(tilePrefab, cell.transform);
                Tile tileComponent = tileObject.GetComponent<Tile>();
                cell.transform.GetComponent<Cell>().tile = tileComponent;
                tileComponent.UpdateTileValue(boardState.board[cell.row, cell.column]);
            }

        ScoreTracker.instance.SetScore(boardState.score);
    }

    public bool BoardIsFull()
    {
        bool boardIsFull = true;

        for (int i = 0; i < allCells.Length; i++)
            if (allCells[i].tile == null)
                boardIsFull = false;

        if (boardIsFull)
            return true;
        else
            return false;
    }

    private bool GetMove()
    {
        if (player == Player.Human)
            return GetHumanMove();
        else
            return GetAIMove();
    }

    private bool GetHumanMove()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || SwipeDetector.instance.SwipedUp())
        {
            moveDirection = Direction.Up;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || SwipeDetector.instance.SwipedDown())
        {
            moveDirection = Direction.Down;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || SwipeDetector.instance.SwipedLeft())
        {
            moveDirection = Direction.Left;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || SwipeDetector.instance.SwipedRight())
        {
            moveDirection = Direction.Right;
            return true;
        }

        return false;
    }

    private bool GetAIMove()
    {
        int randomMove = UnityEngine.Random.Range(0, 4);

        if (randomMove == 0)
        {
            moveDirection = Direction.Up;
            return true;
        }
        else if (randomMove == 1)
        {
            moveDirection = Direction.Down;
            return true;
        }
        else if (randomMove == 2)
        {
            moveDirection = Direction.Left;
            return true;
        }
        else if (randomMove == 3)
        {
            moveDirection = Direction.Right;
            return true;
        }

        return false;
    }

    public void ToggleAI()
    {

        if (player == Player.Human)
        {
            player = Player.AI;
            toggleAIButton.GetComponentInChildren<Text>().text = "Auto: ON";
        }
        else
        {
            player = Player.Human;
            toggleAIButton.GetComponentInChildren<Text>().text = "Auto";
        }

    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void ShowNewGameMenu()
    {
        gameState = GameState.Paused;
        newGameMenu.SetActive(true);
    }

    public void HideNewGameMenu()
    {
        gameState = GameState.AwaitingMove;
        newGameMenu.SetActive(false);
    }

    public void ShowGameOverMenu()
    {
        gameOverScoreText.text = "Score: " + CurrentBoardState().score.ToString();
        gameOverMenu.SetActive(true);
    }

    public void HideGameOverMenu()
    {
        gameState = GameState.AwaitingMove;

        if (player == Player.AI)
            ToggleAI();

        gameOverMenu.SetActive(false);
    }

    public void ShowResetScoreMenu()
    {
        gameState = GameState.Paused;
        resetScoreMenu.SetActive(true);
    }

    public void HideResetScoreMenu()
    {
        gameState = GameState.AwaitingMove;
        resetScoreMenu.SetActive(false);
    }

    public void ResetBestScore()
    {
        ScoreTracker.instance.ResetBestScore();

        HideResetScoreMenu();
    }

    public void HideStartGamePanel()
    {
        startGamePanel.SetActive(false);

    }

}
