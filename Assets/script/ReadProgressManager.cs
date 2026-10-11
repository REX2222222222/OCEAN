using UnityEngine;
using TMPro;

public class ReadProgressManager : MonoBehaviour
{
    public TMP_Text progressText; // 綁定你剛剛做好的文字

    void Start()
    {
        UpdateProgress(); // 遊戲一開始就更新一次數字
    }

    public void UpdateProgress()
    {
        // 讀取已經看了幾個，預設為 0
        int readCount = PlayerPrefs.GetInt("TotalReadCount", 0);
        
        if (progressText != null)
        {
            progressText.text = $"閱讀進度: {readCount} / 12";
        }
    }
}