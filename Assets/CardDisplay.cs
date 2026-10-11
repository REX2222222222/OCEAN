using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Image photoImage;
    public TextMeshProUGUI descriptionText;

    public void SetupCard(OrganismData data)
    {
        nameText.text = data.organismName;
        photoImage.sprite = data.photo;
        descriptionText.text = data.description;
    }
}
