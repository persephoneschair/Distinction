using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    public List<PlayerObject> pendingPlayers = new List<PlayerObject>();
    public List<PlayerObject> players = new List<PlayerObject>();

    [Header("Lobby")]
    public GameObject[] stairsAnimations;
    public Transform stairsTransformTarget;
    public SeatedPlayerObject[] seatedPlayerObjects;

    [Header("Editor Controls")]
    public bool pullingData = true;
    [Range(0,39)] public int playerIndex;

    public void InitialisePlayerObject(PlayerObject pl)
    {
        pendingPlayers.Remove(pl);
        players.Add(pl);
        var x = Instantiate(Extensions.PickRandom(stairsAnimations), stairsTransformTarget);
        x.GetComponent<StairEntrance>().Initiate(pl.profileImage);

        List<SeatedPlayerObject> vacantSeats = seatedPlayerObjects.Where(x => x.containedPlayer == null).ToList();
        Extensions.PickRandom(vacantSeats).OnActivate(pl);

        SaveManager.BackUpData();
    }


    private PlayerObject _focusPlayer;
    public PlayerObject FocusPlayer
    {
        get { return _focusPlayer; }
        set
        {
            if(value != null)
            {
                _focusPlayer = value;
                playerName = value.playerName;
                twitchName = value.twitchName;
                profileImage = value.profileImage;
                flagForCondone = value.flagForCondone;
                wasCorrect = value.wasCorrect;

                points = value.points;
            }
            else
            {
                playerName = "OUT OF RANGE";
                twitchName = "OUT OF RANGE";
                profileImage = null;
                flagForCondone = false;
                wasCorrect = false;

                points = 0;
                totalCorrect = 0;
                currentBid = 0;
                maxPoints = 0;
                submission = "OUT OF RANGE";
                submissionTime = 0;
            }                
        }
    }

    [Header("Fixed Fields")]
    [ShowOnly] public string playerName;
    [ShowOnly] public string twitchName;
    public Texture profileImage;
    [ShowOnly] public bool flagForCondone;
    [ShowOnly] public bool wasCorrect;

    [Header("Variable Fields")]
    public int points;
    public int totalCorrect;
    public int currentBid;
    public int maxPoints;
    public string submission;
    public float submissionTime;

    void UpdateDetails()
    {
        if (playerIndex >= players.Count)
            FocusPlayer = null;
        else
            FocusPlayer = players.OrderBy(x => x.playerName).ToList()[playerIndex];
    }

    private void Update()
    {
        if (pullingData)
            UpdateDetails();
    }

    [Button]
    public void SetPlayerDetails()
    {
        if (pullingData)
            return;
        SetDataBack();
    }

    [Button]
    public void RestoreOrEliminatePlayer()
    {
        if (pullingData)
            return;
        pullingData = true;

    }

    void SetDataBack()
    {
        FocusPlayer.points = points;
        pullingData = true;
    }

    public void SendMessageToAllPlayers(string message)
    {
        foreach (PlayerObject pl in players)
            SendMessageToPlayer(message, pl);
    }

    public void SendMessageToPlayer(string message, PlayerObject pl)
    {
        HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.Information, message);
    }

    public void SendMessageToPlayer(string primaryMessage, string secondaryMessage, PlayerObject pl)
    {
        string payload = string.Join("|", primaryMessage, secondaryMessage);
        HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.DoubleInformation, payload);
    }

    public void SendQuestionToAllNonFinalists(string questionConcat, int halfSecondDelays)
    {
        if (halfSecondDelays == 0)
            SendActionNonFinalists(questionConcat);
        else
            StartCoroutine(DelayedSendToAllNonFinalists(questionConcat, halfSecondDelays));
    }

    IEnumerator DelayedSendToAllNonFinalists(string questionConcat, int halfSecondDelays)
    {
        for (int i = halfSecondDelays; i > 0; i--)
        {
            foreach (PlayerObject pl in players)
                SendMessageToPlayer($"Launching in {i}...", pl);
            yield return new WaitForSeconds(0.5f);
        }
        SendActionNonFinalists(questionConcat);
    }

    public void SendActionNonFinalists(string questionConcat)
    {
        foreach (PlayerObject pl in players.Where(x => !x.finalist))
            SendQuestionToPlayer(questionConcat, pl);
        foreach (PlayerObject pl in players.Where(x => x.finalist))
            SendMessageToPlayer(questionConcat.Split('|').FirstOrDefault(), pl);
    }

    public void SendQuestionToAllPlayers(string questionConcat, int halfSecondDelays)
    {
        if(halfSecondDelays == 0)
        {
            foreach (PlayerObject pl in players)
                SendQuestionToPlayer(questionConcat, pl);
        }
        else
            StartCoroutine(DelayedSendToAll(questionConcat, halfSecondDelays));
    }

    IEnumerator DelayedSendToAll(string questionConcat, int halfSecondDelays)
    {
        for(int i = halfSecondDelays; i > 0; i--)
        {
            foreach (PlayerObject pl in players)
                SendMessageToPlayer($"Launching in {i}...", pl);
            yield return new WaitForSeconds(0.5f);
        }
        foreach (PlayerObject pl in players)
            SendQuestionToPlayer(questionConcat, pl);
    }

    public void SendQuestionToPlayer(string questionConcat, PlayerObject pl)
    {
        HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.SimpleQuestion, questionConcat);
    }

    public void UpdatePlayerScores()
    {
        foreach (PlayerObject pl in players)
            UpdatePlayerScore(pl);
    }

    public void UpdatePlayerScore(PlayerObject pl)
    {
        HostManager.Get.SendPayloadToClient(pl, EventLibrary.HostEventType.UpdateScore, $"Points: {pl.points}");
    }

    public void ResetPlayerVariablesAndLogRoundScore(bool logScore = true)
    {
        foreach (PlayerObject pl in players)
        {
            pl.submittedAnswers.Clear();
            pl.closeSubmittedAnswers.Clear();
            pl.flagForCondone = false;
            pl.wasCorrect = false;
            if(logScore)
            {
                int subtotal = pl.r1Points + pl.r2Points + pl.r3Points + pl.r4Points;
                switch(GameplayManager.Get.currentRound)
                {
                    case GameplayManager.Round.Round1:
                        pl.r1Points = points - subtotal;
                        break;

                    case GameplayManager.Round.Round2:
                        pl.r2Points = points - subtotal;
                        break;

                    case GameplayManager.Round.Round3:
                        pl.r3Points = points - subtotal;
                        break;

                    case GameplayManager.Round.Round4:
                        if (pl.finalist)
                            continue;
                        pl.r4Points = points - subtotal;
                        break;
                }
            }
        }
    }
}
