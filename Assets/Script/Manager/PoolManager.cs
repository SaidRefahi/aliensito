using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static PoolManager Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    
    // Referencia al prefab para poder expandir el pool si se vacía
    private Dictionary<string, GameObject> poolPrefabs;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolPrefabs = new Dictionary<string, GameObject>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.transform.SetParent(this.transform); // Ser hijo del Manager
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
            poolPrefabs.Add(pool.tag, pool.prefab); // Guardar referencia al prefab
        }
    }

    /// <summary>
    /// Pide un objeto del pool.
    /// </summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("El Pool con el tag '" + tag + "' no existe.");
            return null;
        }

        GameObject objectToSpawn;

        if (poolDictionary[tag].Count > 0)
        {
            // 1. Tomar un objeto de la cola
            objectToSpawn = poolDictionary[tag].Dequeue();
        }
        else
        {
            // 2. Si la cola está vacía (pool "seco"), expandimos el pool.
            Debug.LogWarning("Pool '" + tag + "' vacío. Expandiendo pool.");
            objectToSpawn = Instantiate(poolPrefabs[tag]);
            objectToSpawn.transform.SetParent(this.transform);
        }

        // 3. Activar y posicionar el objeto
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // 4. ¡¡YA NO SE VUELVE A ENCOLAR AQUÍ!! (Este era el error)

        return objectToSpawn;
    }

    /// <summary>
    /// Devuelve un objeto al pool.
    /// </summary>
    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("El Pool con el tag '" + tag + "' no existe. Destruyendo objeto.");
            Destroy(objectToReturn);
            return;
        }
        
        // Desactivamos el objeto y lo devolvemos a la cola
        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}