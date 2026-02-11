using PurrLobby;
using Steamworks;
using UnityEngine;

namespace Game.Scripts.Server
{
    public class LeaveLobbyHandler : MonoBehaviour
    {
        private static CallResult<LobbyMatchList_t> _lobbyListCall;
        
        private void Awake() {
            DontDestroyOnLoad(gameObject);
            Application.quitting += OnQuitting;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
        }

        private void OnDestroy() {
            Application.quitting -= OnQuitting;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
#endif
        }
        public static void LeaveAnyLobby() {
            try {
                var call = SteamMatchmaking.RequestLobbyList();
                _lobbyListCall = CallResult<LobbyMatchList_t>.Create(OnLobbyList);
                _lobbyListCall.Set(call);
            }
            catch {
                // Steam might not be initialized yet; catch any exceptions here.
            }
        }
        private static void OnLobbyList(LobbyMatchList_t result, bool ioFailure) {
            if (ioFailure) return;

            CSteamID me;
            try { me = SteamUser.GetSteamID(); }
            catch { return; }

            int leftCount = 0;

            for (int i = 0; i < result.m_nLobbiesMatching; i++) {
                var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                if (!lobbyId.IsValid()) continue;

                int memberCount;
                try { memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId); }
                catch { continue; }

                for (int m = 0; m < memberCount; m++) {
                    var member = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, m);
                    if (member == me) {
                        SteamMatchmaking.LeaveLobby(lobbyId);
                        leftCount++;
                        Debug.Log($"[Bootstrap] Left Steam lobby {lobbyId.m_SteamID} on startup/quit.");
                        break; // move to next lobby
                    }
                }
            }

            if (leftCount == 0)
                Debug.Log("[Bootstrap] No lobbies contained the local user.");
        }


        private void OnQuitting()
        {
            LeaveAnyLobby();
        }

#if UNITY_EDITOR
        private void OnPlayModeChanged(UnityEditor.PlayModeStateChange s) {
            if (s == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                OnQuitting();
        }
#endif
    
    }
}
