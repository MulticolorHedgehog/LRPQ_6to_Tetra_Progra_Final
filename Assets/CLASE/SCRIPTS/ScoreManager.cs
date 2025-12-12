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

    [Networked] private TickTimer victoryTimer { get; set; }
    [Networked] private NetworkBool isVictoryActive { get; set; }
    [Networked] private PlayerRef winnerPlayerRef { get; set; }
    [Networked] private NetworkString<_32> winnerMessage { get; set; }

    private Dictionary<PlayerRef, PlayerScore> playerScoreComponents = new Dictionary<PlayerRef, PlayerScore>();

    // Eventos para feedback visual/audio
    public System.Action<PlayerRef> OnPlayerScored;
    public System.Action<PlayerRef> OnVictoryAchieved;

    public override void Spawned()
    {

        UpdateUI();



        // Ocultar panel de victoria inicialmente
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

    public override void FixedUpdateNetwork()
    {
        // Manejar temporizador de victoria
        if (isVictoryActive && victoryTimer.Expired(Runner))
        {
            RPC_HideVictoryPanel();
        }
    }

    // Registro de jugadores
    public void RegisterPlayer(PlayerScore playerScore)
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

    public void PlayerHit(PlayerRef shooterRef, PlayerRef targetRef, int damage = 1)
    {

        RPC_ProcessHit(shooterRef, targetRef, damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ProcessHit(PlayerRef shooterRef, PlayerRef targetRef, int damage)
    {

        // Verificar que el shooter sea un jugador válido
        if (shooterRef != Player1Ref && shooterRef != Player2Ref)
        {
            Debug.LogWarning("Shooter no es un jugador registrado");
            return;
        }


        PlayerRef scoringPlayer = shooterRef;
        int pointsToAdd = 1; 

        // Actualizar score
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

        CheckVictoryCondition();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnPlayerScored(PlayerRef playerRef, int points)
    {
        // Feedback local cuando un jugador anota
        OnPlayerScored?.Invoke(playerRef);

        Debug.Log($"Jugador {GetPlayerName(playerRef)} anotó {points} puntos!");
    }

    // Verificar condiciones de victoria
    private void CheckVictoryCondition()
    {

        int victoryScore = 1; // Puntos necesarios para ganar
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
            ShowVictory(winner, message);
        }
    }

    private void ShowVictory(PlayerRef winner, string message)
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
        }

        OnVictoryAchieved?.Invoke(winner);

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
        UpdateUI();
    }


    private void UpdateUI()
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

        // Si hay jugadores esperando
        if (Player1Ref == PlayerRef.None)
        {

            player1ScoreText.text = "0";
        }

        if (Player2Ref == PlayerRef.None)
        {

            player2ScoreText.text = "0";
        }
    }

    // Helper methods
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

    // Reiniciar juego
    private void RequestRestartGame()
    {
        RPC_RequestRestart();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestRestart()
    {
        if (Runner.IsServer)
        {
            ResetGame();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ResetGame()
    {
        ResetGameLocal();
    }

    private void ResetGame()
    {
        Player1Score = 0;
        Player2Score = 0;
        isVictoryActive = false;

        RPC_ResetGame();
        RPC_UpdateAllClients();
    }

    private void ResetGameLocal()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        UpdateUI();
    }

    // Getters públicos
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
