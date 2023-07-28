using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.UI.BattleUI
{
    using DB.Data;
    using Skill;

    public class MemberInformationUI : MonoBehaviour
    {
        public Image portrait;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI mpText;

        public EffectsUI effectsUI;
        public MemberSkillSlotUI memberSkillSlotUI;

        public void Initialize(Member member)
        {
            portrait.sprite = member.Portrait;
            nameText.text = member.Name;
            hpText.text = member.Status.Health + "";
            mpText.text = member.Status.Mana + "";

            memberSkillSlotUI.Initialize(member);
        }

        public void AddEffectInformation(Skill effect)
        {
            effectsUI.AddEffectUI(effect);
        }

        public void RemoveEffectInformation(Skill effect)
        {
            effectsUI.RemoveEffectUI(effect);
        }
    }
}
