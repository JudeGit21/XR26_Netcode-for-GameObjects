using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Collections;
using UnityEditor;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private PlayerChat playerChat;

    // Network variable to store player name
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        value: default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        if (string.IsNullOrWhiteSpace(PlayerSettings.PlayerName))
        {
            Debug.Log("Can not assign an empty name to player");
        }
        else
        {
            playerName.Value = PlayerSettings.PlayerName;
            Debug.Log($"The player name is {playerName.Value}");
        }


    }

    private NetworkVariable<float> moveSpeed = new NetworkVariable<float>(
        value: 5f, // default value
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public float jumpForce = 2.0f;

    public float groundDistanceCheck = 1f;
    public LayerMask groundMask;

    [SerializeField]
    private Rigidbody rigidBody;

    private void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");
    }
    private void Update()
    {
        // Only process input for the local player
        if (!IsOwner) return;

        Vector3 input = new Vector3(
            Input.GetAxis("Horizontal"),
            0f,
            Input.GetAxis("Vertical")
        );

        if (Input.GetKey(KeyCode.Space) && IsGrounded())
        {
            Debug.Log("Client: I want to jump");
            Jump();
            JumpServerRpc();
        }

        Vector3 move = input * moveSpeed.Value * Time.deltaTime;

        // Send the movement to the server
        MoveServerRpc(move);

        // Test changing moveSpeed at runtime
        if (Input.GetKeyDown(KeyCode.M))
        {
            RequestChangeMoveSpeedServerRpc(moveSpeed.Value + 1f);
        }
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 move, ServerRpcParams rpcParams = default)
    {
        // Apply movement on the server
        transform.position += move;
    }

    // A server function that calls the jump function to perform jump logic
    [ServerRpc]
    private void JumpServerRpc()
    {
        Debug.Log("SERVER: The Player wants to Jump");
        Jump();
    }

    // Apply velocity to the rigidbody to jump (works on either client or server)
    private void Jump()
    {
        var vel = rigidBody.linearVelocity;
        vel.y = 0f;
        vel.y += jumpForce;
        rigidBody.linearVelocity = vel;
    }
    bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        float radius = 0.25f;

        // Perform a SphereCast from the origin and the radius provided, only return results
        // that hit the ground layermask.
        return Physics.SphereCast(origin, radius, Vector3.down, out _, groundDistanceCheck, groundMask, QueryTriggerInteraction.Ignore);
    }

    // ServerRpc to change the networked moveSpeed value
    [ServerRpc]
    private void RequestChangeMoveSpeedServerRpc(float newSpeed)
    {
        Debug.Log($"SERVER: Changing moveSpeed to {newSpeed}");
        moveSpeed.Value = newSpeed;
    }
}