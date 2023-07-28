using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Hypocrites.UI.BattleUI
{
    public class BattleBeingUI : MonoBehaviour
    {
        public TextMeshProUGUI nameText;

        public string GetName()
        {
            return nameText.text;
        }
    }
}
