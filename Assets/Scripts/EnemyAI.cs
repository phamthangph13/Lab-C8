using UnityEngine;

/// <summary>
/// Enemy AI: tự đuổi theo Player.
/// Gây damage khi chạm Player.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 15f;

    [Header("Attack")]
    public int contactDamage = 1;
    public float attackCooldown = 1f;

    private Transform player;
    private float lastAttackTime = -999f;

    void Start()
    {
        // Tìm Player bằng tag
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null)
        {
            // Thử tìm lại nếu Player spawn sau
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        // Chỉ đuổi khi Player trong phạm vi
        if (distance <= detectionRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Flip theo hướng di chuyển
            if (direction.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (direction.x < 0 ? -1 : 1);
                transform.localScale = scale;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        Health playerHealth = other.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
            Debug.Log($"[EnemyAI] {gameObject.name} attacked Player!");
        }
    }
}
