using Fusion;
using UnityEngine;


namespace Player
{
    public class WeaponHandler : NetworkBehaviour
    {

        [SerializeField] private Weapon actualWeapon;
        

        public override void FixedUpdateNetwork()
        {
            if(GetInput(out NetworkInputData input))
            {
                if(input.shoot)
                {
                    actualWeapon.RpcRaycastShoot();
                    actualWeapon.RpcRigidBodyShoot();

                }
            }

            

        }




    }
}

