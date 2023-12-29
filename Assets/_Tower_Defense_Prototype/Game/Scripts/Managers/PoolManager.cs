using System;
using System.Collections.Generic;
using _Tower_Defense_Prototype.Game.Scripts.Essentials;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace _Tower_Defense_Prototype.Game.Scripts.Managers
{
    public class PoolManager : MonoBehaviour
    {
        [Serializable]
        public struct PoolData
        {
            public ePoolType  PoolType;
            public PoolObject PoolPrefab;
            public int        DefaultCapacity;
            public int        MaxSize;
        }

        public enum ePoolType
        {
            EnemyGoblin,
            EnemyOrc,
            TowerMage,
            TowerBowman,
            ProjectileMagic,
            ProjectileArrow,
        }

        private Dictionary<ePoolType, IObjectPool<PoolObject>> m_Pools;

        [SerializeField] private PoolPrefabDictionary m_PoolData;

        private void Awake()
        {
            transform.hierarchyCapacity = 256;
            SetPoolDictionary();

            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            PoolObject.OnDeactivated += Release;
        }

        private void OnDisable()
        {
            PoolObject.OnDeactivated -= Release;
        }

        private void SetPoolDictionary()
        {
            m_Pools = new Dictionary<ePoolType, IObjectPool<PoolObject>>();
            foreach (var data in m_PoolData.Values)
            {
                m_Pools.Add(data.PoolType, new ObjectPool<PoolObject>(() => Instantiate(data.PoolPrefab, transform),
                                                                      OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject,
                                                                      false, data.DefaultCapacity, data.MaxSize));
            }
        }

        public PoolObject Get(ePoolType type)
        {
            m_Pools[type].Get(out PoolObject obj);
            obj.Init();
            return obj;
        }

        public void Release(PoolObject poolObject)
        {
            m_Pools[poolObject.PoolType].Release(poolObject);
        }

        private void OnReturnedToPool(PoolObject poolObject)
        {
            poolObject.gameObject.SetActive(false);
        }

        private void OnTakeFromPool(PoolObject poolObject)
        {
            poolObject.gameObject.SetActive(true);
        }

        private void OnDestroyPoolObject(PoolObject poolObject)
        {
            Destroy(poolObject.gameObject);
        }

        [Serializable] public class PoolPrefabDictionary : UnitySerializedDictionary<ePoolType, PoolData> { }

        public class PoolObject : MonoBehaviour
        {
            public static event Action<PoolObject> OnDeactivated;

            public static void InjectDependencies(PoolManager poolManager, SplineComputer splineComputer)
            {
                s_PoolManager = poolManager;
                s_Spline      = splineComputer;
            }

            protected static PoolManager    s_PoolManager;
            protected static SplineComputer s_Spline;
            public           ePoolType      PoolType;
            public        bool           IsActive { get; private set; }

            protected virtual void OnEnable()
            {
                GameManager.OnLevelLoaded       += Deactivated;
                GameManager.OnLevelFailed       += OnLevelFailed;
                SceneManager.activeSceneChanged += activeSceneChanged;
            }

            protected virtual void OnDisable()
            {
                GameManager.OnLevelLoaded       -= Deactivated;
                GameManager.OnLevelFailed       -= OnLevelFailed;
                SceneManager.activeSceneChanged -= activeSceneChanged;
            }

            public virtual void Init()
            {
                IsActive = true;
            }

            protected virtual void OnLevelFailed() { }

            private void activeSceneChanged(Scene x, Scene y) => Deactivated();

            protected virtual void Deactivated()
            {
                if (!IsActive) return;

                OnDeactivated?.Invoke(this);
                IsActive = false;
            }
        }
    }
}