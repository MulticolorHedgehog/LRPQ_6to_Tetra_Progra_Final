using Fusion;
using UnityEngine;

public class Health : NetworkBehaviour
{


    [SerializeField] private int health;

    [Networked]  public int _health { get; set; }

    

    public override void Spawned()
    {
        

        _health = health;
    }

    /// <summary>
    /// Cualquiera puede recibir daño, pero solo el host lo ejecuta
    /// porque si en el target pongo All, entonces el objetivo recibira daño
    /// de todos lados
    /// 
    /// </summary>
    
    
    
    [Rpc(RpcSources.All,RpcTargets.All)]
    public void Rpc_TakeDamage(int damage, PlayerRef shooter)
    {
        _health -= damage;
        

        Debug.Log($"{name} recibio daño {shooter}. Vida Actual");

        if( _health <= 0 )
        {
            OnDeath();
        }
    }

    private void OnDeath()
    {
        Debug.Log("Morido");
        
    }
}
