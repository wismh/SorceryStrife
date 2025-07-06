using System;
using System.Collections.Generic;
using EnemyEcs;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace Game
{
    /// <summary>
    /// крок-9: pure Zenject-DI-to-ECS bridge now - holds the scene-authored wave data and pushes it,
    /// plus the DiContainer needed for the still-MonoBehaviour enemy types, into WaveSpawnSystem once
    /// at startup. All per-frame spawn timing/scheduling lives in WaveSpawnSystem (ECS Systems aren't
    /// part of the Zenject container, so this is the same one-shot seam as EcsWorldBridge/EnemyEcsBridge).
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Serializable]
        public struct Wave
        {
            [Serializable]
            public struct EnemySpawnParameters
            {
                public GameObject EnemyPrefab;
                public int Amount;
            }

            public List<EnemySpawnParameters> Enemies;
            public int Duration;
        }

        [SerializeField] private List<Wave> _waves;
        [SerializeField] private Vector2 _range;

        private void Start()
        {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<WaveSpawnSystem>()
                .SetDependencies(_waves, _range);
        }
    }
}
