using Control;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerObject
{
    public string playerClientID;
    public Player playerClientRef;
    public Podium podium;
    public string otp;
    public string playerName;

    public SeatedPlayerObject seat = null;

    public string twitchName;
    public Texture profileImage;

    public int r1Points;
    public int r2Points;
    public int r3Points;
    public int r4Points;
    public bool finalist;

    public List<string> submittedAnswers = new List<string>();
    public int points;
    public string currentPositionString;
    public bool flagForCondone;
    public bool wasCorrect;

    public int pennysWon = 0;
    public int medalStatus = 0;
    //0 = none, 1 = gold, 2 = silver, 3 = bronze, 4 = lobby

    public PlayerObject(Player pl, string name)
    {
        playerClientRef = pl;
        otp = OTPGenerator.GenerateOTP();
        playerName = name;
        points = 0;
    }

    public PlayerObject(string dummyPlayer)
    {
        string[] x = dummyPlayer.Split('|');
        playerName = x[0];
        if (int.TryParse(x[1], out int val))
            points = val;
        else
            submittedAnswers.Add(x[1]);
        profileImage = ColumnManager.Get.incorrectTexture;
    }

    public void ApplyProfilePicture(string name, Texture tx, bool bypassSwitchAccount = false)
    {
        //Player refreshs and rejoins the same game
        if (PlayerManager.Get.players.Count(x => (!string.IsNullOrEmpty(x.twitchName)) && x.twitchName.ToLowerInvariant() == name.ToLowerInvariant()) > 0 && !bypassSwitchAccount)
        {
            PlayerObject oldPlayer = PlayerManager.Get.players.FirstOrDefault(x => x.twitchName.ToLowerInvariant() == name.ToLowerInvariant());
            if (oldPlayer == null)
                return;

            HostManager.Get.SendPayloadToClient(oldPlayer, EventLibrary.HostEventType.SecondInstance, "");

            oldPlayer.playerClientID = playerClientID;
            oldPlayer.playerClientRef = playerClientRef;
            oldPlayer.playerName = playerName;
            //oldPlayer.podium.playerNameMesh.text = playerName;

            otp = "";
            //podium.containedPlayer = null;
            //podium = null;
            playerClientRef = null;
            playerName = "";

            if (PlayerManager.Get.pendingPlayers.Contains(this))
                PlayerManager.Get.pendingPlayers.Remove(this);

            HostManager.Get.SendPayloadToClient(oldPlayer, EventLibrary.HostEventType.Validated, $"{oldPlayer.playerName}|Points: {oldPlayer.points.ToString()}|{oldPlayer.twitchName}");
            //HostManager.Get.UpdateLeaderboards();
            return;
        }
        otp = "";
        twitchName = name.ToLowerInvariant();
        profileImage = tx;
        /*if(!LobbyManager.Get.lateEntry)
        {
            //podium.InitialisePodium();
        }
        else
        {
            points = 0;
            eliminated = true;
        }*/
        HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.Validated, $"{playerName}|Points: {points.ToString()}|{twitchName}");
        PlayerManager.Get.InitialisePlayerObject(this);
    }
}
