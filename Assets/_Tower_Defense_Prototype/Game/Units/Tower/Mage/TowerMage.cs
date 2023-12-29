using _Tower_Defense_Prototype.Game.Units.Tower.Scripts;

namespace _Tower_Defense_Prototype.Game.Units.Tower.Mage
{
    public class TowerMage : TowerBase
    {
        protected override void InitializeObject()
        {
            Attack = new MageAttack(this, s_PoolManager, s_Spline);
            Visual = new TowerVisual(this);
        }
    }
}