using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.VFX;
using Zenject;

namespace Game
{
    public class MagicFieldProjectile : MonoBehaviour
    {
        private static readonly int k_lifetimeId = Shader.PropertyToID("Lifetime");
        private static readonly int k_sizeId = Shader.PropertyToID("Size");

        [SerializeField] private VisualEffect _firstEffect;
        [SerializeField] private VisualEffect _secondEffect;
        private SphereCollider _collider;
        private MagicFieldCaster _caster;

        private readonly HashSet<Unity.Entities.Entity> _hitEcsEnemies = new();

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
            EcsEnemyHits.PushAndDamageExpandingField(transform.position, _collider.radius, _caster.Damage, _hitEcsEnemies);
        }
    }
}
