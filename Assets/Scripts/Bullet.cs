using UnityEngine;

/// <summary>
/// Bullet: tự hủy sau thời gian, gây damage khi chạm enemy.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet")]
    public int damage = 1;
    public float lifetime = 3f;

    void Start()
    {
        // Tự hủy sau lifetime giây
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ gây damage cho object có Health component và không phải Player
        if (other.CompareTag("Player")) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Destroy(gameObject); // Hủy bullet sau khi trúng
        }
    }
}
