using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee/SedDeSangre")]
public class MeleeSedDeSangreSO : MeleeAbilitySO
{
    public float healPercent = 0.1f; // 10% de curación

    public override void Execute(GameObject user)
    {
        Debug.Log($"[Sed de Sangre] Golpe con {damage} de daño y cura {healPercent * 100}% de la vida");
        // lógica de curación al último golpe del combo
    }
}