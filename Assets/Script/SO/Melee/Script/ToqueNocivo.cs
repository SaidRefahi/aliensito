using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee/ToqueNocivo")]
public class MeleeToqueNocivoSO : MeleeAbilitySO
{
    public int poisonStacks = 3;

    public override void Execute(GameObject user)
    {
        Debug.Log($"[Toque Nocivo] Golpe con {damage} de daño + {poisonStacks} acumulaciones de veneno");
        // lógica de aplicar veneno
    }
}