using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
    public string abilityName;
    public Sprite icon;

    public abstract void Execute(GameObject user);
}