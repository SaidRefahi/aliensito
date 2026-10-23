using System.Threading;
using Cysharp.Threading.Tasks;

namespace SaludosCamaradaHumano.GeneticalDrift.Core
{
    public interface ISaveService
    {
        UniTask SaveAsync<T>(string key, T data, CancellationToken ct = default);
        UniTask<T> LoadAsync<T>(string key, T defaultValue = default, CancellationToken ct = default);
        bool Exists(string key);
        void Delete(string key);
    }
}
