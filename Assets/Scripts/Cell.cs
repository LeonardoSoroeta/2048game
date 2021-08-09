using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int row;
    public int column;

    // Neighbor cells;
    public Cell right;
    public Cell down;
    public Cell left;
    public Cell up;

    public Tile tile;

    private void OnEnable()
    {
        GameController.newMoveAction += NewMove;
    }

    private void OnDisable()
    {
        GameController.newMoveAction -= NewMove;
    }

    private void NewMove(Direction direction)
    {

        // Only calls SlideAndMerge if this is one of the 4 edge tiles
        if (GetNeighbor(this, direction) == null)
        {
            SlideAndMerge(this, direction);
            GameController.linesDone++;
            if (GameController.linesDone == 4)
                GameController.gameState = GameState.MoveProcessed;
        }

    }

    private Direction OppositeDirection(Direction direction)
    {
        if (direction == Direction.Up)
        {
            return Direction.Down;
        }
        else if (direction == Direction.Down)
        {
            return Direction.Up;
        }
        else if (direction == Direction.Left)
        {
            return Direction.Right;
        }
        else
        {
            return Direction.Left;
        }
    }

    private Cell GetNeighbor(Cell cell, Direction direction)
    {
        if (direction == Direction.Up)
        {
            return cell.up;
        }
        else if (direction == Direction.Down)
        {
            return cell.down;
        }
        else if (direction == Direction.Left)
        {
            return cell.left;
        }
        else
        {
            return cell.right;
        }
    }

    // Slides (and merges) a row in direction d towards currentCell
    void SlideAndMerge(Cell currentCell, Direction d)
    {
        int speedMultiplier = 1;

        // if reached the last cell
        if (GetNeighbor(currentCell, OppositeDirection(d)) == null)
        {
            return;
        }

        // if current cell is not empty
        if (currentCell.tile != null)
        {
            Cell nextCell = GetNeighbor(currentCell, OppositeDirection(d));

            // nextCell searches for the first cell with a tile
            while (GetNeighbor(nextCell, OppositeDirection(d)) != null && nextCell.tile == null)
            {
                speedMultiplier++;
                nextCell = GetNeighbor(nextCell, OppositeDirection(d));
            }

            // if it found tile
            if (nextCell.tile != null)
            {
                // if same number then slide and merge it
                if (currentCell.tile.value == nextCell.tile.value)
                {
                    nextCell.tile.Double();
                    nextCell.tile.speedMultiplier = (float) speedMultiplier;
                    nextCell.tile.transform.SetParent(currentCell.transform);
                    currentCell.tile = nextCell.tile;
                    nextCell.tile = null;
                    GameController.boardHasChanged = true;
                }
                // else, just slide it
                else if (GetNeighbor(currentCell, OppositeDirection(d)).tile != nextCell.tile)
                {
                    nextCell.tile.speedMultiplier = (float) (speedMultiplier - 1);
                    nextCell.tile.transform.SetParent(GetNeighbor(currentCell, OppositeDirection(d)).transform);
                    GetNeighbor(currentCell, OppositeDirection(d)).tile = nextCell.tile;
                    nextCell.tile = null;
                    GameController.boardHasChanged = true;
                }
            }
            else
            {
                // nextCell has reached the final cell and it is empty, nothing to do
            }
        }
        else
        {
            // currentCell is empty nextCell searches for the first cell with a tile
            Cell nextCell = GetNeighbor(currentCell, OppositeDirection(d));

            while (GetNeighbor(nextCell, OppositeDirection(d)) != null && nextCell.tile == null)
            {
                speedMultiplier++;
                nextCell = GetNeighbor(nextCell, OppositeDirection(d));
            }
            // if it found a tile, bring it to the current empty cell and run SlideAndMerge on the current cell once again
            if (nextCell.tile != null)
            {
                nextCell.tile.speedMultiplier = (float) speedMultiplier;
                nextCell.tile.transform.SetParent(currentCell.transform);
                currentCell.tile = nextCell.tile;
                nextCell.tile = null;
                GameController.boardHasChanged = true;
                SlideAndMerge(currentCell, d);
            }
        }

        // move on with the recursive function to the next cell
        if (GetNeighbor(currentCell, OppositeDirection(d)) != null)
            SlideAndMerge(GetNeighbor(currentCell, OppositeDirection(d)), d);

    }

    public bool MovesAvailable()
    {
        if (tile == null)
            return true;

        foreach (Direction d in System.Enum.GetValues(typeof(Direction)))
            if (GetNeighbor(this, d) != null)
            {
                if (GetNeighbor(this, d).tile == null)
                    return true;

                if (GetNeighbor(this, d).tile.value == tile.value)
                    return true;
            }

        return false;

    }

}
