using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonAnimation : MonoBehaviour
{
    public RectTransform button;
    public static void AnimateButtonAndLoadScene(RectTransform button, string sceneName, float animationDuration = 0.2f, float pressScale = 0.9f)
    {
        button.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(button.DOScale(pressScale, animationDuration / 2).SetEase(Ease.InQuad));
        sequence.Append(button.DOScale(1f, animationDuration / 4).SetEase(Ease.OutQuad));
        sequence.OnComplete(() => {
            SceneManager.LoadScene(sceneName);
        });
    }

    public void OnButtonClicked()
    {
        AnimateButtonAndLoadScene(button, "GamePlayScene");
    }
}
