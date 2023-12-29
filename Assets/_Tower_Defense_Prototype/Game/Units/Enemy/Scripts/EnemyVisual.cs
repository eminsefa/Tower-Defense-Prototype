using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Units.Enemy.Scripts
{
    public class EnemyVisual
    {
        private static readonly int s_Move = Animator.StringToHash("Move");
        private static readonly int s_Dead = Animator.StringToHash("Die");
        private static readonly int s_Win  = Animator.StringToHash("Win");
        private static readonly int s_Idle = Animator.StringToHash("Idle");

        private readonly EnemyBase      m_EnemyBase;
        private readonly Animator       m_Animator;
        private readonly Transform      m_HealthBar;
        private readonly SpriteRenderer m_Body;

        public EnemyVisual(EnemyBase enemyBase, Transform healthBar, SpriteRenderer body)
        {
            m_EnemyBase = enemyBase;
            m_Animator  = enemyBase.GetComponentInChildren<Animator>();
            m_HealthBar = healthBar;
            m_Body      = body;
        }

        public void Init()
        {
            m_Body.gameObject.SetActive(false);
            m_HealthBar.gameObject.SetActive(true);
            SetHealthBar(1);
        }

        public void EnableVisual()
        {
            m_Body.gameObject.SetActive(true);
            m_Animator.Play(s_Idle, 0, Random.Range(0f, 1f));
        }

        public void SetHealthBar(float healthP)
        {
            var s = m_HealthBar.localScale;
            s.x                    = Mathf.Lerp(0, 1, healthP);
            m_HealthBar.localScale = s;
        }

        public void StartMove()
        {
            m_Body.gameObject.SetActive(true);
            m_Animator.SetTrigger(s_Move);
        }

        public void SetSpriteDirection(bool flip)
        {
            m_Body.flipX = flip;
        }

        public void Dead()
        {
            m_Animator.SetTrigger(s_Dead);
        }

        public void LevelFailed()
        {
            m_HealthBar.gameObject.SetActive(false);
            m_Animator.Play(s_Win, 0, Random.Range(0f, 1f));
        }
    }
}