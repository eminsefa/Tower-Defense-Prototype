using _Tower_Defense_Prototype.Game.Units.Tower.Scripts;

namespace _Tower_Defense_Prototype.Game.Units.Tower.Bowman
{
    public class TowerBowman : TowerBase
    {
        protected override void InitializeObject()
        {
            Attack = new BowmanAttack(this, s_PoolManager, s_Spline);
            Visual = new TowerVisual(this);
        }
    }
}