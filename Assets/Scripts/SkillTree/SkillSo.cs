using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill")]
public class SkillSo : ScriptableObject
{
    public string skillName;
    public string description;
    public Sprite icon;
    public List<SkillSo> requiredSkills = new ();
    public int requiredSkillPoints;
}
