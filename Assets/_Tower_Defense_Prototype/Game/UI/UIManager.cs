using System;
using _Tower_Defense_Prototype.Game.Scripts.Essentials;
using _Tower_Defense_Prototype.Game.Scripts.Managers;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.UI
{
    public class UIManager : MonoBehaviour
    {
        private ScreenBase   m_ActiveScreen;

        [SerializeField] private ScreenTypeDictionary m_AllScreens;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void SetScreen(GameManager.eGameState screenType)
        {
            if (m_ActiveScreen != null) CloseScreen();
            m_ActiveScreen = m_AllScreens[screenType];
            m_ActiveScreen.Open();
        }

        public void CloseScreen()
        {
            m_ActiveScreen.Close();
            m_ActiveScreen = null;
        }
    }
    [Serializable] public class ScreenTypeDictionary : UnitySerializedDictionary<GameManager.eGameState, ScreenBase> { }
}