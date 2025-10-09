using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Invisibility")]
public class InvisibilitySO : AbilitySO
{
    public float duration = 5f;

    public override void Execute(GameObject user)
    {
        // Ejemplo simple: desactivar renderers
        Renderer[] rends = user.GetComponentsInChildren<Renderer>();
        foreach (var r in rends) r.enabled = false;

        user.GetComponent<MonoBehaviour>().StartCoroutine(RestoreVisibility(user));

        Debug.Log($"{abilityName} ejecutado: invisibilidad activada");
    }

    private System.Collections.IEnumerator RestoreVisibility(GameObject user)
    {
        yield return new WaitForSeconds(duration);

        Renderer[] rends = user.GetComponentsInChildren<Renderer>();
        foreach (var r in rends) r.enabled = true;

        Debug.Log("Invisibilidad terminada");
    }
}