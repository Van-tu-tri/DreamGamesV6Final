using System.Collections.Generic;
using UnityEngine;


public class ObstacleFunctions : MonoBehaviour
{
    public Grid board;
    public AnimationFunctions animFunctions;
    public BFSFunctions bfsFunctions;
    public PosFunctions posFunctions;
    public Prefabs prefabs;
    public UIFunctions uiFunctions;


    // Damage Functions
    #region DecreaseRemainingObstacle || ObstacleDestroyTile || DamageObstacle

    public void DecreaseRemainingObstacle(TileObject tile)
    {
        board.numberOfRemaningObstacles--;
        if (tile is Box) board.remainingBox--;
        else if (tile is Stone) board.remainingStone--;
        else if (tile is Vase) board.remainingVase--;
    }
    public void DestroyTile(Vector2Int position)
    {

        TileObject tile = board.grid[position.x, position.y];
        if (tile is Obstacle)
        {
            DecreaseRemainingObstacle(tile);
            Debug.Log(board.remainingBox);
            Debug.Log(board.remainingStone);
            Debug.Log(board.remainingVase);
            uiFunctions.ChangeRemaningObstacleText(tile);
        }
        Destroy(tile.tile);
        board.grid[position.x, position.y] = null;
    }

    public void DamageObstacle(int column, int row)     // Used After IsObstacle 
    {
        TileObject tile = board.grid[column, row];
        bool isDestroyed = ((Obstacle)tile).Damage();   // Cast to Obstacle to call Damage

        if (isDestroyed)
        {
            AddToBeDestroyedIndividualTiles(new Vector2Int(column, row));
        }
    }
    #endregion


    // Blast Functions
    #region Normal Blast Functions
    public bool IsValidNeighbor(Vector2Int position)
    {

        bool isInBounds = position.x >= 0 && position.x < board.columns && position.y >= 0 && position.y < board.rows;
        if (!isInBounds)    return false;
        bool isNull = board.grid[position.x, position.y] == null; 
        if (isNull)         return false;
        bool isInValidState = board.grid[position.x, position.y].state == "0"; 
                            return isInValidState;
    }

    public bool IsObstacle(Vector2Int position)     // Used After IsValidNeighbor
    {

        bool isObstacleObject = (board.grid[position.x, position.y] is Obstacle);
        return isObstacleObject;
    }

    public void DamageNearTiles(Vector2Int position, Dictionary<Vector2Int, bool> visitedVases) 
    {

        int[] xdir = { 1, -1, 0, 0 };
        int[] ydir = { 0, 0, 1, -1 };

        for (int k = 0; k < 4; k++)
        {
            int c = position.x + xdir[k];
            int r = position.y + ydir[k];
            Vector2Int neighbor = new Vector2Int(c, r);
            if (IsValidNeighbor(neighbor))
            {
                if (IsObstacle(neighbor))
                {
                    TileObject tile = board.grid[neighbor.x, neighbor.y];
                    if (tile is Box)
                    {
                        DamageObstacle(neighbor.x, neighbor.y);
                    }
                    else if (tile is Stone)
                    {
                        continue;
                    }
                    else if (tile is Vase)
                    {
                        if (!visitedVases.ContainsKey(neighbor))
                        {
                            DamageObstacle(neighbor.x, neighbor.y);
                            visitedVases.Add(neighbor, true);
                        }
                    }
                }    
            }
        }
    }

    public void DestroyRegion(List<AnimationFunctions.RegionElement> region)
    {
        Debug.Log("DestroyRegion is executed");

        Dictionary<Vector2Int, bool> visitedVases = new Dictionary<Vector2Int, bool>();
        for (int i = 0; i < region.Count; i++)
        {
            bool isOriginal = region[i].isOriginalPosition;
            Vector2Int position = region[i].position;
            DamageNearTiles(position, visitedVases);
            if (isOriginal) // Create Rocket
            {
                Vector2 worldPosition = posFunctions.CalculateWorldPosition(position.x, position.y);
                (GameObject tileGO, bool direction) = board.GenerateRandomRocket(worldPosition);
                TileObject rocket;
                if (direction == false)
                {
                    rocket = new Rocket(position.x, position.y, RocketType.Horizontal, "0", worldPosition, 0, tileGO);
                }
                else
                {
                    rocket = new Rocket(position.x, position.y, RocketType.Vertical, "0", worldPosition, 0, tileGO);
                }
                DestroyTile(position);
                board.grid[position.x, position.y] = rocket;
            }
            else
            {
                DestroyTile(position);
            }
        }
    }

