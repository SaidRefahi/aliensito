using UnityEngine;

public class DamageManager : MonoBehaviour
{
    public void ApplyDamage(DamageData data)
    {
        // Ejemplo simple
        switch (data.type)
        {
            case DamageType.Physical:
                Debug.Log($"Daño físico: {data.amount}");
                break;
            case DamageType.Poison:
                Debug.Log($"Aplicando veneno: {data.amount} acumulaciones");
                break;
            case DamageType.Stun:
                Debug.Log("Intentando aturdir al objetivo");
                break;
            case DamageType.Heal:
                Debug.Log($"Curando {data.amount} puntos de vida");
                break;
        }

        // Aquí iría la lógica real de aplicar efectos al target
    }
}
