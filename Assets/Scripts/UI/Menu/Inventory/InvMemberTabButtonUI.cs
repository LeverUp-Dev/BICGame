using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites
{
    public class InvMemberTabButtonUI : TabButtonUI
    {
        public Transform backRoot;
        public Transform foreRoot;

        Transform tr;

        private void Awake()
        {
            tr = GetComponent<Transform>();
        }

        public override void SetHighlight(bool highlighting)
        {
            base.SetHighlight(highlighting);

            if (highlighting)
                tr.SetParent(foreRoot); 
            else
                tr.SetParent(backRoot);
        }
    }
}
