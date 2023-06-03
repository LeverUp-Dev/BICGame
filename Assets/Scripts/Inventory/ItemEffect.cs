using UnityEngine;

namespace Hypocrites.Inventory
{
    public abstract class ItemEffect : ScriptableObject
    {
        // Start is called before the first frame update
        public abstract bool ExecuteRole();
    }
}