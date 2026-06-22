using UnityEngine;
using UnityEngine.UI;

public class BattleCanvas : MonoBehaviour
{
    [SerializeField] private Image playerSprite; 
    [SerializeField] private Image enemySprite; 
    [SerializeField] private Image playerWheel;
    [SerializeField] private Image enemyWheel; 
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerSprite(Sprite sprite)
    {
        playerSprite.sprite = sprite;
    }

    public void SetEnemySprite(Sprite sprite)
    {
        enemySprite.sprite = sprite;
    }

    public void SetPlayerWheel(Sprite sprite)
    {
        playerWheel.sprite = sprite;
    }

    public void SetEnemyWheel(Sprite sprite)
    {
        enemyWheel.sprite = sprite;
    }
}
