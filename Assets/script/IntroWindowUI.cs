using UnityEngine;
using UnityEngine.UI;
using TMPro; // 使用 TextMeshPro 必加

public class IntroWindowUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text contentText;
    public Image displayImage;

    public void LoadPlantData(PlantData data)
    {
        if (data == null) return;

        if (titleText != null) titleText.text = data.plantName;
        if (contentText != null) contentText.text = data.description;
        if (displayImage != null) displayImage.sprite = data.plantSprite;

        // 強制讓老師做的 Scroll View 捲軸回到最上方
        ScrollRect scroll = GetComponentInChildren<ScrollRect>();
        if (scroll != null) scroll.verticalNormalizedPosition = 1f;
    }

    // 👇 這是新加的關閉功能！
}