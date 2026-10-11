using UnityEngine;

[CreateAssetMenu(fileName = "NewOrganism", menuName = "Encyclopedia/Organism Data")]
public class OrganismData : ScriptableObject
{
    public string organismName;
    public Sprite photo;
    [TextArea(3, 10)]
    public string description;
    public string category;
}
