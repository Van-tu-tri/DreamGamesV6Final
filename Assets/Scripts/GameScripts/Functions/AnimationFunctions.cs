using System.Collections.Generic;
using UnityEngine;


public class AnimationFunctions : MonoBehaviour
{
    #region References
    public Grid board;
    public BFSFunctions bfsFunctions;
    public ObstacleFunctions obstacleFunctions;
    public GravityFunctions gravityFunctions;
    public PosFunctions posFunctions;
    public Prefabs prefabs;

    private float rocketSpeed = 40f;
    private float initialShrinkSpeed = 0.5f;
    private float acceleration = 17f;
    private float minScale = 0.1f;
    #endregion

    #region Data Structures For Destroy And Animation


    // Region is basically blasted area. 
    public struct RegionElement
    {
        public Vector2Int position;
        public bool isDestroyed;
        public bool isOriginalPosition;
        public RegionElement(Vector2Int position,  bool isDestroyed, bool isOriginalposition)
        {
            this.position = position;
            this.isDestroyed = isDestroyed;
            this.isOriginalPosition = isOriginalposition;
        }

    }
    public struct RocketParts
    {
        public GameObject left_upRocket;
        public GameObject right_downRocket;
        public bool direction; // 0: Horizontal, 1: Vertical
        public Dictionary<Vector2Int, bool> visitedCells;

        public RocketParts(GameObject left_upRocket, GameObject right_downRocket, bool direction)
        {
            this.left_upRocket = left_upRocket;
            this.right_downRocket = right_downRocket;
            this.direction = direction;
            this.visitedCells = new Dictionary<Vector2Int, bool>();
        }
    }

    // Region Element is needed to control damage
    public List<List<RegionElement>> toBeDestroyedRegion = new List<List<RegionElement>>();

    public List<Vector2Int> toBeDestroyedIndividualTiles = new List<Vector2Int>();
   
    public List<RocketParts> toBeDestroyedRockets = new List<RocketParts>();
    #endregion

   
    // Gradually shrinks and destroy the objects at the same time
    public void ExecuteRegionDestroyAnimation()
    {   
        for (int i = toBeDestroyedRegion.Count - 1; i >= 0; i--)
        {
            List<RegionElement> region = toBeDestroyedRegion[i];
            bool isRegionDestroyed = true;

            for (int k = region.Count - 1; k >= 0; k--)
            {
                Vector2Int position = region[k].position;
                bool isElementDestroyed = region[k].isDestroyed;
                bool isOriginalPosition = region[k].isOriginalPosition;
                if (!isElementDestroyed)
                {
                    isRegionDestroyed = false;
                    TileObject tileObj = board.grid[position.x, position.y];
                    GameObject tile = board.grid[position.x, position.y].tile;
                    UnityEngine.Transform tileTransform = tile.transform;

                    if (tileObj.shrinkSpeed <= 0f)
                        tileObj.shrinkSpeed = initialShrinkSpeed;

                    float currentSpeed = tileObj.shrinkSpeed;
                    float deltaShrink = currentSpeed * Time.deltaTime;

                    tileTransform.localScale -= new Vector3(deltaShrink, deltaShrink, deltaShrink);
                    tileObj.shrinkSpeed = currentSpeed + acceleration * Time.deltaTime;

                    if (tileTransform.localScale.x <= minScale)
                    {
                        RegionElement updatedElement = new RegionElement(position, true, isOriginalPosition);
                        region[k] = updatedElement;
                    }
                }
            }

            if (isRegionDestroyed)
            {
                obstacleFunctions.DestroyRegion(region);
                toBeDestroyedRegion.RemoveAt(i);
            }
        }
    }

    public void ExecuteIndividualTileDestroyAnimation()
    {
        for (int i = toBeDestroyedIndividualTiles.Count - 1; i >= 0; i--)
        {
            Vector2Int position = toBeDestroyedIndividualTiles[i];
            TileObject tileObj = board.grid[position.x, position.y];
            GameObject tile = board.grid[position.x, position.y].tile;
            UnityEngine.Transform tileTransform = tile.transform;

            if (tileObj.shrinkSpeed <= 0f)
                tileObj.shrinkSpeed = initialShrinkSpeed;

            float currentSpeed = tileObj.shrinkSpeed;
            float deltaShrink = currentSpeed * Time.deltaTime;

            tileTransform.localScale -= new Vector3(deltaShrink, deltaShrink, deltaShrink);
            tileObj.shrinkSpeed = currentSpeed + acceleration * Time.deltaTime;

            if (tileTransform.localScale.x <= minScale)
            {
                obstacleFunctions.DestroyTile(position);
                toBeDestroyedIndividualTiles.RemoveAt(i);
            }
        }
    }


    #region Helper Functions for Rocket Animation
    private Vector2 UpdatePosition(GameObject rocketPart, float moveDistance, Vector3 direction) 
    {
        rocketPart.transform.position += direction * moveDistance;
        Vector2 leftPos = rocketPart.transform.position;
        return leftPos;
    }

