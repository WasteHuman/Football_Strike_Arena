using System.Collections.Generic;
using UnityEngine;

namespace SO.Game
{
    [CreateAssetMenu(menuName = "Game/Target Ball Configs/Target Ball Sprites Config", fileName = "TargetBallSpritesConfig")]
    public class TargetBallSpritesConfig : ScriptableObject
    {
        [field: SerializeField] public List<Sprite> TargetBallSprites { get; private set; } = new();

        private Sprite _lastRequestedSprite;

        public Sprite GetRandomSprite()
        {
            int maxAttemps = TargetBallSprites.Count * 2;
            int attemps = 0;
            Sprite sprite = null;

            while(attemps < maxAttemps)
            {
                var randomIndex = Random.Range(0, TargetBallSprites.Count);

                sprite = TargetBallSprites[randomIndex];

                if(sprite != _lastRequestedSprite)
                    break;
                    
                attemps++;
            }

            _lastRequestedSprite = sprite;

            return sprite;
        }
    }
}