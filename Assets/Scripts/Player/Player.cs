using UnityEngine;

namespace Hypocrites.Player
{
    using DB.Data;

    public class Player : MonoBehaviour
    {
        public PlayerData status = new PlayerData();

        public void Dealt(int damage)
        {
            status.Dealt(damage);
        }
    }
}
