using FishNet.Object;
using UnityEngine;
using System.Collections;

public class ProjectileSpawner : NetworkBehaviour
{
    public static ProjectileSpawner Instance;
    [SerializeField] private NetworkObject projectilePrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Call this from server or via ServerRpc to spawn a projectile.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SpawnProjectileServer(Vector3 position, Quaternion rotation, float range, float speed, float dmg, bool isPlayer)
    {
        if (!IsServerInitialized) return;

        // Instantiate projectile
        NetworkObject proj = Instantiate(projectilePrefab, position, rotation);

        Bullet projScript = proj.GetComponent<Bullet>();
        projScript.speed = speed;
        projScript.damage = dmg;

        if (isPlayer)
        {
            // Set tag and layer for player projectiles
            proj.gameObject.tag = "PlayerProjectile";
            proj.gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");
            projScript.AddLayerToMask(LayerMask.NameToLayer("Enemy"));
            //projScript.owner = owner;
        } else
        {
            // Set tag and layer for enemy projectiles
            proj.gameObject.tag = "EnemyProjectile";
            proj.gameObject.layer = LayerMask.NameToLayer("EnemyProjectile");
            projScript.AddLayerToMask(LayerMask.NameToLayer("Player"));
        }
        // Spawn it on all clients (server authority)
        Spawn(proj);

        // Start despawn timer
        StartCoroutine(DespawnAfterDelay(proj, range));
    }

    private IEnumerator DespawnAfterDelay(NetworkObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null && obj.IsSpawned)
            Despawn(obj); // Serverseitige Löschung für alle Clients
    }
}
