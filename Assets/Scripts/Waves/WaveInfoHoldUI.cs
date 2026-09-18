using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WaveInfoHoldUI : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Wave Manager")]
    [SerializeField] private WaveManager waveManager;

    [Header("Panel informacji")]
    [SerializeField] private GameObject infoPanel;

    [Header("Teksty")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text spawnTimeText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text healthText;

    [Header("Przytrzymanie")]
    [Tooltip("Czas przytrzymania przed pokazaniem informacji.")]
    [SerializeField] private float holdTime = 0.4f;

    private Coroutine holdCoroutine;
    private bool isHolding;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (waveManager == null)
        {
            waveManager =
                FindAnyObjectByType<WaveManager>();
        }

        HidePanel();
    }


    // =========================================================
    // POINTER DOWN
    // =========================================================

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isHolding)
            return;

        isHolding = true;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }

        holdCoroutine =
            StartCoroutine(HoldCoroutine());
    }


    // =========================================================
    // POINTER UP
    // =========================================================

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHolding();
    }


    // =========================================================
    // POINTER EXIT
    // =========================================================

    public void OnPointerExit(PointerEventData eventData)
    {
        StopHolding();
    }


    // =========================================================
    // PRZYTRZYMANIE
    // =========================================================

    private IEnumerator HoldCoroutine()
    {
        yield return new WaitForSeconds(holdTime);

        if (!isHolding)
            yield break;

        ShowPanel();

        holdCoroutine = null;
    }


    // =========================================================
    // ZATRZYMANIE
    // =========================================================

    private void StopHolding()
    {
        isHolding = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        HidePanel();
    }


    // =========================================================
    // POKAZANIE PANELU
    // =========================================================

    private void ShowPanel()
    {
        if (infoPanel == null)
            return;

        UpdateInformation();

        infoPanel.SetActive(true);
    }


    // =========================================================
    // UKRYCIE PANELU
    // =========================================================

    private void HidePanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }


    // =========================================================
    // AKTUALIZACJA INFORMACJI
    // =========================================================

    private void UpdateInformation()
    {
        if (waveManager == null)
            return;


        // =====================================================
        // FALA
        // =====================================================

        if (waveText != null)
        {
            waveText.text =
                "Fala " +
                waveManager.CurrentWave;
        }


        // =====================================================
        // CZAS SPAWNU
        // =====================================================

        if (spawnTimeText != null)
        {
            spawnTimeText.text =
                "Czas spawnu: " +
                waveManager.CurrentSpawnTime.ToString("F2") +
                " s";
        }


        // =====================================================
        // OBRAŻENIA
        // =====================================================

        if (damageText != null)
        {
            damageText.text =
                "Obrażenia: " +
                waveManager.CurrentEnemyDamage
                .ToString("F0");
        }


        // =====================================================
        // ZDROWIE
        // =====================================================

        if (healthText != null)
        {
            healthText.text =
                "Zdrowie: " +
                waveManager.CurrentEnemyHealth
                .ToString("F0");
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDisable()
    {
        isHolding = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        HidePanel();
    }
}