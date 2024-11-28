using UnityEngine;
using UnityEngine.UIElements;

public class PlayerLevelUI : NetworkToolkitHelper
{
    public GameResult gameResult;
    private Label playerLevelText;
    private VisualElement playerLevelFill;
    private VisualElement levelBox;
    private PlayerController playerController;

    private void UpdateUI(int expToNextLevel, int playerExpierence, int level, int maxLevel)
    {
        if (!IsOwner) return;

        if (level == maxLevel)
        {
            playerLevelText.text = $"{level} Max LVL";
            playerLevelFill.style.height = new Length(100, LengthUnit.Percent);
            levelBox.tooltip = $"{playerExpierence}/{expToNextLevel} EXP";
            return;
        }

        playerLevelText.text = $"{level} LVL";
        var heightPercentage = (float)playerExpierence / expToNextLevel;
        playerLevelFill.style.height = new Length(heightPercentage * 100, LengthUnit.Percent);
        levelBox.tooltip = $"{playerExpierence}/{expToNextLevel} EXP";
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        gameResult = GetComponent<GameResult>();
        UIDocument = gameResult.GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;

        playerLevelText = GetLabel("PlayerLevel");
        playerLevelFill = GetVisualElement("LevelFill");
        levelBox = GetVisualElement("Level");
    }

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        playerController.OnPlayerLevelChange += UpdateUI;
    }

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
    }
}
