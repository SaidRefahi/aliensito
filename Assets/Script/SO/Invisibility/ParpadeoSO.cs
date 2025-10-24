using UnityEngine;

[CreateAssetMenu(fileName = "Parpadeo", menuName = "Habilidades/Invisibilidad/Parpadeo")]
public class ParpadeoSO : InvisibilitySO
{
    [Header("Configuración de Parpadeo")]
    public float blinkDistance = 10f;

    public override void Execute(GameObject user)
    {
        
        base.Execute(user);
        
        Vector3 forwardDirection = user.transform.forward;
        forwardDirection.y = 0;

        Vector3 blinkVector = forwardDirection.normalized * blinkDistance;

        
        user.transform.position += blinkVector;
    }
}