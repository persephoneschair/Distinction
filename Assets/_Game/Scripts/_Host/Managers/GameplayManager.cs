using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;
using System.Linq;
using Control;

public class GameplayManager : SingletonMonoBehaviour<GameplayManager>
{
    public enum GameplayStage
    {
        RunTitles,
        OpenLobby,
        LockLobby,
        LoadNextRound,

        StartR1AndR3Boards,
        RevealMissingAnswers,

        StartR2Board,
        RevealR2WrongAnswers,
        CalculateR2Points,
        RevealR2CorrectAnswer,
        LoadNewR2Board,

        StartR4Board,
        UnlockForR4Finalist,
        MarkR4Finalist,
        RevealR4Points,
        EndOfR4WrapUp,
        RevealR4Answers,

        EndRound,
        DisplayScores,

        RollCredits,
        DoNothing
    };
    public GameplayStage currentStage = GameplayStage.DoNothing;

    public enum Round
    {
        Round1,
        Round2,
        Round3,
        Round4,
        None
    };

    public Round currentRound = Round.None;
    public int questionsPlayed = 0;

    public List<RoundConfig> roundConfigs = new List<RoundConfig>();
    public RoundConfig currentConfig;

    [Button]
    public void ProgressGameplay()
    {
        switch (currentStage)
        {
            case GameplayStage.RunTitles:
                currentStage = GameplayStage.DoNothing;
                TitlesManager.Get.RunTitleSequence();
                //If in recovery mode, we need to call Restore Players to restore specific player data (client end should be handled by the reload host call)
                //Also need to call Restore gameplay state to bring us back to where we need to be (skipping titles along the way)
                //Reveal instructions would probably be a sensible place to go to, though check that doesn't iterate any game state data itself
                break;

            case GameplayStage.OpenLobby:
                LobbyManager.Get.OnOpenLobby();
                currentStage++;
                break;

            case GameplayStage.LockLobby:
                LobbyManager.Get.OnLockLobby();
                break;

            case GameplayStage.LoadNextRound:
                switch(currentRound)
                {
                    case Round.Round1:
                        //Round 1 boards and config are built/applied at runtime so as to be present during the opening camera pan
                        //Other rounds are built upon load
                        DebugLog.Print("===ROUND 1===", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
                        currentConfig.PrintRoundConfig();
                        AudioManager.Get.StopLoop();
                        AudioManager.Get.Play(AudioManager.LoopClip.EndOfRound, false);
                        CameraManager.Get.TransitionCam(CameraManager.CameraAngle.MultiBox);
                        currentStage++;
                        break;

                    case Round.Round2:
                        DebugLog.Print("===ROUND 2===", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
                        //Note the round config is printed on a delay inside the Build function for...transition reasons I think?
                        ColumnManager.Get.BuildSingleColumn(QuestionManager.GetQuestion(questionsPlayed));
                        CameraManager.Get.TransitionCam(CameraManager.CameraAngle.SingleBox);
                        AudioManager.Get.StopLoop();
                        AudioManager.Get.Play(AudioManager.LoopClip.EndOfRound, false);
                        currentStage = GameplayStage.StartR2Board;
                        break;

                    case Round.Round3:
                        List<Question> nextQs = new List<Question>();
                        for (int i = 0; i < currentConfig.numberOfBoards; i++)
                            nextQs.Add(QuestionManager.GetQuestion(questionsPlayed));

                        DebugLog.Print("===ROUND 3===", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
                        currentConfig.PrintRoundConfig();
                        AudioManager.Get.StopLoop();
                        AudioManager.Get.Play(AudioManager.LoopClip.EndOfRound, false);
                        ColumnManager.Get.BuildMultiColumns(nextQs);
                        CameraManager.Get.TransitionCam(CameraManager.CameraAngle.MultiBox);
                        currentStage++;
                        break;

                    case Round.Round4:
                        DebugLog.Print("===ROUND 4===", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
                        currentConfig.PrintRoundConfig();
                        R4PodiumManager.Get.InitForFinal(ScoreboardManager.Get.GetOrderedPlayers().Take(PlayerManager.Get.players.Count >= 3 ? 3 : PlayerManager.Get.players.Count).ToList(), QuestionManager.GetQuestion(questionsPlayed));
                        AudioManager.Get.StopLoop();
                        AudioManager.Get.Play(AudioManager.LoopClip.EndOfRound, false);
                        CameraManager.Get.TransitionCam(CameraManager.CameraAngle.Final);
                        currentStage = GameplayStage.StartR4Board;
                        break;

                }
                //Audio sting
                break;

            ///---///

            case GameplayStage.StartR1AndR3Boards:
                ColumnManager.Get.RevealMultipleCategories();
                GlobalTimeManager.Get.StartTheClock(currentConfig.roundTime);
                AudioManager.Get.Play(AudioManager.OneShotClip.Wheee);
                AudioManager.Get.Play(AudioManager.LoopClip.R1R3R4, false, 5.5f);
                currentStage = GameplayStage.DoNothing;
                PlayerManager.Get.SendQuestionToAllPlayers(QuestionManager.GetMultiQuestionString() + $"|{(currentConfig.roundTime - 1).ToString()}", 5);
                break;

            case GameplayStage.RevealMissingAnswers:
                ColumnManager.Get.RevealAllAnswersMultiple();
                //SFX
                currentStage = GameplayStage.EndRound;
                break;

            ///---///

            case GameplayStage.StartR2Board:
                ColumnManager.Get.RevealSingleCategory();
                GlobalTimeManager.Get.StartTheClock(currentConfig.roundTime);
                AudioManager.Get.Play(AudioManager.OneShotClip.Wheee);
                AudioManager.Get.Play(AudioManager.LoopClip.R2, false, 2.75f);
                currentStage = GameplayStage.DoNothing;
                StrapManager.Get.InstanceR2Straps();
                PlayerManager.Get.SendQuestionToAllPlayers("<u>GIVE ONE ANSWER ONLY</u>\n" + QuestionManager.GetSingleQuestionString() + $"|{(currentConfig.roundTime - 1).ToString()}", 3);
                break;

            case GameplayStage.RevealR2WrongAnswers:
                DebugLog.Print("CHECK ANSWERS BELOW BEFORE PROGRESSING", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
                StrapManager.Get.RevealIncorrectPlayers();
                DebugLog.Print("PROGRESS GAMEPLAY TO CALCULATE SCORES", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
                currentStage++;
                break;

            case GameplayStage.CalculateR2Points:
                ColumnManager.Get.TallyR2Points();
                currentStage++;
                break;

            case GameplayStage.RevealR2CorrectAnswer:
                StrapManager.Get.FocusPlayer = null;
                ColumnManager.Get.RevealAnswerSingle();
                currentStage = ColumnManager.Get.singleQuestionColumn.answerBoxes.Count(x => x.revealed) == ColumnManager.Get.singleQuestionColumn.answerBoxes.Count() ? GameplayStage.LoadNewR2Board : GameplayStage.RevealR2CorrectAnswer;
                break;

            case GameplayStage.LoadNewR2Board:
                PlayerManager.Get.ResetPlayerVariablesAndLogRoundScore(false);
                StrapManager.Get.ClearStraps();
                DebugLog.Print($"===END OF BOARD #{questionsPlayed - roundConfigs[0].numberOfBoards}===", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
                if (questionsPlayed >= roundConfigs[0].numberOfBoards + roundConfigs[1].numberOfBoards)
                {
                    currentStage = GameplayStage.EndRound;
                    ProgressGameplay();
                    return;
                }
                else
                {
                    CameraManager.Get.TransitionCam(CameraManager.CameraAngle.SingleBox);
                    ColumnManager.Get.BuildSingleColumn(QuestionManager.GetQuestion(questionsPlayed));
                    currentStage = GameplayStage.StartR2Board;
                }
                break;

            ///---///

            case GameplayStage.StartR4Board:
                R4PodiumManager.Get.LaunchQuestion();
                GlobalTimeManager.Get.StartTheClock(currentConfig.roundTime);
                AudioManager.Get.Play(AudioManager.FindAnswerClip.Boing);
                AudioManager.Get.Play(AudioManager.LoopClip.R1R3R4, false, 5.5f);
                currentStage = GameplayStage.DoNothing;
                StrapManager.Get.InstanceR4Straps();
                PlayerManager.Get.SendQuestionToAllNonFinalists(R4PodiumManager.Get.finalQuestion.question + $"|{(currentConfig.roundTime - 1).ToString()}", 5);
                break;

            case GameplayStage.UnlockForR4Finalist:
                AudioManager.Get.Play(AudioManager.OneShotClip.Correct);
                if (!AudioManager.Get.loopingSource.isPlaying)
                    AudioManager.Get.Play(AudioManager.LoopClip.Lobby, true);
                R4PodiumManager.Get.SetPodiaLights();
                currentStage = GameplayStage.DoNothing;
                break;

            case GameplayStage.MarkR4Finalist:
                R4PodiumManager.Get.RevealResponseResult();
                break;

            case GameplayStage.RevealR4Points:
                currentStage = GameplayStage.DoNothing;
                R4PodiumManager.Get.RevealResponsePointDeduction();
                break;

            case GameplayStage.EndOfR4WrapUp:
                R4PodiumManager.Get.EndOfR4WrapUp();
                currentStage++;
                break;

            case GameplayStage.RevealR4Answers:
                R4PodiumManager.Get.RevealAnswer();
                break;

            ///---///

            case GameplayStage.EndRound:
                CameraManager.Get.TransitionCam(CameraManager.CameraAngle.Scores);
                PlayerManager.Get.ResetPlayerVariablesAndLogRoundScore();
                currentStage++;
                break;

            case GameplayStage.DisplayScores:
                ScoreboardManager.Get.DisplayScoreboard();
                currentStage = GameplayStage.DoNothing;
                break;

            ///---///

            case GameplayStage.RollCredits:
                currentStage = GameplayStage.DoNothing;
                CameraLerpManager.Get.ForceSwitchToFloating(true);
                GameplayPennys.Get.UpdatePennysAndMedals();
                CreditsManager.Get.RollCredits();
                break;

            case GameplayStage.DoNothing:
                break;
        }
    }

    public void OnTimeUp()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.TimeUp);
        StartCoroutine(TimeUpRoutine());
    }

    IEnumerator TimeUpRoutine()
    {
        yield return new WaitForSeconds(2f);
        switch(currentRound)
        {
            case Round.Round1:
            case Round.Round3:
                PlayerManager.Get.SendMessageToAllPlayers("TIME UP!\nThat's the end of the round");
                currentStage = GameplayStage.RevealMissingAnswers;
                break;

            case Round.Round2:
                PlayerManager.Get.SendMessageToAllPlayers("TIME UP!\nThat's the end of the board");
                currentStage = GameplayStage.RevealR2WrongAnswers;
                break;

            case Round.Round4:
                PlayerManager.Get.SendMessageToAllPlayers("TIME UP!\nThat's the end of the board");
                Invoke("FinalRoundDebug", 2f);
                break;
        }
    }

    public void FinalRoundDebug()
    {
        foreach (R4PlayerPrefab pr in StrapManager.Get.r4PlayerPrefabs)
            pr.OnSetCompleted();

        DebugLog.Print($"A TOTAL OF {R4PodiumManager.Get.answers.Sum(x => x.playersWhoGaveThisAnswer)} ANSWERS HAVE BEEN PLAYED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);

        currentStage = GameplayStage.UnlockForR4Finalist;
        foreach (LooseAnswer la in R4PodiumManager.Get.answers.OrderByDescending(x => x.playersWhoGaveThisAnswer).ThenBy(x => x.validAnswers.FirstOrDefault()))
            DebugLog.Print($"{la.validAnswers.FirstOrDefault()} [{la.playersWhoGaveThisAnswer}]", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);

        DebugLog.Print($"THERE ARE {R4PodiumManager.Get.answers.Count(x => x.playersWhoGaveThisAnswer == 0)} DISTINCT ANSWERS", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
    }

    public void OnScoreboardFinished()
    {
        DebugLog.Print($"===END OF ROUND {(int)currentRound + 1}===", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
        AudioManager.Get.Play(AudioManager.LoopClip.EndOfRound, false, 0f);
        CameraManager.Get.TransitionCam(CameraManager.CameraAngle.Audience);
        currentRound++;

        if (currentRound == Round.None)
        {
            string[] med = new string[5] { "", "gold", "silver", "bronze", "lobby" };
            PlayerManager.Get.players
                .Where(x => !x.finalist)
                .OrderByDescending(x => x.points)
                .ThenByDescending(x => x.r4Points)
                .ThenByDescending(x => x.r3Points)
                .ThenByDescending(x => x.r2Points)
                .ThenByDescending(x => x.r1Points)
                .ThenBy(x => x.playerName)
                .FirstOrDefault().medalStatus = 4;

            foreach(PlayerObject pl in PlayerManager.Get.players)
            {
                string addendum = (pl.medalStatus != 0 ? ($"\nYou also won the {med[pl.medalStatus]} medal") : "");
                pl.pennysWon = pl.points * GameplayPennys.Get.multiplyFactor;
                if (pl.pennysWon < 0)
                    pl.pennysWon = 0;
                PlayerManager.Get.SendMessageToPlayer($"GAME OVER\nYou earned {pl.pennysWon} Pennys this game{addendum}", pl);
                switch(pl.medalStatus)
                {
                    case 1:
                        DebugLog.Print($"{pl.playerName} won the GOLD medal", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
                        break;

                    case 2:
                        DebugLog.Print($"{pl.playerName} won the SILVER medal", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
                        break;

                    case 3:
                        DebugLog.Print($"{pl.playerName} won the BRONZE medal", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
                        break;

                    case 4:
                        DebugLog.Print($"{pl.playerName} won the LOBBY medal", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
                        break;
                }
            }
            
            currentConfig = null;
            currentStage = GameplayStage.RollCredits;
        }

        else
        {
            PlayerManager.Get.SendMessageToAllPlayers($"END OF ROUND {(int)currentRound}");
            currentConfig = roundConfigs[(int)currentRound];
            currentStage = GameplayStage.LoadNextRound;
        }
    }
}
