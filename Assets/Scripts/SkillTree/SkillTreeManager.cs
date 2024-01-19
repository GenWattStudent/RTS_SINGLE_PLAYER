using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillTreeManager : ToolkitHelper
{
    [SerializeField] private List<SkillSo> skills = new ();
    private int skillPoints = 0;
    public List<SkillSo> unlockedSkills = new ();
    private Label skillPointsText;
    private Button skillTreeClose;
    private VisualElement skillTree;
    public static SkillTreeManager Instance;

    private void PurchaseSkill(SkillSo skillSo)
    {
        if (skillSo.Unlock(unlockedSkills, skillPoints))
        {
            unlockedSkills.Add(skillSo);
            RemoveSkillPoints(skillSo.requiredSkillPoints);
            UpdateUI();
        }
    }

    public bool IsPurchased(SkillSo skillSo)
    {
        return skillSo.IsUnlocked(unlockedSkills);
    }

    private void UpdateSkillUIData(SkillSo skillSo) {
        var skill = GetVisualElement(skillSo.skillTag);
        var title = skill.Q<Label>("Title");
        var description = skill.Q<Label>("Value");
        var cost = skill.Q<Label>("Points");
        var alreadyPurchased = skill.Q<VisualElement>("AlreadyPurchased");

        if (skillSo.CanBePurchased(unlockedSkills, skillPoints))
        {
            skill.SetEnabled(true);
        }
        else if (IsPurchased(skillSo))
        {
            skill.SetEnabled(false);
            alreadyPurchased.style.display = DisplayStyle.Flex;
        }
        else
        {
            skill.SetEnabled(false);
        }

        title.text = skillSo.skillName;
        description.text = skillSo.description;
        cost.text = skillSo.requiredSkillPoints.ToString();
    }

    private void UpdateUI() {
        foreach (var skill in skills)
        {
            UpdateSkillUIData(skill);
        }
    }

    private void AddSkillEvent() {
        foreach (var skill in skills)
        {
            var skillEl = GetVisualElement(skill.skillTag);
            skillEl.RegisterCallback((ClickEvent ev) => PurchaseSkill(skill));
        }
    }

    private void UpdateSkillPoints() {
        skillPointsText.text = $"Skill points: {skillPoints}";
    }

    public void AddSkillPoints(int amount) {
        skillPoints += amount;
        UpdateSkillPoints();
        UpdateUI();
    }

    public void RemoveSkillPoints(int amount) {
        skillPoints -= amount;
        UpdateSkillPoints();
    }

    public void Show() {
        skillTree.style.display = DisplayStyle.Flex;
    }

    public void Hide() {
        skillTree.style.display = DisplayStyle.None;
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        skillPointsText = GetLabel("SkillPoints");
        skillTreeClose = GetButton("SkillTreeClose");
        skillTree = GetVisualElement("SkillTree");

        skillTreeClose.RegisterCallback((ClickEvent ev) => Hide());
        AddSkillEvent();
        UpdateUI();
        UpdateSkillPoints();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (skillTree.style.display == DisplayStyle.None)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            AddSkillPoints(1);
        }
    }
}
