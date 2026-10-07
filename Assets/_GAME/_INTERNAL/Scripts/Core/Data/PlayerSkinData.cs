using UnityEngine;

namespace Core.Data
{
    [System.Serializable]
    public class PlayerSkinData
    {
        public string SkinId;
        public Sprite IdlePose;
        public Sprite HitPreparePose;
        public Sprite HitPose;
    }
}