using UnityEngine;
using TMPro;

public class BookSceneManager : MonoBehaviour
{
    public TMP_Text scoreText; // 綁定右上角的積分文字

    void Start()
    {
        // 讀取尋寶場景存下來的 "GameScore" 分數，如果沒玩過預設為 0
        int currentScore = PlayerPrefs.GetInt("GameScore", 0);
        
        if (scoreText != null)
        {
            scoreText.text = "積分: " + currentScore.ToString();
        }
    }
}