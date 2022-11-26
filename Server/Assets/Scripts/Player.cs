using Riptide;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> List { get; private set; } = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }
    public int Health { get; private set; }

    [SerializeField] private CharacterController controller;
    [SerializeField] private float gravity;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float throwForce;
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject gunMuzzle;

    public bool[] Inputs { get; set; }
    private float yVelocity;

    private void OnValidate()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
        Health = 5;

        Inputs = new bool[6];
    }

    private void FixedUpdate()
    {
        Vector2 inputDirection = Vector2.zero;
        if (Inputs[0])
            inputDirection.y += 1;

        if (Inputs[1])
            inputDirection.y -= 1;

        if (Inputs[2])
            inputDirection.x -= 1;

        if (Inputs[3])
            inputDirection.x += 1;


        Move(inputDirection);
    }

    private void Move(Vector2 inputDirection)
    {
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (Inputs[4])
                yVelocity = jumpSpeed;
        }
        yVelocity += gravity;

        moveDirection.y = yVelocity;
        controller.Move(moveDirection);

        SendMovement();
    }

    public void SetForwardDirection(Vector3 forward, Quaternion gunRotation)
    {
        gun.transform.forward = forward;
        gun.transform.rotation = gunRotation;
        forward.y = 0; // Keep the player upright
        transform.forward = forward;

    }

    public void UpdateHealth(int newHealth)
    {
        Health = newHealth;
    }

    private void OnDestroy()
    {
        List.Remove(Id);
    }

    public static void Spawn(ushort id, string username)
    {
        Player player = Instantiate(NetworkManager.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(username == "" ? "Guest" : username)})";
        player.Id = id;
        player.Username = username;

        player.SendSpawn();
        List.Add(player.Id, player);
    }

    public void CreateBullet(Vector3 origin, Vector3 direction)
    {
        Bullet bullet = Instantiate(NetworkManager.Singleton.BulletPrefab, origin, Quaternion.identity).GetComponent<Bullet>();
        bullet.Initialize(direction, throwForce, Id);

        SendBulletCreate(bullet, origin);
    }

    #region Messages
    /// <summary>Sends a player's info to the given client.</summary>
    /// <param name="toClient">The client to send the message to.</param>
    public void SendSpawn(ushort toClient)
    {
        NetworkManager.Singleton.Server.Send(GetSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.SpawnPlayer)), toClient);
    }
    /// <summary>Sends a player's info to all clients.</summary>
    private void SendSpawn()
    {
        NetworkManager.Singleton.Server.SendToAll(GetSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.SpawnPlayer)));
    }

    private Message GetSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    private void SendMovement()
    {
        Message message = Message.Create(NetworkManager.Singleton.mode, ServerToClientId.PlayerMovement);
        message.AddUShort(Id);
        message.AddVector3(transform.position);
        message.AddVector3(transform.forward);
        message.AddQuaternion(gun.transform.rotation);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void SendBulletCreate(Bullet bullet, Vector3 origin)
    {
        Message message = Message.Create(NetworkManager.Singleton.mode, ServerToClientId.BulletCreate);
        Debug.Log("3:" + bullet.id);
        message.AddInt(bullet.id);
        message.AddVector3(origin);
        message.AddVector3(bullet.transform.forward);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public static void SendBulletPosition(Bullet bullet)
    {
        Message message = Message.Create(NetworkManager.Singleton.mode, ServerToClientId.BulletUpdate);
        message.AddInt(bullet.id);
        message.AddVector3(bullet.transform.position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void SendHealth()
    {
        Message message = Message.Create(NetworkManager.Singleton.mode, ServerToClientId.HealthUpdate);
        message.AddUShort(this.Id);
        message.AddInt(this.Health);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public static void SendBulletDestroy(Bullet bullet)
    {
        Message message = Message.Create(NetworkManager.Singleton.mode, ServerToClientId.BulletDestroy);
        message.AddInt(bullet.id);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    [MessageHandler((ushort)ClientToServerId.PlayerName)]
    private static void PlayerName(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.PlayerInput)]
    private static void PlayerInput(ushort fromClientId, Message message)
    {
        Player player = List[fromClientId];
        message.GetBools(6, player.Inputs);
        player.SetForwardDirection(message.GetVector3(), message.GetQuaternion());
    }

    [MessageHandler((ushort)ClientToServerId.BulletInput)]
    private static void BulletInput(ushort fromClientId, Message message)
    {
        Player player = List[fromClientId];
        Debug.Log("HERENOW");
        player.CreateBullet(player.gunMuzzle.transform.position, player.gunMuzzle.transform.forward);
    }
    #endregion
}
