namespace Core.Data
{
    [System.Serializable]
    public class ShopEntityDataRuntime
    {
        public string EntityID = "Default_Entity";
        public int EntityCost;
        public ShopItemType Type = ShopItemType.Skin;
        public string EntityDescription = "Default_Description";
        public bool IsPurchased = false;

        public ShopEntityDataRuntime(string entityID, int entityCost, ShopItemType type, string entityDescription, bool isPurchased)
        {
            EntityID = entityID;
            EntityCost = entityCost;
            Type = type;
            EntityDescription = entityDescription;
            IsPurchased = isPurchased;
        }
    }
}