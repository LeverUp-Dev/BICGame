using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Hypocrites.Manager
{
    public enum GameState
    {
        OnPlay,
        OnBattle,
        OnMenu
    }

    public class GameStateManager : SingletonMono<GameStateManager>
    {
        public GameState state;

        protected override void Awake()
        {
            base.Awake();
        }
    }
}
