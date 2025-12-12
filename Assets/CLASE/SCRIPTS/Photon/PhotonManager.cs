using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Linq;

public class PhotonManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] NetworkSceneManagerDefault sceneManager;

    [SerializeField] private NetworkPrefabRef jugadorPrefab;
    //[SerializeField]  NetworkDictionary<PlayerRef, NetworkObject> players;

    [SerializeField] Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] private Transform[] spawnPoint;

    private List<Transform> freeSpawnPoints = new List<Transform>();

    private Dictionary<PlayerRef, Transform> playerSpawnMap = new Dictionary<PlayerRef, Transform>();

    [SerializeField] UnityEvent onPlayerJoinedToGame; // Los Unity Events son llamadas que se hacen al invocar un evento



    private async void StartGame(GameMode mode)
    {
        runner.AddCallbacks(this);
        
        runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(0);

        var sceneInfo = new NetworkSceneInfo();

        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            freeSpawnPoints = spawnPoint.ToList();
        }

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "#0001",
            Scene = scene,
            CustomLobbyName = "Oficial",
            SceneManager = sceneManager
        });
    }

    public void StartGameAsHost()
    {
        StartGame(GameMode.Host);
    }

    public void StartGameAsClient()
    {
        StartGame(GameMode.Client);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData()
        {
            move = InputManager.Instance.GetMoveInput(),
            look = InputManager.Instance.GetMouseDelta(),
            isRunning = InputManager.Instance.WasRunInputPressed(),
            yrotation = Camera.main.transform.eulerAngles.y,
            shoot = InputManager.Instance.ShootInputPressed(),
            

        };
        
        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }



    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //if(HasStateAuthority)
        //{

        //    Debug.Log("Se unio un jugador");
        //    NetworkObject playerObject = runner.Spawn(jugadorPrefab, Vector3.up, Quaternion.identity, player);
        //    players.Add(player, playerObject.GetComponent<NetworkObject>());
        //}


        if(runner.IsServer)
        {
            int index = UnityEngine.Random.Range(0, freeSpawnPoints.Count);
            Transform chosenSpawn = freeSpawnPoints[index];

            int randomSpawn = UnityEngine.Random.Range(0, spawnPoint.Length);
            NetworkObject networkPlayer = runner.Spawn(jugadorPrefab, spawnPoint[randomSpawn].position, spawnPoint[randomSpawn].rotation, player);

            players.Add(player, networkPlayer);
            
            playerSpawnMap.Add(player, chosenSpawn);

            freeSpawnPoints.RemoveAt(index);

        }
        onPlayerJoinedToGame.Invoke();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        //if(!HasStateAuthority)
        //{
        //    return;
        //}

        //if(players.TryGet(player, out NetworkObject playerBehaviour))
        //{
        //    players.Remove(player);
        //    Runner.Despawn(playerBehaviour);
        //}

        if(players.TryGetValue(player, out NetworkObject networkPlayer))
        {
            runner.Despawn(networkPlayer);
            players.Remove(player);
            
        }

        
        if (playerSpawnMap.TryGetValue(player, out Transform usedSpawn))
        {
            freeSpawnPoints.Add(usedSpawn);
            playerSpawnMap.Remove(player);
        }

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }
}




