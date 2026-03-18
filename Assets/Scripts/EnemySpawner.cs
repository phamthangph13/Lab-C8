using UnityEngine;

/// <summary>
/// Lab 6 – Spawn enemy prefab at runtime when pressing Space.
/// Attach this to a GameObject and assign the enemyPrefab field.
/// (Lab 6 menu item does this automatically)
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("Drag the Enemy prefab here, or use Tools > Prefab Labs > Lab 6 to auto-assign.")]
    public GameObject enemyPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (enemyPrefab != null)
            {
                Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                Debug.Log("[EnemySpawner] Enemy spawned at " + transform.position);
            }
            else
            {
                Debug.LogWarning("[EnemySpawner] enemyPrefab is not assigned!");
            }
        }
    }
}
