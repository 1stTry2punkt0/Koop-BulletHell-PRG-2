using UnityEngine;
using FishNet.Object;

public class EXP : NetworkBehaviour
{
    public int expAmount = 10;

    private void OnTriggerEnter( Collider other )
    {
        if (other != null && other.gameObject.CompareTag("Player"))
        {
            LootManager.instance.PlayerEXP += expAmount;
            Despawn();
        }
    }

}
