using UnityEngine;
using UnityEngine.EventSystems;

public class TreasureObject : MonoBehaviour, IPointerClickHandler
{
    public PlantData plantData; // 綁定這顆海藻對應的資料卡
    [HideInInspector] public TreasureGameManager gameManager;

    // 當玩家用滑鼠或手指點擊這個 UI 圖片時，會自動觸發這個功能
    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager != null)
        {
            gameManager.OnObjectClicked(this);
        }
    }
}