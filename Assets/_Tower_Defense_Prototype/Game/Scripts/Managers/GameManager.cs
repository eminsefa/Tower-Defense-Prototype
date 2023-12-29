using System;
using _Tower_Defense_Prototype.Game.Levels.Scripts;
using _Tower_Defense_Prototype.Game.UI;
using Cysharp.Threading.Tasks;
using Dreamteck.Splines;
using UnityEngine;
using Zenject;

namespace _Tower_Defense_Prototype.Game.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Inject]
        private void InjectDependencies(LevelManager levelManager, UIManager uiManager, PoolManager poolManager, SplineComputer spline)
        {
            m_LevelManager = levelManager;
            m_UIManager    = uiManager;
            PoolManager.PoolObject.InjectDependencies(poolManager, spline);
        }

        private UIManager    m_UIManager;
        private LevelManager m_LevelManager;

        public enum eGameState
        {
            Lobby,
            Loaded,
            Playing,
            Failed,
        }

        public static event Action OnLevelLoaded;
        public static event Action OnLevelFailed;

        public static eGameState GameState = eGameState.Loaded;


        private void OnEnable()
        {
            ScreenLobby.OnLoadGameButton += OnLoadSavedGame;
            ScreenLobby.OnNewGameButton  += OnNewGame;
            ScreenLoaded.OnTappedToStart += OnStartGame;
            ScreenFailed.OnTappedToRetry += OnRetryLevel;
            ScreenGame.OnLobbyButton     += OnLobbyButton;
            LevelBase.OnBaseDestroyed    += OnBaseDestroyed;

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            m_LevelManager.LoadLobbyScene(OnLobbySceneLoaded);
        }

        private void OnDisable()
        {
            ScreenLobby.OnLoadGameButton -= OnLoadSavedGame;
            ScreenLobby.OnNewGameButton  -= OnNewGame;
            ScreenLoaded.OnTappedToStart -= OnStartGame;
            ScreenFailed.OnTappedToRetry -= OnRetryLevel;
            ScreenGame.OnLobbyButton     -= OnLobbyButton;
            LevelBase.OnBaseDestroyed    -= OnBaseDestroyed;
        }

        private async void OnBaseDestroyed()
        {
            GameState = eGameState.Failed;

            OnLevelFailed?.Invoke();

            m_LevelManager.EndLevel();
            m_UIManager.CloseScreen();
            await UniTask.Delay(2000);
            m_UIManager.SetScreen(eGameState.Failed);
        }

        private void OnLoadSavedGame()
        {
            m_UIManager.CloseScreen();
            m_LevelManager.LoadGameScene(LoadLevel);
        }

        private void OnNewGame()
        {
            StorageManager.ResetProgress();

            m_UIManager.CloseScreen();
            m_LevelManager.LoadGameScene(LoadLevel);
        }

        private void OnStartGame()
        {
            StartLevel();
        }

        private void OnRetryLevel()
        {
            StorageManager.CurrentScore = 0;
            StorageManager.CurrentWave  = 0;
            StorageManager.DeleteSave();
            LoadLevel();
        }

        private async void OnLobbyButton()
        {
            await LevelManager.CurrentLevel.SaveLevel();
            m_LevelManager.LoadLobbyScene(OnLobbySceneLoaded);
        }

        private void OnLobbySceneLoaded()
        {
            GameState = eGameState.Lobby;
            m_UIManager.SetScreen(eGameState.Lobby);
        }

        private void LoadLevel()
        {
            m_LevelManager.LoadLevel(LevelLoaded);
        }

        private void StartLevel()
        {
            LevelStarted();
        }

        private void LevelLoaded()
        {
            GameState = eGameState.Loaded;
            OnLevelLoaded?.Invoke();

            m_UIManager.SetScreen(eGameState.Loaded);
            if (StorageManager.HasSavedData()) LevelManager.CurrentLevel.LoadSavedData();
        }

        private void LevelStarted()
        {
            GameState = eGameState.Playing;
            m_UIManager.SetScreen(eGameState.Playing);
            m_LevelManager.StartLevel();
        }

        private void OnApplicationQuit()
        {
            if (GameState != eGameState.Playing) return;
            LevelManager.CurrentLevel.SaveLevel().Forget();
        }
    }
}