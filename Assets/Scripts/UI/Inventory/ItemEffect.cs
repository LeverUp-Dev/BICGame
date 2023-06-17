using UnityEngine;

namespace Hypocrites.UI.Inventory
{
    public abstract class ItemEffect : ScriptableObject
    {
        // Start is called before the first frame update
        public abstract bool ExecuteRole();
    }
}