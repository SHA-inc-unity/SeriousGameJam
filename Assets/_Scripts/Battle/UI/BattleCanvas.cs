using UnityEngine;
using UnityEngine.UI;

public class BattleCanvas : MonoBehaviour
{
    [SerializeField] private Image playerSprite;
    [SerializeField] private Image enemySprite;
    [SerializeField] private WheelSpinUI playerWheel;
    [SerializeField] private WheelSpinUI enemyWheel;

    [Header("HP Display")]
    [SerializeField] private Transform playerHPAnchor;
    [SerializeField] private Transform enemyHPAnchor;
    [SerializeField] private HPDisplay hpDisplayPrefab;

    private HPDisplay playerHP;
    private HPDisplay enemyHP;

    // Exposed so BattleManager can pass them into its combatantWheelUI dictionary
    public WheelSpinUI PlayerWheelUI => playerWheel;
    public WheelSpinUI EnemyWheelUI  => enemyWheel;

    public void InitPlayerHP(int maxHP)
    {
        playerHP = Instantiate(hpDisplayPrefab);
        playerHP.Init(maxHP, playerHPAnchor);
        playerHP.UpdateHP(maxHP);
    }

    public void InitEnemyHP(int maxHP)
    {
        enemyHP = Instantiate(hpDisplayPrefab);
        enemyHP.Init(maxHP, enemyHPAnchor);
        enemyHP.UpdateHP(maxHP);
    }

    public void UpdatePlayerHP(int currentHP, int overhealth = 0)
    {
        if (overhealth > 0) playerHP.UpdateHPWithOverhealth(currentHP, overhealth);
        else                playerHP.UpdateHP(currentHP);
    }
    public void UpdateEnemyHP(int currentHP, int overhealth = 0)
    {
        if (overhealth > 0) enemyHP.UpdateHPWithOverhealth(currentHP, overhealth);
        else                enemyHP.UpdateHP(currentHP);
    }

    public void SetPlayerSprite(Sprite sprite) => playerSprite.sprite = sprite;
    public void SetEnemySprite(Sprite sprite)  => enemySprite.sprite  = sprite;
    public void SetPlayerWheel(Sprite sprite)  => playerWheel.SetWheelSprite(sprite);
    public void SetEnemyWheel(Sprite sprite)   => enemyWheel.SetWheelSprite(sprite);

    public void PlayPlayerWheelSpin(int winningIndex, int slotCount, System.Action onComplete, float durationOverride = -1f)
        => playerWheel.PlaySpin(winningIndex, slotCount, onComplete, durationOverride);

    public void PlayEnemyWheelSpin(int winningIndex, int slotCount, System.Action onComplete, float durationOverride = -1f)
        => enemyWheel.PlaySpin(winningIndex, slotCount, onComplete, durationOverride);
}