using UnityEngine;

namespace Core.Data
{
    [System.Serializable]
    public class ShopEntityData
    {
        public string EntityID = "Default_Entity";
        public Sprite EntityItemSprite;
        public Sprite EntitySkinSprite;
        public int EntityCost;
        public ShopItemType Type = ShopItemType.Skin;
        [TextArea] public string EntityDescription = "Default_Description";
        public bool IsPurchased = false;
    }
}