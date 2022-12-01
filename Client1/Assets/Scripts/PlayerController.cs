using Riptide;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform camTransform;
    //[SerializeField] private Transform gunTransform;
    private bool[] inputs;

    private void Start()
    {
        inputs = new bool[6];
    }

    private void Update()
    {
        // Sample inputs every frame and store them until they're sent. This ensures no inputs are missed because they happened between FixedUpdate calls
        if (Input.GetKey(KeyCode.W))
            inputs[0] = true;

        if (Input.GetKey(KeyCode.S))
            inputs[1] = true;

        if (Input.GetKey(KeyCode.A))
            inputs[2] = true;

        if (Input.GetKey(KeyCode.D))
            inputs[3] = true;

        if (Input.GetKey(KeyCode.Space))
            inputs[4] = true;

        if (Input.GetMouseButtonDown(0))
        {
            SendBulletShot();
        }
        //gunTransform.forward = camTransform.forward;
    }

    private void FixedUpdate()
    {
        SendInput();

        // Reset input booleans
        for (int i = 0; i < inputs.Length; i++)
            inputs[i] = false;
    }

    #region Messages
    private void SendInput()
    {
        Message message = Message.Create(NetworkManager.Singleton.mode, ClientToServerId.PlayerInput);
        message.AddBools(inputs, false);
        message.AddVector3(camTransform.forward);
        message.AddQuaternion(camTransform.rotation);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void SendBulletShot()
    {
        Message message = Message.Create(NetworkManager.Singleton.mode, ClientToServerId.BulletInput);
        message.AddBool(true);
        NetworkManager.Singleton.Client.Send(message);
    }
    #endregion
}

