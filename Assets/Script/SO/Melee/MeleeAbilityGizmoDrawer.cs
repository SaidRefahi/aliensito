using UnityEngine;

public class AbilityGizmoDrawer : MonoBehaviour
{
    [SerializeField] private MeleeAbilitySO abilityToDraw;

    private void OnDrawGizmos()
    {
        if (abilityToDraw != null)
        {
#if UNITY_EDITOR
            // Le pasamos la posición del jugador para que dibuje el gizmo desde ahí
            abilityToDraw.DrawGizmos(this.transform);
#endif
        }
    }
}