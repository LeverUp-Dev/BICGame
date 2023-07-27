using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.UI.TempBattleUI
{
    using Skill;

    public class MemberSkillSlotUI : MonoBehaviour
    {
        public Button[] skillButtons;
        public TextMeshProUGUI[] skillButtonTexts;

        public void Initialize(Skill[] skillSlot)
        {
            for (int i = 0; i < skillSlot.Length; ++i)
            {
                if (skillSlot[i] != null)
                    skillButtonTexts[i].text = skillSlot[i].Name;
                else
                    skillButtonTexts[i].text = "";
            }
        }
    }
}
