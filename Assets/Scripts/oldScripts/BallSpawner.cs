using UnityEngine;
using FishNet.Object;
using System.Collections;

public class BallSpawner : NetworkBehaviour
{
    public static BallSpawner Instance;
    [SerializeField] GameObject ballPrefab;

    private void Start()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Server]
    public IEnumerator SpawnBall(float delay)
    {
        yield return new WaitForSeconds(delay);
        if(NetworkGameManager.Instance.CurrentState == GameState.Playing)
        {
            GameObject ballInstance = Instantiate(ballPrefab);
            ballInstance.GetComponentInChildren<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
            Spawn(ballInstance);

        }
    }
}
