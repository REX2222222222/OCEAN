using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchGameManager : MonoBehaviour
{
    [Header("資料庫")]
    public PlantDatabase database; 

    [Header("左側題目區 (木板)")]
    public TMP_Text[] questionTexts;       
    public UILineConnection[] lineScripts; 

    [Header("右側答案區 (圓框)")]
    public Image[] answerImages;           
    public GameObject[] answerObjects;     

    [Header("計分與音效系統")]
    public TMP_Text scoreText;          
    public TMP_Text highScoreText;      
    public AudioSource audioSource;     
    public AudioClip correctSound;      
    public AudioClip wrongSound;        
    
    [Header("時間系統")]
    public TMP_Text timeText;           
    public float timeLimit = 30f;       
    
    // 👇 新增：時間到的 UI 面板
    public GameObject timeUpPanel;      
    
    private float currentTime;
    private bool isGameActive = false;  
    private int currentScore = 0;       
    private int highScore = 0;          
    private int solvedCount = 0; 

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateScoreUI(); 
        StartGame();
    }

    void Update()
    {
        if (isGameActive && currentTime > 0)
        {
            currentTime -= Time.deltaTime; 
            timeText.text = "TIME: " + Mathf.CeilToInt(currentTime).ToString();

            if (currentTime <= 0)
            {
                currentTime = 0;
                timeText.text = "TIME: 0";
                GameOver();
            }
        }
    }

    public void StartGame()
    {
        currentScore = 0;
        currentTime = timeLimit;
        isGameActive = true;
        
        // 🌟 遊戲開始時，隱藏「時間到」面板
        if (timeUpPanel != null) timeUpPanel.SetActive(false); 
        
        UpdateScoreUI();
        GenerateRandomQuestions();
    }

    public void GenerateRandomQuestions()
    {
        if (database.allPlants.Count < 3) return;

        solvedCount = 0;
        for (int i = 0; i < 3; i++)
        {
            lineScripts[i].gameObject.SetActive(true);
            answerObjects[i].gameObject.SetActive(true);
            lineScripts[i].ResetLine(); 
        }

        List<PlantData> pool = new List<PlantData>(database.allPlants);
        List<PlantData> selectedPlants = new List<PlantData>();

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            selectedPlants.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex); 
        }

        for (int i = 0; i < 3; i++)
        {
            questionTexts[i].text = $"{i + 1}. {selectedPlants[i].plantName}"; 
            lineScripts[i].correctAnswerName = selectedPlants[i].plantName; 
            lineScripts[i].gameManager = this; 
        }

        List<PlantData> shuffledAnswers = new List<PlantData>(selectedPlants);
        for (int i = 0; i < shuffledAnswers.Count; i++)
        {
            PlantData temp = shuffledAnswers[i];
            int randomIndex = Random.Range(i, shuffledAnswers.Count);
            shuffledAnswers[i] = shuffledAnswers[randomIndex];
            shuffledAnswers[randomIndex] = temp;
        }

        for (int i = 0; i < 3; i++)
        {
            answerImages[i].sprite = shuffledAnswers[i].plantSprite;
            answerObjects[i].name = "Answer_" + shuffledAnswers[i].plantName; 
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore); 
            PlayerPrefs.Save();
        }
        
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "SCORE: " + currentScore.ToString("D5"); 
        if (highScoreText != null) highScoreText.text = "BEST: " + highScore.ToString("D5");
    }

    public void PlayCorrectSound()
    {
        if (audioSource != null && correctSound != null) audioSource.PlayOneShot(correctSound);
    }

    public void PlayWrongSound()
    {
        if (audioSource != null && wrongSound != null) audioSource.PlayOneShot(wrongSound);
    }

    public void OnPairSolved(UILineConnection lineScript, GameObject rightBlock)
    {
        if (!isGameActive) return; 

        AddScore(100);       
        PlayCorrectSound();  
        StartCoroutine(HideBlocksAfterDelay(lineScript, rightBlock, 0.5f));
    }

    private IEnumerator HideBlocksAfterDelay(UILineConnection lineScript, GameObject rightBlock, float delay)
    {
        yield return new WaitForSeconds(delay);

        lineScript.gameObject.SetActive(false);
        rightBlock.SetActive(false);
        if (lineScript.lineRenderer != null) lineScript.lineRenderer.positionCount = 0;

        solvedCount++;
        
        if (solvedCount >= 3 && isGameActive)
        {
            AddScore(200); 
            Invoke("GenerateRandomQuestions", 0.5f); 
        }
    }

    private void GameOver()
    {
        isGameActive = false;
        Debug.Log("⏰ 時間到！遊戲結束！");
        
        // 🌟 時間到時，顯示「時間到」面板
        if (timeUpPanel != null) timeUpPanel.SetActive(true); 
        
        foreach (var script in lineScripts)
        {
            if (script != null) script.gameObject.SetActive(false);
        }
    }

    public void ResetGame()
    {
        StartGame();
    }
}