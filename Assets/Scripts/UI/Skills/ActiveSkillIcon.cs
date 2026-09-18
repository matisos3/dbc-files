using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActiveSkillIcon :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("Ikona")]
    [SerializeField]
    private Image iconImage;

    // =========================================================
    // INTERNAL
    // =========================================================

    private SkillInstance skill;
    private ActiveSkillsUI owner;

    private float holdDuration;
    private float holdTimer;

    private bool pointerDown;
    private bool holdTriggered;

    // =========================================================
    // PUBLIC INFO
    // =========================================================

    public string SkillID
    {
        get
        {
            if (skill == null ||
                skill.data == null)
            {
                return "";
            }

            return skill.data.skillID;
        }
    }

    public int SkillLevel
    {
        get
        {
            if (skill == null)
                return 0;

            return skill.level;
        }
    }

    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        SkillInstance skillInstance,
        ActiveSkillsUI activeSkillsUI,
        float duration)
    {
        skill =
            skillInstance;

        owner =
            activeSkillsUI;

        holdDuration =
            Mathf.Max(
                0.1f,
                duration
            );

        holdTimer = 0f;
        pointerDown = false;
        holdTriggered = false;

        if (iconImage == null)
        {
            iconImage =
                GetComponent<Image>();
        }

        if (iconImage != null)
        {
            if (skill != null &&
                skill.data != null)
            {
                iconImage.sprite =
                    skill.data.icon;

                iconImage.enabled =
                    skill.data.icon != null;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!pointerDown ||
            holdTriggered)
        {
            return;
        }

        holdTimer +=
            Time.unscaledDeltaTime;

        if (holdTimer >= holdDuration)
        {
            TriggerHold();
        }
    }

    // =========================================================
    // POINTER DOWN
    // =========================================================

    public void OnPointerDown(
        PointerEventData eventData)
    {
        pointerDown = true;
        holdTriggered = false;
        holdTimer = 0f;
    }

    // =========================================================
    // POINTER UP
    // =========================================================

    public void OnPointerUp(
        PointerEventData eventData)
    {
        pointerDown = false;
        holdTimer = 0f;
        if (owner != null)
        {
            owner.CloseDescription();
        }
    }

    // =========================================================
    // POINTER EXIT
    // =========================================================

    public void OnPointerExit(
        PointerEventData eventData)
    {
        pointerDown = false;
        holdTimer = 0f;
    }

    // =========================================================
    // HOLD
    // =========================================================

    private void TriggerHold()
    {
        if (holdTriggered)
            return;

        holdTriggered = true;
        pointerDown = false;

        if (owner != null &&
            skill != null)
        {
            owner.OnSkillIconHeld(
                skill
            );
        }
    }

    // =========================================================
    // REFRESH ICON
    // =========================================================

    public void RefreshIcon(
        SkillInstance newSkill)
    {
        skill =
            newSkill;

        if (iconImage == null)
        {
            iconImage =
                GetComponent<Image>();
        }

        if (iconImage == null)
            return;

        if (skill == null ||
            skill.data == null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            return;
        }

        iconImage.sprite =
            skill.data.icon;

        iconImage.enabled =
            skill.data.icon != null;
    }
}