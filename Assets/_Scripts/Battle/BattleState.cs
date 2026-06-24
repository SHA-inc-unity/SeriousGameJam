// Enum-based state machine for the wheel battle.
// Enum + switch is the right call for a 1-week jam: easy to debug,
// easy for teammates to read, no need for a fancy FSM framework.
using System;

[Serializable]
public enum BattleState
{
    Null, // Sorry, from SHA with love
    Intro,          // battle just started (e.g. "Hook-a-duck guy challenges you!")
    PlayerTurn,     // waiting for player to spin
    PlayerResolve,  // applying the result of player's spin
    EnemyTurn,      // enemy spins (probably automatic / no input needed)
    EnemyResolve,   // applying the result of enemy's spin
    Win,
    Lose
}