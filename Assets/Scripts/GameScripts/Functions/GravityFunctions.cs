using UnityEngine;

/*
 NOTE
    Possible optimizations:
        - A data structure to keep affected columns when blast happens in a row. Instead of
traverse whole board, only traverse affected columns.


*/

public class GravityFunctions : MonoBehaviour
{
    public Grid board;
    public Prefabs prefabs;
    public PosFunctions posFunctions;
    public CamFunctions camFunctions;

    private float gravity = -60;

    #region ExecuteGravityFunctions
    private (bool isShiftable, int targetRow) IsLogicallyShiftable(int column, int row)
    {
        if (board.grid[column, row] == null) return (false, -1);
        if (board.grid[column, row].state == "-1") return (false, -1);
        if (board.grid[column, row] is not Cube && board.grid[column, row] is not Rocket && board.grid[column, row] is not Vase) 
            return (false, -1);
        

        int currentRow = row - 1;
        while (currentRow >= 0 && board.grid[column, currentRow] == null) currentRow--;
        int targetRow = currentRow + 1;
        if (targetRow == row)
            return (false, -1);
        else
            return (true, targetRow);
    }

    private void AssignDestinationList(TileObject tile, int initialRow, int targetRow)
    {
        int currentDestination = initialRow - 1;
        float currentWorldPosition = board.bottomLeftPosition.y + (currentDestination * board.tileHeight + board.tileHeight / 2);
        while (currentDestination >= targetRow)
        {
            tile.destinationList.Enqueue(currentWorldPosition);
            currentDestination--;
            currentWorldPosition -= board.tileHeight;
        }
    }
    private void ShiftLogically(int column, int oldRow, int targetRow)
    {
        TileObject toBeShiftedTile = board.grid[column, oldRow];
        board.grid[column, oldRow] = null;
        board.grid[column, targetRow] = toBeShiftedTile;
        board.grid[column, targetRow].internalColumn = column;
        board.grid[column, targetRow].internalRow = targetRow;
        // Set as falling
        board.grid[column, targetRow].state = "1";
        AssignDestinationList(toBeShiftedTile, oldRow, targetRow);
    }
    private int FindLowestNullFromTop(int column)
    {
        int currentRow = board.rows - 1;
        while (currentRow >= 0 && board.grid[column, currentRow] == null) currentRow--;
        return currentRow + 1;
    }
    private bool IsObjectInGrid(Vector2 position)
    {
        float topPosition = position.y + board.tileHeight / 2;
        float bottomPosition = position.y - board.tileHeight / 2;
        return topPosition <= board.rightTopPosition.y && bottomPosition >= board.bottomLeftPosition.y;
    }

    public void ExecuteGravity()
    {
        for (int column = 0; column < board.columns; column++)
        {
            for (int row = 1; row < board.rows; row++)
            {
                (bool isLogicallyShiftable, int targetRow) = IsLogicallyShiftable(column, row);
                if (isLogicallyShiftable == false) continue;
                else
                {
                    ShiftLogically(column, row, targetRow);

                }
            }
            if (board.grid[column, board.rows - 1] == null)
            {
                int lowestNullIndex = FindLowestNullFromTop(column);
                for (int row = lowestNullIndex; row < board.rows; row++)
                {
                    Vector2 spawnPosition;
                    if (row == 0)
                    {
                        spawnPosition = posFunctions.CalculateWorldPosition(column, board.rows);
                    }
                    else
                    {
                        TileObject belowTile = board.grid[column, row - 1];
                        Vector2 belowTileWorldPosition = posFunctions.GetWorldPositionOfAttachedGameObject(belowTile);
                        if (IsObjectInGrid(belowTileWorldPosition)) spawnPosition = posFunctions.CalculateWorldPosition(column, board.rows);
                        else
                        {
                            belowTileWorldPosition.y += board.tileHeight;
                            spawnPosition = belowTileWorldPosition;
                        }
                    }

                    TileObject tile = board.GenerateRandomCube(new Vector2Int(column, row), "1", spawnPosition, 0);
                    int initialRow = posFunctions.CalculateCurrentRowFromWorldPosition(spawnPosition);
                    AssignDestinationList(tile, initialRow, row);
                    board.grid[column, row] = tile;
                }
            }
        }
    }
    #endregion


