using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class UILogoShine : MonoBehaviour
{
    [Header("Shine Settings")]
    public float cycleTime = 2.5f;     // How fast it crosses the image
    public float delayBetween = 1.0f;  // Pause before the next shine
    public float shineWidth = 60f;
    public float shineAngle = 15f;

    [Header("Visuals")]
    public Color shineColor = new Color(1f, 1f, 1f, 0.4f);

    // ── Internal References ─────────────────────────────────────────
    private Image _logoImage;
    private RectTransform _shineRect;
    private Sprite _gradientSprite;

    void Start()
    {
        _logoImage = GetComponent<Image>();

        // 1. Generate the gradient texture via code
        _gradientSprite = CreateGradientSprite();

        // 2. Build the hierarchy
        BuildShineSystem();

        // 3. Start the animation loop
        StartCoroutine(ShineRoutine());
    }

    void BuildShineSystem()
    {
        // --- STEP A: Create the Mask Container ---
        // This container matches your logo exactly and hides the shine outside the logo area.
        GameObject containerGO = new GameObject("ShineMaskContainer");
        containerGO.transform.SetParent(transform, false);

        RectTransform containerRect = containerGO.AddComponent<RectTransform>();
        StretchToFill(containerRect);

        // Give it the same sprite as your logo so the mask is the exact same shape
        Image maskImage = containerGO.AddComponent<Image>();
        maskImage.sprite = _logoImage.sprite;
        maskImage.type = _logoImage.type;
        maskImage.preserveAspect = _logoImage.preserveAspect;
        maskImage.raycastTarget = false;

        // Add the mask and hide the duplicate image (we only want it to act as a window)
        Mask mask = containerGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // --- STEP B: Create the Shine Element ---
        GameObject shineGO = new GameObject("ShineGradient");
        shineGO.transform.SetParent(containerGO.transform, false);

        _shineRect = shineGO.AddComponent<RectTransform>();
        _shineRect.localEulerAngles = new Vector3(0, 0, shineAngle);

        Image shineImage = shineGO.AddComponent<Image>();
        shineImage.sprite = _gradientSprite;
        shineImage.color = shineColor;
        shineImage.raycastTarget = false;

        // Make the shine beam tall enough to cover the logo even when tilted
        float maxDim = Mathf.Max(_logoImage.rectTransform.rect.width, _logoImage.rectTransform.rect.height) * 2f;
        _shineRect.sizeDelta = new Vector2(shineWidth, maxDim);
    }

    IEnumerator ShineRoutine()
    {
        while (true)
        {
            // Calculate how far the shine needs to travel to clear the image completely
            float rectWidth = _logoImage.rectTransform.rect.width;
            float travelDistance = (rectWidth / 2f) + (shineWidth * 2f);

            float startX = -travelDistance;
            float endX = travelDistance;
            float t = 0f;

            // Sweep across
            while (t < 1f)
            {
                t += Time.deltaTime / cycleTime;

                // SmoothStep makes it ease in and out slightly for a premium feel
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                float currentX = Mathf.Lerp(startX, endX, smoothT);
                _shineRect.anchoredPosition = new Vector2(currentX, 0);

                yield return null;
            }

            // Wait before shining again
            yield return new WaitForSeconds(delayBetween);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────

    // Forces a RectTransform to fill its parent
    void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // Procedurally generates a soft glowing beam texture (transparent -> white -> transparent)
    Sprite CreateGradientSprite()
    {
        int width = 64;
        int height = 4;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[width * height];
        for (int x = 0; x < width; x++)
        {
            // Calculate distance from the center (0 to 1)
            float t = (float)x / (width - 1);
            float alpha = 1f - (Mathf.Abs(t - 0.5f) * 2f);

            // Soften the edges of the gradient
            alpha = Mathf.SmoothStep(0f, 1f, alpha);

            Color pixelColor = new Color(1f, 1f, 1f, alpha);

            for (int y = 0; y < height; y++)
            {
                pixels[y * width + x] = pixelColor;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
}