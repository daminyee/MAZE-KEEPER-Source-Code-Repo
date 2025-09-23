using System;
using System.Collections;
using System.Collections.Generic;
using MazeKeeper.Class;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompAttacker_Projectile : CompAttacker
    {
        [Serializable]
        public class ContinuousFireInfo
        {
            [Header("공격 한 번에 발사할 횟수")]
            public int FireCount;
            [Header("연속 발사 간격")]
            public float FireInterval;
            [Header("연속 발사 최초 시작때 한번만 Play되는 사운드")]
            public SoAudioClip OneTimeAudioClip;
        }


        [Header("발사체")]
        [SerializeField] CompPoolItem _projectilePoolItem;

        [Header("연속 발사 옵션")]
        [SerializeField] Option<ContinuousFireInfo> continuousFireOption;

        [SerializeField] List<Transform> _firePointList;

        [SerializeField] CompPoolItem _muzzleFlashPoolItem;


        int m_FirePointIndex;


        /// <summary>
        /// Projectile한개당 데미지.
        /// </summary>
        public override float GetAttackDamage()
        {
            var damagePerProjectile = base.GetAttackDamage();
            if (continuousFireOption.Enabled)
            {
                damagePerProjectile /= continuousFireOption.Value.FireCount;
            }
            return damagePerProjectile;
        }


        public override void Attack()
        {
            base.Attack();
            StartCoroutine(CR_Attack());
        }


        IEnumerator CR_Attack()
        {
            m_IsAttack = true;

            AudioManager.Instance.Play(continuousFireOption.Value.OneTimeAudioClip);

            var fireCount = continuousFireOption.Enabled ? continuousFireOption.Value.FireCount : 1;

            //발사 횟수만큼 발사
            for (int i = 0; i < fireCount; i++)
            {
                m_FirePointIndex %= _firePointList.Count;
                var firePoint = _firePointList[m_FirePointIndex++];
                // 투사체 Spawn
                var projectile = PoolManager.Instance.Spawn(_projectilePoolItem, firePoint.position, firePoint.rotation, true).gameObject.GetComponent<CompProjectile>();
                projectile.Target = Target;
                projectile.Init(this);

                if (_muzzleFlashPoolItem != null)
                {
                    // 발사 이펙트
                    PoolManager.Instance.Spawn(_muzzleFlashPoolItem, firePoint.position, firePoint.rotation, true);
                }

                yield return new WaitForSeconds(continuousFireOption.Value.FireInterval / CompAttackerStat.GetAttackSpeedStat());
                if (IsValidTarget() == false) break;
            }

            m_IsAttack = false;
        }
    }
}