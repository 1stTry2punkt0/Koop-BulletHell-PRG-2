using UnityEngine;
using FishNet.Object;
using GameKit.Dependencies.Utilities.ObjectPooling.Examples;
using UnityEngine.VFX;

public class Bullet : NetworkBehaviour
{
    public Vector3 direction;
    public float speed = 10f;
    public float lifeTime = 5f;
    public float damage = 1;
    public Vector3 noY => new Vector3(direction.x, 0, direction.z).normalized;


    private void Awake()
    {
        StartEffect();
    }


    private void Update() 
    {
        transform.position += noY * speed * Time.deltaTime;
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Despawn();
        }
    }

    public void SetTag(string tag)
    {
        gameObject.tag = tag;
    }

    public void SetLayer(int layer)
    {
        gameObject.layer = layer;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet collided with: " + collision.collider.name);
        Despawn();
    }


    public void StartEffect()
    {
        var vfx = GetComponent<VisualEffect>();
        vfx.Simulate(1.5f); // Effekt 1.5 Sekunden vorspulen
        vfx.Play();
    }
}
