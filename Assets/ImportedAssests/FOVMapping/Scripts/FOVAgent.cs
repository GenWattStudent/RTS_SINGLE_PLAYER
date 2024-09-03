using UnityEngine;
using Unity.Netcode;

namespace FOVMapping
{
	// Attach this component to 'eyes' of the field of view.
	// Works only when this component is enabled.
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
		private bool isUnderFOW = false;
		private Unit unit;
		private NetworkObject networkObject;

		private bool CheckVisibility(ulong clientId)
		{
			// If not spawned, then always return false
			if (!IsSpawned)
			{
				return false;
			}

			// We can do a simple distance check between the NetworkObject instance position and the client
			return !isUnderFOW || clientId == OwnerClientId;
		}

		public override void OnNetworkSpawn()
		{
			unit = GetComponent<Unit>();
			networkObject = GetComponent<NetworkObject>();
			if (!IsServer) return;

			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var client))
			{
				var playerController = client.PlayerObject.GetComponent<PlayerController>();
				Debug.Log(playerController);

				if (playerController != null)
				{
					contributeToFOV = playerController.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId;
					disappearInFOW = !contributeToFOV;
				}
				else
				{
					Debug.LogError("PlayerController component not found on PlayerObject.");
				}
			}
			else
			{
				Debug.LogError("Client not found in ConnectedClients.");
			}

			if (IsServer)
			{
				// The server handles visibility checks and should subscribe when spawned locally on the server-side.
				networkObject.CheckObjectVisibility += CheckVisibility;
				// If we want to continually update, we don't need to check every frame but should check at least once per tick
				NetworkManager.NetworkTickSystem.Tick += OnNetworkTick;
			}

			base.OnNetworkSpawn();
		}

		private void OnNetworkTick()
		{
			if (!IsServer) return;
			// If CheckObjectVisibility is enabled, check the distance to clients
			// once per network tick.
			foreach (var clientId in NetworkManager.ConnectedClientsIds)
			{
				var shouldBeVisibile = CheckVisibility(clientId);
				var isVisibile = networkObject.IsNetworkVisibleTo(clientId);
				var isClientServer = clientId == NetworkManager.ServerClientId;

				if (shouldBeVisibile && !isVisibile && !isClientServer)
				{
					// Note: This will invoke the CheckVisibility check again
					networkObject.NetworkShow(clientId);
				}
				else if (!shouldBeVisibile && isVisibile && !isClientServer)
				{
					Debug.Log("NetworkHide");
					networkObject.NetworkHide(clientId);
				}
			}
		}

		[HideInInspector]
		public void SetUnderFOW(bool isUnder)
		{
			isUnderFOW = isUnder;

			if (disappearInFOW && unit != null)
			{
				if (isUnderFOW)
				{
					unit.ShowUnit();
				}
				else
				{
					unit.HideUnit();
				}
			}
		}

		public bool IsUnderFOW()
		{
			return isUnderFOW;
		}

		public override void OnNetworkDespawn()
		{
			if (IsServer)
			{
				networkObject.CheckObjectVisibility -= CheckVisibility;
				NetworkManager.NetworkTickSystem.Tick -= OnNetworkTick;
			}
			base.OnNetworkDespawn();
		}
	}
}