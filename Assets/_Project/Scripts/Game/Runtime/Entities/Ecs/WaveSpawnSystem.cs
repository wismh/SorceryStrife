using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

// No "using Game;" in this file - see ProjectileVelocity.cs for why. Game-namespace types
// (Enemy/EntityStatsAuthoring/EntityCharacteristics/MoveSpeed/AttackStats/Health/UnitTeam/
// EnemySpawner) written fully qualified.
namespace EnemyEcs
{
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
        private Entity[] _enemyPrefabs;
        private VatAnimationConfig[] _vatConfigs;

        private readonly List<SpawnGroupState> _activeGroups = new();
        private int _currentWaveId;
        private float _waveElapsed;
        private bool _waveStarted;

        public void SetDependencies(List<Game.EnemySpawner.Wave> waves, Vector2 range)
        {
            _waves = waves;
            _range = range;
            InitializePrefabs();
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

        private void InitializePrefabs()
        {
            if (_enemyPrefabs != null)
                return;

            var desc = new RenderMeshDescription(
                ShadowCastingMode.On,
                receiveShadows: true);

            var types = (Game.EnemyType[])System.Enum.GetValues(typeof(Game.EnemyType));
            _enemyPrefabs = new Entity[types.Length];
            _vatConfigs = new VatAnimationConfig[types.Length];

            for (var i = 0; i < types.Length; i++)
            {
                Game.EnemyType type = types[i];
                var config = Resources.Load<VatAnimationConfig>($"VAT/{type}_VatConfig");
                _vatConfigs[i] = config;

                Entity prefab = EntityManager.CreateEntity();
                EntityManager.AddComponent<Prefab>(prefab);
                EntityManager.AddComponent<LocalTransform>(prefab);
                EntityManager.AddComponent<LocalToWorld>(prefab);
                EntityManager.AddComponent<Game.MoveSpeed>(prefab);
                EntityManager.AddComponent<Game.AttackStats>(prefab);
                EntityManager.AddComponent<Game.Health>(prefab);
                EntityManager.AddComponent<Game.UnitTeam>(prefab);
                EntityManager.AddComponent<AttackState>(prefab);
                EntityManager.AddComponent<EnemyEcsType>(prefab);
                EntityManager.AddComponent<EnemyAttackType>(prefab);
                EntityManager.AddComponent<EnemyVatPlayback>(prefab);
                EntityManager.AddComponent<EnemyVatAnimParams>(prefab);

                if (config != null && config.Material != null && config.Mesh != null)
                {
                    var rma = new RenderMeshArray(new[] { config.Material }, new[] { config.Mesh });
                    RenderMeshUtility.AddComponents(prefab, EntityManager, desc, rma, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
                }

                _enemyPrefabs[i] = prefab;
            }
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
            int typeIndex = (int)stats.EnemyType;

            Entity entity;
            if (_enemyPrefabs != null && typeIndex >= 0 && typeIndex < _enemyPrefabs.Length && _enemyPrefabs[typeIndex] != Entity.Null)
            {
                entity = ecb.Instantiate(_enemyPrefabs[typeIndex]);
            }
            else
            {
                entity = ecb.CreateEntity(_enemyArchetype);
            }

            float scale = (stats.EnemyType == Game.EnemyType.BigEye) ? 2.0f : 1.0f;
            ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(position, quaternion.identity, scale));
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

            if (_vatConfigs != null && typeIndex >= 0 && typeIndex < _vatConfigs.Length && _vatConfigs[typeIndex] != null)
            {
                VatAnimationConfig config = _vatConfigs[typeIndex];
                ecb.SetComponent(entity, new EnemyVatPlayback
                {
                    WalkStart = config.WalkStartFrame,
                    WalkCount = config.WalkFrameCount,
                    AttackStart = config.AttackStartFrame,
                    AttackCount = config.AttackFrameCount,
                    DeathStart = config.DeathStartFrame,
                    DeathCount = config.DeathFrameCount,
                    Fps = config.Fps,
                    Time = Random.Range(0f, 10f),
                });
                ecb.SetComponent(entity, new EnemyVatAnimParams
                {
                    Value = new float4(config.WalkStartFrame, config.WalkFrameCount, 0f, config.Fps)
                });
            }
        }
    }
}
