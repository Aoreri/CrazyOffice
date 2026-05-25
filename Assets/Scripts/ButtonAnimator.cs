using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Drop this on any UI Button. No manual setup needed.
/// Automatically moves all existing children (Text, TMP, icons) into
/// the ScaleProxy so they scale with the button and stay visible.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ButtonAnimator : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale")]
    public float hoverScale = 1.07f;
    public float pressedScale = 0.94f;
    public float scaleSpeed = 14f;

    [Header("Hover")]
    [Tooltip("Seconds before confirming cursor left. Prevents flicker at edges.")]
    public float exitDelay = 0.07f;

    [Header("Color")]
    public Color normalColor = new Color(0.13f, 0.13f, 0.20f, 1f);
    public Color hoverColor = new Color(0.25f, 0.45f, 0.95f, 1f);
    public Color pressedColor = new Color(0.10f, 0.28f, 0.75f, 1f);
    public float colorSpeed = 16f;

    [Header("Glow")]
    public Color glowNormal = new Color(0.25f, 0.45f, 0.95f, 0f);
    public Color glowHover = new Color(0.25f, 0.45f, 0.95f, 0.45f);
    public Color glowPressed = new Color(0.10f, 0.28f, 0.75f, 0.75f);

    [Header("Ripple")]
    public float rippleDuration = 0.45f;

    [Header("Spring Bounce")]
    public bool enableBounce = true;
    public float bounceImpulse = 14f;
    public float bounceSpring = 220f;
    public float bounceDamp = 18f;

    [Header("Idle Float")]
    public bool enableFloat = true;
    public float floatAmplitude = 3f;
    public float floatFrequency = 1.1f;

    // ── auto-built references ──────────────────────────────────────
    RectTransform _rootRect;
    RectTransform _proxy;
    Image _proxyImage;
    Image _glowImage;
    Image _rippleImage;
    Vector2 _basePos;

    // ── state ──────────────────────────────────────────────────────
    enum BtnState { Normal, Hovered, Pressed }
    BtnState _state = BtnState.Normal;
    bool _pointerLeftWhilePressed;
    Coroutine _exitRoutine;
    Coroutine _rippleRoutine;

    // ── spring ─────────────────────────────────────────────────────
    float _targetScale = 1f;
    bool _useSpring;
    float _springPos = 1f;
    float _springVel;
    Color _targetColor;
    Color _targetGlow;

    // ──────────────────────────────────────────────────────────────
    void Awake()
    {
        _rootRect = GetComponent<RectTransform>();
        _basePos = _rootRect.anchoredPosition;

        BuildHierarchy();

        _targetColor = normalColor;
        _targetGlow = glowNormal;
        _springPos = 1f;
        _proxy.localScale = Vector3.one;

        if (_proxyImage) _proxyImage.color = normalColor;
        if (_glowImage) _glowImage.color = glowNormal;
        if (_rippleImage)
        {
            _rippleImage.color = Color.clear;
            _rippleImage.rectTransform.localScale = Vector3.one * 0.1f;
        }
    }

    // ── Hierarchy builder ──────────────────────────────────────────
    void BuildHierarchy()
    {
        // Collect all current children BEFORE we create anything new.
        // These are the user's original children (Text, icons, etc.)
        var originalChildren = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
            originalChildren.Add(transform.GetChild(i));

        // ── 1. ScaleProxy ──────────────────────────────────────────
        var proxyGO = new GameObject("ScaleProxy");
        proxyGO.transform.SetParent(transform, false);
        _proxy = proxyGO.AddComponent<RectTransform>();

        _proxy.pivot = _rootRect.pivot;

        StretchToFill(_proxy);

        // ── 2. Move root Image into proxy ──────────────────────────
        var rootImg = GetComponent<Image>();
        if (rootImg != null)
        {
            _proxyImage = proxyGO.AddComponent<Image>();
            _proxyImage.sprite = rootImg.sprite;
            _proxyImage.color = rootImg.color;
            _proxyImage.type = rootImg.type;
            _proxyImage.material = rootImg.material;
            DestroyImmediate(rootImg);
        }
        else
        {
            _proxyImage = proxyGO.AddComponent<Image>();
            _proxyImage.color = normalColor;
        }

        // ── 3. Transparent hit-box image on root ───────────────────
        var hitImage = gameObject.AddComponent<Image>();
        hitImage.color = Color.clear;
        hitImage.raycastTarget = true;

        // ── 4. Glow (behind everything inside proxy) ───────────────
        var glowGO = new GameObject("Glow");
        glowGO.transform.SetParent(_proxy, false);
        var glowRT = glowGO.AddComponent<RectTransform>();
        StretchToFill(glowRT, padding: -12f);
        glowRT.SetAsFirstSibling();
        _glowImage = glowGO.AddComponent<Image>();
        _glowImage.sprite = MakeSoftCircle(128, 0.85f);
        _glowImage.color = glowNormal;
        _glowImage.raycastTarget = false;

        // ── 5. Re-parent original children into proxy ──────────────
        //    They now sit above the background Image and below Ripple.
        foreach (var child in originalChildren)
        {
            child.SetParent(_proxy, false);   // worldPositionStays = false keeps layout
            child.SetAsLastSibling();
        }

        // ── 6. Ripple (on top of everything) ──────────────────────
        var rippleGO = new GameObject("Ripple");
        rippleGO.transform.SetParent(_proxy, false);
        var rippleRT = rippleGO.AddComponent<RectTransform>();
        rippleRT.sizeDelta = Vector2.one * 60f;
        rippleRT.anchoredPosition = Vector2.zero;
        rippleRT.localScale = Vector3.one * 0.1f;
        rippleRT.SetAsLastSibling();
        _rippleImage = rippleGO.AddComponent<Image>();
        _rippleImage.sprite = MakeSoftCircle(128, 0.5f);
        _rippleImage.color = Color.clear;
        _rippleImage.raycastTarget = false;
    }

    // ── Helpers ────────────────────────────────────────────────────
    static void StretchToFill(RectTransform rt, float padding = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.one * padding;
        rt.offsetMax = Vector2.one * -padding;
        rt.localScale = Vector3.one;
        rt.localPosition = Vector3.zero;
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

    // ── Update ─────────────────────────────────────────────────────
    void Update()
    {
        ApplyScale();
        ApplyColor();
        ApplyFloat();
    }

    // ── Pointer events ─────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData _)
    {
        CancelExit();
        _pointerLeftWhilePressed = false;
        if (_state == BtnState.Pressed) return;
        SetState(BtnState.Hovered);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (_state == BtnState.Pressed) { _pointerLeftWhilePressed = true; return; }
        CancelExit();
        _exitRoutine = StartCoroutine(DelayedExit());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CancelExit();
        _pointerLeftWhilePressed = false;
        SetState(BtnState.Pressed);
        if (_rippleRoutine != null) StopCoroutine(_rippleRoutine);
        _rippleRoutine = StartCoroutine(PlayRipple(eventData.position));
    }

    public void OnPointerUp(PointerEventData _)
    {
        bool outside = _pointerLeftWhilePressed;
        _pointerLeftWhilePressed = false;
        SetState(outside ? BtnState.Normal : BtnState.Hovered);
    }

    // ── State machine ──────────────────────────────────────────────
    void SetState(BtnState next)
    {
        _state = next;
        switch (next)
        {
            case BtnState.Normal:
                _targetColor = normalColor; _targetGlow = glowNormal;
                SpringTo(1f, -4f); break;
            case BtnState.Hovered:
                _targetColor = hoverColor; _targetGlow = glowHover;
                SpringTo(hoverScale, -4f); break;
            case BtnState.Pressed:
                _targetColor = pressedColor; _targetGlow = glowPressed;
                SpringTo(pressedScale, -bounceImpulse); break;
        }
    }

    IEnumerator DelayedExit()
    {
        yield return new WaitForSeconds(exitDelay);
        _exitRoutine = null;
        SetState(BtnState.Normal);
    }

    void CancelExit()
    {
        if (_exitRoutine == null) return;
        StopCoroutine(_exitRoutine);
        _exitRoutine = null;
    }

    // ── Scale ──────────────────────────────────────────────────────
    void SpringTo(float target, float impulse)
    {
        _targetScale = target;
        if (enableBounce) { _springPos = _proxy.localScale.x; _springVel = impulse; _useSpring = true; }
        else _useSpring = false;
    }

    void ApplyScale()
    {
        float next;
        if (_useSpring)
        {
            float force = (_targetScale - _springPos) * bounceSpring - _springVel * bounceDamp;
            _springVel += force * Time.deltaTime;
            _springPos += _springVel * Time.deltaTime;
            next = _springPos;
            if (Mathf.Abs(_springPos - _targetScale) < 0.002f && Mathf.Abs(_springVel) < 0.01f)
            { next = _targetScale; _springPos = _targetScale; _springVel = 0f; _useSpring = false; }
        }
        else
        {
            next = Mathf.Lerp(_proxy.localScale.x, _targetScale, Time.deltaTime * scaleSpeed);
        }
        _proxy.localScale = Vector3.one * next;
    }

    // ── Color ──────────────────────────────────────────────────────
    void ApplyColor()
    {
        if (_proxyImage)
            _proxyImage.color = Color.Lerp(_proxyImage.color, _targetColor, Time.deltaTime * colorSpeed);
        if (_glowImage)
            _glowImage.color = Color.Lerp(_glowImage.color, _targetGlow, Time.deltaTime * colorSpeed);
    }

    // ── Idle float ─────────────────────────────────────────────────
    void ApplyFloat()
    {
        if (!enableFloat || _state != BtnState.Normal) return;
        float y = Mathf.Sin(Time.time * floatFrequency * Mathf.PI * 2f) * floatAmplitude;
        _rootRect.anchoredPosition = _basePos + new Vector2(0, y);
    }

    // ── Ripple ─────────────────────────────────────────────────────
    // ── Ripple ─────────────────────────────────────────────────────
    IEnumerator PlayRipple(Vector2 screenPos)
    {
        var rt = _rippleImage.rectTransform;

        // 1. Calculate hit point relative to the _proxy (the ripple's parent)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _proxy, screenPos, null, out Vector2 local);

        // 2. Use localPosition instead of anchoredPosition! 
        // This ignores anchor offsets and places it exactly at the pivot-based coordinates.
        rt.localPosition = local;

        rt.localScale = Vector3.one * 0.1f;
        _rippleImage.color = new Color(1f, 1f, 1f, 0.6f);

        var scaleCurve = AnimationCurve.EaseInOut(0, 0.1f, 1, 2.2f);
        var alphaCurve = AnimationCurve.EaseInOut(0, 0.6f, 1, 0f);

        float e = 0f;
        while (e < rippleDuration)
        {
            float t = e / rippleDuration;
            rt.localScale = Vector3.one * scaleCurve.Evaluate(t);
            _rippleImage.color = new Color(1f, 1f, 1f, alphaCurve.Evaluate(t));
            e += Time.deltaTime;
            yield return null;
        }
        _rippleImage.color = Color.clear;
        rt.localScale = Vector3.one * 0.1f;
        _rippleRoutine = null;
    }

    void OnDisable()
    {
        // 1. Reset state machine
        _state = BtnState.Normal;
        _pointerLeftWhilePressed = false;

        // 2. Clear routine references (Unity stops them automatically on disable)
        _exitRoutine = null;
        _rippleRoutine = null;

        // 3. Hard reset scale and spring math
        _targetScale = 1f;
        _useSpring = false;
        _springPos = 1f;
        _springVel = 0f;
        if (_proxy != null) _proxy.localScale = Vector3.one;

        // 4. Hard reset colors
        _targetColor = normalColor;
        _targetGlow = glowNormal;
        if (_proxyImage != null) _proxyImage.color = normalColor;
        if (_glowImage != null) _glowImage.color = glowNormal;

        // 5. Hard reset the ripple effect
        if (_rippleImage != null)
        {
            _rippleImage.color = Color.clear;
            _rippleImage.rectTransform.localScale = Vector3.one * 0.1f;
        }

        // 6. Hard reset idle float position
        if (_rootRect != null)
        {
            _rootRect.anchoredPosition = _basePos;
        }
    }
}