    private void Hit(int column, int row, List<TileObject> waitingRockets, Dictionary<Vector2Int, bool> visitedCells)
    {
        if (column >= 0 && row >= 0 && column < board.columns && row < board.rows)
        {
            Vector2Int position = new Vector2Int(column, row);
            if (visitedCells.ContainsKey(position)) return;
            TileObject tileObj = board.grid[column, row];
            if (tileObj != null && tileObj.state == "0")
            {
                if (tileObj is Cube)
                {
                    tileObj.state = "-1";
                    toBeDestroyedIndividualTiles.Add(new Vector2Int(column, row));
                }
                else if (tileObj is Rocket)
                {
                    waitingRockets.Add(tileObj);
                }
                else if (tileObj is Obstacle)
                {
                    obstacleFunctions.DamageObstacle(column, row);
                }
                visitedCells.Add(position, true);
            }
        }
    }

    private void DestroyRocketIfOutOfBounds(GameObject rocketPart, RocketParts rocketParts, int direction)
    {
        // 0: left 1: right 2: up 3: down
        switch (direction)
        {
            case 0: //left
                if (rocketPart.transform.position.x < board.bottomLeftPosition.x - board.tileWidth * 2)
                {
                    Destroy(rocketPart);
                    rocketParts.left_upRocket = null;
                }
                return;
            case 1: //right
                if (rocketPart.transform.position.x > board.rightTopPosition.x + board.tileWidth * 2)
                {
                    Destroy(rocketPart);
                    rocketParts.right_downRocket = null;
                }
                return;
            case 2: //up
                if (rocketPart.transform.position.y > board.rightTopPosition.y + board.tileHeight * 2)
                {
                    Destroy(rocketPart);
                    rocketParts.left_upRocket = null;
                }
                return;
            case 3: //down
                if (rocketPart.transform.position.y < board.bottomLeftPosition.y - board.tileHeight * 2)
                {
                    Destroy(rocketPart);
                    rocketParts.right_downRocket = null;
                }
                return;
        }
        
    }
    #endregion

    // ALERT:: Rocket execute damage regularly-> Possible solution::Dictionary // DONE
    public void ExecuteRocketBlastAnimation()
    {
        float moveDistance = rocketSpeed * Time.deltaTime;
        List<TileObject> waitingRockets = new List<TileObject>();


        for (int i = toBeDestroyedRockets.Count - 1; i >= 0; i--)
        {
            // Left Rocket
            RocketParts rocketParts = toBeDestroyedRockets[i];
            if (rocketParts.left_upRocket != null)
            {
                // Horizontal Rocket
                if (rocketParts.direction == false)
                {
                    Vector2 leftPos = UpdatePosition(rocketParts.left_upRocket, moveDistance, Vector3.left);
                    var (column, row) = posFunctions.CalculateColumnRowFromWorldPosition(leftPos);
                    Hit(column, row, waitingRockets, rocketParts.visitedCells);
                    DestroyRocketIfOutOfBounds(rocketParts.left_upRocket, rocketParts, 0);
                }
                // Vertical Rocket
                else
                {
                    Vector2 leftPos = UpdatePosition(rocketParts.left_upRocket, moveDistance, Vector3.up);
                    var (column, row) = posFunctions.CalculateColumnRowFromWorldPosition(leftPos);
                    Hit(column, row, waitingRockets, rocketParts.visitedCells);
                    DestroyRocketIfOutOfBounds(rocketParts.left_upRocket, rocketParts, 2);

                }
            }
            // Right Rocket
            if (rocketParts.right_downRocket != null)
            {
                // Horizontal
                if (rocketParts.direction == false)
                {
                    Vector2 leftPos = UpdatePosition(rocketParts.right_downRocket, moveDistance, Vector3.right);
                    var (column, row) = posFunctions.CalculateColumnRowFromWorldPosition(leftPos);
                    Hit(column, row, waitingRockets, rocketParts.visitedCells);
                    DestroyRocketIfOutOfBounds(rocketParts.right_downRocket, rocketParts, 1);
                }
                // Vertical
                else
                {
                    Vector2 leftPos = UpdatePosition(rocketParts.right_downRocket, moveDistance, Vector3.down);
                    var (column, row) = posFunctions.CalculateColumnRowFromWorldPosition(leftPos);
                    Hit(column, row, waitingRockets, rocketParts.visitedCells);
                    DestroyRocketIfOutOfBounds(rocketParts.right_downRocket, rocketParts, 3);
                }
            }
            if (rocketParts.left_upRocket == null && rocketParts.right_downRocket == null)
            {
                toBeDestroyedRockets.RemoveAt(i);
            }
        }

        for (int i = 0; i < waitingRockets.Count; i++)
        {
            Vector2Int position = new Vector2Int(waitingRockets[i].internalColumn, waitingRockets[i].internalRow);
            obstacleFunctions.AddToBeDestroyedIndividualTilesAndToBeDestroyedRockets(position);
        }
    }
}
