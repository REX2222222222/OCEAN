using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UILineConnection : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // 👉 讓畫筆知道「誰是管家」，以便在連對或連錯時去報告
    [HideInInspector] public MatchGameManager gameManager; 

    [Header("綁定設定")]
    public LineRenderer lineRenderer; 
    public Transform startPoint; // 線條出發點 (木板右側邊緣)

    [Header("答案設定")]
    public string correctAnswerName; 

    [Header("畫筆設定")]
    public float pointSpacing = 0.1f; // 點與點的間距，控制線條的平滑度

    private Camera mainCam;
    private bool isSolved = false; // 紀錄這塊木板是否已經配對成功

    void Start()
    {
        mainCam = Camera.main;
        
        // 初始狀態下隱藏線條
        if (lineRenderer != null) lineRenderer.positionCount = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 如果還沒綁定畫筆，或是已經答對了，就不給畫線
        if (lineRenderer == null || isSolved) return; 
        
        // 開始畫線時，先設定第一個點
        lineRenderer.positionCount = 1;
        
        // 優先使用指定的 startPoint，沒設定的話才用木板中心
        Vector3 startPos = startPoint != null ? startPoint.position : transform.position;
        startPos.z -= 1f; // 稍微推近相機一點，確保線條畫在最上層
        lineRenderer.SetPosition(0, startPos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (lineRenderer == null || isSolved) return; 
        
        // 將滑鼠的 2D 螢幕座標轉換為 3D 世界座標
        Vector3 currentMousePos = eventData.position;
        currentMousePos.z = 10f; 
        Vector3 worldPos = mainCam.ScreenToWorldPoint(currentMousePos);
        worldPos.z = lineRenderer.GetPosition(0).z; 

        // 計算滑鼠現在位置跟「線條最後一個點」的距離
        Vector3 lastPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        
        // 如果距離大於我們設定的間距 (pointSpacing)，就在線上新增一個轉折點
        if (Vector3.Distance(lastPoint, worldPos) > pointSpacing)
        {
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, worldPos);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isSolved) return; 

        // 1. 發射 UI 雷達，掃描手指底下有什麼
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool isConnected = false;
        string expectedTargetName = "Answer_" + correctAnswerName; 

        // 2. 檢查雷達掃到的每一個物件
        foreach (RaycastResult result in results)
        {
            // 只要掃到的物件名字開頭是 "Answer_"，代表玩家確實連到了某個圖框
            if (result.gameObject.name.StartsWith("Answer_"))
            {
                // 3. 核對對錯！
                if (result.gameObject.name == expectedTargetName)
                {
                    Debug.Log("✅ 恭喜！連對了：" + correctAnswerName);
                    isConnected = true;
                    isSolved = true; // 鎖上！這條線永遠固定在這裡了
                    
                    // 🌟 呼叫管家：執行「配對成功」的加分、播音效與方塊消失流程
                    if (gameManager != null)
                    {
                        gameManager.OnPairSolved(this, result.gameObject);
                    }
                    
                    // 尋找圓框底下的 "EndPoint" (左側邊緣) 作為完美吸附點
                    Transform endPoint = result.gameObject.transform.Find("EndPoint");
                    Vector3 targetPos = endPoint != null ? endPoint.position : result.gameObject.transform.position;
                    targetPos.z = lineRenderer.GetPosition(0).z;
                    
                    // 把線條的最後一個點，強力吸附到圓框邊緣上
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, targetPos);
                }
                else
                {
                    string wrongName = result.gameObject.name.Replace("Answer_", "");
                    Debug.Log("❌ 哎呀，連錯了喔！這是：" + wrongName);
                    
                    // 🌟 呼叫管家：播放答錯音效
                    if (gameManager != null)
                    {
                        gameManager.PlayWrongSound();
                    }
                }
                break; // 既然找到答案框了，就停止掃描
            }
        }

        // 4. 如果沒連到正確答案，或者放開在空白處，就把線收回隱藏
        if (!isConnected && lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }

    // 提供給管家呼叫的「重置」功能 (換下一關或按重新開始時使用)
    public void ResetLine()
    {
        isSolved = false; // 解除鎖定
        if (lineRenderer != null) lineRenderer.positionCount = 0; // 清空線條
    }
}