    public void AddToBeDestroyedRegion(List<Vector2Int> region)
    {

        List<AnimationFunctions.RegionElement> toBeAddRegion = new List<AnimationFunctions.RegionElement>();
        for (int i = 0; i < region.Count; i++)
        {
            Vector2Int position = region[i];
            AnimationFunctions.RegionElement element = new AnimationFunctions.RegionElement(position, false, false);
            toBeAddRegion.Add(element);
            board.grid[position.x, position.y].state = "-1";
        }
        animFunctions.toBeDestroyedRegion.Add(toBeAddRegion);
    }

    public void AddToBeDestroyedRegionAndCreateRocket(List<Vector2Int> region, Vector2Int originalPosition)
    {

        List<AnimationFunctions.RegionElement> toBeAddRegion = new List<AnimationFunctions.RegionElement>();
        for (int i = 0; i < region.Count; i++)
        {
            Vector2Int position = region[i];
            bool isOriginalPosition = position == originalPosition;
            AnimationFunctions.RegionElement element = new AnimationFunctions.RegionElement(position, false, isOriginalPosition);
            toBeAddRegion.Add(element);
            board.grid[position.x, position.y].state = "-1";
        }
        animFunctions.toBeDestroyedRegion.Add(toBeAddRegion);
    }
    #endregion


    // Rocket Functions
    #region Rocket Functions

    private bool IsRocket(Vector2Int position)
    {
        TileObject tile = board.grid[position.x, position.y];
        if (tile is Rocket) return true;
        return false;
    }

    private bool IsRocketable(Vector2Int position)
    {
        bool isInBounds = position.x >= 0 && position.x < board.columns && position.y >= 0 && position.y < board.rows;
        if (!isInBounds) return false;
        bool isNull = board.grid[position.x, position.y] == null;
        if (isNull) return false;
        return true;
    }

    public void AddToBeDestroyedIndividualTilesAndToBeDestroyedRockets(Vector2Int position)
    {

        // Alert: Precheck is needed to ensure it is a rocket tile

        TileObject tile = board.grid[position.x, position.y];
        Vector2 worldPosition = posFunctions.CalculateWorldPosition(position.x, position.y);
        GameObject rocketLeftUp;
        GameObject rocketRightDown;
        bool direction;

        if (((Rocket)tile).direction == RocketType.Horizontal)
        {
            rocketLeftUp = Instantiate(prefabs.horizontalRocketLeftPrefab, worldPosition, Quaternion.identity); ;
            rocketRightDown = Instantiate(prefabs.horizontalRocketRightPrefab, worldPosition, Quaternion.identity);
            direction = false;
        }
        else
        {
            rocketLeftUp = Instantiate(prefabs.verticalRocketUpPrefab, worldPosition, Quaternion.identity);
            rocketRightDown = Instantiate(prefabs.verticalRocketDownPrefab, worldPosition, Quaternion.identity);
            direction = true;
        }
        GameObject explosion = Instantiate(prefabs.explosion, worldPosition, Quaternion.identity);
        AnimationFunctions.RocketParts rocketData = new AnimationFunctions.RocketParts(rocketLeftUp, rocketRightDown, direction);
        animFunctions.toBeDestroyedRockets.Add(rocketData);

        board.grid[position.x, position.y].state = "-1";
        animFunctions.toBeDestroyedIndividualTiles.Add(position);
    }
   





    // TO DO :: Divide RocketComboHandler to modular pieces

