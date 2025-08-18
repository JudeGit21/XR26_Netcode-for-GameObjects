using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private float jumpForce = 2.0f;
    private Rigidbody rb;

    private void Start()
    {
        // Cache the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Only process input for the local player
        if (!IsOwner) return;

        // Player movement input
        Vector3 input = new Vector3(
            Input.GetAxis("Horizontal"),
            0f,
            Input.GetAxis("Vertical")
        );

        Vector3 move = input * moveSpeed * Time.deltaTime;

        // Send the movement to the server
        MoveServerRpc(move);

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Debug.Log("CLIENT: I want to jump!");
            JumpServerRpc();
        }
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 move, ServerRpcParams rpcParams = default)
    {
        // Apply movement on the server
        transform.position += move;
    }

    [ServerRpc]
    private void JumpServerRpc(ServerRpcParams rpcParams = default)
    {
        // Apply jump on the server
        if (rb != null && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false; // You should reset this in OnCollisionEnter
        }
    }

    [SerializeField] private string groundTag = "Ground";
    private void OnCollisionEnter(Collision collision)
    {
        // Check if player is on the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}