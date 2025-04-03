
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.IO;
using System.Linq;
using static UnityEngine.Rendering.DebugUI;


public class GridManager : MonoBehaviour
{
    // Main Manager

    #region References
    public LevelData currentLevel;
    public Prefabs prefabs;
    public CamFunctions camFunctions;
    public PosFunctions posFunctions;
    public Grid board;
    public BFSFunctions bfsFunctions;
    public GravityFunctions gravityFunctions;
    public AnimationFunctions animFunctions;
    public UIFunctions uiFunctions;


    public UnityEngine.Transform maskTransform;
    public UnityEngine.RectTransform gridBackGroundTransform;
    public UnityEngine.RectTransform topUITransform;

    public GameObject winPanel;
    public UnityEngine.RectTransform winTransform;
    public GameObject losePanel;
    public UnityEngine.RectTransform loseTransform;



    public TMP_Text remainingMovesText;
    #endregion


    private bool gameContinue = true;
    private Levels levels = new Levels();


    #region Load-Save Functions
    private void LoadLevelFromSaveJson()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        SaveData saveData;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Debug.LogError("Save file not found at: " + filePath);
            return;
        }

        if (saveData.current_level <= 0 || saveData.current_level >= levels.levels.Length)
        {
            Debug.LogError("Saved level index is out of range: " + saveData.current_level);
            saveData.current_level = 1;
        }

        string level = levels.levels[saveData.current_level];

        TextAsset jsonFile = Resources.Load<TextAsset>("Levels/" + level);
        if (jsonFile == null)
        {
            Debug.LogError("Could not find level file in Resources: " + level);
            return;
        }

        currentLevel = JsonUtility.FromJson<LevelData>(jsonFile.text);
        Debug.Log("Loaded Level: " + currentLevel.level_number);
    }


    private void SaveLevel()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        SaveData saveData;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            saveData = new SaveData { current_level = 0 };
        }

        // Update the current level.
        saveData.current_level++;
        if (saveData.current_level >= 10)
            saveData.current_level = -1;  

        string newJson = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(filePath, newJson);
    }


    #endregion

    #region BoardGeneration
    private void GenerateGrid()
    {
        LoadLevelFromSaveJson();
        board.InitializeUI();
        board.CalculateTileSize();
   
        // Initialize Board Dimentions and Load Level
        board.CalculateBoardDimentions(currentLevel);
        board.CalculateMoveCount(currentLevel);

        List<string> layout = currentLevel.grid;
        for (int row = 0; row < board.rows; row++)
        {
            for (int column = 0; column < board.columns; column++)
            {
                string type = board.CalculateType(row, column, layout);
                Vector2 worldPosition = posFunctions.CalculateWorldPosition(column, row);
                TileObject tileObject = board.GenerateTile(new Vector2Int(column, row), type, "0", worldPosition, 0);
                if (tileObject is Obstacle)
                {
                    if (tileObject is Box) board.remainingBox++;
                    else if (tileObject is Stone) board.remainingStone++;
                    else if (tileObject is Vase) board.remainingVase++;
                    board.numberOfRemaningObstacles++;
                }
                board.grid[column, row] = tileObject;
            }
        }
        board.CalculateZeroPositions();
    }
    #endregion

    #region Setters

    // Sprite Mask
    private void SetMaskScale()
    {
        (float camHeight, float camWidth) = camFunctions.GetCamDimentions();
        maskTransform.localScale = new Vector3(camWidth, camHeight, 1f);
    }

    private void SetMaskPosition()
    {
        (float camHeight, float camWidth) = camFunctions.GetCamDimentions();
        maskTransform.localPosition = new Vector3(0, board.rightTopPosition.y + camHeight/2, 0);
        Debug.Log(board.rightTopPosition.y + camHeight / 2);
    }

    private void SetGridBackGround()
    {
        float gridWidth = (board.tileWidth * board.columns);
        float gridHeight = (board.tileHeight * board.rows);
        float backgroundWidth = gridWidth * 1.02f;
        float backgroundHeight = gridHeight * 1.02f;
        gridBackGroundTransform.sizeDelta = new Vector2(backgroundWidth, backgroundHeight);
        gridBackGroundTransform.position =
            new Vector2(board.bottomLeftPosition.x + gridWidth / 2, board.bottomLeftPosition.y + gridHeight / 2);
    }

    public void SetRemainingMoveUI()
    {
        remainingMovesText.text = board.remainingMoves.ToString();
        remainingMovesText.transform.DOKill();
        remainingMovesText.transform.localScale = Vector3.one;
        remainingMovesText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1);
    }

    public void SetWinPanel(RectTransform panel, float duration = 0.5f, Ease easeType = Ease.OutBack)
    {

        panel.localScale = Vector3.zero;
        panel.gameObject.SetActive(true);
        panel.DOScale(Vector3.one * 0.8f, duration).SetEase(easeType);
    }

    public void SetLosePanel(RectTransform panel, float duration = 0.5f, Ease easeType = Ease.OutBack)
    {
        panel.localScale = Vector3.zero;
        panel.gameObject.SetActive(true);
        panel.DOScale(Vector3.one * 0.8f, duration).SetEase(easeType);
    }
    #endregion

    #region Input And Move Counter
    void mouseClick()
        {
            if (Input.GetMouseButtonDown(0)) // 0 = left click
            {
                Vector3 mousePos = posFunctions.GetMousePosition();

                int row;
                int column;
                (column, row) = posFunctions.CalculateColumnRowFromWorldPosition(mousePos);

                // IsWorldPositionInBounds(Vector2Int)
                if (posFunctions.IsWorldPositionInBounds(mousePos)) Debug.Log("Out of bounds");
                else
                {

                    Vector2Int position = new Vector2Int(column, row);
                    bool isExecuted = bfsFunctions.executeBFS(position);
                    if (isExecuted)
                    {
                        board.remainingMoves--; SetRemainingMoveUI();
                    }
                    Debug.Log("BFS executed at: " + column + ", " + row);
                } 
            }
        }
        #endregion

    #region GameLoop
    void Awake()
    {
        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);

        GenerateGrid();
        SetMaskScale();
        SetMaskPosition();
        SetGridBackGround();
        SetRemainingMoveUI();
        uiFunctions.SetRemainingObstacles();
    }

    private void Update()
    {
        if (gameContinue)
        {

            bfsFunctions.ExecuteBFSMatch();
            if (board.remainingMoves > 0 && board.numberOfRemaningObstacles > 0)
            {
                mouseClick();
            }
            else
            {
                bool isIdle = board.CheckBoardState();
                if (isIdle)
                {
                    this.gameContinue = false;
                    if (board.numberOfRemaningObstacles == 0)
                    {
                        // win
                        Debug.Log("you win");
                        SetWinPanel(winTransform);
                        SaveLevel();
                    }
                    else
                    {
                        //lose
                        Debug.Log("you lose");
                        SetLosePanel(loseTransform);
                    }
                }
            }
            gravityFunctions.ExecuteGravity();
            gravityFunctions.ApplyGravity();
        }
    }

    private void LateUpdate()
    {
        if (gameContinue)
        { 
            animFunctions.ExecuteRegionDestroyAnimation();
            animFunctions.ExecuteRocketBlastAnimation();
            animFunctions.ExecuteIndividualTileDestroyAnimation();
        }
        // ExecuteBombCreationAnimation();
    }
    #endregion
    
}
    


