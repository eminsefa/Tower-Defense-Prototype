using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Tower_Defense_Prototype.Game.Scripts.Managers;
using _Tower_Defense_Prototype.Game.Units.Enemy.Scripts;
using _Tower_Defense_Prototype.Game.Units.Tower.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Tower_Defense_Prototype.Game.Levels.Scripts
{
    //This is where we can create an automatized system, we only need level data(path and env)
    //Then set pooled tiles based on the data for visuals
    //Then convert rotation tile points to spline for path
    public class LevelBase
    {
        public static event Action OnWaveStarted;
        public static event Action OnBaseDestroyed;

        public float     WavePercent          { get; private set; }
        public bool      SpawnPointAvailable  { get; private set; } = true;
        public bool      DeSpawnUnitAvailable => m_SpawnedTowers.Count > 0;
        public LevelData LevelData            => m_LevelData;

        private int    m_Health;
        private Camera m_MainCam;
        private bool   m_WaveSpawned;
        private int    m_WaveEnemySpawnedCount;

        private readonly LevelData                      m_LevelData;
        private readonly LevelManager                   m_LevelManager;
        private readonly PoolManager                    m_PoolManager;
        private readonly GameObject                     m_Map;
        private readonly Dictionary<Vector2, TowerBase> m_SpawnedTowers;
        private readonly HashSet<EnemyBase>             m_SpawnedEnemies;

        private CancellationTokenSource m_DisposeCts;

        public LevelBase(LevelManager levelManager, PoolManager poolManager, LevelData data, GameObject map)
        {
            m_LevelManager = levelManager;
            m_PoolManager  = poolManager;
            m_LevelData    = data;

            m_Map = map;
            map.gameObject.SetActive(true);

            m_SpawnedTowers  = new Dictionary<Vector2, TowerBase>();
            m_SpawnedEnemies = new HashSet<EnemyBase>();
            m_DisposeCts     = new CancellationTokenSource();

            EnemyBase.OnEnemyDead        += OnEnemyDead;
            EnemyBase.OnEnemyReachedBase += OnEnemyReachedBase;
        }

        public void Dispose()
        {
            m_DisposeCts.Cancel();
            m_DisposeCts.Dispose();
            GC.SuppressFinalize(this);

            EnemyBase.OnEnemyDead        -= OnEnemyDead;
            EnemyBase.OnEnemyReachedBase -= OnEnemyReachedBase;
        }

        public async void LoadSavedData()
        {
            var savedData = StorageManager.GetSavedData();

            m_WaveEnemySpawnedCount = savedData.WaveEnemySpawnedCount;
            WavePercent             = savedData.WavePercent;

            List<UniTask> spawnTasks = new List<UniTask>();

            foreach (var enemySaveData in savedData.EnemySaveData)
            {
                spawnTasks.Add(SpawnSavedEnemy(enemySaveData.EnemyData.PoolType, enemySaveData));
            }

            await UniTask.WhenAll(spawnTasks);

            foreach (var towerSaveData in savedData.TowerSaveData)
            {
                var t = SpawnTowerAtPosition(towerSaveData.TowerData.PoolType, towerSaveData.Position).GetAwaiter().GetResult();
                while (!t.IsActive) await UniTask.Yield();
                t.AttackTimer = towerSaveData.AttackTimer;
            }
        }

        private async UniTask SpawnSavedEnemy(PoolManager.ePoolType enemyType, StorageManager.EnemySaveData enemySaveData)
        {
            var e = SpawnEnemy(enemyType).GetAwaiter().GetResult();
            while (!e.IsAlive) await UniTask.Yield();
            e.SetSavedData(enemySaveData);
        }

        private void OnEnemyDead(EnemyBase enemy)
        {
            m_SpawnedEnemies.Remove(enemy);

            if (m_SpawnedEnemies.Count > 0 || !m_WaveSpawned) return;

            m_DisposeCts.Cancel();
            m_DisposeCts = new CancellationTokenSource();

            StartNextWave();
            RunLevel().Forget();
        }

        private void OnEnemyReachedBase(EnemyBase enemy)
        {
            var d = enemy.EnemyData.Damage;
            m_Health -= d;
            if (m_Health <= 0) OnBaseDestroyed?.Invoke();
        }

        public void DisableMap()
        {
            m_Map.gameObject.SetActive(false);
        }

        public void ResetLevel()
        {
            m_DisposeCts.Cancel();
            m_SpawnedEnemies.Clear();
            m_SpawnedTowers.Clear();
            m_Map.SetActive(true);
        }

        public void DisableLevel()
        {
            ResetLevel();
            m_Map.SetActive(false);
        }

        public void StartLevel()
        {
            m_MainCam    = Camera.main;
            m_DisposeCts = new CancellationTokenSource();

            m_Health = m_LevelData.BaseHealth;

            if (StorageManager.HasSavedData())
                foreach (var spawnedEnemy in m_SpawnedEnemies)
                    spawnedEnemy.StartLevel();
            else
            {
                SpawnPointAvailable     = true;
                m_WaveSpawned           = false;
                m_WaveEnemySpawnedCount = 0;
            }

            RunLevel().Forget();

            if (StorageManager.HasSavedData())
            {
                RunWave(m_WaveEnemySpawnedCount).Forget();
            }
        }

        public void EndLevel()
        {
            m_DisposeCts.Cancel();
        }

        private async UniTask RunLevel()
        {
            var waveCount = m_LevelData.Waves.Length;
            var startWave = StorageManager.CurrentWave % waveCount;
            while (GameManager.GameState == GameManager.eGameState.Playing)
            {
                for (int i = startWave; i < waveCount; i++)
                {
                    var waveDelay = m_LevelData.Waves[i].StartDelay.Evaluate(StorageManager.CurrentWave);
                    var startTime = WavePercent * waveDelay;
                    while (startTime < waveDelay)
                    {
                        startTime   += Time.deltaTime;
                        WavePercent =  startTime / waveDelay;
                        await UniTask.NextFrame(m_DisposeCts.Token);
                    }

                    if (m_DisposeCts.IsCancellationRequested) break;
                    StartNextWave();
                }

                startWave = 0;
            }
        }

        private async UniTask RunWave(int startSpawn = 0)
        {
            var wave        = StorageManager.CurrentWave - 1; //RunLevel counts current wave, RunWave counts prev wave
            var index       = wave % m_LevelData.Waves.Length;
            var waveData    = m_LevelData.Waves[index];
            var spawnAmount = (int) (waveData.SpawnAmount.Evaluate(wave));

            if (wave == StorageManager.CurrentWave - 1) m_WaveEnemySpawnedCount = 0;

            for (int i = startSpawn; i < spawnAmount; i++)
            {
                if (i != 0)
                    await UniTask.Delay((int) (waveData.SpawnRate.Evaluate(wave) * 1000),
                                        true, PlayerLoopTiming.Update, m_DisposeCts.Token);
                if (m_DisposeCts.IsCancellationRequested) break;
                var e = SpawnEnemy(waveData.EnemyType).GetAwaiter().GetResult();

                while (!e.IsAlive) await UniTask.NextFrame();

                e.StartLevel();
                if (wave == StorageManager.CurrentWave - 1) m_WaveEnemySpawnedCount++;
            }

            if (wave == StorageManager.CurrentWave - 1) m_WaveSpawned = true;
        }

        private void StartNextWave()
        {
            m_WaveSpawned = false;
            WavePercent   = 0;

            StorageManager.CurrentWave++;
            OnWaveStarted?.Invoke();
            
            RunWave().Forget();
        }

        private async UniTask<EnemyBase> SpawnEnemy(PoolManager.ePoolType enemyType)
        {
            var e = m_PoolManager.Get(enemyType) as EnemyBase;
            m_SpawnedEnemies.Add(e);
            while (e != null && !e.IsActive) await UniTask.Yield();
            return e;
        }

        public void SpawnTowerRandom()
        {
            var availableCells = m_LevelData.PathData.Positions
                                            .Skip(1) //Ignoring path start and end 
                                            .Take(m_LevelData.PathData.Positions.Length - 2)
                                            .Where(pos => !m_SpawnedTowers.ContainsKey(pos))
                                            .ToList();
            var count                           = availableCells.Count;
            if (count == 1) SpawnPointAvailable = false;

            while (true)
            {
                if (availableCells.Count < 1)
                {
                    SpawnPointAvailable = false;
                    Debug.LogError("No free position left");
                    return;
                }

                var randTower = m_LevelData.TowerTypes[Random.Range(0, m_LevelData.TowerTypes.Length)];

                //Try neighbour cells and spawn if it is empty
                var       randCellPos = availableCells[Random.Range(0, availableCells.Count)];
                Vector2[] offset      = {Vector2.up, Vector2.down, Vector2.right, Vector2.left,};

                var rng = new System.Random();
                offset = offset.OrderBy(x => rng.Next()).ToArray();

                for (var i = 0; i < offset.Length; i++)
                {
                    var randPos = randCellPos + offset[i] * m_LevelData.PathData.GridSize;
                    if (!IsCellEmpty(randPos) || !IsCellInScreen(randPos)) continue;
                    SpawnTowerAtPosition(randTower, randPos).Forget();
                    return;
                }

                availableCells.Remove(randCellPos);
            }
        }

        private async UniTask<TowerBase> SpawnTowerAtPosition(PoolManager.ePoolType towerType, Vector2 pos)
        {
            var t = m_PoolManager.Get(towerType) as TowerBase;
            while (t != null && !t.IsActive) await UniTask.Yield();

            t.transform.position = pos;
            m_SpawnedTowers.Add(pos, t);
            return t;
        }

        private bool IsCellEmpty(Vector2 position)
        {
            var isEmpty = !m_LevelData.PathData.Positions.Contains(position) && !m_SpawnedTowers.Keys.Contains(position);
            return isEmpty;
        }

        private bool IsCellInScreen(Vector2 position)
        {
            var  viewPos         = m_MainCam.WorldToViewportPoint(position);
            bool isOutsideScreen = viewPos.x is < 0f or > 1f || viewPos.y is < 0f or > 1f;
            return !isOutsideScreen;
        }

        public void DeSpawnRandom()
        {
            if (m_SpawnedTowers.Count < 1) return;
            SpawnPointAvailable = true;
            var randomIndex = Random.Range(0, m_SpawnedTowers.Count);
            var randomCell  = m_SpawnedTowers.Keys.ElementAt(randomIndex);
            var randomTower = m_SpawnedTowers[randomCell];

            m_PoolManager.Release(randomTower);

            m_SpawnedTowers.Remove(randomCell);
        }

        public async UniTask SaveLevel()
        {
            List<StorageManager.EnemySaveData> enemySaveData = new List<StorageManager.EnemySaveData>();
            foreach (var spawnedEnemy in m_SpawnedEnemies)
            {
                enemySaveData.Add(new StorageManager.EnemySaveData()
                                  {
                                      EnemyData   = spawnedEnemy.EnemyData,
                                      Health      = spawnedEnemy.Health,
                                      MovePercent = spawnedEnemy.GetPathPercentage,
                                  });
            }

            List<StorageManager.TowerSaveData> towerSaveData = new List<StorageManager.TowerSaveData>();
            foreach (var posTowerKvp in m_SpawnedTowers)
            {
                towerSaveData.Add(new StorageManager.TowerSaveData()
                                  {
                                      TowerData   = posTowerKvp.Value.TowerData,
                                      Position    = posTowerKvp.Key,
                                      AttackTimer = posTowerKvp.Value.AttackTimer
                                  });
            }

            StorageManager.SaveData(enemySaveData, towerSaveData, WavePercent, m_WaveEnemySpawnedCount);
            await UniTask.Yield();
        }
    }
}