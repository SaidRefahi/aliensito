using UnityEngine;

[CreateAssetMenu(fileName = "Dominio", menuName = "Habilidades/Invisibility/Dominio")]
public class DominioSO : InvisibilitySO
{
    [Header("Configuración de Dominio")]
    public float auraRadius = 6f;
    public float poisonDamagePerSecond = 1f;
    public float poisonDuration = 3f;

    [Header("Configuración de VFX")]
    public string vfxPoolTag = "DominioVFX"; 

    public override bool Execute(GameObject user)
    {
        // 1. Ejecutar la lógica base
        bool executed = base.Execute(user);

        // 2. Si la base tuvo éxito...
        if (executed)
        {
            // --- ¡LÓGICA DE VFX MODIFICADA! ---
            GameObject vfxInstance = null; // Variable para guardar el VFX
            if (PoolManager.Instance != null && !string.IsNullOrEmpty(vfxPoolTag))
            {
                vfxInstance = PoolManager.Instance.SpawnFromPool(vfxPoolTag, user.transform.position, Quaternion.identity);
                
                if (vfxInstance != null)
                {
                    // Hacemos que el VFX sea hijo del 'user' para que se mueva con él
                    vfxInstance.transform.SetParent(user.transform); 
                }
            }
            // --- FIN DE LÓGICA MODIFICADA ---

            // ...Obtenemos el TargetingProfile
            TargetingProfile targeting = user.GetComponent<TargetingProfile>();
            if (targeting == null)
            {
                Debug.LogError($"¡DominioSO: {user.name} no tiene un componente TargetingProfile!");
                // Si fallamos aquí, debemos apagar el VFX que acabamos de encender
                if(vfxInstance != null) PoolManager.Instance.ReturnToPool(vfxPoolTag, vfxInstance);
                return false;
            }

            // ...Añadimos el componente de aura
            var aura = user.AddComponent<PoisonAura>();
            aura.radius = auraRadius;
            aura.poisonDamage = poisonDamagePerSecond;
            aura.poisonDuration = poisonDuration;
            aura.enemyLayers = targeting.DamageableLayers; 

            // ...Buscamos el "runner" que la base acaba de crear
            var runner = user.GetComponent<InvisibilitySO.InvisibilityRunner>();
            if (runner != null)
            {
                // --- ¡LÓGICA DE EVENTO MODIFICADA! ---
                // Ahora nos suscribimos a OnEnd para destruir AMBAS cosas: el aura y el VFX.
                runner.OnInvisibilityEnd += () => 
                { 
                    // 1. Destruir el componente de lógica (el aura)
                    if (aura != null) 
                    {
                        GameObject.Destroy(aura); 
                    }
                    
                    // 2. Devolver el efecto visual (VFX) al pool
                    if (vfxInstance != null && PoolManager.Instance != null)
                    {
                        // ¡Importante! Si el VFX era hijo, hay que "des-hacerlo"
                        // para que no se desactive si el jugador se desactiva.
                        vfxInstance.transform.SetParent(PoolManager.Instance.transform);
                        PoolManager.Instance.ReturnToPool(vfxPoolTag, vfxInstance);
                    }
                };
                // --- FIN DE LÓGICA MODIFICADA ---
            }
        }

        // 3. Devuelve el resultado de la ejecución base.
        return executed;
    }
}