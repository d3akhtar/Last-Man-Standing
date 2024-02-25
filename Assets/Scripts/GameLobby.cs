using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class GameLobby : MonoBehaviour
{
    public static GameLobby Instance { get; private set; }

    private Lobby joinedLobby;

    public event EventHandler OnTryingToCreateLobby;
    public event EventHandler OnTryingToJoinLobby;
    public event EventHandler OnJoinedLobby;
    public event EventHandler OnFailedToJoinLobby;
    public event EventHandler<OnUpdateLobbyListEventArgs> OnUpdateLobbyList;

    public class OnUpdateLobbyListEventArgs : EventArgs
    {
        public List<Lobby> lobbies;
    }

    private float heartbeatTimer = 15f;
    private float heartbeatTimerMax = 15f;

    private float updateLobbyListTimer = 5f;
    private float updateLobbyListTimerMax = 5f;


    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(this.gameObject);

        InitializeUnityAuthentication();
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void Update()
    {
        HandleHeartbeat();
        HandleUpdateLobbyList();
    }

    private void HandleHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                heartbeatTimer = heartbeatTimerMax;
            }
        }
    }

    private void HandleUpdateLobbyList()
    {
        if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn && SceneManager.GetActiveScene().name == Loader.Scene.LobbyScene.ToString())
        {
            updateLobbyListTimer -= Time.deltaTime;
            if (updateLobbyListTimer < 0)
            {
                updateLobbyListTimer = updateLobbyListTimerMax;

                ListLobbies();
            }
        }
    }

    private async void ListLobbies()
    {
        try
        {
            // list lobbies that aren't full
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter> { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) },
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);

            OnUpdateLobbyList?.Invoke(this, new OnUpdateLobbyListEventArgs
            {
                lobbies = queryResponse.Results,
            }) ;
        }

        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await Relay.Instance.CreateAllocationAsync(3);

            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);

            return default;
        }
    }

    private async Task<string> GetRelayCode(Allocation allocation)
    {
        try
        {
            string code = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

            return code;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);

            return default;
        }
    }

    private async Task<JoinAllocation> JoinAllocation(string code)
    {
        try
        {
            JoinAllocation joinAllocation = await Relay.Instance.JoinAllocationAsync(code);
            
            return joinAllocation;
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);

            return default;
        }
    }

    public async void CreateLobby(bool isPrivate, string name)
    {
        OnTryingToCreateLobby?.Invoke(this, EventArgs.Empty);

        if (name == null)
        {
            name = "no name";
        }

        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(name, GameMultiplayer.MAX_PLAYER_AMOUNT, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
            });

            Allocation allocation = await AllocateRelay();

            string code = await GetRelayCode(allocation);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RELAY_JOIN_CODE", new DataObject(DataObject.VisibilityOptions.Member, code) },
                },
            });

            GameMultiplayer.Instance.StartHost();

            Loader.LoadNetwork(Loader.Scene.PostJoinLobbyScene);
        }
        catch (LobbyServiceException ex)
        {
            OnFailedToJoinLobby?.Invoke(this, EventArgs.Empty);
            Debug.LogException(ex);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string joinRelayCode = joinedLobby.Data["RELAY_JOIN_CODE"].Value;

            JoinAllocation joinAllocation = await JoinAllocation(joinRelayCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            GameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            OnFailedToJoinLobby?.Invoke(this, EventArgs.Empty);
            Debug.LogException(ex);
        }
    }

    public async void JoinLobbyByCode(string code)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);

            string joinRelayCode = joinedLobby.Data["RELAY_JOIN_CODE"].Value;

            JoinAllocation joinAllocation = await JoinAllocation(joinRelayCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            GameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            OnFailedToJoinLobby?.Invoke(this, EventArgs.Empty);
            Debug.LogException(ex);
        }
    }

    public async void JoinLobbyById(string id)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(id);

            string joinRelayCode = joinedLobby.Data["RELAY_JOIN_CODE"].Value;

            JoinAllocation joinAllocation = await JoinAllocation(joinRelayCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            GameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException ex)
        {
            OnFailedToJoinLobby?.Invoke(this, EventArgs.Empty);
            Debug.LogException(ex);
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    public string GetLobbyJoinCode()
    {
        return joinedLobby.LobbyCode;
    }

}
