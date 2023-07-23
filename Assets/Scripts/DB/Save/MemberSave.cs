using System;

namespace Hypocrites.DB.Save
{
    using DB.Data;

    [Serializable]
    public class MemberSave : BeingSave
    {
        public string portraitPath;
        public bool isMember;
        public string[] skillSlot;

        public MemberSave(Member data) : base(data)
        {
            portraitPath = data.PortraitPath;
            isMember = data.IsMember;
            for (int i = 0; i < 4; i++)
            {
                skillSlot[i] = data.SkillSlot[i]?.Name;
            }
        }
    }
}
