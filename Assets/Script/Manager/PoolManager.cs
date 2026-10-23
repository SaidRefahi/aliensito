using System.Collections.Generic;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public class PoolManager : MonoBehaviour, IPoolService
    {
        public static PoolManager Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            [Required]
            public string tag;
            [Required]
            public GameObject prefab;
            [Min(1)]
            public int size = 10;
        }

        [Title("Object Pools Configuration")]
        [TableList]
        [SerializeField] private List<Pool> pools = new List<Pool>();

        private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> poolPrefabs = new Dictionary<string, GameObject>();
        private readonly Dictionary<int, string> instanceToTagMap = new Dictionary<int, string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializePools();
        }

        private void InitializePools()
        {
            poolDictionary.Clear();
            poolPrefabs.Clear();
            instanceToTagMap.Clear();

            foreach (Pool pool in pools)
            {
                if (string.IsNullOrEmpty(pool.tag) || pool.prefab == null) continue;

                var objectPool = new Queue<GameObject>(pool.size);
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, transform);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                    instanceToTagMap[obj.GetInstanceID()] = pool.tag;
                }

                poolDictionary[pool.tag] = objectPool;
                poolPrefabs[pool.tag] = pool.prefab;
            }
        }

        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
        {
            return SpawnFromPool(tag, position, rotation);
        }

        public T Spawn<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
        {
            GameObject obj = Spawn(tag, position, rotation);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (string.IsNullOrEmpty(tag) || !poolDictionary.TryGetValue(tag, out var queue))
            {
                Debug.LogWarning($"[PoolManager] Pool with tag '{tag}' does not exist.");
                return null;
            }

            GameObject objectToSpawn;
            if (queue.Count > 0)
            {
                objectToSpawn = queue.Dequeue();
            }
            else
            {
                objectToSpawn = Instantiate(poolPrefabs[tag], transform);
                instanceToTagMap[objectToSpawn.GetInstanceID()] = tag;
            }

            if (objectToSpawn != null)
            {
                Transform objTransform = objectToSpawn.transform;
                objTransform.position = position;
                objTransform.rotation = rotation;
                objectToSpawn.SetActive(true);
            }

            return objectToSpawn;
        }

        public void Return(GameObject objectToReturn)
        {
            if (objectToReturn == null) return;

            if (instanceToTagMap.TryGetValue(objectToReturn.GetInstanceID(), out string tag))
            {
                ReturnToPool(tag, objectToReturn);
            }
            else
            {
                objectToReturn.SetActive(false);
            }
        }

        public void ReturnToPool(string tag, GameObject objectToReturn)
        {
            if (objectToReturn == null) return;

            if (poolDictionary.TryGetValue(tag, out var queue))
            {
                objectToReturn.SetActive(false);
                queue.Enqueue(objectToReturn);
            }
            else
            {
                Debug.LogWarning($"[PoolManager] Pool with tag '{tag}' not found. Deactivating object.");
                objectToReturn.SetActive(false);
            }
        }
    }
}
