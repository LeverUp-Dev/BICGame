using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Hypocrites.Event.SO
{
    [CreateAssetMenu(fileName = "New Bool Event", menuName = "Event/Bool")]
    public class BoolEventSO : BaseSO
    {
        public UnityAction<bool> action;

        public void Raise(bool arg)
        {
            action?.Invoke(arg);
        }
    }
}
