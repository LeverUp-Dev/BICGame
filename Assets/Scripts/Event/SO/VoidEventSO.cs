using UnityEngine;
using UnityEngine.Events;

namespace Hypocrites.Event.SO
{
    [CreateAssetMenu(fileName = "New Void Event", menuName = "Event/Void")]
    public class VoidEventSO : BaseSO
    {
        public UnityAction action;

        public void Raise()
        {
            action?.Invoke();
        }
    }
}
