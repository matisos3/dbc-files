using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public GameObject highlight;
    private Image image;
    private SkillData skill;

    private SkillSelectionManager manager;

    private Color normalColor = Color.white;
    private Color selectedColor = Color.yellow;


    private void Awake()
    {
        image = GetComponent<Image>();
    }


    public void Setup(SkillData newSkill, SkillSelectionManager newManager)
    {
        skill = newSkill;
        manager = newManager;

        image.sprite = skill.icon;

        RemoveHighlight();

        highlight.SetActive(false);

        gameObject.SetActive(true);
    }


    public void Click()
    {
        manager.ButtonClicked(this, skill);
    }


    public void Highlight()
    {
        highlight.SetActive(true);
    }


    public void RemoveHighlight()
    {
        highlight.SetActive(false);
    }
}