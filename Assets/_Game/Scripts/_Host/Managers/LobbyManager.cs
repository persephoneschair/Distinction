using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameplayManager;

public class LobbyManager : SingletonMonoBehaviour<LobbyManager>
{
    public TextMeshPro lobbyCodeMesh;
    private const string welcomeMessage = "https://persephoneschair.itch.io\n/gamenight\n<size=500%>[ABCD]";

    [Button]
    public void OnOpenLobby()
    {
        lobbyCodeMesh.text = welcomeMessage.Replace("[ABCD]", HostManager.Get.host.RoomCode.ToUpperInvariant());
    }

    [Button]
    public void OnLockLobby()
    {
        GameplayManager.Get.currentStage++;
        GameplayManager.Get.ProgressGameplay();
    }
}
