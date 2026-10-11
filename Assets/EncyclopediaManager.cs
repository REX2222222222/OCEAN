using UnityEngine;
using System.Collections.Generic;

public class EncyclopediaManager : MonoBehaviour
{
    public Transform contentContainer;
    public GameObject cardPrefab;
    public List<OrganismData> database;

    public void GenerateCardsByCategory(string targetCategory)
    {
        // 每次生成前先清除舊的卡片，避免重複疊加
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (OrganismData organism in database)
        {
            if (organism.category == targetCategory)
            {
                GameObject newCard = Instantiate(cardPrefab, contentContainer);
                newCard.transform.localPosition = Vector3.zero;
                newCard.transform.localScale = Vector3.one;
                newCard.GetComponent<CardDisplay>().SetupCard(organism);
            }
        }
    }
}
