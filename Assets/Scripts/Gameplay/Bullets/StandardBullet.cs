using UnityEngine;

public class StandardBullet : BulletBase
{
    public override void Flying()
    {
        rb.AddForce(transform.forward*speed);
    }

    public override void OnHit(Collider hit)
    {
        print("hit something!");
        if (hit.gameObject.CompareTag(gameObject.tag)) return;
        if (hit.transform.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.OnHit(ulong.MaxValue, damage);
        }
        
        Destroy(this.gameObject);
    }
}
