using System.Collections.Generic;
using UnityEngine;

/*
    Possible optimization for rocket hint:
    - Instead of checking every frame, we can check it in two places:
        * After executing BFS
        * When a board goes from notIdle to Idle

*/

public class BFSFunctions : MonoBehaviour
{
    #region References
    public Grid board;
    public Prefabs prefabs;
    public PosFunctions posFunctions;
    public CamFunctions camFunctions;
    public AnimationFunctions animFunctions;
    public ObstacleFunctions obstacleFunctions;
    public GridManager gridManager;
    #endregion

    #region Check Functions
    private bool IsExecutable(Vector2Int position)
    {
        int column = position.x;
        int row = position.y;
        TileObject tileObject = board.grid[column, row];
        bool isNull = tileObject == null;
        bool isObstacle = tileObject is Obstacle;
        bool isIdle = tileObject.state == "0";
        return !isNull && !isObstacle && isIdle;
    }

    private bool IsValidNeighbor(Vector2Int originalPosition, Vector2Int neighbor, Dictionary<Vector2Int, bool> map)
    {
        bool isInBounds = neighbor.x >= 0 && neighbor.x < board.columns && neighbor.y >= 0 && neighbor.y < board.rows;
        if (!isInBounds) return false;

        bool isValid = board.grid[neighbor.x, neighbor.y] != null && board.grid[neighbor.x, neighbor.y].state == "0";
        bool isSameCube = false;
        if (board.grid[originalPosition.x, originalPosition.y] is Cube original && board.grid[neighbor.x, neighbor.y] is Cube comparable)
        {
            isSameCube = original.color == comparable.color;
        }
        bool isRocket = false;
        if (board.grid[originalPosition.x, originalPosition.y] is Rocket originalR && board.grid[neighbor.x, neighbor.y] is Rocket comparableR)
        {
            isRocket = true;
        }
            bool isNotVisited = !map.ContainsKey(neighbor);

        return (isSameCube || isRocket) && isNotVisited && isValid;
    }
    #endregion

    #region ExecuteBFS
    public bool executeBFS(Vector2Int originalPosition)
    {
        if (IsExecutable(originalPosition))
        {

            Dictionary<Vector2Int, bool> map = new Dictionary<Vector2Int, bool>();
            List<Vector2Int> region = new List<Vector2Int>();
            Queue<Vector2Int> q = new Queue<Vector2Int>();

            q.Enqueue(originalPosition);
            map.Add(originalPosition, true);

            while (q.Count > 0)
            {
                Vector2Int currentPosition = q.Dequeue();
                region.Add(currentPosition);

                int[] xdir = { 1, -1, 0, 0 };
                int[] ydir = { 0, 0, 1, -1 };

                for (int i = 0; i < 4; i++)
                {
                    int c = currentPosition.x + xdir[i];
                    int r = currentPosition.y + ydir[i];
                    Vector2Int neighbor = new Vector2Int(c, r);
                    if (IsValidNeighbor(originalPosition, neighbor, map))
                    {
                        q.Enqueue(neighbor);
                        map.Add(neighbor, true);
                    }
                }
            }
            // If match found
            if (board.grid[originalPosition.x, originalPosition.y] is Rocket)
            {
                if (region.Count >= 2)
                {
                    // Combo needs to be implemented 31.03.25
                    obstacleFunctions.RocketComboHandler(region, originalPosition);
                }
                else
                {
                    Vector2Int position = region[0];
                    obstacleFunctions.AddToBeDestroyedIndividualTilesAndToBeDestroyedRockets(position);
                }
                return true;
            }
            else // Is cube
            {
                // This part works
                if (region.Count >= 4)
                {
                    obstacleFunctions.AddToBeDestroyedRegionAndCreateRocket(region, originalPosition);
                    return true;
                }
                else if (3 >= region.Count && region.Count >= 2)
                {

                    obstacleFunctions.AddToBeDestroyedRegion(region);
                    return true;
                }
                else
                {
                    Debug.Log("No blast!");
                    return false;
                }
            }
        }
        return false;
    }
    #endregion

