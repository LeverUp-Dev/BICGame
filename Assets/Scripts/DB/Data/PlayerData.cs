namespace Hypocrites.DB.Data
{
    using DB.Save;

    public class PlayerData : BeingData
    {
        public bool IsMember { get; private set; }

        public PlayerData() : base()
        {

        }

        public PlayerData(PlayerSave save) : base(save)
        {
            IsMember = save.isMember;
        }
    }
}
