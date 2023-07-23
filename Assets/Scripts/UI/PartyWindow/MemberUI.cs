using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.UI.PartyWindow
{
    using DB.Data;
    using UI.StatusWindow;

    public class MemberUI : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI expText;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI mpText;
        public TextMeshProUGUI StrengthText;
        public TextMeshProUGUI DexterityText;
        public TextMeshProUGUI IntelligenceText;
        public TextMeshProUGUI VitalityText;
        public TextMeshProUGUI LuckText;

        public delegate void OnClickMemberInfo(string name);
        public OnClickMemberInfo onClickMemberInfo;

        public void LoadMember(Member member)
        {
            image.sprite = member.Portrait;

            nameText.text = member.Name;
            //levelText.text = "Lv." + member.Level;
            //expText.text = "(" + member.Exp + "/100)";

            hpText.text = "HP : " + member.Status.Health + "/100";
            mpText.text = "MP : " + member.Status.Mana + "/100";

            /*StrengthText.text = "STR\n" + member.Strength;
            DexterityText.text = "DEX\n" + member.Dexterity;
            IntelligenceText.text = "INT\n" + member.Intelligence;
            VitalityText.text = "VIT\n" + member.Vitality;
            LuckText.text = "LUK\n" + member.Luck;*/
        }

        public string GetMemberName()
        {
            return nameText.text;
        }

        public void LoadStatusWindow()
        {
            onClickMemberInfo(GetMemberName());
        }
    }
}
