using UnityEngine;
using Fragsurf.Movement;
using Unity.Netcode;
using System.Collections;

public class JumpPad : NetworkBehaviour
{
    public enum allowedTeams
    {
        Aliens,
        Farmers,
        Both
    }

    [SerializeField] private float launchForce;
    [SerializeField] public allowedTeams selectedTeam;
    [SerializeField] private float lifetime;
    public Material AlienPlatformMat;
    public Material FarmerPlatformMat;
    public Material PublicPlatformMat;

    private Renderer objRenderer;



    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        UpdateMaterials();
        StartCoroutine(Lifetime());
    }
    public void UpdateMaterials()
    {
        if (objRenderer != null)
        {
            if (selectedTeam == allowedTeams.Aliens)
                objRenderer.material = AlienPlatformMat;
            if (selectedTeam == allowedTeams.Farmers)
                objRenderer.material = FarmerPlatformMat;
            if (selectedTeam == allowedTeams.Both)
                objRenderer.material = PublicPlatformMat;
        }
    }

    private void OnTriggerEnter(Collider hit)
    {
        GameObject gmObj = hit.gameObject;
        if (gmObj.layer == LayerMask.NameToLayer("Player"))
        {
            if ((selectedTeam == allowedTeams.Aliens && gmObj.tag == "Alien") || (selectedTeam == allowedTeams.Farmers && gmObj.tag == "Farmer") || (selectedTeam == allowedTeams.Both))
                LaunchPlr(hit.transform);
        }
    }

    private void LaunchPlr(Transform launchTarget)
    {
        Debug.Log("launching..." + launchTarget.parent.name);
        SurfCharacter surfChar = launchTarget.parent.GetComponent<SurfCharacter>(); //surfchar returns null for some reason.
        if (surfChar != null)
        {
            launchTarget.parent.position += new Vector3(0, 0.05f, 0);
            NetSfx.Play(SfxId.abilities, transform.position, 3);//jump pad sound :D
            Debug.Log("Launched!");
            surfChar.ApplyImpulse(transform.up, launchForce);
        }

    }

    IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
