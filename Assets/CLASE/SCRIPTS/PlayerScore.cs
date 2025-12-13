using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerScore : NetworkBehaviour
{
    [Networked] public int Score { get; set; }
    [Networked] public NetworkString<_16> PlayerName { get; set; }

    [SerializeField] private ScoreManager scoreManager;
    

    public override void Spawned()
    {

        scoreManager = FindObjectOfType<ScoreManager>();

        if (scoreManager != null)
        {
            
            if (Object.HasInputAuthority)
            {
                RPC_SyncInitialScore(Score);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_AddScore(int points)
    {
        if (Object.HasInputAuthority)
        {
            Score += points;

            

        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SyncInitialScore(int currentScore)
    {
        Score = currentScore;
        
    }

    
    public void ResetScore()
    {
        if (Object.HasInputAuthority)
        {
            Score = 0;

        }
    }
}

    


