using UnityEngine;

public class MeleeAbilityGizmoDrawer : MonoBehaviour
{
    [Tooltip("Arrastra aquí el ScriptableObject de la habilidad melee que quieres visualizar.")]
    [SerializeField] private MeleeAbilitySO abilityToDraw;

    // Usamos OnDrawGizmosSelected para que solo se dibuje cuando seleccionas al jugador.
    private void OnDrawGizmosSelected()
    {
        if (abilityToDraw != null)
        {
            // El código dentro de DrawGizmos solo se compila en el editor,
            // por lo que es seguro llamarlo directamente.
            abilityToDraw.DrawGizmos(this.transform);
        }
    }
}