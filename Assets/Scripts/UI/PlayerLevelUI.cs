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

    private void UpdateUI(int expToNextLevel, int playerExpierence, int level)
    {
        playerLevelText.text = $"{level}LVL";
        Debug.Log(playerExpierence + " " + expToNextLevel);
        var heightPercentage = (float)playerExpierence / expToNextLevel;
        Debug.Log(heightPercentage);
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
