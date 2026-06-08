using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CloseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Animasyon Ayarlarý")]
    public Color neonGlowColor = new Color(1f, 0.3f, 0.3f, 1f); // Parlak neon kýrmýzýsý
    public float scaleMultiplier = 1.1f; // Çarpý olduðu için biraz daha belirgin büyüsün (1.1)
    public float animationSpeed = 15f;

    private Image buttonImage;
    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovering = false;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        originalScale = transform.localScale;
        originalColor = buttonImage.color;
    }

    void Update()
    {
        // Hedef büyüklük ve renk
        Vector3 targetScale = isHovering ? originalScale * scaleMultiplier : originalScale;
        Color targetColor = isHovering ? neonGlowColor : originalColor;

        // Pürüzsüz animasyon geçiþi (Lerp)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
    }

    // --- FARE ÜZERÝNE GELDÝÐÝNDE ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    // --- FARE ÜZERÝNDEN ÇIKTIÐINDA ---
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    // --- ÇARPIYA TIKLANDIÐINDA ÇALIÞACAK FONKSÝYON ---
    public void QuitGame()
    {
        Debug.Log("Kýrmýzý çarpýya basýldý, oyun kapatýlýyor...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}