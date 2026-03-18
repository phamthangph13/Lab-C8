using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Unity Editor automation for 6 Prefab Labs.
/// Access all labs via menu: Tools > Prefab Labs
/// Run labs in order (Lab 1 → Lab 6) for best results.
/// </summary>
public static class PrefabLabsAutomation
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string MaterialFolder = "Assets/Materials";
    private const string ScriptFolder = "Assets/Scripts";

    #region Helpers

    private static void EnsureFolder(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            // Split and create each level
            string[] parts = folderPath.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }

    private static Material CreateOrLoadMaterial(string name, Color color)
    {
        EnsureFolder(MaterialFolder);
        string path = $"{MaterialFolder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            // Use URP Lit shader if available, fallback to Standard
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            mat.color = color;
            // For URP, also set _BaseColor
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    private static GameObject CreateQuadWithCollider(string name, Color color, string matName)
    {
        // Create a Quad as visual representation
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = name;

        // Remove 3D collider that comes with Quad
        var meshCollider = go.GetComponent<MeshCollider>();
        if (meshCollider != null) Object.DestroyImmediate(meshCollider);

        // Add 2D collider
        go.AddComponent<BoxCollider2D>();

        // Apply material
        Material mat = CreateOrLoadMaterial(matName, color);
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;

        return go;
    }

    #endregion

    // ================================================================
    // LAB 1 – Creating Prefab
    // ================================================================
    [MenuItem("Tools/Prefab Labs/Lab 1 – Create Enemy Prefab")]
    public static void Lab1_CreateEnemyPrefab()
    {
        EnsureFolder(PrefabFolder);

        // Create Enemy GameObject in scene
        GameObject enemy = CreateQuadWithCollider("Enemy", Color.magenta, "EnemyMaterial");
        enemy.transform.position = Vector3.zero;

        // Save as Prefab
        string prefabPath = $"{PrefabFolder}/Enemy.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
            enemy, prefabPath, InteractionMode.UserAction);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Select the prefab in Project window
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);

        EditorUtility.DisplayDialog("Lab 1 Complete",
            $"✅ Enemy Prefab created at:\n{prefabPath}\n\n" +
            "Check the Project window – the Prefab has been selected.",
            "OK");

        Debug.Log($"[Lab 1] Enemy Prefab created at {prefabPath}");
    }

    // ================================================================
    // LAB 2 – Prefab Instance
    // ================================================================
    [MenuItem("Tools/Prefab Labs/Lab 2 – Spawn Prefab Instances")]
    public static void Lab2_SpawnInstances()
    {
        string prefabPath = $"{PrefabFolder}/Enemy.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Enemy.prefab not found!\nPlease run Lab 1 first.", "OK");
            return;
        }

        // Define varied positions and scales
        Vector3[] positions = {
            new Vector3(-4, 2, 0),
            new Vector3(-2, -1, 0),
            new Vector3(0, 3, 0),
            new Vector3(2, -2, 0),
            new Vector3(4, 1, 0)
        };

        Vector3[] scales = {
            new Vector3(1f, 1f, 1f),
            new Vector3(1.5f, 1.5f, 1f),
            new Vector3(0.7f, 0.7f, 1f),
            new Vector3(2f, 2f, 1f),
            new Vector3(0.5f, 0.5f, 1f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            // InstantiatePrefab keeps the Prefab link (unlike Instantiate)
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = $"Enemy_Instance_{i + 1}";
            instance.transform.position = positions[i];
            instance.transform.localScale = scales[i];
            Undo.RegisterCreatedObjectUndo(instance, "Spawn Enemy Instance");
        }

        // Mark scene dirty so user knows to save
        EditorSceneManager.MarkSceneDirty(
            EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Lab 2 Complete",
            $"✅ Spawned {positions.Length} Enemy instances.\n\n" +
            "Check the Hierarchy – each instance has different Position & Scale.\n" +
            "Select any instance and check Inspector for Prefab link.",
            "OK");

        Debug.Log($"[Lab 2] Spawned {positions.Length} Enemy instances in Scene.");
    }

    // ================================================================
    // LAB 3 – Prefab Update
    // ================================================================
    [MenuItem("Tools/Prefab Labs/Lab 3 – Update Prefab (Color + Collider)")]
    public static void Lab3_UpdatePrefab()
    {
        string prefabPath = $"{PrefabFolder}/Enemy.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Enemy.prefab not found!\nPlease run Lab 1 first.", "OK");
            return;
        }

        // Edit prefab contents
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

        // Change color to Red
        var renderer = prefabRoot.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = CreateOrLoadMaterial("EnemyMaterial", Color.red);
            renderer.sharedMaterial = mat;
        }

        // Resize BoxCollider2D
        var collider = prefabRoot.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = new Vector2(2f, 2f);
        }

        // Save changes back to prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Lab 3 Complete",
            "✅ Prefab Updated!\n\n" +
            "• Color → Red\n" +
            "• BoxCollider2D size → (2, 2)\n\n" +
            "All instances in the Scene should now reflect these changes automatically.",
            "OK");

        Debug.Log("[Lab 3] Enemy Prefab updated: color=Red, collider size=(2,2).");
    }

    // ================================================================
    // LAB 4 – Nested Prefabs
    // ================================================================
    [MenuItem("Tools/Prefab Labs/Lab 4 – Nested Prefabs (Player + Gun)")]
    public static void Lab4_NestedPrefabs()
    {
        EnsureFolder(PrefabFolder);

        // --- Step 1: Create Gun Prefab ---
        string gunPrefabPath = $"{PrefabFolder}/Gun.prefab";
        GameObject gunPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gunPrefabPath);

        if (gunPrefab == null)
        {
            GameObject gunTemp = CreateQuadWithCollider("Gun", Color.yellow, "GunMaterial");
            gunTemp.transform.localScale = new Vector3(0.3f, 0.6f, 1f);
            gunPrefab = PrefabUtility.SaveAsPrefabAsset(gunTemp, gunPrefabPath);
            Object.DestroyImmediate(gunTemp);
        }

        // --- Step 2: Create Player with nested Gun ---
        string playerPrefabPath = $"{PrefabFolder}/Player.prefab";

        // Create Player base object
        GameObject playerGO = CreateQuadWithCollider("Player", Color.blue, "PlayerMaterial");
        playerGO.transform.localScale = new Vector3(1f, 1.5f, 1f);

        // Instantiate Gun as nested prefab (child of Player)
        GameObject gunInstance = (GameObject)PrefabUtility.InstantiatePrefab(gunPrefab);
        gunInstance.transform.SetParent(playerGO.transform);
        gunInstance.transform.localPosition = new Vector3(0.7f, 0f, -0.1f);

        // Save Player as prefab (Gun inside will be a nested prefab)
        PrefabUtility.SaveAsPrefabAssetAndConnect(
            playerGO, playerPrefabPath, InteractionMode.UserAction);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Select Player prefab
        GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
        Selection.activeObject = savedPrefab;
        EditorGUIUtility.PingObject(savedPrefab);

        EditorUtility.DisplayDialog("Lab 4 Complete",
            "✅ Nested Prefabs created!\n\n" +
            "• Gun.prefab → Yellow quad\n" +
            "• Player.prefab → Blue quad with Gun nested inside\n\n" +
            "Check Hierarchy – Player has Gun as child.\n" +
            "Select Player in Project and check the nested prefab icon.",
            "OK");

        Debug.Log("[Lab 4] Nested Prefabs: Player (with Gun nested) created.");
    }

    // ================================================================
    // LAB 5 – Prefab Variants
    // ================================================================
    [MenuItem("Tools/Prefab Labs/Lab 5 – Prefab Variants (EnemyFast + EnemyStrong)")]
    public static void Lab5_PrefabVariants()
    {
        EnsureFolder(PrefabFolder);

        // --- Step 1: Create EnemyBase Prefab ---
        string basePrefabPath = $"{PrefabFolder}/EnemyBase.prefab";
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);

        if (basePrefab == null)
        {
            GameObject baseGO = CreateQuadWithCollider("EnemyBase", Color.gray, "EnemyBaseMaterial");
            basePrefab = PrefabUtility.SaveAsPrefabAsset(baseGO, basePrefabPath);
            Object.DestroyImmediate(baseGO);
        }

        // --- Step 2: Create EnemyFast Variant ---
        string fastPath = $"{PrefabFolder}/EnemyFast.prefab";
        {
            // Instantiate base to create variant from
            GameObject fastGO = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            fastGO.name = "EnemyFast";
            fastGO.transform.localScale = new Vector3(0.6f, 0.6f, 1f); // smaller = faster look

            // Change color to green
            Material fastMat = CreateOrLoadMaterial("EnemyFastMaterial", Color.green);
            fastGO.GetComponent<MeshRenderer>().sharedMaterial = fastMat;

            // Save as variant
            PrefabUtility.SaveAsPrefabAsset(fastGO, fastPath);
            Object.DestroyImmediate(fastGO);
        }

        // --- Step 3: Create EnemyStrong Variant ---
        string strongPath = $"{PrefabFolder}/EnemyStrong.prefab";
        {
            GameObject strongGO = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            strongGO.name = "EnemyStrong";
            strongGO.transform.localScale = new Vector3(2f, 2f, 1f); // bigger = stronger look

            // Change color to dark red
            Material strongMat = CreateOrLoadMaterial("EnemyStrongMaterial", new Color(0.6f, 0f, 0f, 1f));
            strongGO.GetComponent<MeshRenderer>().sharedMaterial = strongMat;

            // Bigger collider
            var col = strongGO.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(1.5f, 1.5f);

            PrefabUtility.SaveAsPrefabAsset(strongGO, strongPath);
            Object.DestroyImmediate(strongGO);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Also spawn both variants in scene for visual comparison
        GameObject fastInstance = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(fastPath));
        fastInstance.transform.position = new Vector3(-3, 0, 0);
        Undo.RegisterCreatedObjectUndo(fastInstance, "Spawn EnemyFast");

        GameObject strongInstance = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(strongPath));
        strongInstance.transform.position = new Vector3(3, 0, 0);
        Undo.RegisterCreatedObjectUndo(strongInstance, "Spawn EnemyStrong");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Lab 5 Complete",
            "✅ Prefab Variants created!\n\n" +
            "• EnemyBase.prefab → Gray (base)\n" +
            "• EnemyFast.prefab → Green, small (variant)\n" +
            "• EnemyStrong.prefab → Dark red, large (variant)\n\n" +
            "Both variants have been placed in the Scene for comparison.",
            "OK");

        Debug.Log("[Lab 5] Prefab Variants: EnemyFast (green, small) + EnemyStrong (dark red, large).");
    }

    // ================================================================
    // LAB 6 – Setup Spawner (Runtime Script)
    // ================================================================
    [MenuItem("Tools/Prefab Labs/Lab 6 – Setup Enemy Spawner")]
    public static void Lab6_SetupSpawner()
    {
        string prefabPath = $"{PrefabFolder}/Enemy.prefab";
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (enemyPrefab == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Enemy.prefab not found!\nPlease run Lab 1 first.", "OK");
            return;
        }

        // Check if EnemySpawner script exists
        string scriptPath = $"{ScriptFolder}/EnemySpawner.cs";
        if (!File.Exists(scriptPath))
        {
            EditorUtility.DisplayDialog("Error",
                "EnemySpawner.cs not found at Assets/Scripts/.\n" +
                "The script should already be in the project. Please check.",
                "OK");
            return;
        }

        // Create Spawner GameObject in scene
        GameObject spawner = new GameObject("EnemySpawner");
        spawner.transform.position = new Vector3(0, 5, 0);

        // Add the EnemySpawner component
        var spawnerScript = spawner.AddComponent<EnemySpawner>();
        spawnerScript.enemyPrefab = enemyPrefab;

        Undo.RegisterCreatedObjectUndo(spawner, "Create Enemy Spawner");
        Selection.activeGameObject = spawner;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Lab 6 Complete",
            "✅ Enemy Spawner created!\n\n" +
            "• GameObject 'EnemySpawner' added to Scene at (0, 5, 0)\n" +
            "• EnemySpawner script attached\n" +
            "• enemyPrefab field assigned = Enemy.prefab\n\n" +
            "Press Play → Press Space to spawn enemies!",
            "OK");

        Debug.Log("[Lab 6] EnemySpawner setup complete. Press Play & Space to spawn.");
    }

    // ================================================================
    // LAB 7 – Setup Gameplay (Move + Attack + Health + Enemy AI)
    // ================================================================
    [MenuItem("Tools/Prefab Labs/Lab 7 – Setup Gameplay (Move + Attack)")]
    public static void Lab7_SetupGameplay()
    {
        EnsureFolder(PrefabFolder);

        // ---- Ensure "Player" tag exists (it's built-in) ----
        // ---- Ensure "Enemy" tag exists ----
        AddTagIfMissing("Enemy");

        // ==================================================
        // STEP 1: Create Bullet Prefab
        // ==================================================
        string bulletPrefabPath = $"{PrefabFolder}/Bullet.prefab";
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPrefabPath);

        if (bulletPrefab == null)
        {
            // Small yellow-orange quad
            GameObject bulletGO = CreateQuadWithCollider("Bullet", new Color(1f, 0.7f, 0f), "BulletMaterial");
            bulletGO.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            // Make collider a trigger (no physics push)
            var col = bulletGO.GetComponent<BoxCollider2D>();
            if (col != null) col.isTrigger = true;

            // Add Rigidbody2D for velocity
            var rb = bulletGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Add Bullet script
            bulletGO.AddComponent<Bullet>();

            bulletPrefab = PrefabUtility.SaveAsPrefabAsset(bulletGO, bulletPrefabPath);
            Object.DestroyImmediate(bulletGO);
        }

        // ==================================================
        // STEP 2: Update Player Prefab with gameplay scripts
        // ==================================================
        string playerPrefabPath = $"{PrefabFolder}/Player.prefab";
        GameObject playerPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);

        if (playerPrefabAsset == null)
        {
            // Create Player if not exists (in case Lab 4 wasn't run)
            GameObject playerGO = CreateQuadWithCollider("Player", Color.blue, "PlayerMaterial");
            playerGO.transform.localScale = new Vector3(1f, 1.5f, 1f);
            playerPrefabAsset = PrefabUtility.SaveAsPrefabAsset(playerGO, playerPrefabPath);
            Object.DestroyImmediate(playerGO);
        }

        // Edit Player prefab contents
        GameObject playerRoot = PrefabUtility.LoadPrefabContents(playerPrefabPath);
        {
            // Tag
            playerRoot.tag = "Player";

            // Collider as trigger
            var col = playerRoot.GetComponent<BoxCollider2D>();
            if (col == null) col = playerRoot.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            // Rigidbody2D (kinematic – movement controlled by script)
            var rb = playerRoot.GetComponent<Rigidbody2D>();
            if (rb == null) rb = playerRoot.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            // PlayerMovement
            if (playerRoot.GetComponent<PlayerMovement>() == null)
                playerRoot.AddComponent<PlayerMovement>();

            // PlayerAttack – wire bullet prefab
            var attack = playerRoot.GetComponent<PlayerAttack>();
            if (attack == null) attack = playerRoot.AddComponent<PlayerAttack>();
            attack.bulletPrefab = bulletPrefab;

            // Health
            var health = playerRoot.GetComponent<Health>();
            if (health == null) health = playerRoot.AddComponent<Health>();
            health.maxHealth = 5;
        }
        PrefabUtility.SaveAsPrefabAsset(playerRoot, playerPrefabPath);
        PrefabUtility.UnloadPrefabContents(playerRoot);

        // ==================================================
        // STEP 3: Update Enemy Prefab with AI + Health
        // ==================================================
        string enemyPrefabPath = $"{PrefabFolder}/Enemy.prefab";
        GameObject enemyPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);

        if (enemyPrefabAsset == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Enemy.prefab not found!\nPlease run Lab 1 first.", "OK");
            return;
        }

        GameObject enemyRoot = PrefabUtility.LoadPrefabContents(enemyPrefabPath);
        {
            // Tag
            enemyRoot.tag = "Enemy";

            // Collider as trigger
            var col = enemyRoot.GetComponent<BoxCollider2D>();
            if (col == null) col = enemyRoot.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            // Rigidbody2D (kinematic)
            var rb = enemyRoot.GetComponent<Rigidbody2D>();
            if (rb == null) rb = enemyRoot.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            // EnemyAI
            if (enemyRoot.GetComponent<EnemyAI>() == null)
                enemyRoot.AddComponent<EnemyAI>();

            // Health
            var health = enemyRoot.GetComponent<Health>();
            if (health == null) health = enemyRoot.AddComponent<Health>();
            health.maxHealth = 3;
        }
        PrefabUtility.SaveAsPrefabAsset(enemyRoot, enemyPrefabPath);
        PrefabUtility.UnloadPrefabContents(enemyRoot);

        // ==================================================
        // STEP 4: Place Player + Enemies in Scene
        // ==================================================

        // Spawn Player
        GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath));
        playerInstance.transform.position = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(playerInstance, "Spawn Player");

        // Spawn a few enemies
        GameObject enemyAsset = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);
        Vector3[] enemyPositions = {
            new Vector3(-5, 3, 0),
            new Vector3(5, -2, 0),
            new Vector3(3, 4, 0)
        };
        foreach (var pos in enemyPositions)
        {
            GameObject eInst = (GameObject)PrefabUtility.InstantiatePrefab(enemyAsset);
            eInst.transform.position = pos;
            Undo.RegisterCreatedObjectUndo(eInst, "Spawn Enemy");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeGameObject = playerInstance;

        EditorUtility.DisplayDialog("Lab 7 Complete",
            "✅ Gameplay Setup!\n\n" +
            "• Player (blue) at center → WASD to move\n" +
            "• Left-click to shoot bullets toward mouse\n" +
            "• 3 Enemies (red) chase the Player\n" +
            "• Player HP: 5 | Enemy HP: 3\n" +
            "• Enemies die when hit by bullets\n\n" +
            "Press PLAY to test!",
            "OK");

        Debug.Log("[Lab 7] Gameplay setup complete: Move(WASD) + Attack(LeftClick) + Health + EnemyAI.");
    }

    // ================================================================
    // Helper: Add custom tag if it doesn't exist
    // ================================================================
    private static void AddTagIfMissing(string tag)
    {
        // Open tag manager
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Check if tag already exists
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"[PrefabLabs] Added tag: {tag}");
        }
    }
}
