using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Ability
{
    public SkillSo skill;
    public float cooldownTimer = 0;
    public bool isOnCooldown => cooldownTimer > 0;
}

public class AbilityManager : NetworkToolkitHelper
{
    [SerializeField] private VisualTreeAsset abilityUIPrefab;
    public List<Ability> Abilities = new();

    private PowerUp _powerUp;
    private VisualElement _abilityContainer;
    private SelectionManager _selectionManager;

    protected override void OnEnable()
    {
        base.OnEnable();

        _abilityContainer = GetVisualElement("AbilityContainer");
        SelectionManager.OnSelect += HandleSelectionChanged;

        _abilityContainer.Clear();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _powerUp.unlockedSkillsIndex.OnListChanged += HandleUnlockedSkillsChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        _powerUp.unlockedSkillsIndex.OnListChanged -= HandleUnlockedSkillsChanged;
    }

    private void HandleSelectionChanged(List<Selectable> list)
    {
        DrawAbilities(list);
    }

    private void DrawAbilities(List<Selectable> list)
    {
        if (!IsOwner) return;
        _abilityContainer.Clear();

        foreach (var selectable in list)
        {
            if (selectable.TryGetComponent(out Unit unit) && selectable.GetComponent<Building>() == null)
            {
                foreach (var ability in Abilities)
                {
                    if (unit.unitSo.unitName == ability.skill.unitName)
                    {
                        AddAbilityUI(ability, unit);
                    }
                }
            }
        }
    }

    private void AddAbilityUI(Ability ability, Unit unit)
    {
        var abilityUI = abilityUIPrefab.CloneTree();

        abilityUI.style.backgroundImage = new StyleBackground(ability.skill.icon);
        abilityUI.RegisterCallback<ClickEvent>((ev) => HandleAbilityClicked(ability, unit));

        _abilityContainer.Add(abilityUI);
    }

    private void HandleAbilityClicked(Ability ability, Unit unit)
    {
        if (ability.isOnCooldown) return;

        ActivateAbilityServerRpc(unit.GetComponent<NetworkObject>(), Abilities.IndexOf(ability));
    }

    [ServerRpc]
    private void ActivateAbilityServerRpc(NetworkObjectReference nor, int abilityIndex)
    {
        if (nor.TryGet(out var no))
        {
            var unit = no.GetComponent<Unit>();

            if (unit != null)
            {
                var ability = Abilities[abilityIndex];

                if (ability.skill.isAbility && !ability.isOnCooldown)
                {
                    ability.cooldownTimer = ability.skill.cooldown;
                    ability.skill.Activate(unit);
                }
            }
        }
    }

    private void Awake()
    {
        _powerUp = GetComponent<PowerUp>();
        _selectionManager = GetComponentInParent<SelectionManager>();
    }

    private void HandleUnlockedSkillsChanged(NetworkListEvent<int> changeEvent)
    {
        var unlockedAbilities = _powerUp.GetUnlockedAbilities();

        foreach (var ability in unlockedAbilities)
        {
            if (!Abilities.Exists(a => a.skill == ability))
            {
                Abilities.Add(new Ability { skill = ability });
            }
        }

        DrawAbilities(_selectionManager.selectedObjects);
    }

    private void Update()
    {
        foreach (var ability in Abilities)
        {
            if (ability.isOnCooldown)
            {
                ability.cooldownTimer -= Time.deltaTime;
            }
        }
    }
}
