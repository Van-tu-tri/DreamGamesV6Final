using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public Prefabs prefabs;


    // Dimention Calculation
    public Vector2 bottomLeftPosition;
    public Vector2 rightTopPosition;

    public TileObject[,] grid;
    public int rows;
    public int columns;
    public float tileWidth;
    public float tileHeight;
    public int remainingMoves = 10;

    public int remainingBox = 0;
    public int remainingStone = 0;
    public int remainingVase = 0;
    public int numberOfRemaningObstacles;

    // Field initializer
    public void InitializeUI()
    {
        remainingBox = 0;
        remainingStone = 0;
        remainingVase = 0;
        numberOfRemaningObstacles = 0;
    }


    // Board State Functions
    #region Board State Functions

    public bool CheckBoardState()
    {
        for (int column = 0; column < columns; column++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (grid[column, row] == null) continue;
                if (grid[column, row].state != "0") return false;
            }
        }
        return true;
    }

    public void CalculateMoveCount(LevelData currentLevel)
    {
        this.remainingMoves = currentLevel.move_count;
    }
    #endregion

    // Dimention Calculators
    #region Dimention Calculators
    public void CalculateZeroPositions()
    {
        Vector2 offSet = new Vector2(tileWidth / 2, tileHeight / 2);
        this.bottomLeftPosition = this.grid[0, 0].position - offSet;
        this.rightTopPosition = this.grid[columns - 1, rows - 1].position + offSet;
        Debug.Log("Zero position is: " + this.bottomLeftPosition);
        Debug.Log("Top right position is: " + this.rightTopPosition);
    }

    public void CalculateTileSize()
    {
        SpriteRenderer sr = prefabs.referenceCube.GetComponent<SpriteRenderer>();
        this.tileWidth = sr.bounds.size.x * 1.05f;
        this.tileHeight = sr.bounds.size.y * 1.05f;
    }

    public void CalculateBoardDimentions(LevelData currentLevel)
    {
        this.rows = currentLevel.grid_height;
        this.columns = currentLevel.grid_width;
        this.grid = new TileObject[columns, rows];
        this.numberOfRemaningObstacles = 0;
    }

    #endregion

    // Board Generation
    #region BoardGeneration
    public TileObject GenerateRandomCube(Vector2Int internalPosition, string state, Vector2 position, float speed)
    {
        GameObject tileGO;
        int rand = UnityEngine.Random.Range(0, 4);
        switch (rand)
        {
            case 0:
                tileGO = Instantiate(prefabs.redCubePrefab, position, Quaternion.identity);
                return (new Cube(internalPosition.x, internalPosition.y, CubeColor.Red, state, position, speed, tileGO));
            case 1:
                tileGO = Instantiate(prefabs.blueCubePrefab, position, Quaternion.identity);
                return (new Cube(internalPosition.x, internalPosition.y, CubeColor.Blue, state, position, speed, tileGO));
            case 2:
                tileGO = Instantiate(prefabs.greenCubePrefab, position, Quaternion.identity);
                return (new Cube(internalPosition.x, internalPosition.y, CubeColor.Green, state, position, speed, tileGO));
            case 3:
                tileGO = Instantiate(prefabs.yellowCubePrefab, position, Quaternion.identity);
                return (new Cube(internalPosition.x, internalPosition.y, CubeColor.Yellow, state, position, speed, tileGO));
            default:
                tileGO = Instantiate(prefabs.yellowCubePrefab, position, Quaternion.identity);
                return (new Cube(internalPosition.x, internalPosition.y, CubeColor.Yellow, state, position, speed, tileGO));
        }
    }

    public TileObject GenerateTile(Vector2Int internalPosition, string tileType, string state, Vector2 position, float speed)
    {
        GameObject tileGO;
        switch (tileType)
        {
            case "r":
                tileGO = Instantiate(prefabs.redCubePrefab, position, Quaternion.identity);
                return (new Cube(internalPosition.x, internalPosition.y, CubeColor.Red, state, position, speed, tileGO));
            case "b":
                tileGO = Instantiate(prefabs.blueCubePrefab, position, Quaternion.identity);
                return (new Cube(internalPosition.x, internalPosition.y, CubeColor.Blue, state, position, speed, tileGO));
            case "g":
                tileGO = Instantiate(prefabs.greenCubePrefab, position, Quaternion.identity);
                return (new Cube(internalPosition.x, internalPosition.y, CubeColor.Green, state, position, speed, tileGO));
            case "y":
                tileGO = Instantiate(prefabs.yellowCubePrefab, position, Quaternion.identity);
                return (new Cube(internalPosition.x, internalPosition.y, CubeColor.Yellow, state, position, speed, tileGO));
            case "rand":
                return GenerateRandomCube(internalPosition, state, position, speed);
            case "bo":
                tileGO = Instantiate(prefabs.boxPrefab, position, Quaternion.identity);
                return (new Box(internalPosition.x, internalPosition.y, 1, state, position, speed, tileGO));
            case "s":
                tileGO = Instantiate(prefabs.stonePrefab, position, Quaternion.identity);
                return (new Stone(internalPosition.x, internalPosition.y, 1, state, position, speed, tileGO));
            case "v":
                tileGO = Instantiate(prefabs.vasePrefab, position, Quaternion.identity);
                return (new Vase(internalPosition.x, internalPosition.y, 2, state, position, speed, tileGO, prefabs));
            default: return null;
        }
    }

    public (GameObject gameObj, bool direction) GenerateRandomRocket(Vector2 worldPosition)
    {
        int rand = UnityEngine.Random.Range(0, 2);
        switch (rand)
        {
            case 0:
                return (Instantiate(prefabs.horizontalRocketPrefab, worldPosition, Quaternion.identity), false);
            case 1:
                return (Instantiate(prefabs.verticalRocketPrefab, worldPosition, Quaternion.identity), true);
            default: return (null, false);
        }
    }

    public string CalculateType(int row, int column, List<string> layout)
    {
        int index = row * columns + column;
        return layout[index];
    }

    #endregion
}
