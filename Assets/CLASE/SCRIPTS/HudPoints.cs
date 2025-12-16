using Fusion;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudPoints : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;
    [SerializeField] private TextMeshProUGUI winText;
    

    private ScoreManager _scoreManager;

    private void Start()
    {
        _scoreManager = FindObjectOfType<ScoreManager>();

        if (_scoreManager == null)
        {
            Debug.LogError("No se encontró NetworkedScoreManager!");
        }

        
        winText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_scoreManager != null)
        {
            UpdateScoreUI();
        }
    }

    private void UpdateScoreUI()
    {
        // Aquí puedes actualizar UI específica si es necesario
        // El ScoreManager ya maneja la actualización principal
    }

    // Método para mostrar victoria (llamado desde RPC)
    public void ShowWinner(string winnerName)
    {
        winText.text = $"¡{winnerName} Gana!";
        winText.gameObject.SetActive(true);
        
    }






}
