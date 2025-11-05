// Ruta: Assets/Script/SO/Invisibility/DominioSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Dominio", menuName = "Habilidades/Invisibilidad/Dominio")]
public class DominioSO : InvisibilitySO
{
    [Header("Configuración de Dominio")]
    public float auraRadius = 6f;
    public float poisonDamagePerSecond = 1f;
    public float poisonDuration = 3f;
    public LayerMask enemyLayers;

    // --- ¡MÉTODO CORREGIDO! ---
    public override bool Execute(GameObject user)
    {
        var activeRunner = user.GetComponent<InvisibilitySO.InvisibilityRunner>();
        if (activeRunner != null)
        {
            // Si ya eres invisible, desactiva la invisibilidad
            activeRunner.Stop();
            var existingAura = user.GetComponent<PoisonAura>();
            if (existingAura != null) Destroy(existingAura);
            
            // La acción (detener) fue exitosa
            return true;
        }
        else
        {
            // Activa la invisibilidad normal (llamando a la base)
            // 1. Capturamos el resultado de la clase base
            bool executed = base.Execute(user); 
            
            // 2. Si la base tuvo éxito (ej. no estaba en cooldown), añadimos el aura
            if (executed)
            {
                var aura = user.AddComponent<PoisonAura>();
                aura.radius = auraRadius;
                aura.poisonDamage = poisonDamagePerSecond;
                aura.poisonDuration = poisonDuration;
                aura.enemyLayers = enemyLayers;
            }
            
            // 3. Devolvemos el resultado de la ejecución base
            return executed;
        }
    }
}