using System;

namespace Hypocrites.DB.Save
{
    using DB.Data;

    [Serializable]
    public class PlayerSave : BeingSave
    {
        public string portraitPath;
        public int exp;
        public bool isMember;

        public PlayerSave(PlayerData data) : base(data)
        {
            portraitPath = data.PortraitPath;
            exp = data.Exp;
            isMember = data.IsMember;
        }
    }
}
