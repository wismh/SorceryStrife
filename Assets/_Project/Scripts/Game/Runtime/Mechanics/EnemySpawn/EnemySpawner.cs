using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EnemyEcs;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Game
{
    public class EnemySpawner : MonoBehaviour
    {
        [Serializable]
        public struct Wave
        {
            [Serializable]
            public struct EnemySpawnParameters
            {
                public Enemy EnemyPrefab;
                public int Amount;
            }

            public List<EnemySpawnParameters> Enemies;
            public int Duration;
        }

        [SerializeField] private List<Wave> _waves;
        [SerializeField] private Vector2 _range;

        private float _spawnDelay;
        private DiContainer _container;
        private Entity _player;
        private EnemyEcsSpawner _enemyEcsSpawner;

        [Inject]
        public void Construct(DiContainer container, Player player, EnemyEcsSpawner enemyEcsSpawner)
        {
            _container = container;
            _player = player.GetComponent<Entity>();
            _enemyEcsSpawner = enemyEcsSpawner;
        }

        private void Start()
        {
            SpawnRoutineAsync().Forget();
        }

        private async UniTaskVoid SpawnRoutineAsync()
        {
            if (_waves.Count == 0)
                return;

            var cancellationToken = this.GetCancellationTokenOnDestroy();
            var currentWaveId = 0;

            while (_player.IsAlive && currentWaveId < _waves.Count)
            {
                var currentWave = _waves[currentWaveId];

                foreach (var enemy in currentWave.Enemies)
                    SpawnEnemyAsync(currentWave.Duration, enemy, cancellationToken).Forget();

                await UniTask.WaitForSeconds(currentWave.Duration, cancellationToken: cancellationToken);
                currentWaveId++;
            }
        }

        private async UniTaskVoid SpawnEnemyAsync(float duration, Wave.EnemySpawnParameters parameters, CancellationToken cancellationToken)
        {
            var delay = duration / parameters.Amount;
            var spawnedNumber = 0;

            while (spawnedNumber < parameters.Amount)
            {
                var position = Random.insideUnitCircle.normalized * Random.Range(_range.x, _range.y);
                var worldPosition = _player.transform.position + new Vector3(position.x, 0, position.y);

                // крок-8: melee types (Minion/Mutant/Ogr/OldMutant) carry EntityStatsAuthoring and
                // spawn as pure ECS entities instead - see EnemyEcsSpawner. Devil/HotDevil/Eye/BigEye
                // don't have it and keep spawning as MonoBehaviour, unchanged.
                if (parameters.EnemyPrefab.TryGetComponent(out EntityStatsAuthoring stats))
                {
                    _enemyEcsSpawner.Spawn(stats, worldPosition);
                }
                else
                {
                    var clone = _container.InstantiatePrefabForComponent<Enemy>(parameters.EnemyPrefab);
                    clone.transform.position = worldPosition;
                }

                spawnedNumber += 1;
                await UniTask.WaitForSeconds(delay, cancellationToken: cancellationToken);
            }
        }
    }
}
