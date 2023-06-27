using UnityEngine;

namespace Hypocrites.DB.Data
{
    using DB.Save;
    using Defines;

    public class PlayerData : BeingData
    {
        public Sprite Portrait { get; private set; }
        public int Exp { get; private set; }
        public bool IsMember { get; set; }
        public string PortraitPath { get; private set; }

        /* 상태창 UI 콜백 */
        public delegate void OnHpChanged(int max, int cur);
        public OnHpChanged onHpChanged;

        public PlayerData() : base()
        {

        }

        public PlayerData(PlayerSave save) : base(save)
        {
            PortraitPath = save.portraitPath;
            Portrait = Resources.Load<Sprite>(save.portraitPath);
            Exp = save.exp;
            IsMember = save.isMember;
        }

        public override void Dealt(int damage)
        {
            base.Dealt(damage);

            onHpChanged(BeingConstants.MAX_STAT_HEALTH, Health);
        }

        public override void Healed(int healPoint)
        {
            base.Healed(healPoint);

            onHpChanged(BeingConstants.MAX_STAT_HEALTH, Health);
        }
    }
}
