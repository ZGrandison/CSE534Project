using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Transports.Udp;
using Riptide.Transports.Quic;
using Riptide.Utils;
using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public enum ServerToClientId : ushort
{
    SpawnPlayer = 1,
    PlayerMovement,
    BulletCreate,
    BulletUpdate,
    BulletDestroy,
    HealthUpdate
}
public enum ClientToServerId : ushort
{
    PlayerName = 1,
    PlayerInput,
    BulletInput
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject bulletPrefab;

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;
    public GameObject BulletPrefab => bulletPrefab;

    public Client Client { get; private set; }

    public MessageSendMode mode;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client(new UdpClient(), "SERVER");
        mode = MessageSendMode.Unreliable;
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;

        Singleton = null;
    }

    private void FixedUpdate()
    {
        Client.Update();
    }

    private void OnApplicationQuit()
    {
        foreach (KeyValuePair<ushort, Player> entry in Player.list)
        {
            File.WriteAllLines("UDPPlayer" + entry.Key + ".txt", entry.Value.networkData);
            // do something with entry.Value or entry.Key
        }
        Disconnect();
    }

    public void Disconnect()
    {
        Client.Disconnect();

        Client.Connected -= DidConnect;
        Client.ConnectionFailed -= FailedToConnect;
        Client.ClientDisconnected -= PlayerLeft;
        Client.Disconnected -= DidDisconnect;
    }

    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }

    private void DidConnect(object sender, EventArgs e)
    {
        UIManager.Singleton.SendName();
    }

    private void FailedToConnect(object sender, ConnectionFailedEventArgs e)
    {
        UIManager.Singleton.BackToMain();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Id].gameObject);
    }

    private void DidDisconnect(object sender, DisconnectedEventArgs e)
    {
        UIManager.Singleton.BackToMain();

        foreach (Player player in Player.list.Values)
            Destroy(player.gameObject);
    }
}

