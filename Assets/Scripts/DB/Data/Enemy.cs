using Hypocrites.DB.Save;

namespace Hypocrites.DB.Data
{
    public class Enemy : Being
    {
        public float Height { get; private set; }

        public Enemy(EnemySave enemy) : base(enemy)
        {
            Height = enemy.height;
        }
    }
}
