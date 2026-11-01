namespace Core.Services.AdsService
{
    [System.Serializable]
    public class AdEntity
    {
        public AdEntityType PlacementType;
        public AdType Type;
        public string PlacementId;

        public AdEntity(AdEntityType placementType, AdType type, string placementId)
        {
            PlacementType = placementType;
            Type = type;
            PlacementId = placementId;
        }
    }
}