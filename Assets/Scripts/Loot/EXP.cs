using UnityEngine;
using FishNet.Object;

public class EXP : NetworkBehaviour
{
    public int expAmount = 10;
    [SerializeField] Attackmodifire attackmodifire;

    private void OnTriggerEnter( Collider other )
    {
        if (other != null && other.gameObject.CompareTag("Player"))
        {
            LootManager.instance.AddEXP(expAmount);
            if (attackmodifire != Attackmodifire.none)
            {
                //Get playeractions of other
                PlayerActions pa = other.gameObject.GetComponent<PlayerActions>();
                pa.AddAttackModifire(attackmodifire);
            }
            Despawn();
        }
    }

}
