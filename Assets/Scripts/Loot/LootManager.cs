using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager instance;

    public int PlayerEXP;

    [SerializeField] GameObject LevelUpUi;
    [SerializeField] GameObject LootUi;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChestCollected()
    {
        //LootUi.SetActive(true);
        //Time.timeScale = 0f;
    }
}
