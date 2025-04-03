using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class UIFunctions : MonoBehaviour
{
    public Transform position11;

    public Transform position21;
    public Transform position22;

    public Transform position31;
    public Transform position32;
    public Transform position33;


    public Grid board;
    public Prefabs prefabs;

    private GameObject remainingBox = null;
    private GameObject remainingStone = null;
    private GameObject remainingVase = null;

    public void SetRemainingObstacles()
    {
        int obstacleTypeCount = 0;
        if (board.remainingBox > 0) obstacleTypeCount++;
        if (board.remainingStone > 0) obstacleTypeCount++;
        if (board.remainingVase > 0) obstacleTypeCount++;   

        if (obstacleTypeCount == 1)
        {
            Vector2 position = position11.position;
            if (board.remainingBox > 0)
            {
                remainingBox = Instantiate(prefabs.remainingBoxPrefab, position, Quaternion.identity);
                TMP_Text tmpText = remainingBox.GetComponentInChildren<TMP_Text>();
                tmpText.text = board.remainingBox.ToString();
            }
            if (board.remainingStone > 0)
            {
                remainingStone = Instantiate(prefabs.remainingStonePrefab, position, Quaternion.identity);
                TMP_Text tmpText = remainingStone.GetComponentInChildren<TMP_Text>();
                tmpText.text = board.remainingStone.ToString();
            }
            if (board.remainingVase > 0)
            {
                remainingVase = Instantiate(prefabs.remainingVasePrefab, position, Quaternion.identity);
                TMP_Text tmpText = remainingVase.GetComponentInChildren<TMP_Text>();
                tmpText.text = board.remainingVase.ToString();
            }
        }
        else if (obstacleTypeCount == 2)
        {
            Vector2 position1 = position21.position;
            Vector2 position2 = position22.position;
            Vector2[] positions = { position1, position2 };
            int index = 0;
            if (board.remainingBox > 0)
            {
                remainingBox = Instantiate(prefabs.remainingBoxPrefab, positions[index], Quaternion.identity);
                TMP_Text tmpText = remainingBox.GetComponentInChildren<TMP_Text>();
                tmpText.text = board.remainingBox.ToString();
                index++;
            }
            if (board.remainingStone > 0)
            {
                remainingStone = Instantiate(prefabs.remainingStonePrefab, positions[index], Quaternion.identity);
                TMP_Text tmpText = remainingStone.GetComponentInChildren<TMP_Text>();
                tmpText.text = board.remainingStone.ToString();
                index++;
            }
            if (board.remainingVase > 0)
            {
                remainingVase = Instantiate(prefabs.remainingVasePrefab, positions[index], Quaternion.identity);
                TMP_Text tmpText = remainingVase.GetComponentInChildren<TMP_Text>();
                tmpText.text = board.remainingVase.ToString();
                index++;
            }
        }
        else if (obstacleTypeCount == 3)
        {
            Vector2 position1 = position31.position;
            Vector2 position2 = position32.position;
            Vector2 position3 = position33.position;
            Vector2[] positions = {position1, position2, position3};
            int index = 0;
            if (board.remainingBox > 0)
            {
                remainingBox = Instantiate(prefabs.remainingBoxPrefab, positions[index], Quaternion.identity);
                TMP_Text tmpText = remainingBox.GetComponentInChildren<TMP_Text>();
                tmpText.text = board.remainingBox.ToString();
                index++;
            }
            if (board.remainingStone > 0)
            {
                remainingStone = Instantiate(prefabs.remainingStonePrefab, positions[index], Quaternion.identity);
                TMP_Text tmpText = remainingStone.GetComponentInChildren<TMP_Text>();
                tmpText.text = board.remainingStone.ToString();
                index++;
            }
            if (board.remainingVase > 0)
            {
                remainingVase = Instantiate(prefabs.remainingVasePrefab, positions[index], Quaternion.identity);
                TMP_Text tmpText = remainingVase.GetComponentInChildren<TMP_Text>();
                tmpText.text = board.remainingVase.ToString();
                index++;
            }
        }
    }

    public void ChangeRemaningObstacleText(TileObject tile)
    {
        if (tile is Obstacle)
        {
            if (tile is Box)
            {
                if (remainingBox != null)
                {
                    TMP_Text tmpText = remainingBox.GetComponentInChildren<TMP_Text>();
                    tmpText.text = board.remainingBox.ToString();
                }
            }
            else if (tile is Stone)
            {
                if (remainingStone != null)
                {
                    TMP_Text tmpText = remainingStone.GetComponentInChildren<TMP_Text>();
                    tmpText.text = board.remainingStone.ToString();
                }
            }
            else if (tile is Vase)
            {
                if (remainingVase != null)
                {
                    TMP_Text tmpText = remainingVase.GetComponentInChildren<TMP_Text>();
                    tmpText.text = board.remainingVase.ToString();
                }
            }
        }
    }


}
