using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Hypocrites.Event.SO
{
    using Skill;

    [CreateAssetMenu(fileName = "New Skill Event", menuName = "Event/Skill")]
    public class SkillEventSO : BaseSO
    {
        public UnityAction<Skill> action;

        public void Raise(Skill arg)
        {
            action?.Invoke(arg);
        }
    }
}
