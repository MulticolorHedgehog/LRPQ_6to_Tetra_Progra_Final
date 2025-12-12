using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;

    [Header("Victoria UI")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private TextMeshProUGUI winnerNameText;

    // Networked properties
    [Networked] private int Player1Score { get; set; }
    [Networked] private int Player2Score { get; set; }
    [Networked] private NetworkString<_16> Player1Name { get; set; }
    [Networked] private NetworkString<_16> Player2Name { get; set; }
    [Networked] private PlayerRef Player1Ref { get; set; }
    [Networked] private PlayerRef Player2Ref { get; set; }

    
    [Networked] private NetworkBool isVictoryActive { get; set; }
    [Networked] private PlayerRef winnerPlayerRef { get; set; }
    [Networked] private NetworkString<_32> winnerMessage { get; set; }

    private Dictionary<PlayerRef, PlayerScore> playerScoreComponents = new Dictionary<PlayerRef, PlayerScore>();

    
    

    public override void Spawned()
    {

        RPC_UpdateUI();
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (Object.HasStateAuthority)
        {
            Runner.AddCallbacks();
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (runner != null)
        {
            runner.RemoveCallbacks();
        }
    }

    public void LocalRegisterPlayer(PlayerScore playerScore)
    {
        if (!playerScoreComponents.ContainsKey(playerScore.Object.InputAuthority))
        {
            playerScoreComponents[playerScore.Object.InputAuthority] = playerScore;
            RPC_RegisterPlayer(playerScore.Object.InputAuthority, playerScore.PlayerName.ToString());
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RegisterPlayer(PlayerRef playerRef, string playerName)
    {
        if (Runner.IsServer && !isVictoryActive)
        {
            if (Player1Ref == PlayerRef.None)
            {
                Player1Ref = playerRef;
                Player1Name = playerName;
                Debug.Log($"Jugador 1 registrado: {playerName}");
            }
            else if (Player2Ref == PlayerRef.None && Player1Ref != playerRef)
            {
                Player2Ref = playerRef;
                Player2Name = playerName;
                Debug.Log($"Jugador 2 registrado: {playerName}");
            }

            RPC_UpdateAllClients();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayerHit(PlayerRef shooterRef, PlayerRef targetRef, int damage = 1)
    {

        RPC_ProcessHit(shooterRef, targetRef, damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ProcessHit(PlayerRef shooterRef, PlayerRef targetRef, int damage)
    {



        PlayerRef scoringPlayer = shooterRef;
        int pointsToAdd = 1; 

        if (scoringPlayer == Player1Ref)
        {
            Player1Score += pointsToAdd;
            Debug.Log($"Jugador 1 anotó {pointsToAdd} puntos! Total: {Player1Score}");
        }
        else if (scoringPlayer == Player2Ref)
        {
            Player2Score += pointsToAdd;
            Debug.Log($"Jugador 2 anotó {pointsToAdd} puntos! Total: {Player2Score}");
        }

        RPC_UpdateAllClients();

        RPC_OnPlayerScored(scoringPlayer, pointsToAdd);

        RPC_CheckVictoryCondition();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnPlayerScored(PlayerRef playerRef, int points)
    {
        

        Debug.Log($"Jugador {GetPlayerName(playerRef)} anotó {points} puntos!");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_CheckVictoryCondition()
    {

        int victoryScore = 1; 
        PlayerRef winner = PlayerRef.None;
        string message = "";

        if (Player1Score >= victoryScore)
        {
            winner = Player1Ref;
            message = $"{Player1Name} GANÓ!";
        }
        else if (Player2Score >= victoryScore)
        {
            winner = Player2Ref;
            message = $"{Player2Name} GANÓ!";
        }

        if (winner != PlayerRef.None)
        {
            LocalShowVictory(winner, message);
        }
    }

    private void LocalShowVictory(PlayerRef winner, string message)
    {

        isVictoryActive = true;
        winnerPlayerRef = winner;
        winnerMessage = message;

        RPC_ShowVictoryPanel(winner, message);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowVictoryPanel(PlayerRef winner, string message)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            victoryText.text = "¡VICTORIA!";
            winnerNameText.text = message;
            RPC_HideVictoryPanel();
        }

        Debug.Log($"VICTORIA: {message}");

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HideVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }



        if (Object.HasStateAuthority)
        {
            isVictoryActive = false;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateAllClients()
    {
        RPC_UpdateUI();
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateUI()
    {

        bool isPlayer1Local = Runner.LocalPlayer == Player1Ref;


        if (isPlayer1Local)
        {
            player1ScoreText.text = Player1Score.ToString();

            player2ScoreText.text = Player2Score.ToString();
        }
        else
        {
            player1ScoreText.text = Player2Score.ToString();

            player2ScoreText.text = Player1Score.ToString();
        }

        
        if (Player1Ref == PlayerRef.None)
        {

            player1ScoreText.text = "0";
        }

        if (Player2Ref == PlayerRef.None)
        {

            player2ScoreText.text = "0";
        }
    }

    
    private string GetPlayerName(PlayerRef playerRef)
    {
        if (playerRef == Player1Ref)
        {
            Player1Name.ToString();
        }

        if (playerRef == Player2Ref)
        {
            Player2Name.ToString();
        }

        return "Desconocido";
    }

    public int GetPlayerScore(PlayerRef playerRef)
    {
        if (playerRef == Player1Ref)
        {
            return Player1Score;
        }
            
        if (playerRef == Player2Ref)
        {
            return Player2Score;
        }
        return 0;
    }

    public PlayerRef GetOpponent(PlayerRef playerRef)
    {
        if (playerRef == Player1Ref)
        {
            return Player2Ref;
        }

        if (playerRef == Player2Ref)
        {
            return Player1Ref;
        }
        return PlayerRef.None;
    }




}
