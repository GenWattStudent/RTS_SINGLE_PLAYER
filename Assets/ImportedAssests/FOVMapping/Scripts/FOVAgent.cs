using UnityEngine;
using Unity.Netcode;

namespace FOVMapping
{
	// Attach this component to 'eyes' of the field of view.
	// Works only when this component is enabled.
	[DefaultExecutionOrder(3)]
	public class FOVAgent : NetworkBehaviour
	{
		[Tooltip("Is this agent an eye(set to true for friendly agents and false for hostile agents)?")]
		[SerializeField]
		private bool _contributeToFOV = true;
		public bool contributeToFOV { get => _contributeToFOV; set => _contributeToFOV = value; }

		[Tooltip("How far can an agent see? This value must be equal to or less than the samplingRange of a generated FOV map.")]
		[SerializeField]
		[Range(0.0f, 1000.0f)]
		private float _sightRange = 50.0f;
		public float sightRange { get => _sightRange; set => _sightRange = value; }

		[Tooltip("How widely can an agent see?")]
		[SerializeField]
		[Range(0.0f, 360.0f)]
		private float _sightAngle = 240.0f;
		public float sightAngle { get => _sightAngle; set => _sightAngle = value; }

		[Tooltip("Will this agent disappear if it is in a fog of war(set to true for hostile agents and false for friendly agents)?")]
		[SerializeField]
		private bool _disappearInFOW = false;
		public bool disappearInFOW { get => _disappearInFOW; set => _disappearInFOW = value; }

		[Tooltip("On the boundary of a field of view, if an agent with `disappearInFOW` set to true is under a fog of war whose opacity is larger than this value, the agent disappears.")]
		[SerializeField]
		[Range(0.0f, 1.0f)]
		private float _disappearAlphaThreshold = 0.1f;
		public float disappearAlphaThreshold { get => _disappearAlphaThreshold; set => _disappearAlphaThreshold = value; }
		public Damagable Damagable;

		private PlayerController playerController;

		private void Awake()
		{
			var unit = GetComponent<Unit>();
			var building = GetComponent<Building>();

			if (building != null)
			{
				sightAngle = building.buildingSo.sightAngle;
				sightRange = building.buildingSo.sightRange;
			}
			else if (unit.unitSo != null)
			{
				sightAngle = unit.unitSo.sightAngle;
				sightRange = unit.unitSo.sightRange;
			}
		}

		private void Start()
		{

			// playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
			// Damagable = GetComponent<Damagable>();

			// playerController.teamType.OnValueChanged += HandleTeamChange;
			// Damagable.teamType.OnValueChanged += HandleUnitTeamChange;

			// AddAgentToFogOfWar(this, playerController.teamType.Value, Damagable.teamType.Value);
		}
		private void HandleTeamChange(TeamType oldValue, TeamType newValue)
		{
			AddAgentToFogOfWar(GetComponent<FOVAgent>(), newValue, Damagable.teamType.Value);
		}

		private void HandleUnitTeamChange(TeamType oldValue, TeamType newValue)
		{
			AddAgentToFogOfWar(GetComponent<FOVAgent>(), playerController.teamType.Value, newValue);
		}

		private void AddAgentToFogOfWar(FOVAgent fovAgent, TeamType playerTeamType, TeamType unitTeamType)
		{
			var construction = GetComponent<Construction>();

			fovAgent.disappearInFOW = unitTeamType != playerTeamType;
			fovAgent.contributeToFOV = unitTeamType == playerTeamType && construction == null;

			var fogOfWar = FindFirstObjectByType<FOVManager>();
			if (fogOfWar != null && !fogOfWar.ContainsFOVAgent(fovAgent))
			{
				fogOfWar.AddFOVAgent(fovAgent);
			}
		}
	}
}