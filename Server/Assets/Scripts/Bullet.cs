using Riptide.Demos.DedicatedServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public static Dictionary<int, Bullet> bullets = new Dictionary<int, Bullet>();
    public static int nextBulletId = 1;

    public int id;
    public Rigidbody rigidBody;
    public int playerId;
    public Vector3 initialForce;

    private void FixedUpdate()
    {
        Player.SendBulletPosition(this);
    }

    public void Initialize(Vector3 initialDirection, float forceStrength, int playerId)
    {
        id = nextBulletId;
        nextBulletId++;
        bullets.Add(id, this);
        Debug.Log("1:" + id);

        Debug.Log("2:" + id);
        this.transform.forward = initialDirection;
        this.initialForce = initialDirection * forceStrength;
        this.playerId = playerId;

        rigidBody.AddForce(initialForce);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("PLAYER HIT");
            Player player = collision.collider.GetComponent<Player>();
            player.UpdateHealth(player.Health - 1);
            player.SendHealth();
        }
        else
            Debug.Log("GROUND HIT");

        Player.SendBulletDestroy(this);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        bullets.Remove(id);
    }
}
