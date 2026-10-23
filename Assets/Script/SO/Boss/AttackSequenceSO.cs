using System.Collections.Generic;
using UnityEngine;
using SaludosCamaradaHumano.GeneticalDrift.Core;
using TriInspector;

namespace SaludosCamaradaHumano.GeneticalDrift.Gameplay
{
    public enum TelegraphType
    {
        None,
        Center,
        Sides,
        All
    }

    [CreateAssetMenu(fileName = "NewAttackSequence", menuName = "Boss/Attack Sequence")]
    public class AttackSequenceSO : ScriptableObject
    {
        [Title("Attack Sequence Configuration")]
        [Tooltip("Sequence of ability SOs to execute in order.")]
        [SerializeField] private List<AbilitySO> attacksInSequence = new List<AbilitySO>();

        [Title("Telegraph Configuration")]
        [SerializeField] private TelegraphType telegraphType = TelegraphType.Center;

        public IReadOnlyList<AbilitySO> AttacksInSequence => attacksInSequence;
        public TelegraphType TelegraphType => telegraphType;
    }
}
