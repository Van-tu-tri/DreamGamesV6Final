using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public Button button;  

    private void SetMainMenuButton()
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
            saveData = new SaveData { current_level = 1 };
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(filePath, json);
        }

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (saveData.current_level <= 0)
        {
            buttonText.text = "Finished";
            saveData.current_level = 1;
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(filePath, json);
        }
        else
        {
            buttonText.text = "Level " + saveData.current_level;
        }
    }

    private void Awake()
    {
        SetMainMenuButton();
    }
}

