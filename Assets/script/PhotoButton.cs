using UnityEngine;
using UnityEngine.UI;

public class PhotoButton : MonoBehaviour
{
    public PhotoPocketEffect effectController;
    public PlantData myData; // 在 Inspector 拖入對應的資料卡

    void Start()
    {
        UnityEngine.UI.Button btn = GetComponent<UnityEngine.UI.Button>();
        if (btn != null && myData != null)
        {
            // 點擊時，把整張資料卡傳給控制中心
            btn.onClick.AddListener(() => 
                effectController.OpenIntroFromData(GetComponent<RectTransform>(), myData)
            );
        }
    }
}