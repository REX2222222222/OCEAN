using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // 保持 DOTween 動畫
using UnityEngine.SceneManagement; // 用於重新開始遊戲

public class TreasureGameManager : MonoBehaviour
{
    [Header("資料庫")]
    public PlantDatabase database; 
    
    [Header("UI 顯示")]
    public TMP_Text targetHintText; 
    public TMP_Text scoreText;      

    [Header("音效系統")]
    public AudioSource audioSource; 
    public AudioClip correctSound;  
    public AudioClip wrongSound;    

    // 🌟 核心新增：時間限制區塊
    [Header("時間限制")]
    public float maxTime = 30f; // 總時間（秒）
    public TMP_Text timerText;  // 顯示時間的 UI 文字
    public GameObject gameOverPanel; // 遊戲結束的 UI 面板
    public TMP_Text finalScoreText; // 遊戲結束面板上的最終分數文字

    private PlantData currentTargetPlant;
    private int score = 0;
    
    // 🌟 核心新增：內部狀態變數
    private float timer;
    private bool isGameRunning = true;

    void Start()
    {
        // 🌟 初始化計時器
        timer = maxTime;
        isGameRunning = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(false); // 隱藏結束面板
        UpdateTimerUI();

        score = PlayerPrefs.GetInt("GameScore", 0); 
        UpdateScoreUI();
        PickNewTarget();
    }

    // 🌟 核心新增：每幀更新計時
    void Update()
    {
        if (!isGameRunning) return;

        // 倒數計時
        timer -= Time.deltaTime;
        UpdateTimerUI();

        // 時間到
        if (timer <= 0)
        {
            EndGame();
        }
    }

    void PickNewTarget()
    {
        // 如果遊戲已結束，不再換題
        if (!isGameRunning) return;

        if (database == null || database.allPlants.Count == 0) return;
        
        int randomIndex = Random.Range(0, database.allPlants.Count);
        currentTargetPlant = database.allPlants[randomIndex];

        if (targetHintText != null)
        {
            targetHintText.text = $"任務目標：請在潮間帶找到【{currentTargetPlant.plantName}】！";
        }

        TreasureObject[] allObjects = FindObjectsOfType<TreasureObject>();
        foreach (var obj in allObjects)
        {
            obj.gameManager = this;
        }
    }

    public void OnObjectClicked(TreasureObject clickedObject)
    {
        // 🌟 如果遊戲已結束，點擊不反應
        if (!isGameRunning) return;

        Image targetImage = clickedObject.GetComponent<Image>();
        RectTransform targetRect = clickedObject.GetComponent<RectTransform>();

        if (clickedObject.plantData == currentTargetPlant)
        {
            Debug.Log("✅ 點對了！");

            if (targetRect != null)
            {
                targetRect.DOKill(true); 
                targetRect.DOPunchScale(new Vector3(0.3f, 0.3f, 0), 0.5f, 5, 1f);
            }
            
            if (audioSource != null && correctSound != null)
            {
                audioSource.PlayOneShot(correctSound);
            }
            
            // 計分與解鎖邏輯 (保持不變)
            string unlockKey = "Unlocked_" + currentTargetPlant.plantName;
            
            if (PlayerPrefs.GetInt(unlockKey, 0) == 0)
            {
                score += 10;
                PlayerPrefs.SetInt(unlockKey, 1);
                
                int unlockCount = PlayerPrefs.GetInt("TotalUnlockedCount", 0);
                unlockCount++;
                PlayerPrefs.SetInt("TotalUnlockedCount", unlockCount);
                
                if (unlockCount >= 12 && PlayerPrefs.GetInt("GotCompleteBonus", 0) == 0)
                {
                    score += 50;
                    PlayerPrefs.SetInt("GotCompleteBonus", 1); 
                }
            }
            
            PlayerPrefs.SetInt("GameScore", score); 
            PlayerPrefs.Save();

            UpdateScoreUI();
            PickNewTarget(); 
        }
        else
        {
            Debug.Log("❌ 點錯囉，再找找看！");

            if (targetRect != null)
            {
                targetRect.DOKill(true); 
                targetRect.DOShakeAnchorPos(0.4f, new Vector2(20f, 0), 15, 90, false, true); 
            }

            if (targetImage != null)
            {
                targetImage.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(() => 
                {
                    targetImage.color = Color.white;
                });
            }
            
            if (audioSource != null && wrongSound != null)
            {
                audioSource.PlayOneShot(wrongSound);
            }
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "積分: " + score.ToString();
    }

    // 🌟 新增：更新時間 UI
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Mathf.Max 確保不會顯示負數，T0 顯示整數秒
            timerText.text = "剩餘時間: " + Mathf.Max(0, timer).ToString("F0") + "s";
            
            // 提示：剩餘 10 秒時把文字變紅色 (可選)
            if (timer <= 10f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white; // 假設預設是白色
            }
        }
    }

    // 🌟 新增：遊戲結束處理
    void EndGame()
    {
        isGameRunning = false;
        timer = 0; // 歸零
        UpdateTimerUI();

        Debug.Log("⛔ 時間到！遊戲結束。");

        // 顯示最終分數
        if (finalScoreText != null)
        {
            finalScoreText.text = "最終積分: " + score;
        }

        // 顯示遊戲結束面板
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // 提示：結束面板可以加個 DOTween 動畫滑入 (可選)
            // gameOverPanel.transform.localScale = Vector3.zero;
            // gameOverPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }

    // 🌟 新增：供介面按鈕呼叫的函數 - 重新開始遊戲
    public void RestartGame()
    {
        // 重新載入目前的場景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 🌟 新增：供介面按鈕呼叫的函數 - 返回圖鑑 (假設圖鑑場景叫 "BookScene")
    public void BackToBook()
    {
        SceneManager.LoadScene("BookScene"); 
    }
}