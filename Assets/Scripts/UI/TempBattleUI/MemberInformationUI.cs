using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.UI.TempBattleUI
{
    using DB.Data;

    public class MemberInformationUI : MonoBehaviour
    {
        public Image portrait;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI mpText;
        public MemberSkillSlotUI memberSkillSlotUI;

        public void Initialize(Member member)
        {
            portrait.sprite = member.Portrait;
            nameText.text = member.Name;
            hpText.text = member.Status.Health + "";
            mpText.text = member.Status.Mana + "";

            memberSkillSlotUI.Initialize(member.SkillSlot);
        }
    }
}
