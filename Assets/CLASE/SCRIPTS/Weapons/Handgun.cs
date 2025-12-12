using UnityEngine;
using Fusion;
using static Unity.Collections.Unicode;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Unity.Burst.Intrinsics;



public class Handgun : Weapon
{

    private ScoreManager scoreManager;
    private PlayerScore playerScore;
    /// <summary>
    /// Un RPC es un protocolo para mandar a llamar un metodo en diferentes clientes
    /// 
    /// RPCSources es quien lo manda a llamar
    /// RPCTargets es quien lo ejecuta
    /// </summary>

    [Rpc(RpcSources.InputAuthority,RpcTargets.All)]
    public override void RpcRaycastShoot(RpcInfo info = default)
    {
        
        scoreManager = FindObjectOfType<ScoreManager>();
        playerScore = GetComponent<PlayerScore>();

        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hit, range, layerMask))
        {
            Debug.Log(hit.collider.name);

            

            if (hit.collider.TryGetComponent(out Health health))
            {
                health.Rpc_TakeDamage(damage, info.Source);
                playerScore.AddScore(1);
            }
            else
            {
                Debug.Log("No hay componente de vida :(");
                
            }
        }
    }

    
    
    public override void RpcRigidBodyShoot()
    {
        

        RpcPhysicShoot(ShootPoint.position, ShootPoint.rotation);


    }
    
    /// <summary>
    /// Ya que el RpcTarget recibe el estado, este lo invoca.
    /// 
    /// Cuando el RpcSource le dice al RpcTarget invoca esto, automaticamente el RpcTarget tambien recibe informacion sobre el Rpc mismo
    /// </summary>
    

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcPhysicShoot(Vector3 pos, Quaternion rot, RpcInfo info = default)
    {
        if (bullet.IsValid)
        {
            NetworkObject bulletInstance = Runner.Spawn(bullet, pos, rot, info.Source);

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.indianRed;
        Gizmos.DrawRay(playerCam.transform.position, playerCam.transform.forward * range);
    }
}