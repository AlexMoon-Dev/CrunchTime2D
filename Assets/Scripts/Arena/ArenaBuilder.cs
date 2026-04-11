using UnityEngine;

/// <summary>
/// Builds the arena geometry at runtime using placeholder colored sprites.
/// Attach to an empty "ArenaBuilder" GameObject in the scene.
/// All visuals are colored BoxCollider2D objects — replace with Tilemaps when art is ready.
///
/// Layout:
///   Ground floor   — full width, y = -3.5
///   Left platform  — x = -5,  y = -1
///   Right platform — x =  5,  y = -1
///   Top platform   — x =  0,  y =  1.5
/// </summary>
public class ArenaBuilder : MonoBehaviour
{
    [Header("Arena Dimensions")]
    public float groundWidth  = 24f;
    public float groundHeight = 0.5f;
    public float groundY      = -3.5f;

    [Header("Platform Prefab (optional — auto-built if null)")]
    public GameObject platformPrefab;

    [Header("Wall / Boundary")]
    public float arenaHalfWidth = 12f;
    public float wallHeight     = 10f;

    private void Awake()
    {
        BuildGround();
        BuildPlatform(-5f, -1f,   6f, 0.3f, "LeftPlatform");
        BuildPlatform( 5f, -1f,   6f, 0.3f, "RightPlatform");
        BuildPlatform( 0f,  1.5f, 3f, 0.3f, "TopPlatform");
        BuildWalls();
        SetupCamera();
        PlaceSpawnPoints();
    }

    private void BuildGround()
    {
        var go = CreateBox("Ground", new Vector2(0f, groundY),
            new Vector2(groundWidth, groundHeight), new Color(0.35f, 0.25f, 0.15f));
        // Ground is a solid collider — no effector
        go.layer = LayerMask.NameToLayer("Ground");
        if (go.layer < 0) go.layer = 0;
    }

    private void BuildPlatform(float x, float y, float width, float height, string label)
    {
        var go = CreateBox(label, new Vector2(x, y), new Vector2(width, height),
            new Color(0.3f, 0.5f, 0.3f));

        // One-way effector
        var effector = go.AddComponent<PlatformEffector2D>();
        effector.useOneWay         = true;
        effector.surfaceArc        = 180f;
        effector.useOneWayGrouping = true;

        var col = go.GetComponent<Collider2D>();
        col.usedByEffector = true;

        go.AddComponent<OneWayPlatform>();
        go.layer = LayerMask.NameToLayer("Platform");
        if (go.layer < 0) go.layer = 0;
    }

    private void BuildWalls()
    {
        // Left wall
        CreateBox("WallLeft",  new Vector2(-arenaHalfWidth - 0.5f, 0f),
            new Vector2(1f, wallHeight), Color.gray);
        // Right wall
        CreateBox("WallRight", new Vector2( arenaHalfWidth + 0.5f, 0f),
            new Vector2(1f, wallHeight), Color.gray);
    }

    private void SetupCamera()
    {
        // Fixed orthographic camera showing the whole arena
        var cam = Camera.main;
        if (cam == null) return;
        cam.orthographic     = true;
        cam.orthographicSize = 6f;            // adjust to taste
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor  = new Color(0.05f, 0.05f, 0.1f);
        // TODO: Cinemachine confiner or manual bounds if arena expands
    }

    private void PlaceSpawnPoints()
    {
        // Create child spawn point transforms used by WaveManager
        var wm = FindFirstObjectByType<WaveManager>();
        if (wm == null) return;

        var spawns = new Vector2[]
        {
            new Vector2(-10f, groundY + 2f),
            new Vector2( 10f, groundY + 2f),
            new Vector2(  0f, groundY + 2f),
            new Vector2( -5f, 2f),
            new Vector2(  5f, 2f),
        };

        var points = new Transform[spawns.Length];
        for (int i = 0; i < spawns.Length; i++)
        {
            var go = new GameObject($"SpawnPoint{i}");
            go.transform.parent   = transform;
            go.transform.position = spawns[i];
            points[i] = go.transform;
        }
        wm.spawnPoints = points;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private GameObject CreateBox(string name, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.parent   = transform;
        go.transform.position = position;

        // Visual
        var sr        = go.AddComponent<SpriteRenderer>();
        sr.sprite     = CreateSquareSprite();
        sr.color      = color;
        sr.drawMode   = SpriteDrawMode.Sliced;
        sr.size       = size;
        sr.sortingLayerName = "Default";

        // Collider
        var col    = go.AddComponent<BoxCollider2D>();
        col.size   = size;

        return go;
    }

    private Sprite CreateSquareSprite()
    {
        // 1×1 white pixel sprite — reused for all placeholder shapes
        // TODO: replace with real Tilemap/art
        var tex    = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f,
            0, SpriteMeshType.FullRect, Vector4.one);
    }
}
