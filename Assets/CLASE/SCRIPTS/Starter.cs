using Fusion;
using UnityEngine;

public class Starter : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animationTrigger = "StartAnim";

    private bool animationPlayed = false;

    public override void FixedUpdateNetwork()
    {
        
        if (!Runner.IsServer) return;

        
        if (animationPlayed) return;

        
        if (Runner.SessionInfo.PlayerCount >= 2)
        {
            animationPlayed = true;
            RPC_PlayAnimation();
        }
    }

    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAnimation()
    {
        animator.SetTrigger(animationTrigger);
    }
}