    // Precondition: More than one rocket
    public void RocketComboHandler(List<Vector2Int> region, Vector2Int originalPosition)
    {
        int[] xdir = { 1, -1, 0, 0 };
        int[] ydir = { 0, 0, 1, -1 };

        for (int k = 0; k < 4; k++)
        {
            int c = originalPosition.x + xdir[k];
            int r = originalPosition.y + ydir[k];
            Vector2Int neighbor = new Vector2Int(c, r);
            if (IsValidNeighbor(neighbor))
            {
                if (IsRocket(neighbor))
                {
                    AddToBeDestroyedIndividualTiles(neighbor);
                }
            }
        }

        // 12 part Rocket Creation
        GameObject rocketUP;
        GameObject rocketDOWN;
        GameObject rocketRIGHT;
        GameObject rocketLEFT;
        AnimationFunctions.RocketParts rocketData;
        bool direction;

        // RIGHT One OnlyRightHorizontal; One Vertical;
        int column = originalPosition.x + xdir[0];
        int row = originalPosition.y + ydir[0];
        Vector2Int position = new Vector2Int(column, row);
        Vector2 worldPosition = posFunctions.CalculateWorldPosition(position.x, position.y);
        if (IsRocketable(position))
        {
            rocketRIGHT = Instantiate(prefabs.horizontalRocketRightPrefab, worldPosition, Quaternion.identity);
            rocketLEFT = null;
            direction = false;
            
            rocketData = new AnimationFunctions.RocketParts(rocketLEFT, rocketRIGHT, direction);
            animFunctions.toBeDestroyedRockets.Add(rocketData);

            rocketUP = Instantiate(prefabs.verticalRocketUpPrefab, worldPosition, Quaternion.identity);
            rocketDOWN = Instantiate(prefabs.verticalRocketDownPrefab, worldPosition, Quaternion.identity);
            direction = true;

            rocketData = new AnimationFunctions.RocketParts(rocketUP, rocketDOWN, direction);
            animFunctions.toBeDestroyedRockets.Add(rocketData);

            GameObject explosion = Instantiate(prefabs.explosion, worldPosition, Quaternion.identity);
        }

        // LEFT One OnlyLeftHorizontal; One Vertical
        column = originalPosition.x + xdir[1];
        row = originalPosition.y + ydir[1];
        position = new Vector2Int (column, row);
        worldPosition = posFunctions.CalculateWorldPosition(position.x, position.y);
        if (IsRocketable(position))
        {
            rocketRIGHT = null;
            rocketLEFT = Instantiate(prefabs.horizontalRocketLeftPrefab, worldPosition, Quaternion.identity);
            direction = false;

            rocketData = new AnimationFunctions.RocketParts(rocketLEFT, rocketRIGHT, direction);
            animFunctions.toBeDestroyedRockets.Add(rocketData);

            rocketUP = Instantiate(prefabs.verticalRocketUpPrefab, worldPosition, Quaternion.identity);
            rocketDOWN = Instantiate(prefabs.verticalRocketDownPrefab, worldPosition, Quaternion.identity);
            direction = true;

            rocketData = new AnimationFunctions.RocketParts(rocketUP, rocketDOWN, direction);
            animFunctions.toBeDestroyedRockets.Add(rocketData);

            GameObject explosion = Instantiate(prefabs.explosion, worldPosition, Quaternion.identity);
        }

        // UP One HoriztontalRocket; One OnlyUpVertical
        column = originalPosition.x + xdir[2];
        row = originalPosition.y + ydir[2];
        position = new Vector2Int(column, row);
        worldPosition = posFunctions.CalculateWorldPosition(position.x, position.y);
        if (IsRocketable(position))
        {
            rocketRIGHT = Instantiate(prefabs.horizontalRocketRightPrefab, worldPosition, Quaternion.identity);
            rocketLEFT = Instantiate(prefabs.horizontalRocketLeftPrefab, worldPosition, Quaternion.identity);
            direction = false;

            rocketData = new AnimationFunctions.RocketParts(rocketLEFT, rocketRIGHT, direction);
            animFunctions.toBeDestroyedRockets.Add(rocketData);

            rocketUP = Instantiate(prefabs.verticalRocketUpPrefab, worldPosition, Quaternion.identity);
            rocketDOWN = null;
            direction = true;

            rocketData = new AnimationFunctions.RocketParts(rocketUP, rocketDOWN, direction);
            animFunctions.toBeDestroyedRockets.Add(rocketData);

            GameObject explosion = Instantiate(prefabs.explosion, worldPosition, Quaternion.identity);
        }

        // DOWN One HorizontalRocket; One OnlyDownVertical
        column = originalPosition.x + xdir[3];
        row = originalPosition.y + ydir[3];
        position = new Vector2Int(column, row);
        worldPosition = posFunctions.CalculateWorldPosition(position.x, position.y);
        if (IsRocketable(position))
        {
            rocketRIGHT = Instantiate(prefabs.horizontalRocketRightPrefab, worldPosition, Quaternion.identity);
            rocketLEFT = Instantiate(prefabs.horizontalRocketLeftPrefab, worldPosition, Quaternion.identity);
            direction = false;

            rocketData = new AnimationFunctions.RocketParts(rocketLEFT, rocketRIGHT, direction);
            animFunctions.toBeDestroyedRockets.Add(rocketData);

            rocketUP = null;
            rocketDOWN = Instantiate(prefabs.verticalRocketDownPrefab, worldPosition, Quaternion.identity); ;
            direction = true;

            rocketData = new AnimationFunctions.RocketParts(rocketUP, rocketDOWN, direction);
            animFunctions.toBeDestroyedRockets.Add(rocketData);

            GameObject explosion = Instantiate(prefabs.explosion, worldPosition, Quaternion.identity);
        }

        AddToBeDestroyedIndividualTiles(originalPosition);
        for (int i = 0; i < region.Count; i++)
        {
            position = region[i];
            if (IsValidNeighbor(position))
            {
                AddToBeDestroyedIndividualTilesAndToBeDestroyedRockets(position);
            }
        }

    }
    #endregion


    // Individual Functions
    public void AddToBeDestroyedIndividualTiles(Vector2Int position)
    {
        board.grid[position.x, position.y].state = "-1";
        animFunctions.toBeDestroyedIndividualTiles.Add(position);
    }
}
