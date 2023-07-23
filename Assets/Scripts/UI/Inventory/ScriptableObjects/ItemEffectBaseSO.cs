namespace Hypocrites.UI.Inventory
{
    using Event.SO;
    using DB.Data;

    public abstract class ItemEffectBaseSO : BaseSO
    {
        public abstract bool Execute(Member target);
    }
}