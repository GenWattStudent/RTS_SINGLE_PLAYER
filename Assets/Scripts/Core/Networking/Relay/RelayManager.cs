using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : Singleton<RelayManager>
{
    public string JoinCode { get; private set; }
    public string Ip { get; private set; }
    public int Port { get; private set; }
    public byte[] ConnectionData { get; private set; }
    public Guid AllocationId { get; private set; }
    public bool IsHost = false;
    public Allocation Allocation { get; private set; }
    public JoinAllocation JoinAllocation { get; private set; }

    public async Task<string> CreateRelay(int maxPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Allocation = allocation;

            RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.FirstOrDefault(endpoint => endpoint.ConnectionType == "dtls");

            Ip = dtlsEndpoint.Host;
            Port = dtlsEndpoint.Port;
            ConnectionData = allocation.ConnectionData;
            AllocationId = allocation.AllocationId;
            IsHost = true;

            return JoinCode;
        }
        catch (Exception)
        {
            Debug.LogError("Failed to create relay");
            return null;
        }
    }

    public async Task<bool> JoinRelay(string code)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(code);
            JoinAllocation = allocation;

            RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.FirstOrDefault(endpoint => endpoint.ConnectionType == "dtls");

            Ip = dtlsEndpoint.Host;
            Port = dtlsEndpoint.Port;
            ConnectionData = allocation.ConnectionData;
            AllocationId = allocation.AllocationId;

            return true;
        }
        catch (Exception)
        {
            Debug.LogError("Failed to join relay");
            return false;
        }
    }
}