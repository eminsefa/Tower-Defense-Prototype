using System;
using _Tower_Defense_Prototype.Game.Levels.Scripts;
using _Tower_Defense_Prototype.Game.Scripts.Managers;
using _Tower_Defense_Prototype.Game.Units.Enemy.Scripts;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Tower_Defense_Prototype.Game.UI
{
    public class ScreenGame : ScreenBase
    {
        private const string c_Score = "Score : ";
        private const string c_FPS   = "FPS : ";
        private const string c_Wave  = "WAVE ";

        public static event Action OnLobbyButton;

        private Tweener m_TwWaveBar;

        [BoxGroup("HUD")] [SerializeField] private Image           m_WaveBarFill;
        [BoxGroup("HUD")] [SerializeField] private TextMeshProUGUI m_WaveText;

        [BoxGroup("HUD")] [SerializeField] private TextMeshProUGUI m_ScoreText;

        [BoxGroup("HUD")] [SerializeField] private TextMeshProUGUI m_FpsText;
        [BoxGroup("HUD")] [SerializeField] private Button          m_FpsButton;


        [BoxGroup("Input")] [SerializeField] private Button m_MainMenuButton;

        [BoxGroup("Input")] [SerializeField] private Button     m_SpawnButton;
        [BoxGroup("Input")] [SerializeField] private GameObject m_SpawnGroup;

        [BoxGroup("Input")] [SerializeField] private Button     m_DeSpawnButton;
        [BoxGroup("Input")] [SerializeField] private GameObject m_DeSpawnGroup;

        protected override void Awake()
        {
            m_MainMenuButton.onClick.AddListener(OnMenuButtonTapped);
            m_SpawnButton.onClick.AddListener(OnSpawnButtonTapped);
            m_FpsButton.onClick.AddListener(() => m_FpsText.gameObject.SetActive(!m_FpsText.gameObject.activeSelf));
            m_DeSpawnButton.onClick.AddListener(OnDeSpawnButtonTapped);

            EnemyBase.OnEnemyDead   += OnEnemyDead;
            LevelBase.OnWaveStarted += OnWaveStarted;

            m_TwWaveBar = DOTween.To(() => m_WaveBarFill.fillAmount, x => m_WaveBarFill.fillAmount = x, 0, 0.5f)
                                 .From(1)
                                 .SetAutoKill(false)
                                 .Pause();
        }

        private void OnEnable()
        {
            if (DOTween.IsTweening(m_TwWaveBar)) m_TwWaveBar.Pause();
            m_WaveBarFill.fillAmount = 0;

            m_WaveText.transform.localScale = Vector3.one;
            m_WaveText.text                 = c_Wave + StorageManager.CurrentWave;

            SetScoreText(StorageManager.CurrentScore);
            SetSpawnButton(LevelManager.CurrentLevel.SpawnPointAvailable);
            SetDeSpawnButton(LevelManager.CurrentLevel.DeSpawnUnitAvailable);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_MainMenuButton.onClick.RemoveListener(OnMenuButtonTapped);
            m_SpawnButton.onClick.RemoveListener(OnSpawnButtonTapped);
            m_DeSpawnButton.onClick.RemoveListener(OnDeSpawnButtonTapped);
            m_FpsButton.onClick.RemoveListener(() => m_FpsText.gameObject.SetActive(!m_FpsText.gameObject.activeSelf));

            EnemyBase.OnEnemyDead   -= OnEnemyDead;
            LevelBase.OnWaveStarted -= OnWaveStarted;
        }

        private void Update()
        {
            m_FpsText.text = c_FPS + FPSManager.FPSAverageLastUpdate;

            UpdateWaveBar(LevelManager.CurrentLevel.WavePercent);
            SetSpawnButton(LevelManager.CurrentLevel.SpawnPointAvailable);
            SetDeSpawnButton(LevelManager.CurrentLevel.DeSpawnUnitAvailable);
        }

        private void OnMenuButtonTapped()
        {
            OnLobbyButton?.Invoke();
        }

        private void OnSpawnButtonTapped()
        {
            if (!LevelManager.CurrentLevel.SpawnPointAvailable)
            {
                SetSpawnButton(false);
                return;
            }

            m_SpawnButton.interactable = false;
            LevelManager.CurrentLevel.SpawnTowerRandom();
        }

        private void OnDeSpawnButtonTapped()
        {
            if (!LevelManager.CurrentLevel.DeSpawnUnitAvailable)
            {
                SetDeSpawnButton(false);
                return;
            }

            m_DeSpawnButton.interactable = false;
            LevelManager.CurrentLevel.DeSpawnRandom();
        }

        private void OnEnemyDead(EnemyBase enemy)
        {
            var enemyScore = enemy.EnemyData.Score;
            var newScore   = StorageManager.CurrentScore += enemyScore;
            SetScoreText(newScore);
        }

        private void SetScoreText(int score)
        {
            m_ScoreText.text = c_Score + score;
        }

        private void SetSpawnButton(bool active)
        {
            m_SpawnGroup.SetActive(active);
            m_SpawnButton.interactable = active;
        }

        private void SetDeSpawnButton(bool active)
        {
            m_DeSpawnGroup.SetActive(active);
            m_DeSpawnButton.interactable = active;
        }

        private void UpdateWaveBar(float percent)
        {
            if (DOTween.IsTweening(m_TwWaveBar)) return;
            m_WaveBarFill.fillAmount = percent;
        }

        private void OnWaveStarted()
        {
            if (m_WaveBarFill.fillAmount > 0.1f) m_TwWaveBar.Restart();

            m_WaveText.text = c_Wave + StorageManager.CurrentWave;

            m_WaveText.transform.DOPunchScale(Vector3.one * 0.25f, 0.5f);
        }
    }
}