using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee/MusculaturaBestial")]
public class MeleeMusculaturaBestialSO : MeleeAbilitySO
{
    [Range(0f,1f)] public float stunChance = 0.25f;

    public override void Execute(GameObject user)
    {
        Debug.Log($"[Musculatura Bestial] Golpe con {damage} de daño y {stunChance * 100}% de probabilidad de aturdir");
        // lógica de aplicar stun
    }
}