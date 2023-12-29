using UnityEngine;

namespace _Tower_Defense_Prototype.Game.UI
{
    public class ScreenBase : MonoBehaviour
    {
        protected virtual void Awake(){}
        protected virtual void OnDestroy(){}
        
        public virtual void Open()
        {
            gameObject.SetActive(true);
        }
        
        public virtual void Close()
        {
            gameObject.SetActive(false);
        }
    }
}