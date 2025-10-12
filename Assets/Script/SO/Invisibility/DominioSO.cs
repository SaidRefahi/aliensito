using UnityEngine;

[CreateAssetMenu(fileName = "Dominio", menuName = "Habilidades/Invisibilidad/Dominio")]
public class DominioSO : InvisibilitySO
{
    [Header("Configuración de Dominio")]
    public float auraRadius = 6f;
    public float poisonDamagePerSecond = 1f;
    public float poisonDuration = 3f;
    public LayerMask enemyLayers;

    public override void Execute(GameObject user)
    {
        var activeRunner = user.GetComponent<InvisibilitySO.InvisibilityRunner>();
        if (activeRunner != null)
        {
            // Si ya eres invisible, desactiva la invisibilidad (el aura se destruirá con el jugador).
            activeRunner.Stop();
            var existingAura = user.GetComponent<PoisonAura>();
            if (existingAura != null) Destroy(existingAura);
        }
        else
        {
            // Activa la invisibilidad normal.
            base.Execute(user); 
            // Y añade el componente del aura de veneno.
            var aura = user.AddComponent<PoisonAura>();
            aura.radius = auraRadius;
            aura.poisonDamage = poisonDamagePerSecond;
            aura.poisonDuration = poisonDuration;
            aura.enemyLayers = enemyLayers;
        }
    }
}