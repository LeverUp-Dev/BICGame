using System;

namespace Hypocrites.DB.Save
{
    [Serializable]
    public class PlayerSave : BeingSave
    {
        public string portraitPath;
        public int exp;
        public bool isMember;
    }
}
