using System.Collections.Generic;
using System.Linq;
using MazeKeeper.Class;
using MazeKeeper.Define;
using MazeKeeper.Manager;
using MazeKeeper.ScriptableObject;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompAttacker_Laser : CompAttacker
    {
        [SerializeField] LineRenderer _lineRenderer;
        [SerializeField] Option<int>  _chainOption;
        [SerializeField] Transform    _fireTransform;
        [SerializeField] CompPoolItem _hitEffectTemplate;
        [SerializeField] SoAudioClip  _hitAudioClip;
        [SerializeField] GameObject   _muzzleFlashGO;

        readonly List<LineRenderer> m_LineRendererList     = new();
        readonly List<CompEnemy>    m_ChainedCompEnemyList = new();
        readonly List<CompPoolItem> m_HitEffectList        = new();

        int ChainCount => _chainOption.Enabled ? _chainOption.Value : 1;


        void Start()
        {
            _lineRenderer.positionCount = 2;
            m_LineRendererList.Add(_lineRenderer);
            for (int i = 1; i < ChainCount; i++)
            {
                var newLine = Instantiate(_lineRenderer, transform);
                m_LineRendererList.Add(newLine);
            }
        }


        protected override void Update()
        {
            base.Update();

            //초기화
            if (HasTarget == false)
            {
                foreach (LineRenderer lineRenderer in m_LineRendererList)
                {
                    if (lineRenderer.gameObject.activeSelf)
                    {
                        lineRenderer.gameObject.SetActive(false);
                    }
                }

                if (m_HitEffectList.Count > 0)
                {
                    foreach (var item in m_HitEffectList)
                    {
                        PoolManager.Instance.Despawn(item);
                    }

                    m_HitEffectList.Clear();
                }

                if (_muzzleFlashGO && _muzzleFlashGO.activeSelf)
                {
                    _muzzleFlashGO.SetActive(false);
                }

                return;
            }

            if (_muzzleFlashGO)
            {
                _muzzleFlashGO.transform.position = _fireTransform.position;
                if (_muzzleFlashGO.activeSelf == false)
                {
                    _muzzleFlashGO.SetActive(true);
                }
            }

            UpdateChainedEnemyList();


            // Line Renderer 표시

            Vector3 previousPoint = _fireTransform.position;
            for (int i = 0; i < m_ChainedCompEnemyList.Count; i++)
            {
                var lineGO = m_LineRendererList[i].gameObject;
                if (lineGO.activeSelf == false) lineGO.SetActive(true);

                var chainedEnemy = m_ChainedCompEnemyList[i];
                m_LineRendererList[i].SetPosition(0, previousPoint);
                m_LineRendererList[i].SetPosition(1, chainedEnemy.TargetTransform.position);
                previousPoint = chainedEnemy.TargetTransform.position;
            }
            // 불필요한 Line은 Off
            for (int i = m_ChainedCompEnemyList.Count; i < ChainCount; i++)
            {
                m_LineRendererList[i].gameObject.SetActive(false);
            }


            // Hit Effect 표시. 부족하면 Spawn

            if (_hitEffectTemplate != null)
            {
                var diff = m_ChainedCompEnemyList.Count - m_HitEffectList.Count;

                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                    {
                        CompPoolItem poolItem = PoolManager.Instance.Spawn(_hitEffectTemplate, Vector3.zero, Quaternion.identity, false);
                        m_HitEffectList.Add(poolItem);
                    }
                }
                else if (diff < 0)
                {
                    for (int i = 0; i < Mathf.Abs(diff); i++)
                    {
                        var removeEffect = m_HitEffectList[^1];
                        m_HitEffectList.Remove(removeEffect);
                        PoolManager.Instance.Despawn(removeEffect);
                    }
                }

                for (int i = 0; i < m_HitEffectList.Count; i++)
                {
                    var hitEffect = m_HitEffectList[i];
                    hitEffect.transform.position = m_ChainedCompEnemyList[i].TargetTransform.position;
                    if (hitEffect.gameObject.activeSelf == false)
                    {
                        hitEffect.gameObject.SetActive(true);
                    }
                }
            }
        }


        void OnDisable()
        {
            foreach (var item in m_HitEffectList)
            {
                PoolManager.Instance.Despawn(item);
            }

            m_HitEffectList.Clear();
        }


        /// <summary>
        /// ChainCount 수 만큼 적에게 레이저가 튕기도록 리스트에 추가하는 함수
        /// </summary>
        public override void Attack()
        {
            base.Attack();

            // 두번째 타겟부터 Damage가 절반씩 감소한다.
            const float DecreaseDamage = 0.5f;

            if (m_ChainedCompEnemyList.Count == 0)
            {
                UpdateChainedEnemyList();
            }

            //체인 리스트에 있는 모든 대상 공격
            for (var i = 0; i < m_ChainedCompEnemyList.Count; i++)
            {
                CompEnemy chainedEnemy = m_ChainedCompEnemyList[i];
                if (chainedEnemy.IsValid() == false) continue;

                ApplyEnemyStatusEffect(chainedEnemy);

                chainedEnemy.GetDamage(GetAttackDamage() * Mathf.Pow(DecreaseDamage, i));
                if (_hitAudioClip != null)
                {
                    AudioManager.Instance.Play(_hitAudioClip);
                }
            }
        }


        void UpdateChainedEnemyList()
        {
            if (Target == null)
            {
                Debug.LogError($"{Target}이 {nameof(CompEnemy)}이 아닙니다.", this);
            }

            m_ChainedCompEnemyList.Clear();
            m_ChainedCompEnemyList.Add(Target);

            var range            = GetAttackRange(); // CompStat.GetFinalStatValue(StatType.AttackRange);
            var lastChainedEnemy = Target;

            for (int i = 0; i < ChainCount - 1; i++)
            {
                var foundEnemy = GameManager.Instance.GameEnemyController.FindEnemy(FindEnemyMethod.Closest, lastChainedEnemy.transform.position, range, m_ChainedCompEnemyList);
                if (foundEnemy == null)
                {
                    break;
                }

                lastChainedEnemy = foundEnemy;
                m_ChainedCompEnemyList.Add(lastChainedEnemy);
            }
        }
    }
}