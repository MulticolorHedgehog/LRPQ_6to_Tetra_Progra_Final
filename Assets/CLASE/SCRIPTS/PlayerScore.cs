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
            scoreManager.RegisterPlayer(this);

            
            if (Object.HasInputAuthority)
            {
                RPC_SyncInitialScore(Score);
            }
        }
    }

    public void AddScore(int points)
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

    


