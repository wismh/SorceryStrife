using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Visual-only companion for an ECS-simulated melee enemy (крок-8) - Animator + death-fade
    /// logic ported from EnemyAnimator.cs, minus Entity/EntityDamagable/movement/attack logic
    /// (all of that lives in EnemyEcs systems now). Positioned every frame by whichever ECS
    /// entity currently owns it (EnemyCompanionAssignmentSystem); has no Entity/Team of its own.
    /// </summary>
    public class EnemyCompanion : MonoBehaviour
    {
        [SerializeField] private Material _deathMaterial;

        private static readonly int k_deathId = Animator.StringToHash("Base Layer.Death");
        private static readonly int k_attackId = Animator.StringToHash("Base Layer.Attack");
        private static readonly int k_opacity = Shader.PropertyToID("_Opacity");
        private static readonly int k_mainTex = Shader.PropertyToID("_MainTex");

        private Animator _animator;
        private AnimationClip _attackClip;
        private AnimationClip _deathClip;
        private SkinnedMeshRenderer[] _skinnedMeshRenderers;
        private Material[] _originalMaterials;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _attackClip = _animator.runtimeAnimatorController.animationClips.First(c => c.name == "Attack");
            _deathClip = _animator.runtimeAnimatorController.animationClips.First(c => c.name == "Death");

            _skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            _originalMaterials = _skinnedMeshRenderers.Select(r => r.sharedMaterial).ToArray();
        }

        public void SetTransform(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>Restores idle visuals - call right after acquiring from the pool, before reassigning to a new entity.</summary>
        public void ResetForReuse()
        {
            _animator.speed = 1;
            for (var i = 0; i < _skinnedMeshRenderers.Length; i++)
                _skinnedMeshRenderers[i].sharedMaterial = _originalMaterials[i];
        }

        public void PlayAttack(float duration)
        {
            _animator.speed = _attackClip.length / duration;
            _animator.Play(k_attackId);

            AttackRoutineAsync(duration).Forget();
        }

        public void PlayDeathAndRelease(Action onReleased)
        {
            const float duration = 2f;

            foreach (var skinned in _skinnedMeshRenderers)
            {
                var instance = new Material(_deathMaterial);
                var texture = skinned.material.mainTexture;

                skinned.material = instance;
                skinned.material.mainTexture = texture;
                skinned.material.SetTexture(k_mainTex, texture);

                var opacity = 1f;
                DOTween.To(() => opacity, value =>
                {
                    opacity = value;
                    instance.SetFloat(k_opacity, value);
                }, 0f, duration - 0.5f).SetDelay(0.5f);
            }

            _animator.speed = _deathClip.length / duration;
            _animator.Play(k_deathId);

            DeathRoutineAsync(duration, onReleased).Forget();
        }

        private async UniTaskVoid AttackRoutineAsync(float duration)
        {
            await UniTask.WaitForSeconds(duration, cancellationToken: this.GetCancellationTokenOnDestroy());
            _animator.speed = 1;
        }

        private async UniTaskVoid DeathRoutineAsync(float duration, Action onReleased)
        {
            await UniTask.WaitForSeconds(duration, cancellationToken: this.GetCancellationTokenOnDestroy());
            onReleased?.Invoke();
        }
    }
}
