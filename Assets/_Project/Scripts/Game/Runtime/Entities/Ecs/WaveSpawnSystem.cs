using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

// No "using Game;" in this file - see ProjectileVelocity.cs for why. Game-namespace types
// (Enemy/EntityStatsAuthoring/EntityCharacteristics/MoveSpeed/AttackStats/Health/UnitTeam/
// EnemySpawner) written fully qualified.
namespace EnemyEcs
{
    /// <summary>
    /// крок-9: replaces EnemySpawner's UniTask coroutine with per-frame ECS scheduling driven by
    /// SystemAPI.Time. EnemySpawner (now a passive Bridge, see its Start()) pushes wave data and
    /// the DiContainer once; every group in the current wave ticks its own stagger timer here.
    /// Melee types (carry EntityStatsAuthoring) spawn as pure ECS entities via EntityCommandBuffer,
    /// batched once per frame instead of крок-8's EnemyEcsSpawner (one immediate EntityManager
    /// structural change per enemy, now removed). Devil/HotDevil/Eye/BigEye still spawn as
    /// MonoBehaviour through the DiContainer, unchanged from EnemySpawner's old branch.
    /// </summary>
    public partial class WaveSpawnSystem : SystemBase
    {
        private struct SpawnGroupState
        {
            public Game.EntityStatsAuthoring Stats;
            public int Amount;
            public float Delay;
            public float Timer;
            public int SpawnedCount;
        }

        private List<Game.EnemySpawner.Wave> _waves;
        private Vector2 _range;
        private EntityArchetype _enemyArchetype;

        private readonly List<SpawnGroupState> _activeGroups = new();
        private int _currentWaveId;
        private float _waveElapsed;
        private bool _waveStarted;

        public void SetDependencies(List<Game.EnemySpawner.Wave> waves, Vector2 range)
        {
            _waves = waves;
            _range = range;
        }

        protected override void OnCreate()
        {
            _enemyArchetype = EntityManager.CreateArchetype(
                typeof(LocalTransform),
                typeof(Game.MoveSpeed),
                typeof(Game.AttackStats),
                typeof(Game.Health),
                typeof(Game.UnitTeam),
                typeof(AttackState),
                typeof(EnemyEcsType),
                typeof(EnemyAttackType));
        }

        protected override void OnUpdate()
        {
            if (_waves == null || _waves.Count == 0)
                return;

            if (!SystemAPI.TryGetSingleton(out PlayerPositionSingleton player) || !player.IsAlive)
                return;

            if (!_waveStarted)
            {
                StartWave(_currentWaveId);
                _waveStarted = true;
            }

            float deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            TickGroups(deltaTime, player.Position, ecb);

            ecb.Playback(EntityManager);
            ecb.Dispose();

            AdvanceWave(deltaTime);
        }

        private void StartWave(int waveId)
        {
            _waveElapsed = 0f;
            _activeGroups.Clear();

            foreach (Game.EnemySpawner.Wave.EnemySpawnParameters enemy in _waves[waveId].Enemies)
            {
                enemy.EnemyPrefab.TryGetComponent(out Game.EntityStatsAuthoring stats);

                _activeGroups.Add(new SpawnGroupState
                {
                    Stats = stats,
                    Amount = enemy.Amount,
                    Delay = (float)_waves[waveId].Duration / enemy.Amount,
                    Timer = 0f,
                    SpawnedCount = 0,
                });
            }
        }

        private void AdvanceWave(float deltaTime)
        {
            _waveElapsed += deltaTime;
            if (_waveElapsed < _waves[_currentWaveId].Duration || _currentWaveId >= _waves.Count - 1)
                return;

            _currentWaveId++;
            StartWave(_currentWaveId);
        }

        private void TickGroups(float deltaTime, float3 playerPosition, EntityCommandBuffer ecb)
        {
            for (var i = 0; i < _activeGroups.Count; i++)
            {
                SpawnGroupState group = _activeGroups[i];
                if (group.SpawnedCount >= group.Amount)
                    continue;

                group.Timer -= deltaTime;
                while (group.Timer <= 0f && group.SpawnedCount < group.Amount)
                {
                    Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(_range.x, _range.y);
                    var worldPosition = playerPosition + new float3(offset.x, 0f, offset.y);

                    if (group.Stats != null)
                        SpawnEnemy(ecb, group.Stats, worldPosition);

                    group.SpawnedCount++;
                    group.Timer += group.Delay;
                }

                _activeGroups[i] = group;
            }
        }

        private void SpawnEnemy(EntityCommandBuffer ecb, Game.EntityStatsAuthoring stats, float3 position)
        {
            Game.EntityCharacteristics characteristics = stats.Characteristics;
            Entity entity = ecb.CreateEntity(_enemyArchetype);

            ecb.SetComponent(entity, new LocalTransform { Position = position, Rotation = quaternion.identity, Scale = 1f });
            ecb.SetComponent(entity, new Game.MoveSpeed { Value = characteristics.MoveSpeed });
            ecb.SetComponent(entity, new Game.AttackStats
            {
                Attack = characteristics.Attack,
                RangeOfAttack = characteristics.RangeOfAttack,
                AttackSpeed = characteristics.AttackSpeed,
            });
            ecb.SetComponent(entity, new Game.Health { Value = characteristics.MaxHealth, Max = characteristics.MaxHealth });
            ecb.SetComponent(entity, new Game.UnitTeam { Value = stats.Team });
            ecb.SetComponent(entity, new AttackState { Phase = AttackPhase.Idle, Timer = 0f });
            ecb.SetComponent(entity, new EnemyEcsType { Value = stats.EnemyType });
            ecb.SetComponent(entity, new EnemyAttackType { Value = stats.AttackType });
        }
    }
}
