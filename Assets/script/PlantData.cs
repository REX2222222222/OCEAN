using UnityEngine;

// 這行會讓你在 Unity 的右鍵選單中看到自定義的創建按鈕
[CreateAssetMenu(fileName = "NewPlantData", menuName = "海洋圖鑑/植物資料卡")]
public class PlantData : ScriptableObject
{
    [Header("基本資料")]
    public string plantName;          // 植物名稱
    public string scientificName;     // 學名 (可選)

    [Header("詳細介紹 (支援捲動文字)")]
    [TextArea(10, 20)]               // 讓輸入框變大，方便輸入 100 字以上的介紹
    public string description;

    [Header("圖片資源")]
    public Sprite plantSprite;
	
	public string commonName;     
    public string feature;       
    public string habitat;     
	
	[Header("電子書串接")]
    public int bookPageIndex;   
}