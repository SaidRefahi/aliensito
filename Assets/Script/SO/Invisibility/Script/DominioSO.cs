using UnityEngine;

[CreateAssetMenu(fileName = "Dominio", menuName = "Habilidades/Invisibilidad/Dominio")]
public class DominioSO : InvisibilitySO
{
    [Header("Configuración de Dominio")]
    public float auraRadius = 6f;
    public float poisonDamagePerSecond = 1f;
    public float poisonDuration = 3f;
    // public LayerMask enemyLayers; // <-- ¡BORRADO! Usaremos el TargetingProfile (SRP)

    public override bool Execute(GameObject user)
    {
        // 1. Ejecutar la lógica base (¡que ahora tiene el cooldown y el anti-spam!)
        bool executed = base.Execute(user);

        // 2. Si la base tuvo éxito (no estaba en CD, no estaba ya activo)...
        if (executed)
        {
            // ...Obtenemos el TargetingProfile para saber a quién dañar
            TargetingProfile targeting = user.GetComponent<TargetingProfile>();
            if (targeting == null)
            {
                Debug.LogError($"¡DominioSO: {user.name} no tiene un componente TargetingProfile!");
                return false;
            }

            // ...Añadimos el componente de aura
            var aura = user.AddComponent<PoisonAura>();
            aura.radius = auraRadius;
            aura.poisonDamage = poisonDamagePerSecond;
            aura.poisonDuration = poisonDuration;
            aura.enemyLayers = targeting.DamageableLayers; // ¡Pasa el LayerMask correcto!

            // ...Buscamos el "runner" que la base acaba de crear
            var runner = user.GetComponent<InvisibilitySO.InvisibilityRunner>();
            if (runner != null)
            {
                // Y nos suscribimos a su evento OnEnd para destruir el aura (KISS)
                runner.OnInvisibilityEnd += () => 
                { 
                    if (aura != null) GameObject.Destroy(aura); 
                };
            }
        }

        // 3. Devuelve el resultado de la ejecución base.
        return executed;
    }
}