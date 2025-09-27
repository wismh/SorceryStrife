using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;
using Zenject;
using Entity = Unity.Entities.Entity;

namespace Game
{
    public class MagicFieldProjectile : MonoBehaviour
    {
        private static readonly int k_lifetimeId = Shader.PropertyToID("Lifetime");
        private static readonly int k_sizeId = Shader.PropertyToID("Size");

        private const float PushDistance = 2f;

        [SerializeField] private VisualEffect _firstEffect;
        [SerializeField] private VisualEffect _secondEffect;
        private SphereCollider _collider;
        private MagicFieldCaster _caster;

        private readonly List<EntityDamagable> _entities = new();
        private readonly HashSet<Entity> _hitEcsEnemies = new();

        public void Construct(MagicFieldCaster caster)
        {
            _caster = caster;
        }

        private void Awake()
        {
            _collider = GetComponentInChildren<SphereCollider>();
        }

        private void Start()
        {
            if (TryGetComponent(out TempObject tempObject))
                tempObject.TimeOfLife = _caster.Duration;

            _firstEffect.SetFloat(k_lifetimeId, _caster.Duration);
            _firstEffect.SetFloat(k_sizeId, _caster.Radius * 2f);
            _secondEffect.SetFloat(k_lifetimeId, _caster.Duration);
            _secondEffect.SetFloat(k_sizeId, _caster.Radius * 2f);

            _collider.radius = 0;

            DOTween.To(() => _collider.radius, value => _collider.radius = value, _caster.Radius, _caster.Duration / 2)
                .SetLoops(2, LoopType.Yoyo);
        }

        private void Update()
        {
            // Melee-type ECS enemies have no Collider (EnemyCompanion is visual-only), so
            // OnCollisionEnter below never sees them - push+damage them directly here instead,
            // using the collider's own currently-tweened radius so it matches what a real
            // enemy would physically feel this frame.
            EcsMeleeEnemyHits.DamageAndPushInRange(transform.position, _collider.radius, _caster.Damage, PushDistance, _hitEcsEnemies);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!other.transform.TryGetComponent(out EntityDamagable damagable))
                return;

            if (_entities.Contains(damagable))
                return;

            _entities.Add(damagable);
            damagable.Damage(_caster.Damage);
        }
    }
}
