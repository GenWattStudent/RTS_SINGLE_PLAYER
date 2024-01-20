using UnityEngine;
using UnityEngine.UIElements;

public class PlayerLevelUI : ToolkitHelper
{
    private Label playerLevelText;
    private VisualElement playerLevelFill;
    private VisualElement levelBox;

    private void OnDisable()
    {
        PlayerController.Instance.OnPlayerLevelChange -= UpdateUI;
    }

    private void UpdateUI(int expToNextLevel, int playerExpierence, int level, int maxLevel)
    {
        if (level == maxLevel) {
            playerLevelText.text = $"{level} Max LVL";
            playerLevelFill.style.height = new Length(100, LengthUnit.Percent);
            levelBox.tooltip = $"{playerExpierence}/{expToNextLevel} EXP";
            return;
        }

        playerLevelText.text = $"{level}LVL";
        var heightPercentage = (float)playerExpierence / expToNextLevel;
        playerLevelFill.style.height = new Length(heightPercentage * 100, LengthUnit.Percent);
        levelBox.tooltip = $"{playerExpierence}/{expToNextLevel} EXP";
    }

    private void Start()
    {
        PlayerController.Instance.OnPlayerLevelChange += UpdateUI;
        playerLevelText = GetLabel("PlayerLevel");
        playerLevelFill = GetVisualElement("LevelFill");
        levelBox = GetVisualElement("Level");
    }
}
