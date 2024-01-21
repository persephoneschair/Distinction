using Control;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInputManager : SingletonMonoBehaviour<PlayerInputManager>
{
    public Transform popUpTransformTarget;
    public GameObject popUpObject;

    public void InputReceived(PlayerObject player, string response)
    {
        InstantiatePlayerPopup(player);
        AudioManager.Get.Play((AudioManager.FindAnswerClip)UnityEngine.Random.Range(11, 15));
        switch(GameplayManager.Get.currentRound)
        {
            case GameplayManager.Round.Round1:
            case GameplayManager.Round.Round3:
                R1And3Handling(player, response);
                break;

            case GameplayManager.Round.Round2:
                R2Handling(player, response);
                break;

            case GameplayManager.Round.Round4:
                if(player.finalist)
                    R4FinalistHandling(player, response);
                else
                    R4NonFinalistHandling(player, response);
                break;
        }
    }

    #region Round 1 & 3

    public void R1And3Handling(PlayerObject player, string response)
    {
        Column c = null;
        AnswerBox a = null;
        double matchRate = 0;
        double threshold = Operator.Get.spellingThreshold;
        for (int i = 0; i < 3; i++)
        {
            Column col = ColumnManager.Get.multiQuestionColumns[i];

            ///Set spelling threshold
            if (Operator.Get.exactSpellingRequiredMulti[i])
                threshold = 0.999f;
            else
                threshold = Operator.Get.spellingThreshold;

            foreach (AnswerBox ans in col.answerBoxes)
                foreach (string valid in ans.validAnswers)

                    ///This line checks
                    ///a) Whether the response is above the threshold
                    ///b) Whether it's greater than the currently set match rate
                    ///Hopefully the submitted answer will always return a "best fit"
                    if (Extensions.Spellchecker(response, valid) >= threshold && Extensions.Spellchecker(response, valid) > matchRate)
                        ///Now check that the answer is unrevealed and they player hasn't already submitted the box's main answer
                        if (!ans.revealed && !player.submittedAnswers.Contains(ans.validAnswers.FirstOrDefault()))
                        {
                            matchRate = Extensions.Spellchecker(response, valid);
                            c = col;
                            a = ans;
                        }
        }
        if (matchRate >= threshold)
        {
            var ax = c.answerBoxes.FirstOrDefault(x => x.Equals(a));
            bool beatenToThePunch = ax.flaggedForReveal;
            ax.FlagAnswerAsFound(player);
            PlayerManager.Get.UpdatePlayerScore(player);
            if (!beatenToThePunch)
                PlayerManager.Get.SendMessageToPlayer($"<color=green>CORRECT</color>\nYou've got {player.submittedAnswers.Count}/{GameplayManager.Get.currentConfig.maximumAnswers}:\n<i>{string.Join("\n", player.submittedAnswers)}</i>", player);
            else
                PlayerManager.Get.SendMessageToPlayer($"<color=orange>CORRECT</color>\n<size=50%>(but someone <i>JUST</i> beat you to it!)</size>\nYou've got {player.submittedAnswers.Count}/{GameplayManager.Get.currentConfig.maximumAnswers}:\n<i>{string.Join("\n", player.submittedAnswers)}</i>", player);
        }
        else
            PlayerManager.Get.SendMessageToPlayer($"<color=red>INCORRECT, REPEAT OR ALREADY FOUND</color>\nYou've got {player.submittedAnswers.Count}/{GameplayManager.Get.currentConfig.maximumAnswers}:\n<i>{string.Join("\n", player.submittedAnswers)}</i>", player);

        StartCoroutine(DelayInterfaceReturnR1R3(player));
    }

    IEnumerator DelayInterfaceReturnR1R3(PlayerObject player)
    {
        yield return new WaitForSeconds(1.25f);
        if(GlobalTimeManager.Get.RoundActive())
        {
            if (player.submittedAnswers.Count >= GameplayManager.Get.currentConfig.maximumAnswers)
                PlayerManager.Get.SendMessageToPlayer($"You've found {GameplayManager.Get.currentConfig.maximumAnswers} answers! Good job!\n<i>{string.Join("\n", player.submittedAnswers)}</i>", player);
            else
                PlayerManager.Get.SendQuestionToPlayer(QuestionManager.GetMultiQuestionString() + $"|{(GlobalTimeManager.Get.GetIntTimeRemaining() - 1)}", player);
        }
    }

    #endregion

    #region Round 2

    public void R2Handling(PlayerObject player, string response)
    {
        Column col = ColumnManager.Get.singleQuestionColumn;
        AnswerBox a = null;
        double matchRate = 0;
        double threshold = Operator.Get.spellingThreshold;

        ///Set spelling threshold
        if (Operator.Get.exactSpellingRequiredSingle)
            threshold = 0.999f;
        else
            threshold = Operator.Get.spellingThreshold;

        foreach (AnswerBox ans in col.answerBoxes)
            foreach (string valid in ans.validAnswers)

                ///This line checks
                ///a) Whether the response is above the threshold
                ///b) Whether it's greater than the currently set match rate
                ///Hopefully the submitted answer will always return a "best fit"
                if (Extensions.Spellchecker(response, valid) >= threshold && Extensions.Spellchecker(response, valid) > matchRate)
                {
                    matchRate = Extensions.Spellchecker(response, valid);
                    a = ans;
                }

        if (matchRate >= threshold)
        {
            player.wasCorrect = true;
            var ax = col.answerBoxes.FirstOrDefault(x => x.Equals(a));
            player.submittedAnswers.Add(ax.validAnswers.FirstOrDefault());
            PlayerManager.Get.SendMessageToPlayer($"<color=green>CORRECT</color>\nYour answer <i>({response})</i> matched with <color=green><u>{ax.validAnswers.FirstOrDefault()}</u></color> which is on the list. Let's see how many points it's worth...", player);
            DebugLog.Print($"{player.playerName} said {response} (matched with {ax.validAnswers.FirstOrDefault()})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
        }
        else
        {
            player.submittedAnswers.Add(response);
            PlayerManager.Get.SendMessageToPlayer($"<color=red>INCORRECT</color>\nYour answer <i>({response})</i> did not match with anything on the list", player);
            DebugLog.Print($"{player.playerName} said {response} (no match)", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
        }

        StrapManager.Get.PlayerResponse(player);
    }

    #endregion

    #region Round 4

    public void R4NonFinalistHandling(PlayerObject player, string response)
    {
        double matchRate = 0;
        double threshold = Operator.Get.spellingThreshold;
        LooseAnswer a = null;

        ///Set spelling threshold
        if (Operator.Get.exactSpellingRequiredSingle)
            threshold = 0.999f;
        else
            threshold = Operator.Get.spellingThreshold;

        foreach (LooseAnswer ans in R4PodiumManager.Get.answers)
            foreach (string valid in ans.validAnswers)

                ///This line checks
                ///a) Whether the response is above the threshold
                ///b) Whether it's greater than the currently set match rate
                ///Hopefully the submitted answer will always return a "best fit"
                if (Extensions.Spellchecker(response, valid) >= threshold && Extensions.Spellchecker(response, valid) > matchRate)
                    ///Now check that the answer is unrevealed and they player hasn't already submitted the box's main answer
                    if (!player.submittedAnswers.Contains(ans.validAnswers.FirstOrDefault()))
                    {
                        matchRate = Extensions.Spellchecker(response, valid);
                        a = ans;
                    }

        if (matchRate >= threshold)
        {
            a.playersWhoGaveThisAnswer++;
            player.submittedAnswers.Add(a.validAnswers.FirstOrDefault());
            var strap = StrapManager.Get.r4PlayerPrefabs.FirstOrDefault(x => x.containedPlayer == player);
            strap.Hit();
            DebugLog.Print($"{a.validAnswers.FirstOrDefault()} found by {(player == null ? "UNKNOWN" : player.playerName)}", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
            PlayerManager.Get.SendMessageToPlayer($"<color=green>CORRECT</color>\nYou've got {player.submittedAnswers.Count}/{GameplayManager.Get.currentConfig.maximumAnswers}:\n<i>{string.Join("\n", player.submittedAnswers)}</i>", player);
        }
        else
            PlayerManager.Get.SendMessageToPlayer($"<color=red>INCORRECT OR REPEAT</color>\nYou've got {player.submittedAnswers.Count}/{GameplayManager.Get.currentConfig.maximumAnswers}:\n<i>{string.Join("\n", player.submittedAnswers)}</i>", player);

        StartCoroutine(DelayInterfaceReturnR4(player));
    }
    IEnumerator DelayInterfaceReturnR4(PlayerObject player)
    {
        yield return new WaitForSeconds(1.25f);
        if (GlobalTimeManager.Get.RoundActive())
        {
            if (player.submittedAnswers.Count >= GameplayManager.Get.currentConfig.maximumAnswers)
            {
                DebugLog.Print($"{(player == null ? "UNKNOWN" : player.playerName)} has found five", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
                PlayerManager.Get.SendMessageToPlayer($"You've found {GameplayManager.Get.currentConfig.maximumAnswers} answers! Good job!\n<i>{string.Join("\n", player.submittedAnswers)}</i>", player);
            }                
            else
                PlayerManager.Get.SendQuestionToPlayer(R4PodiumManager.Get.finalQuestion.question + $"|{(GlobalTimeManager.Get.GetIntTimeRemaining() - 1)}", player);
        }
    }

    public void R4FinalistHandling(PlayerObject player, string response)
    {
        double matchRate = 0;
        double threshold = Operator.Get.spellingThreshold;
        LooseAnswer a = null;

        ///Set spelling threshold
        if (Operator.Get.exactSpellingRequiredSingle)
            threshold = 0.999f;
        else
            threshold = Operator.Get.spellingThreshold;

        foreach (LooseAnswer ans in R4PodiumManager.Get.answers)
            foreach (string valid in ans.validAnswers)

                ///Slightly altered checker
                ///a) Now gets matchrate for every variation but still only stores the highest
                ///b) Debug changes dependent on whether it breaches threshold
                if (Extensions.Spellchecker(response, valid) >= 0 && Extensions.Spellchecker(response, valid) > matchRate)
                    ///Now check that the answer is unrevealed and they player hasn't already submitted the box's main answer
                    if (!ans.revealed)
                    {
                        matchRate = Extensions.Spellchecker(response, valid);
                        a = ans;
                    }

        PlayerManager.Get.SendMessageToPlayer($"You said <u>{response}</u>\nLet's see if it's right...", player);
        R4PodiumManager.Get.OnReceiveResponse(response, a, matchRate >= threshold);
        if (matchRate >= threshold)
            DebugLog.Print($"{response} matches with {a.validAnswers.FirstOrDefault()} [{matchRate.ToString("0.00")}] : CONTINUE?", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
        else
            DebugLog.Print($"{response} does not match with anything. The best unrevealed match was {a.validAnswers.FirstOrDefault()} [{matchRate.ToString("00.00")}] : CONTINUE?", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
    }

    #endregion

    #region General

    public void InstantiatePlayerPopup(PlayerObject player)
    {
        var x = Instantiate(popUpObject, popUpTransformTarget);
        x.GetComponent<PlayerPopUpLerper>().InitPopUp(player);
    }

    #endregion
}
