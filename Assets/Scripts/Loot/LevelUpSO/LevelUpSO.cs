using UnityEngine;

[CreateAssetMenu(fileName = "LevelUpSO", menuName = "Scriptable Objects/LevelUpSO")]
public class LevelUpSO : ScriptableObject
{
    public LevelUpType levelUpType;
    public float commonAmount;
    public float rareAmount;
    public float epicAmount;
    public float legendaryAmount;

    public float GetEffectAmount(LevelUpRarity rarity)
    {
        return rarity switch
        {
            LevelUpRarity.Common => commonAmount,
            LevelUpRarity.Rare => rareAmount,
            LevelUpRarity.Epic => epicAmount,
            LevelUpRarity.Legendary => legendaryAmount,
            _ => 0f,
        };
    }
}

