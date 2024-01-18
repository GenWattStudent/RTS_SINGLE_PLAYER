using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class SkillTreeManager : ToolkitHelper
{
    public List<SkillSo> skills = new ();
    public int marginBetweenSkills = 50;
    public VisualTreeAsset skillUi;
    private VisualElement skillTreeContainer;

    private void GenerateHorizontalSkillTree(SkillSo skill = null, int depth = 0, int x = 0, int y = 0) {
        if (skill == null) {
            var rootSkills = skills.Where(s => s.requiredSkills.Count == 0).ToList();
            foreach (var rootSkill in rootSkills) {
                GenerateHorizontalSkillTree(rootSkill, depth, x, y);
            }
        } else {
            var skillUiInstance = skillUi.Instantiate();
            var skillName = skillUiInstance.Q<Label>("SkillName");

            skillUiInstance.style.position = Position.Absolute;
            skillUiInstance.style.left = x;
            skillUiInstance.style.top = y;
            skillUiInstance.style.marginLeft = marginBetweenSkills * depth;
            skillUiInstance.style.marginTop = marginBetweenSkills * depth;

            skillName.text = skill.skillName;

            skillTreeContainer.Add(skillUiInstance);

            if (skill.requiredSkills.Count > 0) {
                var requiredSkills = skill.requiredSkills;
                foreach (var requiredSkill in requiredSkills) {
                    GenerateHorizontalSkillTree(requiredSkill, depth + 1, x, y);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        skillTreeContainer = GetVisualElement("SkillTreeContainer");
        GenerateHorizontalSkillTree();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
