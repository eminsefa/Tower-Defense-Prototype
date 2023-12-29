using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Units.Tower.Scripts
{
    public class TowerVisual
    {
        private static readonly int s_Idle = Animator.StringToHash("Idle");
        private static readonly int s_Lose = Animator.StringToHash("Lose");

        private readonly Animator  m_Animator;
        private readonly TowerBase m_TowerBase;

        public TowerVisual(TowerBase towerBase)
        {
            m_TowerBase = towerBase;
            m_Animator  = towerBase.transform.GetComponentInChildren<Animator>();
        }

        public void Init()
        {
            m_Animator.SetBool(s_Idle, true);
            m_Animator.Play(s_Idle, 0, Random.Range(0f, 1f));
        }

        public void SetIdle(bool IsIdle)
        {
            m_Animator.SetBool(s_Idle, IsIdle);
        }

        public void SetDead()
        {
            m_Animator.SetTrigger(s_Lose);
        }
    }
}