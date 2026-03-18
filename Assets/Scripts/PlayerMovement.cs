using UnityEngine;

/// <summary>
/// WASD / Arrow keys movement for Player.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        Vector3 direction = new Vector3(h, v, 0f).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Flip sprite based on direction
        if (spriteRenderer != null && h != 0)
        {
            spriteRenderer.flipX = h < 0;
        }
    }
}
