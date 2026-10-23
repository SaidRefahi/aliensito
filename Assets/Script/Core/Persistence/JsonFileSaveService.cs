using System;
using System.IO;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public class JsonFileSaveService : ISaveService
    {
        private readonly string baseDirectory;

        public JsonFileSaveService()
        {
            baseDirectory = Application.persistentDataPath;
        }

        public JsonFileSaveService(string customDirectory)
        {
            baseDirectory = string.IsNullOrEmpty(customDirectory) ? Application.persistentDataPath : customDirectory;
        }

        private string GetFilePath(string key)
        {
            return Path.Combine(baseDirectory, $"{key}.json");
        }

        public async UniTask SaveAsync<T>(string key, T data, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            string json = JsonUtility.ToJson(data, true);
            string filePath = GetFilePath(key);

            await UniTask.SwitchToThreadPool();

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(filePath, json, ct);
            }
            finally
            {
                await UniTask.SwitchToMainThread();
            }
        }

        public async UniTask<T> LoadAsync<T>(string key, T defaultValue = default, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            string filePath = GetFilePath(key);
            if (!File.Exists(filePath))
            {
                return defaultValue;
            }

            await UniTask.SwitchToThreadPool();

            string json;
            try
            {
                json = await File.ReadAllTextAsync(filePath, ct);
            }
            finally
            {
                await UniTask.SwitchToMainThread();
            }

            if (string.IsNullOrEmpty(json))
            {
                return defaultValue;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonFileSaveService] Failed to deserialize '{filePath}': {ex.Message}");
                return defaultValue;
            }
        }

        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            return File.Exists(GetFilePath(key));
        }

        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            string filePath = GetFilePath(key);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
