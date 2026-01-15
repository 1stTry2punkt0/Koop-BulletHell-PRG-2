using UnityEngine;
using FishNet.Object;
using GameKit.Dependencies.Utilities.ObjectPooling.Examples;
using UnityEngine.VFX;

public class Bullet : NetworkBehaviour
{
    public float speed = 10f;
    public float lifeTime = 5f;
    public float damage = 1;
    [SerializeField] LayerMask layerMask;



    private void Awake()
    {
        StartEffect();
    }


    private void Update() 
    {
        if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out RaycastHit hit, speed * Time.deltaTime, layerMask))
        {
            if (hit.collider != null )
            {
                if (IsServerInitialized)
                    Despawn();
            }
        }

        //move the bullet forward in z direction
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            if(IsServerInitialized)
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
        if (IsServerInitialized)
            Despawn();
    }


    public void StartEffect()
    {
        var vfx = GetComponent<VisualEffect>();
        vfx.Simulate(1.5f); // Effekt 1.5 Sekunden vorspulen
        vfx.Play();
    }
}
