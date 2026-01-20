using UnityEngine;
using FishNet.Object;
using System.Collections;

public class Spawner : NetworkBehaviour
{
    public NetworkObject cubePrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return null; // 1 Frame warten

        var obj = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity);

        // WICHTIG: exakt dieselbe Methode wie in deinem Projekt
        Spawn(obj);
    }
}
