using System;
using _Tower_Defense_Prototype.Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _Tower_Defense_Prototype.Game.UI
{
    public class ScreenLobby : ScreenBase
    {
        public static event Action OnLoadGameButton;
        public static event Action OnNewGameButton;

        [SerializeField] private CanvasGroup m_LoadGameGroup;
        
        [SerializeField] private Button      m_LoadGameButton;
        [SerializeField] private Button      m_NewGameButton;

        protected override void Awake()
        {
            base.Awake();
            m_LoadGameButton.onClick.AddListener(OnLoadButtonTapped);
            m_NewGameButton.onClick.AddListener(OnNewGameButtonTapped);
        }

        private void OnEnable()
        {
            var hasData = StorageManager.HasSavedData();
            m_LoadGameButton.interactable = hasData;
            m_LoadGameGroup.alpha         = hasData ? 1 : 0.5f;
        }

        protected override void OnDestroy()
        {
            m_LoadGameButton.onClick.RemoveListener(OnLoadButtonTapped);
            m_NewGameButton.onClick.RemoveListener(OnNewGameButtonTapped);
        }

        private void OnLoadButtonTapped()
        {
            OnLoadGameButton?.Invoke();
        }

        private void OnNewGameButtonTapped()
        {
            OnNewGameButton?.Invoke();
        }
    }
}