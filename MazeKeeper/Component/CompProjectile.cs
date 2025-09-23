using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;
using UnityEngine.Serialization;


namespace MazeKeeper.Component
{
    public abstract class CompProjectile : MonoBehaviour
    {
        public CompEnemy Target { get; set; }

        [SerializeField] protected bool  _lookAtDir;
        [SerializeField] protected float _elapsedTime = 0f;

        [SerializeField] protected CompPoolItem _shootEffect;
        [SerializeField] protected SoAudioClip  _shootAudioClip;

        [SerializeField] protected CompPoolItem _hitEffect;
        [FormerlySerializedAs("_hitSFXSound")]
        [SerializeField] protected SoAudioClip _hitAudioClip;

        protected CompAttacker_Projectile m_OwnerAttacker;


        public virtual void Init(CompAttacker_Projectile owner)
        {
            m_OwnerAttacker = owner;
            PlayShootEffectAndAudio();
        }


        protected virtual void PlayHitEffectAndAudio()
        {
            if (_hitAudioClip != null)
            {
                AudioManager.Instance.Play(_hitAudioClip);
            }
            if (_hitEffect != null)
            {
                PoolManager.Instance.Spawn(_hitEffect, transform.position, transform.rotation, true);
            }
        }


        protected virtual void Despawn()
        {
            PoolManager.Instance.Despawn(GetComponent<CompPoolItem>());
        }


        protected void GiveDamage(CompEnemy enemy, Vector3? direction = null)
        {
            PlayHitEffectAndAudio();

            // 부모 Attacker에서 목표에게 상태이상 부여
            m_OwnerAttacker.ApplyEnemyStatusEffect(enemy);
            // 부모 Attacker에서 피해량을 가져와 적에게 피해를 입힘
            enemy.GetDamage(m_OwnerAttacker.GetAttackDamage(), direction);
        }


        void PlayShootEffectAndAudio()
        {
            if (_shootAudioClip != null)
            {
                AudioManager.Instance.Play(_shootAudioClip);
            }
            if (_shootEffect != null)
            {
                PoolManager.Instance.Spawn(_shootEffect, transform.position, transform.rotation, true);
            }
        }
    }
}