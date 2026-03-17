using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("결과물")]
    public ItemData resultItem;
    public int resultAmount = 1;
    public float craftTime = 3.0f; // ✨ 제작에 걸리는 시간 (초)

    [System.Serializable]
    public struct Ingredient // 재료 구조체
    {
        public ItemData item;
        public int amount;
    }

    [Header("필요 재료")]
    public List<Ingredient> ingredients;
}