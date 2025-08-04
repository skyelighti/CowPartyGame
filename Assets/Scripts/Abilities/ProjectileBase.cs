using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
public abstract class ProjectileBase : PlayerAbilties
{
    public GameObject bulletObj;
    //private Transform raycastMarker;
    Camera cam;
    CinemachineBrain brain;

    private void OnEnable()
    {
        cam = Camera.main;
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    public override void Ability()
    {
        if (IsOwner)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 camPos = brain.State.GetFinalPosition();
            Vector3 toPlayer = transform.position - camPos;
            Vector3 forward = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;
            ray.origin = camPos + Vector3.Project(toPlayer, forward);
            ShootServerServerRpc(gameObject.tag, ray.origin, ray.direction);
        }
    }

    [Rpc(SendTo.Server)]
    public void ShootServerServerRpc(string shooterTag, Vector3 origin, Vector3 direction)
    {
        //maybe add a spread to the gun
        //SERVER PROJECTILE!!!!!!
        //instantiate bullet with correct rotation
        Quaternion bulletForward = Quaternion.LookRotation(direction - origin);
        Instantiate(bulletObj, origin, bulletForward);
        bulletObj.tag = gameObject.tag;
        //launch bullet in direction
    }

    public PlayerController player;
}
