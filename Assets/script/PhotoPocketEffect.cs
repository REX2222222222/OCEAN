using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class PhotoPocketEffect : MonoBehaviour
{
    [Header("UI 設定")]
    public GameObject introWindowPrefab; 
    public RectTransform canvasRect; 
    
    [Header("動畫參數")]
    public float animationDuration = 0.5f; 
    public Ease animationEase = Ease.OutBack; 

    private GameObject currentWindow; 

    public void OpenIntroFromData(RectTransform photoRect, PlantData data)
    {
        if (currentWindow != null) return;

        // 1. 生成 Prefab
        currentWindow = Instantiate(introWindowPrefab, canvasRect);

        // 2. 叫 Prefab 讀取這份資料
        IntroWindowUI ui = currentWindow.GetComponent<IntroWindowUI>();
        if (ui != null) ui.LoadPlantData(data);

        // 3. 設定初始位置與 DOTween 動畫 (這段你之前不小心刪掉了！)
        RectTransform windowRect = currentWindow.GetComponent<RectTransform>();
        
        Vector2 startPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            RectTransformUtility.WorldToScreenPoint(null, photoRect.position), 
            null, 
            out startPos
        );
        
        // 設定初始縮小且位在點擊點
        windowRect.anchoredPosition = startPos;
        windowRect.localScale = Vector3.zero;
        windowRect.localRotation = Quaternion.Euler(0, 0, -20f);

        // 播放彈出動畫
        Sequence openSequence = DOTween.Sequence();
        openSequence.Join(windowRect.DOAnchorPos(Vector2.zero, animationDuration));
        openSequence.Join(windowRect.DOScale(Vector3.one, animationDuration));
        openSequence.Join(windowRect.DOLocalRotate(Vector3.zero, animationDuration));
        openSequence.SetEase(animationEase); 

        // 4. 綁定關閉按鈕
        Button closeBtn = currentWindow.GetComponentInChildren<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(CloseIntroWindow);
        }
    }

    public void CloseIntroWindow()
    {
        if (currentWindow == null) return;

        RectTransform windowRect = currentWindow.GetComponent<RectTransform>();

        // 播放縮小並消失動畫，然後銷毀物件
        windowRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(currentWindow);
            currentWindow = null;
        });
    }
}