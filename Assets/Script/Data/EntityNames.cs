using UnityEngine;

public enum EntityNames
{
    Chest,
    Crafting,
    MonsterBox
}

public class ChooseEntity : MonoBehaviour
{
    public EntityNames entityType;
}