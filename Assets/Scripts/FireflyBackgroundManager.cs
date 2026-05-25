using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Put this on an empty GameObject positioned in front of your camera (e.g., Z = 5).
/// Works perfectly behind a Canvas set to "Screen Space - Camera".
/// </summary>
public class FireflyBackgroundManager: MonoBehaviour
{
    [Header("Swarm Settings")]
    public int fireflyCount = 40;
    public float moveSpeed = 2f;

    [Header("Visuals")]
    public Color fireflyColor = new Color(0.6f, 0.9f, 1.0f, 1f);
    public float headSize = 0.15f;

    [Header("Trail Settings")]
    public float trailDuration = 2.5f;
    public float trailStartWidth = 0.12f;

    // ── Internal state ─────────────────────────────────────────────
    class Firefly
    {
        public Transform transform;
        public TrailRenderer trail;
        public Vector3 target;
        public float noiseOffset;
    }

    private List<Firefly> _swarm = new List<Firefly>();
    private Sprite _glowSprite;
    private Material _glowMaterial;
    private Vector2 _screenBounds;

    // ──────────────────────────────────────────────────────────────
    void Start()
    {
        CalculateScreenBounds();

        // Procedurally generate textures so no manual assignment is needed
        _glowSprite = MakeSoftCircle(64, 0.8f);
        _glowMaterial = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < fireflyCount; i++)
        {
            SpawnFirefly();
        }
    }

    void CalculateScreenBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Automatically figure out the width and height of the screen in world-space
        if (cam.orthographic)
        {
            float height = cam.orthographicSize * 2f;
            _screenBounds = new Vector2(height * cam.aspect, height);
        }
        else
        {
            // For Perspective cameras, calculate the view size at this object's exact Z distance
            float distance = Mathf.Abs(cam.transform.position.z - transform.position.z);
            float height = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            _screenBounds = new Vector2(height * cam.aspect, height);
        }

        // Add padding so they don't pop out visibly on the exact pixel edge
        _screenBounds += new Vector2(2f, 2f);
    }

    void SpawnFirefly()
    {
        GameObject go = new GameObject("Firefly_3D");
        go.transform.SetParent(transform);
        go.transform.localPosition = GetRandomPosition();

        // ── Head Setup ──
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _glowSprite;
        sr.color = fireflyColor;
        sr.material = _glowMaterial;
        go.transform.localScale = Vector3.one * headSize;

        // ── Smooth Trail Setup ──
        TrailRenderer tr = go.AddComponent<TrailRenderer>();
        tr.time = trailDuration;
        tr.startWidth = trailStartWidth;
        tr.endWidth = 0f;
        tr.material = _glowMaterial;
        tr.startColor = fireflyColor;
        tr.endColor = new Color(fireflyColor.r, fireflyColor.g, fireflyColor.b, 0f);
        tr.minVertexDistance = 0.02f; // Low distance = much smoother curves
        tr.numCornerVertices = 5;     // Rounds off sharp turns
        tr.numCapVertices = 5;        // Rounds off the end of the trail

        _swarm.Add(new Firefly
        {
            transform = go.transform,
            trail = tr,
            target = GetRandomPosition(),
            noiseOffset = Random.Range(0f, 1000f)
        });
    }

    void Update()
    {
        foreach (var f in _swarm)
        {
            float noiseX = Mathf.PerlinNoise(Time.time * 0.8f, f.noiseOffset) - 0.5f;
            float noiseY = Mathf.PerlinNoise(Time.time * 0.8f, f.noiseOffset + 100f) - 0.5f;
            Vector3 noiseWobble = new Vector3(noiseX, noiseY, 0) * 3f;

            Vector3 dir = (f.target - f.transform.localPosition).normalized;
            f.transform.localPosition += (dir + noiseWobble).normalized * (moveSpeed * Time.deltaTime);

            if (Vector3.Distance(f.transform.localPosition, f.target) < 1.5f)
            {
                f.target = GetRandomPosition();
            }

            WrapBounds(f);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────
    Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-_screenBounds.x / 2f, _screenBounds.x / 2f),
            Random.Range(-_screenBounds.y / 2f, _screenBounds.y / 2f),
            0f // Keep them perfectly flat on the Z axis relative to the parent
        );
    }

    void WrapBounds(Firefly f)
    {
        Vector3 pos = f.transform.localPosition;
        float halfX = _screenBounds.x / 2f;
        float halfY = _screenBounds.y / 2f;
        bool wrapped = false;

        if (pos.x > halfX) { pos.x = -halfX; wrapped = true; }
        else if (pos.x < -halfX) { pos.x = halfX; wrapped = true; }

        if (pos.y > halfY) { pos.y = -halfY; wrapped = true; }
        else if (pos.y < -halfY) { pos.y = halfY; wrapped = true; }

        if (wrapped)
        {
            f.transform.localPosition = pos;
            f.trail.Clear(); // Erases the trail instantly so it doesn't draw a line across the screen
        }
    }

    static Sprite MakeSoftCircle(int res, float feather)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        float half = res * 0.5f;
        var px = new Color[res * res];
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float a = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy)),
                                     1f / Mathf.Max(feather, 0.01f));
                px[y * res + x] = new Color(1, 1, 1, a);
            }
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f);
    }
}