using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public int id;

    public void Initialize(int id)
    {
        this.id = id;
    }

    public void DestroyBullet()
    {
        Debug.Log("Bullet Hit: " + id);
        Destroy(gameObject);
    }
}
