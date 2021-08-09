using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardState
{
    public int[,] board;
    public int score;

    public BoardState()
    {
        board = new int[4, 4];

        foreach (Cell cell in GameController.instance.allCells)
        {
            if(cell.tile != null)
                board[cell.row, cell.column] = cell.tile.value;
        }

        score = ScoreTracker.instance.GetScore();
    }

}
