namespace Hypocrites.DB.Data
{
    using DB.Save;

    public class PlayerData : BeingData
    {
        public int Exp { get; private set; }
        public bool IsMember { get; private set; }

        public PlayerData() : base()
        {

        }

        public PlayerData(PlayerSave save) : base(save)
        {
            Exp = save.exp;
            IsMember = save.isMember;
        }
    }
}
