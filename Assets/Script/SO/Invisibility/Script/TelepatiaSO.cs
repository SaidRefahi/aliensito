using UnityEngine;

[CreateAssetMenu(fileName = "Telepatia", menuName = "Habilidades/Invisibilidad/Telepatia")]
public class TelepatiaSO : InvisibilitySO
{
    [Header("Configuración de Telepatía")]
    public float criticalBuffDuration = 3f;
    
    public override bool Execute(GameObject user)
    {
        // 1. Ejecutar la lógica base (¡que ahora tiene el cooldown y el anti-spam!)
        bool executed = base.Execute(user);

        // 2. Si la base tuvo éxito (no estaba en CD, no estaba ya activo)...
        if (executed)
        {
            // ...entonces busca el "runner" que la base acaba de crear.
            var runner = user.GetComponent<InvisibilitySO.InvisibilityRunner>();
            if (runner != null)
            {
                // Y añade tu lógica extra.
                runner.OnInvisibilityEnd += () => ApplyCriticalBuff(user);
            }
        }

        // 3. Devuelve el resultado de la ejecución base.
        return executed;
    }

    private void ApplyCriticalBuff(GameObject user)
    {
        if (user.TryGetComponent<StatusEffectManager>(out var effectManager))
        {
            effectManager.ApplyEffect(new CriticalBuff(user, criticalBuffDuration));
        }
    }
}