namespace Hypocrites.DB.Data
{
    using DB.Save;

    public class PlayerData : BeingData
    {
        public int Exp { get; set; }
        public bool IsMember { get; set; }

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
