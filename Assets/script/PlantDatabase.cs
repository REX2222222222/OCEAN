using UnityEngine;
using System.Collections.Generic;

// 這個標籤讓你可以像建立資料卡一樣，在右鍵選單建立一個總資料庫
[CreateAssetMenu(fileName = "PlantDatabase", menuName = "海洋圖鑑/總資料庫")]
public class PlantDatabase : ScriptableObject
{
    [Header("把所有的植物資料卡拖進這個清單裡")]
    public List<PlantData> allPlants = new List<PlantData>();

    // 這個方法讓 YOLO UIManager 可以透過名字找到對應的資料卡
    public PlantData GetPlantByName(string name)
    {
        // 在清單中尋找名稱相符的植物
        return allPlants.Find(p => p.plantName == name);
    }
}