    #region ApplyGravityFunctions
    private float TravelTimeToDistance(TileObject tile)
    {
        float startY = tile.tile.transform.position.y;
        float destinationY = tile.destinationList.Peek();
        float initialSpeed = tile.speed;
        float delta = destinationY - startY;
        float discriminant = initialSpeed * initialSpeed + 2f * gravity * delta;
        if (discriminant < 0)
        {
            return -1f;
        }
        float sqrtDisc = Mathf.Sqrt(discriminant);
        float t1 = (-initialSpeed + sqrtDisc) / gravity;
        float t2 = (-initialSpeed - sqrtDisc) / gravity;
        float travelTime = Mathf.Infinity;
        if (t1 > 0 && t1 < travelTime)
            travelTime = t1;
        if (t2 > 0 && t2 < travelTime)
            travelTime = t2;
        if (travelTime == Mathf.Infinity)
            return -1f;
        return travelTime;
    }

    private bool WillCollide(float firstTilePosition, float secondTilePosition, float firstTileSpeed, float secondTileSpeed, float travelTime)
    {
        SpriteRenderer sr = prefabs.referenceCube.GetComponent<SpriteRenderer>();
        float spriteHeight = sr.bounds.size.y;
        float adjustedFirstPos = firstTilePosition - spriteHeight / 2f;
        float adjustedSecondPos = secondTilePosition + spriteHeight / 2f;

        if (adjustedFirstPos - adjustedSecondPos > 0)
        { 
            if (travelTime <= 0.001f)
                return false;

            
            float relativeSpeed = secondTileSpeed - firstTileSpeed;

           
            if (Mathf.Approximately(relativeSpeed, 0f))
                return (adjustedFirstPos - adjustedSecondPos) <= 0;

            float tCollision = (adjustedFirstPos - adjustedSecondPos) / relativeSpeed;
            return (tCollision >= 0f && tCollision <= travelTime);
        }
        else
        {
            return true;
        }
    }

    public void ApplyGravity()
    {
        for (int column = 0; column < board.columns; column++)
        {
            for (int row = 0; row < board.rows; row++)
            {
                TileObject tile = board.grid[column, row];
                if (tile != null && tile.state == "1")
                {
                    UnityEngine.Transform tileTransform = tile.tile.transform;
                    Vector3 pos = tileTransform.position;
                    float targetY = tile.destinationList.Peek();
                    pos.y += Time.deltaTime * tile.speed + 0.5f * gravity * Time.deltaTime * Time.deltaTime;
                    tile.speed += gravity * Time.deltaTime;

                    if (pos.y <= targetY)
                    {

                        tile.destinationList.Dequeue();

                        if (tile.destinationList.Count == 0)
                        {
                            pos.y = targetY;
                            tile.speed = 0;
                            tile.state = "0";
                        }
                        else
                        {
                            // Check for a collision with the tile below if available.
                            if (row != 0)
                            {
                                TileObject belowTile = board.grid[column, row - 1];
                                float belowTileSpeed = belowTile.speed;
                                Vector2 belowTilePosition = posFunctions.GetWorldPositionOfAttachedGameObject(belowTile);

                                float travelTime = TravelTimeToDistance(tile);

                                bool willCollide = WillCollide(pos.y, belowTilePosition.y, tile.speed, belowTileSpeed, travelTime);
                                if (willCollide)
                                {
                                    pos.y = targetY;
                                    tile.speed = 0f;
                                }

                            }
                        }
                    }
                    tileTransform.position = pos;
                }
            }
        }
    }
    #endregion
}
