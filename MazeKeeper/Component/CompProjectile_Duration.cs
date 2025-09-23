using MazeKeeper.Class;
using MazeKeeper.Manager;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompProjectile_Duration : CompProjectile
    {
        [SerializeField] Option<AnimationCurve> _moveHeightCurveOption;
        [SerializeField] Option<AnimationCurve> _moveSpeedCurveOption;

        [SerializeField] float _moveDuration;

        [SerializeField] bool _damageOnArrive;

        Vector3 m_MoveStartPos;
        Vector3 m_UpdatedTargetPos;


        void Update()
        {
            _elapsedTime += Time.deltaTime;

            if (Target != null)
            {
                m_UpdatedTargetPos = Target.TargetTransform.position;
            }
            else
            {
                Target = null;
            }

            var normalizedTime = _elapsedTime / _moveDuration;

            var dir = Vector3.Normalize(m_UpdatedTargetPos - transform.position);

            //커브 옵션값에 따라 시간 계산, 시간에 따라 이동
            if (_moveSpeedCurveOption.Enabled)
            {
                // 비등속 운동 
                normalizedTime = _moveSpeedCurveOption.Value.Evaluate(normalizedTime);
            }

            var nextPosition = Vector3.Lerp(m_MoveStartPos, m_UpdatedTargetPos, normalizedTime);

            if (_moveHeightCurveOption.Enabled)
            {
                var height = _moveHeightCurveOption.Value.Evaluate(normalizedTime);
                nextPosition.y += height;
            }

            //목표 바라보기
            if (_lookAtDir)
            {
                transform.LookAt(nextPosition);
            }

            transform.position = nextPosition;

            //피격 시
            if (normalizedTime >= 1.0f)
            {
                if (_damageOnArrive && Target != null)
                {
                    var enemy = Target.GetComponent<CompEnemy>();
                    if (enemy.IsValid())
                    {
                        GiveDamage(enemy, dir);
                        if (_hitAudioClip != null)
                        {
                            AudioManager.Instance.Play(_hitAudioClip);
                        }
                    }
                }
                Despawn();
            }
        }


        public override void Init(CompAttacker_Projectile owner)
        {
            base.Init(owner);

            m_MoveStartPos     = transform.position;
            m_UpdatedTargetPos = Target.TargetTransform.position;
            _elapsedTime       = 0f;
        }
    }
}