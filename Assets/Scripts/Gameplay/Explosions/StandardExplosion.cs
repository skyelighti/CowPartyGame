using UnityEngine;

public class StandardExplosion : ExplosionBase
{
    [SerializeField] float pushForce;
    public override void OnHit(Collider hit)
    {
        print("hit something! (explosion)");
        if (hit.gameObject.CompareTag(gameObject.tag)) return;
        if (hit.transform.gameObject.TryGetComponent(out IDamageable damageable))
        {
            foreach (GameObject compareObj in alreadyHit)
            {
                if (hit.gameObject == compareObj) return;
            }
            alreadyHit.Add(hit.gameObject);
            damageable.OnHit(ulong.MaxValue, damage);
            //knockback here too
        }
    }
}
