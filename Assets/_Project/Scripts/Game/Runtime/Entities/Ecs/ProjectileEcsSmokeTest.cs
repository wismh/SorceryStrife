using Game;
using Unity.Mathematics;
using UnityEngine;
using Zenject;

namespace ProjectileEcs
{
    /// <summary>
    /// THROWAWAY verification harness for крок-7's ECS projectile infra - fires a few plain-mesh
    /// test projectiles from the player on start so the pipeline (movement, hit-detection,
    /// EntityDamagable damage, Entities Graphics render) is visible in Play mode instead of being
    /// inert code nobody can check. Delete this once a real Caster calls ProjectileEcsSpawner
    /// directly.
    /// </summary>
    public class ProjectileEcsSmokeTest : MonoBehaviour
    {
        [SerializeField] private float _speed = 8f;
        [SerializeField] private float _damage = 5f;
        [SerializeField] private float _lifetime = 5f;

        private Player _player;
        private ProjectileEcsSpawner _spawner;

        [Inject]
        public void Construct(Player player, ProjectileEcsSpawner spawner)
        {
            _player = player;
            _spawner = spawner;
        }

        private void Start()
        {
            float3 origin = _player.transform.position;

            foreach (float3 direction in new[] { new float3(1, 0, 0), new float3(0.7f, 0, 0.7f), new float3(0.7f, 0, -0.7f) })
            {
                _spawner.SpawnProjectile(origin, direction * _speed, _damage, Team.Ally, _lifetime);
            }
        }
    }
}
