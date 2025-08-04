//using Mirror;
//using UnityEngine;

//public class SoundNetworkManager : NetworkManager
//{
//    [Header("Singleton audio router")]
//    public NetSfx netSfxPrefab;
//
//    public override void OnStartServer()
//    {
//        base.OnStartServer();
//
//        if (NetSfx.Instance == null)
//        {
//            NetSfx sfx = Instantiate(netSfxPrefab);
//            NetworkServer.Spawn(sfx.gameObject);
//        }
//    }
//}
