using System;

namespace Hypocrites.DB.Save
{
    [Serializable]
    public class PlayerSave : BeingSave
    {
        public int exp;

        public bool isMember;
    }
}
