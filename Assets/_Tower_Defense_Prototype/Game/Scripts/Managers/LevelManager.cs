using System;
using _Tower_Defense_Prototype.Game.Levels.Scripts;
using _Tower_Defense_Prototype.Game.Scripts.Essentials;
using Cysharp.Threading.Tasks;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace _Tower_Defense_Prototype.Game.Scripts.Managers
{
    //If we set the path with data, we can also set the environment with data
    //Then we wouldn't need Level manager as an game object
    public class LevelManager : MonoBehaviour
    {
        [Inject] private PoolManager m_PoolManager;

        public static LevelBase CurrentLevel { get; private set; }

        private const int LOBBY_SCENE = 1;
        private const int GAME_SCENE  = 2;

        private GameConfig.LevelVariables m_LevelVars => GameConfig.Instance.Level;

        [SerializeField] private SplineComputer m_SplineComputer;
        [SerializeField] private GameObject[]   m_Levels;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnDisable()
        {
            CurrentLevel?.Dispose();
        }

        public void StartLevel() => CurrentLevel.StartLevel();
        public void EndLevel()   => CurrentLevel.EndLevel();

        public async void LoadLobbyScene(Action onLobbyLoaded)
        {
            CurrentLevel?.DisableLevel();
            await LoadScene(LOBBY_SCENE);
            onLobbyLoaded?.Invoke();
        }

        public async void LoadGameScene(Action onGameLoaded)
        {
            await UniTask.Delay(100);
            await LoadScene(GAME_SCENE);
            onGameLoaded?.Invoke();
        }

        public void LoadLevel(Action onLevelLoaded)
        {
            var index     = StorageManager.CurrentLevel % m_Levels.Length;
            var levelData = m_LevelVars.LevelData[index];

            if (CurrentLevel?.LevelData == levelData)
            {
                CurrentLevel?.ResetLevel();
                onLevelLoaded?.Invoke();
                return;
            }

            CurrentLevel?.Dispose();
            CurrentLevel?.DisableMap();

            m_SplineComputer.SetPoints(levelData.PathData.SplinePoints);
            m_SplineComputer.RebuildImmediate();

            CurrentLevel = new LevelBase(this, m_PoolManager, levelData, m_Levels[index]);

            onLevelLoaded?.Invoke();
        }

        private async UniTask LoadScene(int index)
        {
            await SceneManager.LoadSceneAsync(index);
        }
    }
}