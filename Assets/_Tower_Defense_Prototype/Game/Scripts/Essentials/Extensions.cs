using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Scripts.Essentials
{
    public static class Extensions
    {
        public enum eAxis
        {
            x,y,z
        }

        public static void SetScaleAxis(this Transform tr, eAxis axis, float value)
        {
            var scale = tr.localScale;
            switch (axis)
            {
                case eAxis.x:
                    scale.x = value;
                    break;
                case eAxis.y:
                    scale.y = value;
                    break;
                case eAxis.z:
                    scale.z = value;
                    break;
            }

            tr.localScale = scale;
        }
    }
}