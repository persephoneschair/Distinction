using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using Newtonsoft.Json;
using NaughtyAttributes;
using System.Linq;
using TMPro;

public class GameplayPennys : SingletonMonoBehaviour<GameplayPennys>
{
    private PlayerShell playerList;
    private MedalTableObject medalList;
    private List<LeaderboardObject> leaderboardList;
    private readonly string path = @"D:\Unity Projects\HerokuPennyData\PennyStorage";

    public int authorPennys;
    [Range (1, 50)] public int multiplyFactor;
    public string gameName;

    [Button]
    public void UpdatePennysAndMedals()
    {
        AwardPennys();
        if(!string.IsNullOrEmpty(gameName))
            AwardMedals();
        WriteNewFile();
    }

    private void LoadJSON()
    {
        playerList = JsonConvert.DeserializeObject<PlayerShell>(File.ReadAllText(path + @"\NewPennys.txt"));
    }

    private void LoadMedalJSON()
    {
        medalList = JsonConvert.DeserializeObject<MedalTableObject>(File.ReadAllText(path + $@"\{gameName}.txt"));
    }

    private void AwardPennys()
    {
        List<PlayerObject> list = PlayerManager.Get.players.OrderByDescending(p => p.points).ThenBy(p => p.twitchName).Where(x => x.points > 0).ToList();
        PlayerPennyData ppd;

        LoadJSON();
        foreach (PlayerObject p in list)
        {
            ppd = playerList.playerList.FirstOrDefault(x => x.PlayerName.ToLowerInvariant() == p.twitchName.ToLowerInvariant());
            if (ppd == null)
                CreateNewPlayer(p);
            else
            {
                ppd.CurrentSeasonPennys += p.pennysWon;
                ppd.AllTimePennys += p.pennysWon;
            }
        }

        ppd = null;
        ppd = playerList.playerList.FirstOrDefault(x => x.PlayerName.ToLowerInvariant() == QuestionManager.currentPack.author.ToLowerInvariant());
        if (ppd == null)
            CreateNewAuthor(QuestionManager.currentPack.author.ToLowerInvariant());
        else
        {
            ppd.CurrentSeasonPennys += authorPennys;
            ppd.AllTimePennys += authorPennys;
            ppd.AuthorCredits++;
        }
    }

    private void AwardMedals()
    {
        List<PlayerObject> medallists = PlayerManager.Get.players.Where(x => x.medalStatus != 0).ToList();
        LoadMedalJSON();

        foreach(PlayerObject player in medallists)
            switch(player.medalStatus)
            {
                case 1:
                    medalList.goldMedallists.Add(player.twitchName.ToLowerInvariant());
                    break;

                case 2:
                    medalList.silverMedallists.Add(player.twitchName.ToLowerInvariant());
                    break;

                case 3:
                    medalList.bronzeMedallists.Add(player.twitchName.ToLowerInvariant());
                    break;

                case 4:
                    medalList.lobbyMedallists.Add(player.twitchName.ToLowerInvariant());
                    break;
            }
    }

    private void CreateNewPlayer(PlayerObject p)
    {
        PlayerPennyData newP = new PlayerPennyData()
        {
            PlayerName = p.twitchName.ToLowerInvariant(),
            CurrentSeasonPennys = p.pennysWon,
            AllTimePennys = p.pennysWon
        };
        playerList.playerList.Add(newP);
    }

    private void CreateNewAuthor(string p)
    {
        PlayerPennyData newP = new PlayerPennyData()
        {
            PlayerName = p,
            CurrentSeasonPennys = authorPennys,
            AllTimePennys = authorPennys,
            AuthorCredits = 1
        };
        playerList.playerList.Add(newP);
    }

    private void WriteNewFile()
    {
        string pennyPath = Operator.Get.testMode ? path + @"\NewPennysTest.txt" : path + @"\NewPennys.txt";
        string medalPath = Operator.Get.testMode ? path + $@"\{gameName}Test.txt" : path + $@"\{gameName}.txt";

        string newDataContent = JsonConvert.SerializeObject(playerList, Formatting.Indented);
        File.WriteAllText(pennyPath, newDataContent);

        if (!string.IsNullOrEmpty(gameName))
        {
            newDataContent = JsonConvert.SerializeObject(medalList, Formatting.Indented);
            File.WriteAllText(medalPath, newDataContent);
        }

        if (Operator.Get.testMode)
            DebugLog.Print("TEST DATA WRITTEN TO DRIVE", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
        else
            DebugLog.Print("DATA WRITTEN TO DRIVE", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
    }
}
