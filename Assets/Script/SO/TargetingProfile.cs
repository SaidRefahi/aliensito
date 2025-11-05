using UnityEngine;
public class TargetingProfile : MonoBehaviour
{
    [Tooltip("Define las layers a las que este personaje puede dañar.")]
    public LayerMask DamageableLayers;
}