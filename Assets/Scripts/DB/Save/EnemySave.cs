using System;

namespace Hypocrites.DB.Save
{
    using DB.Data;

    [Serializable]
    public class EnemySave : BeingSave
    {
        public float height;

        public EnemySave()
        {

        }

        public EnemySave(Enemy enemy) : base(enemy)
        {
            height = enemy.Height;
        }
    }
}
