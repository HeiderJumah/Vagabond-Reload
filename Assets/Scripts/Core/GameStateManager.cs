using System;
using UnityEngine;

namespace Vagabond.Core
{
    public class GameStateManager : MonoBehaviour
    {
        public GameState CurrentState { get; private set; }

        public event Action<GameState> OnStateChanged;

        public void Initialize()
        {
            Debug.Log("[GameStateManager] Initialized");
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState)
                return;

            Debug.Log($"[GameStateManager] State change: {CurrentState} -> {newState}");

            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}

