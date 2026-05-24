using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueBubble : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public CanvasGroup canvasGroup;
    public GameObject closeOption; // Added close option reference

    [Header("Typewriter")]
    public float charDelay = 0.04f;

    [Header("Animation")]
    public float fadeInDuration = 0.12f;
    public float fadeOutDuration = 0.18f;

    private string[] _lines;
    private int _lineIndex;
    private Coroutine _typeCoroutine;
    private bool _lineFinished;
    private bool _closing;
    private System.Action _onEnd;

    private int _lastInteractFrame = -1; // Prevents double-firing if a UI button and Update both call OnInteract

    public void Init(string[] lines, System.Action onEnd = null)
    {
        _onEnd = onEnd;
        _lines = lines;
        _lineIndex = 0;
        _lineFinished = false;
        _closing = false;

        dialogueText.text = "";
        UpdateCloseOption();

        StartCoroutine(FadeCanvas(0f, 1f, fadeInDuration));
        PlayLine();
    }

    private void Update()
    {
        // Global screen click
        if (Input.GetMouseButtonDown(0))
        {
            OnInteract();
        }
    }

    public void OnInteract()
    {
        // Prevent double trigger in the exact same frame (e.g. from UI Button + Global Click simultaneously)
        if (Time.frameCount == _lastInteractFrame) return;
        _lastInteractFrame = Time.frameCount;

        if (_closing) return;

        if (!_lineFinished)
        {
            SkipTypewriter();
        }
        else
        {
            if (_lineIndex < _lines.Length - 1)
            {
                _lineIndex++;
                PlayLine();
            }
            else
            {
                // All lines finished, close it
                StartCoroutine(CloseRoutine());
            }
        }
    }

    // ── typewriter ────────────────────────────────────────────────────────────

    private void PlayLine()
    {
        _lineFinished = false;
        UpdateCloseOption();

        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _typeCoroutine = StartCoroutine(TypeLine(_lines[_lineIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        string currentText = "";

        for (int i = 0; i < line.Length; i++)
        {
            currentText += line[i];
            dialogueText.text = currentText;
            dialogueText.ForceMeshUpdate();

            // Only truncate if the layout has a valid height > 0
            if (dialogueText.rectTransform.rect.height > 0)
            {
                while (dialogueText.preferredHeight > dialogueText.rectTransform.rect.height && dialogueText.textInfo.lineCount > 1)
                {
                    int firstCharIdx = dialogueText.textInfo.lineInfo[1].firstCharacterIndex;
                    currentText = currentText.Substring(firstCharIdx);
                    dialogueText.text = currentText;
                    dialogueText.ForceMeshUpdate();
                }
            }

            yield return new WaitForSeconds(charDelay);
        }

        OnLineComplete();
    }

    private void SkipTypewriter()
    {
        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        string line = _lines[_lineIndex];

        dialogueText.text = line;
        dialogueText.ForceMeshUpdate();

        if (dialogueText.rectTransform.rect.height > 0)
        {
            while (dialogueText.preferredHeight > dialogueText.rectTransform.rect.height && dialogueText.textInfo.lineCount > 1)
            {
                int firstCharIdx = dialogueText.textInfo.lineInfo[1].firstCharacterIndex;
                line = line.Substring(firstCharIdx);
                dialogueText.text = line;
                dialogueText.ForceMeshUpdate();
            }
        }

        OnLineComplete();
    }

    private void OnLineComplete()
    {
        _lineFinished = true;
        UpdateCloseOption();
    }

    // ── close option ──────────────────────────────────────────────────────────

    private void UpdateCloseOption()
    {
        if (closeOption != null)
        {
            // Only show when the current line has finished typing AND it is the very last line in the dialogue array
            bool isLastLineAndFinished = _lineFinished && (_lineIndex >= _lines.Length - 1);
            closeOption.SetActive(isLastLineAndFinished);
        }
    }

    // ── close ─────────────────────────────────────────────────────────────────

    private IEnumerator CloseRoutine()
    {
        _closing = true;
        if (closeOption != null) closeOption.SetActive(false);

        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeOutDuration));
        _onEnd?.Invoke();
        Destroy(gameObject);
    }

    // ── canvas fade ───────────────────────────────────────────────────────────

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;
        float t = 0f;
        canvasGroup.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
