using UnityEngine;


[CreateAssetMenu(menuName = "Abilities/Melee/OrgulloRacial")]
public class MeleeOrgulloRacialSO : MeleeAbilitySO
{
    

    public override void Execute(GameObject user)
    {
        Debug.Log($"[Orgullo Racial] Golpe con {damage} de daño y {range} de alcance extra");
        // lógica de daño + alcance aumentado
    }
}