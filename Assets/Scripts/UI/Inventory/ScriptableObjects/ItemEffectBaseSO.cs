namespace Hypocrites.UI.Inventory
{
    using SO;
    using DB.Data;

    public abstract class ItemEffectBaseSO : BaseSO
    {
        public abstract bool Execute(PlayerData target);
    }
}