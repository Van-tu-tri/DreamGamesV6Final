using UnityEngine;


public class PosFunctions : MonoBehaviour
{
    public CamFunctions camFunctions;
    public Grid board;


    #region Position Functions
    public Vector2 CalculateWorldPosition(int column, int row)
    {
        Debug.Log("Calculate World position is executed");
        float camHeight, camWidth;
        (camHeight, camWidth) = camFunctions.GetCamDimentions();
        float gridWidth = board.columns * board.tileWidth;

        Vector2 position = new Vector2(column * board.tileWidth, row * board.tileHeight);
        Vector2 offset = new Vector2(gridWidth / 2 - board.tileWidth / 2, camHeight / 2.2f - board.tileHeight / 2);
        return position - offset;
    }

    public Vector2 CalculateOffset()
    {
        float camHeight, camWidth;
        (camHeight, camWidth) = camFunctions.GetCamDimentions();
        float gridWidth = board.columns * board.tileWidth;
        return new Vector2(gridWidth / 2 - board.tileWidth / 2, camHeight / 2.2f - board.tileHeight / 2);
    }

    public Vector2 GetMousePosition()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return mousePos;
    }

    public (int column, int row) CalculateColumnRowFromWorldPosition(Vector2 mousePos)
    {
        int row;
        int column;
        column = (int)((mousePos.x - board.bottomLeftPosition.x) / board.tileWidth);
        row = (int)((mousePos.y - board.bottomLeftPosition.y) / board.tileHeight);
        return (column, row);
    }

    public bool IsWorldPositionInBounds(Vector2 mousePos)
    {
        return mousePos.x < board.bottomLeftPosition.x || mousePos.x > board.rightTopPosition.x ||
                mousePos.y < board.bottomLeftPosition.y || mousePos.y > board.rightTopPosition.y;
    }


    // Used in Gravity Function
    public int CalculateCurrentRowFromWorldPosition(Vector2 position)
    {
        float y = position.y;
        float remainder = (y - board.bottomLeftPosition.y) % board.tileHeight;
        int row = (int)((y - board.bottomLeftPosition.y) / board.tileHeight);
        if (remainder > 0.5)
        {
            row++;
        }
        return row;
    }
    public Vector2 GetWorldPositionOfAttachedGameObject(TileObject tile)
    {
        return tile.tile.transform.position;
    }
    #endregion

}
