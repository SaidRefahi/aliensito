using UnityEngine;

[CreateAssetMenu(fileName = "Parpadeo", menuName = "Habilidades/Invisibilidad/Parpadeo")]
public class ParpadeoSO : InvisibilitySO // Hereda de la invisibilidad base.
{
    [Header("Configuración de Parpadeo")]
    public float blinkDistance = 10f;

    public override void Execute(GameObject user)
    {
        // 1. Llama a la lógica de la invisibilidad base.
        base.Execute(user);
        
        // 2. Añade el teletransporte.
        user.transform.position += user.transform.forward * blinkDistance;
    }
}