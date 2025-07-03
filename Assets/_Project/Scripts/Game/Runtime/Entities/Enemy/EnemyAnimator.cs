using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game
{
    public class EnemyAnimator : MonoBehaviour
    {
        [SerializeField] private Material _deathMaterial;

        private static readonly int k_deathId = Animator.StringToHash("Base Layer.Death");
        private static readonly int k_attackId = Animator.StringToHash("Base Layer.Attack");
        private static readonly int k_opacity = Shader.PropertyToID("_Opacity");
        private static readonly int k_mainTex = Shader.PropertyToID("_MainTex");

        private Animator _animator;
        private Entity _entity;

        private EnemyMeleeFight _enemyMeleeFight;
        private EnemyRangeFight _enemyRangeFight;

        private AnimationClip _attackClip;
        private AnimationClip _deathClip;

        private void Awake()
        {
            _entity = GetComponent<Entity>();
            _animator = GetComponentInChildren<Animator>();
            _enemyMeleeFight = GetComponent<EnemyMeleeFight>();
            _enemyRangeFight = GetComponent<EnemyRangeFight>();
        }

        private void Start()
        {
            _attackClip = _animator
                .runtimeAnimatorController
                .animationClips
                .First(c => c.name == "Attack");

            _deathClip = _animator
                .runtimeAnimatorController
                .animationClips
                .First(c => c.name == "Death");

            _entity.OnDeath += DeathHandle;

            if (_enemyMeleeFight) _enemyMeleeFight.OnAttack += AttackHandle;
            if (_enemyRangeFight) _enemyRangeFight.OnAttack += AttackHandle;
        }

        private void OnDestroy()
        {
            _entity.OnDeath -= DeathHandle;

            if (_enemyMeleeFight) _enemyMeleeFight.OnAttack -= AttackHandle;
            if (_enemyRangeFight) _enemyRangeFight.OnAttack -= AttackHandle;
        }

        private void DeathHandle()
        {
            const float duration = 2;

            var renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skinned in renderers)
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

            DeathRoutineAsync(duration).Forget();
        }

        private void AttackHandle(float duration)
        {
            _animator.speed = _attackClip.length / duration;
            _animator.Play(k_attackId);

            AttackRoutineAsync(duration).Forget();
        }

        private async UniTaskVoid AttackRoutineAsync(float duration)
        {
            await UniTask.WaitForSeconds(duration, cancellationToken: this.GetCancellationTokenOnDestroy());
            _animator.speed = 1;
        }

        private async UniTaskVoid DeathRoutineAsync(float duration)
        {
            await UniTask.WaitForSeconds(duration, cancellationToken: this.GetCancellationTokenOnDestroy());
            Destroy(gameObject);
        }
    }
}
