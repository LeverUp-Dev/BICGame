using UnityEngine;

namespace Hypocrites.DB.Data
{
    using DB.Save;

    public class PlayerData : BeingData
    {
        public Sprite Portrait { get; private set; }
        public int Exp { get; private set; }
        public bool IsMember { get; private set; }

        public PlayerData() : base()
        {

        }

        public PlayerData(PlayerSave save) : base(save)
        {
            Portrait = Resources.Load<Sprite>(save.portraitPath);
            Exp = save.exp;
            IsMember = save.isMember;
        }
    }
}
