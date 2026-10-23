using UnityEngine;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public interface IPoolService
    {
        GameObject Spawn(string tag, Vector3 position, Quaternion rotation);
        T Spawn<T>(string tag, Vector3 position, Quaternion rotation) where T : Component;
        void Return(GameObject obj);
    }
}
