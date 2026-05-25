using UnityEngine;

/// <summary>
/// Generates a soft radial circle sprite at runtime — no external assets needed.
/// Call SpriteGenerator.SoftCircle() anywhere to get a usable Sprite.
/// </summary>
public static class SpriteGenerator
{
    /// <summary>
    /// Creates a white circle with soft feathered edges, perfect for ripples and glows.
    /// </summary>
    /// <param name="resolution">Texture size in pixels (power of 2 recommended)</param>
    /// <param name="feather">0 = hard edge, 1 = fully soft/gaussian</param>
    public static Sprite SoftCircle(int resolution = 128, float feather = 0.6f)
    {
        var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        float half = resolution * 0.5f;
        float radius = half;

        Color[] pixels = new Color[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dx = (x - half) / radius;   // –1 … +1
                float dy = (y - half) / radius;
                float dist = Mathf.Sqrt(dx * dx + dy * dy); // 0 = centre, 1 = edge

                // Smooth falloff: 1 at centre → 0 at edge
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = Mathf.Pow(alpha, 1f / Mathf.Max(feather, 0.01f));

                pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f)   // pivot = centre
        );
    }

    /// <summary>
    /// Hard-edged circle, useful as a mask or solid dot.
    /// </summary>
    public static Sprite HardCircle(int resolution = 128)
        => SoftCircle(resolution, feather: 0.05f);
}