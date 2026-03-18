using UnityEngine;

/// <summary>
/// Health system — dùng được cho cả Player và Enemy.
/// Khi hết máu → destroy GameObject.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Visual Feedback")]
    public bool flashOnHit = true;
    public Color hitColor = Color.white;
    public float flashDuration = 0.1f;

    private MeshRenderer meshRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.material != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"[Health] {gameObject.name} took {amount} damage. HP: {currentHealth}/{maxHealth}");

        // Flash effect
        if (flashOnHit && meshRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        if (meshRenderer.material.HasProperty("_BaseColor"))
            meshRenderer.material.SetColor("_BaseColor", hitColor);
        else
            meshRenderer.material.color = hitColor;

        yield return new WaitForSeconds(flashDuration);

        if (meshRenderer != null)
        {
            if (meshRenderer.material.HasProperty("_BaseColor"))
                meshRenderer.material.SetColor("_BaseColor", originalColor);
            else
                meshRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        Debug.Log($"[Health] {gameObject.name} destroyed!");
        Destroy(gameObject);
    }
}
