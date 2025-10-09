using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee/Base")]
public class MeleeBase : MeleeAbilitySO
{
    // Implementación mínima: usa la lógica provista por MeleeAbilitySO.
    // No añade comportamiento extra; las variantes específicas pueden sobreescribir Execute.
    public override void Execute(GameObject user)
    {
        base.Execute(user);
    }
}