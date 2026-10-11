using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class YoloUIManager : MonoBehaviour
{
    [Header("系統連結")]
    public PlantDatabase database;             // 你的總資料庫 (要在 Inspector 拖入)
    public PhotoPocketEffect effectController; // 負責彈出動畫的控制器

    [Header("提示視窗 UI")]
    public GameObject resultPanel;             // 剛剛做的小提示視窗 (YoloResult_Panel)
    public TMP_Text resultText;                // 提示文字
    public Button viewDetailsBtn;              // 查看介紹的按鈕

    private PlantData currentDetectedData;     // 暫存目前辨識到的植物資料

    // 當 YOLO 辨識出結果時，呼叫這個方法並傳入植物名稱
    public void OnPlantDetected(string detectedName)
    {
        // 1. 拿著 YOLO 給的名字，去資料庫找資料卡
        currentDetectedData = database.GetPlantByName(detectedName);

        // 2. 如果資料庫裡有這個植物，就顯示提示視窗
        if (currentDetectedData != null)
        {
            resultText.text = "發現目標：" + currentDetectedData.plantName + "！";
            resultPanel.SetActive(true); // 顯示提示框

            // 3. 綁定按鈕：點擊後打開詳細介紹
            viewDetailsBtn.onClick.RemoveAllListeners(); // 清除之前的綁定
            viewDetailsBtn.onClick.AddListener(OpenIntroWindow);
        }
        else
        {
            Debug.Log("資料庫裡找不到這個植物：" + detectedName);
        }
    }

    // 按下「查看介紹」後執行的動作
    private void OpenIntroWindow()
    {
        resultPanel.SetActive(false); // 關閉提示框

        // 呼叫你之前做好的彈出動畫！起點就設定在這個「查看介紹」按鈕的位置
        RectTransform btnRect = viewDetailsBtn.GetComponent<RectTransform>();
        effectController.OpenIntroFromData(btnRect, currentDetectedData);
    }
}