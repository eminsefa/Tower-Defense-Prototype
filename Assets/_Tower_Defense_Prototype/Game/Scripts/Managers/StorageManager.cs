using System;
using System.Collections.Generic;
using System.IO;
using _Tower_Defense_Prototype.Game.Units.Enemy.Scripts;
using _Tower_Defense_Prototype.Game.Units.Tower.Scripts;
using UnityEngine;

namespace _Tower_Defense_Prototype.Game.Scripts.Managers
{
    public class StorageManager : MonoBehaviour
    {
        private static float s_LastSave;

        private static string SavePath => Path.Combine(Application.persistentDataPath, "SaveData.json");

        public static int CurrentLevel
        {
            get => PlayerPrefs.GetInt(nameof(CurrentLevel), 0);
            set => PlayerPrefs.SetInt(nameof(CurrentLevel), value);
        }

        public static int CurrentWave
        {
            get => PlayerPrefs.GetInt(nameof(CurrentWave), 0);
            set => PlayerPrefs.SetInt(nameof(CurrentWave), value);
        }

        public static int CurrentScore
        {
            get => PlayerPrefs.GetInt(nameof(CurrentScore), 0);
            set => PlayerPrefs.SetInt(nameof(CurrentScore), value);
        }

        public static void ResetProgress()
        {
            CurrentWave  = 0;
            CurrentLevel = 0;
            CurrentScore = 0;
            DeleteSave();
        }

        public static bool HasSavedData() => File.Exists(SavePath);

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }

        public static LevelSaveData GetSavedData()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                var    data = JsonUtility.FromJson<LevelSaveData>(json);

                CurrentLevel = data.LevelNumber;
                CurrentWave  = data.WaveNumber;
                CurrentScore = data.Score;

                return data;
            }

            Debug.Log($"Saved data not found");
            return new LevelSaveData();
        }

        public static void SaveData(List<EnemySaveData> enemyData, List<TowerSaveData> towerData, float wavePercent, int spawnedCount)
        {
            if (Time.time - s_LastSave < 1) return; //Save min interval
            
            s_LastSave = Time.time;
            var saveData = new LevelSaveData()
                           {
                               LevelNumber           = CurrentLevel,
                               WaveNumber            = CurrentWave,
                               Score                 = CurrentScore,
                               EnemySaveData         = enemyData,
                               TowerSaveData         = towerData,
                               WavePercent           = wavePercent,
                               WaveEnemySpawnedCount = spawnedCount
                           };

            string json = JsonUtility.ToJson(saveData);
            File.WriteAllText(SavePath, json);
        }

        [Serializable]
        public struct LevelSaveData
        {
            public int                 LevelNumber;
            public int                 WaveNumber;
            public int                 Score;
            public float               WavePercent;
            public int                 WaveEnemySpawnedCount;
            public List<TowerSaveData> TowerSaveData;
            public List<EnemySaveData> EnemySaveData;
        }

        [Serializable]
        public struct TowerSaveData
        {
            public float     AttackTimer;
            public TowerData TowerData;
            public Vector2   Position;
        }

        [Serializable]
        public struct EnemySaveData
        {
            public EnemyData EnemyData;
            public double    MovePercent;
            public int       Health;
        }
    }
}