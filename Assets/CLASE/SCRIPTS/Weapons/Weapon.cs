using Fusion;
using UnityEngine;

public abstract class Weapon : NetworkBehaviour
{
    [SerializeField] protected ShootType type;
    [SerializeField] protected Transform ShootPoint;
    [SerializeField] protected Camera playerCam;
    [SerializeField] protected NetworkPrefabRef bullet;
    [SerializeField] protected LayerMask layerMask;
    
    [SerializeField] protected int damage;
    [SerializeField] protected float range;
    [SerializeField] protected int actualAmmo;
    [SerializeField] protected int MaxAmmo;

    public abstract void RpcRaycastShoot(RpcInfo info = default);


    public abstract void RpcRigidBodyShoot();

    

    


}

public enum ShootType
{
    Rigidbody, Raycast
}