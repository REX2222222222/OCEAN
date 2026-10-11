using UnityEngine;
using UnityEngine.UI;

public class AutoOpenIntro : MonoBehaviour
{
    void Start()
    {
        // 1. 檢查有沒有從圖鑑場景帶過來的「暗號紙條」？
        if (PlayerPrefs.HasKey("TargetPlant"))
        {
            // 讀取紙條上寫的名字 (例如 "Monostroma")
            string targetName = PlayerPrefs.GetString("TargetPlant");
            
            // 2. 在場景中尋找這個名字的物件
            GameObject targetObj = GameObject.Find(targetName);
            
            if (targetObj != null)
            {
                // 判斷一：如果你的物件是個按鈕，我們就用程式幫玩家「點」它一下來觸發動畫
                Button btn = targetObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.Invoke(); 
                }
                else
                {
                    // 判斷二：如果它不是按鈕，只是被隱藏起來的面板，我們就直接顯示它
                    targetObj.SetActive(true);
                }
            }

            // 3. 任務完成，把紙條燒掉！(避免下次單純進 book 場景時又亂彈出面板)
            PlayerPrefs.DeleteKey("TargetPlant"); 
        }
    }
}