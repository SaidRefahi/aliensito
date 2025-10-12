using UnityEngine;

[CreateAssetMenu(fileName = "Telepatia", menuName = "Habilidades/Invisibilidad/Telepatia")]
public class TelepatiaSO : InvisibilitySO
{
    [Header("Configuración de Telepatía")]
    public float criticalBuffDuration = 3f;
    
    public override void Execute(GameObject user)
    {
        var activeRunner = user.GetComponent<InvisibilitySO.InvisibilityRunner>();
        if (activeRunner != null)
        {
            activeRunner.Stop();
        }
        else
        {
            var runner = user.AddComponent<InvisibilitySO.InvisibilityRunner>();
            runner.StartInvisibility(this);
            // Nos "enganchamos" al evento de que la invisibilidad ha terminado.
            runner.OnInvisibilityEnd += () => ApplyCriticalBuff(user);
        }
    }

    private void ApplyCriticalBuff(GameObject user)
    {
        if (user.TryGetComponent<StatusEffectManager>(out var effectManager))
        {
            effectManager.ApplyEffect(new CriticalBuff(user, criticalBuffDuration));
        }
    }
}