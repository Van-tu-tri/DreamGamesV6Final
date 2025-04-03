using UnityEngine;

public class CamFunctions : MonoBehaviour
{
    public (float camHeight, float camWidth) GetCamDimentions()
    {
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;
        return (camHeight, camWidth);
    }
}