    #region ExecuteBFSMatch // Mark Region As Rocketable
    private bool IsMatchable(Vector2Int position, Dictionary<Vector2Int, bool> visitedCells)
    {
        bool isNull = board.grid[position.x, position.y] == null;
        if (isNull) return false;

        bool isIdle = board.grid[position.x, position.y].state == "0";
        bool isObstacle = board.grid[position.x, position.y] is Obstacle;
        bool isRocket = board.grid[position.x, position.y] is Rocket;
        bool isVisited = visitedCells.ContainsKey(position);

        return isIdle && !isObstacle && !isRocket && !isVisited;

    }

    #region Load Sprites
    private Sprite LoadHintSprite(Vector2Int position)
    {
        TileObject currentTile = board.grid[position.x, position.y];
        CubeColor color = ((Cube)currentTile).color;

        switch (color)
        {
            case CubeColor.Red:
                return prefabs.redCubeHint;
            case CubeColor.Blue:
                return prefabs.blueCubeHint;
            case CubeColor.Green:
                return prefabs.greenCubeHint;
            case CubeColor.Yellow:
                return prefabs.yellowCubeHint;
            default: return null;
        }
    }

    private Sprite LoadCubeSprite(Vector2Int position)
    {
        TileObject currentTile = board.grid[position.x, position.y];
        CubeColor color = ((Cube)currentTile).color;

        switch (color)
        {
            case CubeColor.Red:
                return prefabs.redCubeSprite;
            case CubeColor.Blue:
                return prefabs.blueCubeSprite;
            case CubeColor.Green:
                return prefabs.greenCubeSprite;
            case CubeColor.Yellow:
                return prefabs.yellowCubeSprite;
            default: return null;
        }
    }
    #endregion


    private void MarkRegionAsRocketable(List<Vector2Int> region)
    {
        if (region.Count >= 4)
        {

            for (int i = 0; i < region.Count; i++)
            {
                Vector2Int position = region[i];
                Sprite hintSprite = LoadHintSprite(position);
                board.grid[position.x, position.y].tile.GetComponent<SpriteRenderer>().sprite = hintSprite;
            }
        }
        else
        {
            for (int i = 0; i < region.Count; i++)
            {
                Vector2Int position = region[i];
                Sprite hintSprite = LoadCubeSprite(position);
                board.grid[position.x, position.y].tile.GetComponent<SpriteRenderer>().sprite = hintSprite;
            }
        }
    }

    
    public void ExecuteBFSMatch()
    {
        Dictionary<Vector2Int, bool> visitedCells = new Dictionary<Vector2Int, bool>();
        for (int column = 0; column < board.columns; column++)
        {
            for (int row = 0; row < board.rows; row++)
            {
                Vector2Int position = new Vector2Int(column, row);
                if (!IsMatchable(position, visitedCells)) continue;

                List<Vector2Int> region = new List<Vector2Int>();
                Queue<Vector2Int> q = new Queue<Vector2Int>();

                q.Enqueue(position);
                visitedCells.Add(position, true);

                while (q.Count > 0)
                {
                    Vector2Int currentPosition = q.Dequeue();
                    region.Add(currentPosition);

                    int[] xdir = { 1, -1, 0, 0 };
                    int[] ydir = { 0, 0, 1, -1 };

                    for (int i = 0; i < 4; i++)
                    {
                        int c = currentPosition.x + xdir[i];
                        int r = currentPosition.y + ydir[i];
                        Vector2Int neighbor = new Vector2Int(c, r);
                        if (IsValidNeighbor(position, neighbor, visitedCells))
                        {
                            q.Enqueue(neighbor);
                            visitedCells.Add(neighbor, true);
                        }
                    }
                }
                MarkRegionAsRocketable(region);
            }
        }
    }
    #endregion

}
