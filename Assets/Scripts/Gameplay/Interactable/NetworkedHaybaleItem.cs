using Unity.Netcode;
using UnityEngine;

public class NetworkedHaybaleItem : NetworkedItem
{
    [SerializeField] private GameObject haybalePrefab;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) { return; }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) { return; }
        
        // Cows are on the Default layer
        if (collision.gameObject.CompareTag("Cow")) { return; }
        // UFOs are Aliens but not Players
        if (collision.gameObject.CompareTag("Alien")) { return; }
        // This should never happen (All Farmers are Players), but we may add tractors later or something
        if (collision.gameObject.CompareTag("Farmer")) { return; }

        if (!wasThrown) { return; }

        networkObject.Despawn(true);
        GameObject newObj = Instantiate(haybalePrefab, transform.position, Quaternion.identity);
        newObj.GetComponent<NetworkObject>().Spawn();
    }
}
