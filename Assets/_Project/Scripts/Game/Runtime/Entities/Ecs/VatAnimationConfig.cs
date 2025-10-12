using UnityEngine;

namespace EnemyEcs
{
    [CreateAssetMenu(fileName = "New Vat Config", menuName = "Game/Vat Animation Config")]
    public class VatAnimationConfig : ScriptableObject
    {
        [field: SerializeField] public Mesh Mesh { get; set; }
        [field: SerializeField] public Material Material { get; set; }
        [field: SerializeField] public Texture2D PositionTexture { get; set; }
        [field: SerializeField] public Texture2D NormalTexture { get; set; }
        [field: SerializeField] public Vector2 TextureSize { get; set; }

        [field: SerializeField] public int WalkStartFrame { get; set; }
        [field: SerializeField] public int WalkFrameCount { get; set; }

        [field: SerializeField] public int AttackStartFrame { get; set; }
        [field: SerializeField] public int AttackFrameCount { get; set; }

        [field: SerializeField] public int DeathStartFrame { get; set; }
        [field: SerializeField] public int DeathFrameCount { get; set; }

        [field: SerializeField] public float Fps { get; set; } = 24f;
    }
}
