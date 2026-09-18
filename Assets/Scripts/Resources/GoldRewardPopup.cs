using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GoldRewardPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Image goldIcon;

    [Header("Animation")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float lifetime = 1f;

    private float timer;
    private Vector3 startPosition;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Setup(int amount)
    {
        if (amountText != null)
        {
            amountText.text = "+" + amount.ToString();
        }

        timer = 0f;
        startPosition = transform.position;

        canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Unoszenie
        transform.position +=
            Vector3.up *
            moveSpeed *
            Time.deltaTime;

        // Postęp animacji
        float progress =
            timer / lifetime;

        // Zanikanie
        canvasGroup.alpha =
            Mathf.Lerp(
                1f,
                0f,
                progress
            );

        // Koniec
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}