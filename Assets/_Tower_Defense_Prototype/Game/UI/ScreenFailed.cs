using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Tower_Defense_Prototype.Game.UI
{
    public class ScreenFailed : ScreenBase
    {
        public static event Action OnTappedToRetry;

        [SerializeField] private Button m_TapButton;

        protected override void Awake()
        {
            base.Awake();
            m_TapButton.onClick.AddListener(OnTapButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_TapButton.onClick.RemoveListener(OnTapButton);
        }

        private void OnTapButton()
        {
            OnTappedToRetry?.Invoke();
        }
    }
}