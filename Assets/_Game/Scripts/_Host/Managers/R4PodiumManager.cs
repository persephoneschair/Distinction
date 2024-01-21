using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class R4PodiumManager : SingletonMonoBehaviour<R4PodiumManager>
{
    public Question finalQuestion;
    public FinalPodium[] podia;

    public Animator questionStrapAnim;
    public TextMeshProUGUI questionMesh;

    public List<LooseAnswer> answers = new List<LooseAnswer>();

    public int currentFocus = 0;
    public bool resendPossible = false;

    public int pointsToDrain = 0;
    public int playersWhoSaidIt = 0;
    public bool correctAnswer;
    public bool distinctAnswer;
    public TextMeshProUGUI distinctionAlert;

    string[] distinctAlertVariations = new string[4] { "<pend>", "<wave>", "<wiggle>", "<rot>" };
    private const string distinctAlert = "<rainb><swing><fade><color=red>D<color=green>I<color=blue>S<color=yellow>T<color=orange>I<color=purple>N<color=white>C<color=red>T<color=green>I<color=blue>O<color=yellow>N<color=orange>!<color=white>";
    private const string regularAlert = "<fade><pend><color=yellow>";
    private const string incorrectAlert = "<color=red><shake><fade>WRONG!";

    private int directionOfPlay = 1;
    

    public void InitForFinal(List<PlayerObject> finalists, Question q)
    {
        finalQuestion = q;
        for(int i = 0; i < (finalists.Count > podia.Length ? podia.Length : finalists.Count); i++)
        {
            finalists[i].finalist = true;
            podia[i].ActivateForFinal(finalists[i]);
        }
        foreach (string s in finalQuestion.answers)
            answers.Add(new LooseAnswer(s));
    }

    public void LaunchQuestion()
    {
        questionMesh.text = finalQuestion.question;
        questionStrapAnim.SetTrigger("toggle");
    }

    public void SetPodiaLights()
    {
        pointsToDrain = 0;
        playersWhoSaidIt = 0;
        correctAnswer = false;
        distinctAnswer = false;
        for (int i = 0; i < podia.Length; i++)
            podia[i].SetPodiumLights(i == currentFocus);
        SendQuestion();
        resendPossible = true;
    }

    public void SendQuestion()
    {
        PlayerManager.Get.SendQuestionToPlayer(finalQuestion.question, podia[currentFocus].containedPlayer);
    }

    public void OnReceiveResponse(string response, LooseAnswer bestFit, bool clearedThreshold)
    {
        resendPossible = false;
        podia[currentFocus].ReceiveAnswer(response);
        correctAnswer = clearedThreshold;
        playersWhoSaidIt = bestFit.playersWhoGaveThisAnswer;
        distinctAnswer = playersWhoSaidIt == 0;
        if (clearedThreshold)
        {
            bestFit.revealed = true;
            pointsToDrain = Math.Min(GameplayManager.Get.currentConfig.finalReductionPerAnswer * bestFit.playersWhoGaveThisAnswer, GameplayManager.Get.currentConfig.finalMaximumLoss);
        }
        else
            pointsToDrain = GameplayManager.Get.currentConfig.finalMaximumLoss;

        if(clearedThreshold)
            DebugLog.Print($"{bestFit.playersWhoGaveThisAnswer} people said {bestFit.validAnswers.FirstOrDefault()}...", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);

        DebugLog.Print($"{pointsToDrain} points will be deducted for {response}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.MarkR4Finalist;
    }

    public void OverrideResponse()
    {
        var bestFit = answers[Operator.Get.answerToCondoneAsIndex];
        correctAnswer = true;
        playersWhoSaidIt = bestFit.playersWhoGaveThisAnswer;
        distinctAnswer = playersWhoSaidIt == 0;
        bestFit.revealed = true;
        pointsToDrain = Math.Min(GameplayManager.Get.currentConfig.finalReductionPerAnswer * bestFit.playersWhoGaveThisAnswer, GameplayManager.Get.currentConfig.finalMaximumLoss);
        DebugLog.Print($"RESPONSE OVERRIDDEN AS {bestFit.validAnswers.FirstOrDefault()}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
        DebugLog.Print($"{pointsToDrain} points will be deducted for this answer ({bestFit.playersWhoGaveThisAnswer} people said it...)", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
    }

    public void RevealResponseResult()
    {
        podia[currentFocus].MarkAnswer(correctAnswer);
        if(!correctAnswer)
        {
            podia[currentFocus].PointDrain(pointsToDrain, correctAnswer, playersWhoSaidIt);
            IncorrectAlert();
            PlayerManager.Get.SendMessageToPlayer($"<color=red>BAD LUCK</color>\nThat was a wrong answer and you lose {pointsToDrain} points", podia[currentFocus].containedPlayer);
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.UnlockForR4Finalist;
            SwitchFocusPlayer();
        }
        else
        {
            AudioManager.Get.Play(AudioManager.OneShotClip.Correct);
            PlayerManager.Get.SendMessageToPlayer($"<color=green>CORRECT</color>\nThat was a correct answer. Let's see how many people said it...", podia[currentFocus].containedPlayer);
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.RevealR4Points;
        }
    }

    public void RevealResponsePointDeduction()
    {
        if(distinctAnswer)
        {
            podia[currentFocus].SetDistinct();
            DistinctionAlert();
            PlayerManager.Get.SendMessageToPlayer($"<color=green>CORRECT</color>\nThat was a <color=red>D<color=green>I<color=blue>S<color=yellow>T<color=orange>I<color=purple>N<color=white>C<color=red>T<color=white> answer so you keep your points", podia[currentFocus].containedPlayer);
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.UnlockForR4Finalist;
            SwitchFocusPlayer();
        }
        else
            podia[currentFocus].PointDrain(pointsToDrain, correctAnswer, playersWhoSaidIt);
    }

    [Button]
    public void DistinctionAlert()
    {
        AudioManager.Get.Play(AudioManager.FindAnswerClip.Wow);
        string disp = "";
        for (int i = 0; i < distinctAlertVariations.Length; i++)
            disp += UnityEngine.Random.Range(0, 2) == 0 ? distinctAlertVariations[i] : "";
        disp += distinctAlert;
        distinctionAlert.text = disp;
    }

    [Button]
    public void RegularAlert()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.PointDrain);
        distinctionAlert.text = regularAlert + $"{playersWhoSaidIt} SAID IT\n-{pointsToDrain} POINTS";
    }

    [Button]
    public void IncorrectAlert()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.WrongAnswerReveal);
        distinctionAlert.text = incorrectAlert;
    }

    public void SwitchFocusPlayer()
    {
        currentFocus += directionOfPlay;
        if (currentFocus == podia.Length)
        {
            directionOfPlay = -1;
            currentFocus += directionOfPlay;
        }
        else if (currentFocus == -1)
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.EndOfR4WrapUp;
    }

    public void EndOfR4WrapUp()
    {
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.LoopClip.EndOfRound, false);
        DebugLog.Print("If anybody asks, count back goes R4 => Total (4+3+2+1) => R3 => R2 => R1", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Yellow);
        var plF = PlayerManager.Get.players.Where(x => x.finalist);

        //Finalise scores
        foreach (PlayerObject pl in plF)
        {
            pl.r4Points = pl.points;
            pl.points += pl.r1Points + pl.r2Points + pl.r3Points;
        }
        List<PlayerObject> orderedFinalists = plF
            .OrderByDescending(x => x.r4Points)
            .ThenByDescending(x => x.points)
            .ThenByDescending(x => x.r3Points)
            .ThenByDescending(x => x.r2Points)
            .ThenByDescending(x => x.r1Points)
            .ToList();

        //Apply medal status to finalists and set winner lights on and runner-up lights off
        for(int i = 0; i < plF.Count(); i++)
        {
            orderedFinalists[i].medalStatus = i + 1;
            podia.FirstOrDefault(x => x.containedPlayer == orderedFinalists[i]).SetPodiumLights(i == 0);
        }
        PlayerManager.Get.SendMessageToPlayer($"<color=green>CONGRATULATIONS</color>\nYou've won the game", orderedFinalists.FirstOrDefault());

        //Display winner strap on screen
        distinctionAlert.text = "<rainb><swing><fade>" + orderedFinalists.FirstOrDefault().playerName + " IS THE WINNER!";

        //"Unreveal" all loose answers, ready for showcasing
        foreach (LooseAnswer a in answers)
            a.revealed = false;

        //Calculate points for loose answers
        //THIS BIT SHOULDN'T BE NEEDED HERE, SINCE SUBMISSIONS ARE TRACKED IN REALTIME (UNLIKE R2)
        /*foreach (LooseAnswer ax in answers)
            ax.playersWhoGaveThisAnswer += PlayerManager.Get.players
                .Where(x => !x.finalist).Count(pl => pl.submittedAnswers.FirstOrDefault() == ax.validAnswers.FirstOrDefault());*/

        List<LooseAnswer> orderedAnswers = answers.OrderBy(x => x.playersWhoGaveThisAnswer == 0 ? int.MaxValue : x.playersWhoGaveThisAnswer).ToList();
        int currentPoints = GameplayManager.Get.currentConfig.basePoints * orderedAnswers.Count;

        for (int i = 0; i < orderedAnswers.Count(); i++)
        {
            if (orderedAnswers[i].playersWhoGaveThisAnswer == 0)
                orderedAnswers[i].pointsBoxIsWorth = 0;

            else if (orderedAnswers[i].pointsBoxIsWorth != 0)
                continue;

            else
            {
                int reduction = 0;
                foreach (LooseAnswer a in orderedAnswers.Where(x => x.playersWhoGaveThisAnswer == orderedAnswers[i].playersWhoGaveThisAnswer))
                {
                    a.pointsBoxIsWorth = currentPoints;
                    reduction++;
                }
                currentPoints -= (GameplayManager.Get.currentConfig.basePoints * reduction);
            }
        }
    }

    public void RevealAnswer()
    {
        List<LooseAnswer> ans = answers.Where(x => !x.revealed).OrderByDescending(x => x.playersWhoGaveThisAnswer).ThenBy(x => x.validAnswers.FirstOrDefault()).ToList();

        AudioManager.Get.Play((AudioManager.FindAnswerClip)UnityEngine.Random.Range(11, 15));
        var rev = ans.FirstOrDefault();
        rev.revealed = true;

        //Get non-finalists who said answer to be revealed
        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => !x.finalist && x.submittedAnswers.Contains(rev.validAnswers.FirstOrDefault())))
        {
            pl.points += rev.pointsBoxIsWorth;
            PlayerManager.Get.UpdatePlayerScore(pl);
            PlayerManager.Get.SendMessageToPlayer($"<color=green><i>{rev.validAnswers.FirstOrDefault()}</i></color> earned you {rev.pointsBoxIsWorth} points", pl);
            StrapManager.Get.r4PlayerPrefabs.FirstOrDefault(x => x.containedPlayer == pl).OnSetHighlighted();
        }

        questionMesh.text = finalQuestion.question + $"\n<size=150%>{rev.playersWhoGaveThisAnswer} said <color=yellow><u>{rev.validAnswers.FirstOrDefault()}</u></color> [{rev.pointsBoxIsWorth} pts]";

        //This is the last answer
        if (ans.Count == 1)
        {
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.DoNothing;
            Invoke("InvokeClearStrap", 4f);
        }            
    }

    private void InvokeClearStrap()
    {
        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => x.finalist))
            PlayerManager.Get.UpdatePlayerScore(pl);

        AudioManager.Get.Play(AudioManager.OneShotClip.FlyIn);
        questionStrapAnim.SetTrigger("toggle");
        StrapManager.Get.AnimationToggle();
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.EndRound;
    }
}
