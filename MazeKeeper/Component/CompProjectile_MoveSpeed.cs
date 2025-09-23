using MazeKeeper.Class;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompProjectile_MoveSpeed : CompProjectile
    {
        [SerializeField] float                  _moveSpeed;
        [SerializeField] Option<AnimationCurve> _moveSpeedCurveOption;

        [SerializeField] bool _damageOnArrive;


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

            var curPos = transform.position;

            // Direction : 원점으로부터 상대적으로 존재하는 특정 좌표를 정규화한 값. (그 길이는 항상 1이다.)
            var dir = Vector3.Normalize(m_UpdatedTargetPos - curPos);

            var speed = _moveSpeed;

            // 커브 옵션 설정에 따라 속도 계산
            if (_moveSpeedCurveOption.Enabled)
            {
                speed *= _moveSpeedCurveOption.Value.Evaluate(_elapsedTime);
            }

            var distancePerFrame = speed * Time.deltaTime;

            // 다음 프레임에서의 위치 계산
            var nextPosition = curPos + dir * distancePerFrame;

            var targetSqrDist = Vector3.SqrMagnitude(m_UpdatedTargetPos - curPos);
            var newPosSqrDist = Vector3.SqrMagnitude(curPos - nextPosition);

            if (newPosSqrDist >= targetSqrDist)
            {
                nextPosition = m_UpdatedTargetPos;
            }

            // 목표 바라보기
            if (_lookAtDir)
            {
                transform.LookAt(nextPosition);
            }

            transform.position = nextPosition;

            //피격 시
            if (Vector3.SqrMagnitude(nextPosition - m_UpdatedTargetPos) <= 0.1f * 0.1f)
            {
                if (_damageOnArrive && Target != null)
                {
                    var enemy = Target.GetComponent<CompEnemy>();
                    if (enemy.IsValid())
                    {
                        GiveDamage(enemy, dir);
                    }
                }

                Despawn();
            }
        }


        public override void Init(CompAttacker_Projectile owner)
        {
            base.Init(owner);

            m_UpdatedTargetPos = Target.TargetTransform.position;
            _elapsedTime       = 0f;
        }
    }
}