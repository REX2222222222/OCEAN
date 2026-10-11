using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookGridItem : MonoBehaviour
{
    [Header("綁定植物資料")]
    public PlantData plantData;     

    [Header("綁定卡片 UI")]
    public Image plantImage;        
    public TMP_Text plantNameText;  
    public Button introButton;      

    // 尋找場景中的動畫總管
    private PhotoPocketEffect photoEffectManager;

    void Start()
    {
        // 遊戲開始時，自動找到掛著 PhotoPocketEffect 的物件
        photoEffectManager = FindObjectOfType<PhotoPocketEffect>();

        if (plantData == null) return;

        // 填入名稱與圖片
        plantNameText.text = plantData.plantName;
        plantImage.sprite = plantData.plantSprite;

        // 🌟 確保圖片是全彩的，不受之前設定影響
        if (plantImage != null)
        {
            plantImage.color = Color.white; 
        }
        
        // 🌟 確保按鈕隨時可以點擊，並綁定彈出動畫功能
        if (introButton != null)
        {
            introButton.interactable = true; 
            introButton.onClick.AddListener(OpenIntroWithAnimation);
        }
    }

    void OpenIntroWithAnimation()
    {
        if (photoEffectManager != null)
        {
            // 1. 呼叫動畫總管播起動畫
            RectTransform myRect = GetComponent<RectTransform>();
            photoEffectManager.OpenIntroFromData(myRect, plantData);

            // 2. 🌟 閱讀故事加 5 分 (防呆機制：每種限加一次)
            string readKey = "ReadStory_" + plantData.plantName; 
            
            // 如果這個故事還沒被讀過 (數值為 0)
            if (PlayerPrefs.GetInt(readKey, 0) == 0)
            {
                // 讀取目前總分，加上 5 分
                int currentScore = PlayerPrefs.GetInt("GameScore", 0);
                currentScore += 5;
                
                // 存檔並標記為「已讀」
                PlayerPrefs.SetInt("GameScore", currentScore);
                PlayerPrefs.SetInt(readKey, 1); 

                // 👇====== 新增：記錄閱讀總數 ======👇
                int readCount = PlayerPrefs.GetInt("TotalReadCount", 0);
                readCount++; // 閱讀數 +1
                PlayerPrefs.SetInt("TotalReadCount", readCount);
                // 👆===============================👆

                PlayerPrefs.Save();

                // 🌟 即時更新畫面右上角的積分文字
                GameObject scoreObj = GameObject.Find("SCORE");
                if (scoreObj != null)
                {
                    TMP_Text scoreText = scoreObj.GetComponentInChildren<TMP_Text>();
                    if (scoreText != null) scoreText.text = "積分: " + currentScore.ToString();
                }

                // 👇====== 新增：通知畫面更新進度數字 ======👇
                ReadProgressManager progressUI = FindObjectOfType<ReadProgressManager>();
                if (progressUI != null)
                {
                    progressUI.UpdateProgress();
                }
                // 👆=====================================👆
            }
        }
    }
}