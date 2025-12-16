using System.Globalization;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerScore : NetworkBehaviour
{
    [Networked] public int Score { get; set; }
    [Networked] public NetworkString<_16> PlayerName { get; set; }
    [Networked] public PlayerRef NetworkPlayer { get; set; }

    private ScoreManager _scoreManager;

    public override void Spawned()
    {
        base.Spawned();

        
        _scoreManager = FindObjectOfType<ScoreManager>();
        if (_scoreManager == null)
        {
            GameObject go = new GameObject("ScoreManager");
            _scoreManager = go.AddComponent<ScoreManager>();
        }

        
        if (Object.HasInputAuthority)
        {
            PlayerName = "Player_" + Runner.LocalPlayer;
        }

        if (_scoreManager != null)
        {
            _scoreManager.RPC_RegisterPlayer(this);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (_scoreManager != null)
        {
            _scoreManager.RPC_UnregisterPlayer(this);
        }
        base.Despawned(runner, hasState);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddScore(int amount)
    {
        Score += amount;

        
        if (_scoreManager != null)
        {
            _scoreManager.RPC_OnScoreChanged(this);

            
            if (Score >= _scoreManager.WinScore)
            {
                _scoreManager.RPC_PlayerWon(this);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ResetScore()
    {
        Score = 0;
    }


}





    


