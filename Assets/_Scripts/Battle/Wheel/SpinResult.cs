// Holds the results of a single simultaneous spin round.
// Passed into effect execution so defensive effects can check
// what the opponent landed THIS spin, with no persistent state needed.
public class SpinResult
{
    public WheelSlotEffect playerEffect;
    public WheelSlotEffect enemyEffect;
    public int playerSlotIndex; 
    public int enemySlotIndex;   

    public WheelSlotEffect GetOpponentEffect(Combatant combatant, Combatant player)
        => combatant == player ? enemyEffect : playerEffect;
}