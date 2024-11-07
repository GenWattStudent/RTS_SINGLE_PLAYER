using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillTreeManager : NetworkToolkitHelper
{
    public List<SkillSo> skills = new();
    private NetworkVariable<int> skillPoints = new(0);
    private Label skillPointsText;
    private Button skillTreeClose;
    private VisualElement skillTree;
    private PowerUp powerUp;

    [ServerRpc(RequireOwnership = false)]
    private void PurchaseSkillServerRpc(int skillIndex, ServerRpcParams serverRpcParams = default)
    {
        var skillSo = skills[skillIndex];

        if (powerUp.Unlock(skillSo, skillIndex, skillPoints.Value, serverRpcParams))
        {
            RemoveSkillPoints(skillSo.requiredSkillPoints);

            ClientRpcParams clientRpcParams = default;
            clientRpcParams.Send.TargetClientIds = new ulong[] { OwnerClientId };
            UnlockSkillClientRpc(clientRpcParams);
        }
    }

    [ClientRpc]
    private void UnlockSkillClientRpc(ClientRpcParams clientRpcParams = default)
    {
        UpdateUI();
    }

    private void UpdateSkillUIData(SkillSo skillSo)
    {
        var skill = GetVisualElement(skillSo.skillTag);
        var title = skill.Q<Label>("Title");
        var description = skill.Q<Label>("Value");
        var cost = skill.Q<Label>("Points");
        var alreadyPurchased = skill.Q<VisualElement>("AlreadyPurchased");

        if (powerUp.CanBePurchased(skillSo, skillPoints.Value))
        {
            skill.SetEnabled(true);
        }
        else if (powerUp.IsUnlocked(skillSo))
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

    private void UpdateUI()
    {
        foreach (var skill in skills)
        {
            UpdateSkillUIData(skill);
        }
    }

    private void AddSkillEvent()
    {
        foreach (var skill in skills)
        {
            var skillEl = GetVisualElement(skill.skillTag);

            skillEl.RegisterCallback((ClickEvent ev) =>
            {
                var skillIndex = skills.IndexOf(skill);
                Debug.Log($"Purchasing skill {skill.skillName} at index {skillIndex}");
                PurchaseSkillServerRpc(skillIndex);
            });
        }
    }

    private void UpdateSkillPoints()
    {
        skillPointsText.text = $"Skill points: {skillPoints.Value} ";
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddSkillPointsServerRpc(int amount, ServerRpcParams serverRpcParams = default)
    {
        skillPoints.Value += amount;
        ClientRpcParams clientRpcParams = default;
        clientRpcParams.Send.TargetClientIds = new ulong[] { OwnerClientId };
    }

    [ClientRpc]
    public void AddSkillPointsClientRpc(ClientRpcParams clientRpc = default)
    {
        UpdateUI();
    }

    public void RemoveSkillPoints(int amount)
    {
        skillPoints.Value -= amount;
    }

    public void Show()
    {
        skillTree.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        skillTree.style.display = DisplayStyle.None;
    }

    private void OnSkillPointsChange(int oldValue, int newValue)
    {
        UpdateSkillPoints();
        UpdateUI();
    }

    private void OnSkillListChanged(NetworkListEvent<int> changeEvent)
    {
        UpdateUI();
    }

    private void Awake()
    {
        powerUp = GetComponent<PowerUp>();
    }

    void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        skillPointsText = GetLabel("SkillPoints");
        skillTreeClose = GetButton("SkillTreeClose");
        skillTree = GetVisualElement("SkillTreeContainer");

        skillTreeClose.RegisterCallback((ClickEvent ev) => Hide());
        skillPoints.OnValueChanged += OnSkillPointsChange;
        powerUp.unlockedSkillsIndex.OnListChanged += OnSkillListChanged;

        Hide();
        AddSkillEvent();
        UpdateUI();
        UpdateSkillPoints();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        skillPoints.OnValueChanged -= OnSkillPointsChange;
    }

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

        if (Input.GetKeyDown(KeyCode.P) && IsOwner)
        {
            AddSkillPointsServerRpc(1);
        }
    }
}
