using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Riptide;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();
    public static Dictionary<int, BulletManager> bullets = new Dictionary<int, BulletManager>();

    [SerializeField] private ushort id;
    [SerializeField] private string username;
    [SerializeField] private Transform fakeCam;
    private float runningJitterAvg;
    private float lastPing = -1f;
    public List<string> networkData = new List<string>();

    private int health;

    public void Move(Vector3 newPosition, Vector3 forward)
    {
        transform.position = newPosition;

        if (id != NetworkManager.Singleton.Client.Id) // Don't overwrite local player's forward direction to avoid noticeable rotational snapping
            transform.forward = forward;
    }

    private void Update()
    {
        float ping = NetworkManager.Singleton.Client.Connection.SmoothRTT;
        if (ping > 0)
        {
            if (lastPing == -1)
                runningJitterAvg = 0;
            else
                runningJitterAvg = (Math.Abs(ping - lastPing) + runningJitterAvg) / 2;
            lastPing = ping;
        }
        networkData.Add(ping + "," + runningJitterAvg);
        Debug.Log("Ping: " + ping + " | Jitter: " + runningJitterAvg);
    }

    private void Disconnect(ushort id)
    {
        Debug.Log(this.id);
        Debug.Log(id);
        Debug.Log(NetworkManager.Singleton.Client.Id);
        if (isCurrentPlayer(id))
            NetworkManager.Singleton.Disconnect();
    }

    private void OnDestroy()
    {
        list.Remove(id);
    }

    private void updateHealthUI(int health)
    {
        UIManager.Singleton.updateHealthText(health);
    }

    private bool isCurrentPlayer(int id)
    {
        return id == NetworkManager.Singleton.Client.Id;
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
            player = Instantiate(NetworkManager.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
        else
            player = Instantiate(NetworkManager.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();

        player.name = $"Player {id} ({username})";
        player.id = id;
        player.username = username;
        player.health = 5;
        list.Add(player.id, player);
    }

    public static void SpawnBullet(int id, Vector3 position, Vector3 forward)
    {
        BulletManager bullet = Instantiate(NetworkManager.Singleton.BulletPrefab, position, Quaternion.identity).GetComponent<BulletManager>();
        bullet.transform.forward = forward;
        bullet.Initialize(id);
        bullets.Add(id, bullet);

        Debug.Log(bullets[id]);
    }

    #region Messages
    [MessageHandler((ushort)ServerToClientId.SpawnPlayer)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.PlayerMovement)]
    private static void PlayerMovement(Message message)
    {
        ushort playerId = message.GetUShort();
        if (list.TryGetValue(playerId, out Player player))
        {
            Vector3 position = message.GetVector3();
            Vector3 forward = message.GetVector3();
            player.Move(position, forward);
            if (!player.isCurrentPlayer(playerId))
            {
                Debug.Log(player.fakeCam.name);
                player.fakeCam.forward = forward;
                player.fakeCam.rotation = message.GetQuaternion();
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.BulletCreate)]
    private static void BulletCreate(Message message)
    {
        Debug.Log("ARE WE HERE");
        int bulletId = message.GetInt();
        Vector3 position = message.GetVector3();
        Vector3 forward = message.GetVector3();

        SpawnBullet(bulletId, position, forward);
    }

    [MessageHandler((ushort)ServerToClientId.BulletUpdate)]
    private static void BulletUpdate(Message message)
    {
        int bulletId = message.GetInt();
        Vector3 position = message.GetVector3();

        bullets[bulletId].transform.position = position;
    }

    [MessageHandler((ushort)ServerToClientId.BulletDestroy)]
    private static void BulletDestroy(Message message)
    {
        int bulletId = message.GetInt();

        bullets[bulletId].DestroyBullet();
        bullets.Remove(bulletId);
    }

    [MessageHandler((ushort)ServerToClientId.HealthUpdate)]
    private static void HealthUpdate(Message message)
    {
        ushort playerId = message.GetUShort();
        int playerHealth = message.GetInt();

        list[playerId].health = playerHealth;
        if (playerHealth <= 0)
            list[playerId].Disconnect(playerId);

        if (list[playerId].isCurrentPlayer(playerId))
        {
            list[playerId].updateHealthUI(playerHealth);
        }
        Debug.Log("ID: " + playerId + "| HEALTH: " + playerHealth);
    }
    #endregion
}

