using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreManager : NetworkBehaviour
{

    [Header("UI References")]
    public TextMeshProUGUI TextoJ1;
    public TextMeshProUGUI TextoJ2;
    public TextMeshProUGUI WinnerText;
    public GameObject WinPanel;

    [Header("Game Settings")]
    public int WinScore = 1;

    [Networked, Capacity(4)]
    private NetworkLinkedList<PlayerScore> Players { get; }

    [Networked] private TickTimer WinCooldown { get; set; }
    [Networked] private NetworkBool GameEnded { get; set; }

    private void Awake()
    {
        
        if (FindObjectsOfType<ScoreManager>().Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public override void Spawned()
    {
        base.Spawned();

        // Inicializar UI
        if (WinnerText != null) WinnerText.gameObject.SetActive(false);
        if (WinPanel != null) WinPanel.SetActive(false);
        GameEnded = false;

        RPC_UpdateUI();
    }

    public override void FixedUpdateNetwork()
    {
        
        if (GameEnded && WinCooldown.Expired(Runner))
        {
            RPC_ResetGame();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RegisterPlayer(PlayerScore player)
    {
        if (!Players.Contains(player))
        {
            Players.Add(player);
            RPC_UpdateUI();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_UnregisterPlayer(PlayerScore player)
    {
        if (Players.Contains(player))
        {
            Players.Remove(player);
            RPC_UpdateUI();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_OnScoreChanged(PlayerScore changedPlayer)
    {
        RPC_UpdateUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateUI()
    {
        if (Players.Count == 0) return;

        
        PlayerScore localPlayer = null;
        PlayerScore remotePlayer = null;

        foreach (var player in Players)
        {
            if (player.Object.HasInputAuthority)
            {
                localPlayer = player;
            }
            else
            {
                remotePlayer = player;
            }
        }

        
        if (TextoJ1 != null)
        {
            TextoJ1.text = localPlayer != null ? $"{localPlayer.Score}" : "??";
        }

        if (TextoJ2 != null)
        {
            TextoJ2.text = remotePlayer != null ? $"{remotePlayer.Score}" : "??";
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayerWon(PlayerScore winner)
    {
        if (GameEnded) return;

        GameEnded = true;
        WinCooldown = TickTimer.CreateFromSeconds(Runner, 3f);

        
        if (WinnerText != null)
        {
            WinnerText.gameObject.SetActive(true);
            WinnerText.text = $"{winner.PlayerName} GANA!\nPuntuación: {winner.Score}";
        }

        if (WinPanel != null)
        {
            WinPanel.SetActive(true);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ResetGame()
    {
        GameEnded = false;

        
        if (WinnerText != null)
        {
            WinnerText.gameObject.SetActive(false);
        }
            
        if (WinPanel != null)
        {
            WinPanel.SetActive(false);
        }
            

        
        foreach (var player in Players)
        {
            player.RPC_ResetScore();
        }

        RPC_UpdateUI();
    }

    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddScoreToPlayer(PlayerRef playerRef, int amount)
    {
        foreach (var player in Players)
        {
            if (player.Object.InputAuthority == playerRef)
            {
                player.RPC_AddScore(amount);
                break;
            }
        }
    }

    
    public int GetLocalPlayerScore()
    {
        foreach (var player in Players)
        {
            if (player.Object.HasInputAuthority)
            {
                return player.Score;
            }
        }
        return 0;
    }

    public int GetOpponentScore()
    {
        foreach (var player in Players)
        {
            if (!player.Object.HasInputAuthority)
            {
                return player.Score;
            }
        }
        return 0;
    }



}
