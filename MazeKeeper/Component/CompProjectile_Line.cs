using MazeKeeper.Class;
using MazeKeeper.Manager;
using UnityEngine;


namespace MazeKeeper.Component
{
    public class CompProjectile_Line : CompProjectile
    {
        [SerializeField] Option<AnimationCurve> _moveSpeedCurveOption;

        [SerializeField] LineRenderer _lineRenderer;

        [SerializeField] float _lifeTime;

        private Vector3 m_StartPosition;
        private Vector3 m_TargetPosition;


        void Update()
        {
            _elapsedTime += Time.deltaTime;

            //터렛에서 적을 향해 총알을 발사하는듯한 이펙트
            if (_moveSpeedCurveOption.Enabled)
            {
                var normalizedTime = _elapsedTime / _lifeTime;

                normalizedTime = _moveSpeedCurveOption.Value.Evaluate(normalizedTime);
                var nextPosition = Vector3.Lerp(m_StartPosition, m_TargetPosition, normalizedTime);

                _lineRenderer.SetPosition(0, nextPosition);
            }

            if (_elapsedTime >= _lifeTime)
            {
                Despawn();
            }
        }


        public override void Init(CompAttacker_Projectile owner)
        {
            base.Init(owner);

            _elapsedTime = 0f;

            m_StartPosition  = transform.position;
            m_TargetPosition = Target.TargetTransform.position;

            //적과 터렛에 LineRenderer 점 찍기
            if (Target != null)
            {
                _lineRenderer.SetPosition(0, m_StartPosition);
                _lineRenderer.SetPosition(1, m_TargetPosition);

                var enemy = Target.GetComponent<CompEnemy>();
                if (enemy.IsValid())
                {
                    GiveDamage(enemy);
                }
            }
        }


        protected override void PlayHitEffectAndAudio()
        {
            if (_hitAudioClip != null)
            {
                AudioManager.Instance.Play(_hitAudioClip);
            }
            if (_hitEffect != null)
            {
                PoolManager.Instance.Spawn(_hitEffect, Target.TargetTransform.position, Target.TargetTransform.rotation, true);
            }
        }
    }
}