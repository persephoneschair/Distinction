using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using NaughtyAttributes;
using System.Linq;
using UnityEngine.Rendering;

public class Operator : SingletonMonoBehaviour<Operator>
{
    [Header("Game Settings")]
    [Tooltip("Supresses Twitch chat messages and will store Pennys and medals in a separate test file")]
    public bool testMode;
    [Tooltip("Skips opening titles")]
    public bool skipOpeningTitles;
    [Tooltip("Players must join the room with valid Twitch username as their name; this will skip the process of validation")]
    public bool fastValidation;
    [Tooltip("Start the game in recovery mode to restore any saved data from a previous game crash")]
    public bool recoveryMode;
    [Tooltip("Limits the number of accounts that may connect to the room (set to 0 for infinite)")]
    [Range(0, 100)] public int playerLimit;

    [Header("Quesion Data")]
    public TextAsset questionPack;

    [Header("Round Configs")]
    [Range(1, 5)] public int[] boards = new int[4] { 3, 3, 3, 1 };
    [Range(0, 5)] public int[] bonusesPerBoard = new int[4] { 1, 0, 1, 0 };
    public bool[] firstAnswerOnly = new bool[4] { false, false, false, false };
    public float[] secondaryAnswerDelay = new float[4] { 5f, 0f, 5f, 0f };
    [Range(1, 6)] public int[] maximumAnswers = new int[4] { 3, 1, 3, 5 };
    public float[] roundTime = new float[4] { 60f, 30f, 60f, 30f };
    public int[] basePoints = new int[4] { 20, 10, 40, 2 };
    public int[] pointsForSecondary = new int[4] { 10, 0, 20, 0 };
    public int[] pointsForBonus = new int[4] { 30, 0, 60, 0 };
    public int[] finalReductionPerAnswer = new int[4] { 0, 0, 0, 5 };
    public int[] finalMaximumLoss = new int[4] { 0, 0, 0, 100 };

    [Header("Board Configs")]
    public bool[] exactSpellingRequiredMulti = new bool[3] { false, false, false };
    public bool exactSpellingRequiredSingle = false;
    public double spellingThreshold = 0.81f;

    [Header("Round Two/Four Marking")]
    [ShowOnly] public string focusPlayer = "NO PLAYER IN FOCUS";
    [Range(0, 50)] public int answerToCondoneAsIndex = 0;
    [ShowOnly] public string answerToCondoneAs = "NO ANSWER SELECTED";

    [Header("Debug")]
    [Range(1, 3)] public int roundToForceTo = 1;

    public void Update()
    {
        if (GameplayManager.Get.currentStage == GameplayManager.GameplayStage.CalculateR2Points)
            R2Marker();
        else if (GameplayManager.Get.currentStage == GameplayManager.GameplayStage.MarkR4Finalist)
            R4Marker();
        else
            answerToCondoneAs = "NO ANSWER SELECTED";
    }

    public void R2Marker()
    {
        if (answerToCondoneAsIndex > ColumnManager.Get.singleQuestionColumn.answerBoxes.Count - 1)
            answerToCondoneAs = "OUT OF RANGE";
        else
            answerToCondoneAs = ColumnManager.Get.singleQuestionColumn.answerBoxes[answerToCondoneAsIndex].validAnswers.FirstOrDefault();
    }
    public void R4Marker()
    {
        if (answerToCondoneAsIndex > R4PodiumManager.Get.answers.Count - 1)
            answerToCondoneAs = "OUT OF RANGE";
        else
            answerToCondoneAs = R4PodiumManager.Get.answers[answerToCondoneAsIndex].validAnswers.FirstOrDefault();
    }

    public override void Awake()
    {
        base.Awake();
        if (recoveryMode)
            skipOpeningTitles = true;
    }

    private void Start()
    {
        if(recoveryMode)
            skipOpeningTitles = true;

        //Setup config files
        for(int i = 0; i < 4; i++)
        {
            //Fix R1 & R3 to three boards
            //Could lead to weirdness with round counts, but will prevent crashing by trying to build > 3 boards
            if ((i == 0 || i == 2) && boards[i] > ColumnManager.Get.multiQuestionColumns.Count)
                boards[i] = ColumnManager.Get.multiQuestionColumns.Count;

            //Set delay to 0 if we're only taking first answers (R1 & R3)
            if (firstAnswerOnly[i])
                secondaryAnswerDelay[i] = 0f;

            GameplayManager.Get.roundConfigs.Add(new RoundConfig(
                boards[i],
                bonusesPerBoard[i],
                firstAnswerOnly[i],
                secondaryAnswerDelay[i],
                maximumAnswers[i],
                roundTime[i],
                basePoints[i],
                pointsForSecondary[i],
                pointsForBonus[i],
                finalReductionPerAnswer[i],
                finalMaximumLoss[i]));
        }

        GameplayManager.Get.currentConfig = GameplayManager.Get.roundConfigs.FirstOrDefault();

        HostManager.Get.host.ReloadHost = recoveryMode;
        if (recoveryMode)
            SaveManager.RestoreData();

        if (questionPack != null)
            QuestionManager.DecompilePack(questionPack);
        else
            DebugLog.Print("NO QUESTION PACK LOADED; PLEASE ASSIGN ONE AND RESTART THE BUILD", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);

        DataStorage.CreateDataPath();
        GameplayEvent.Log("Game initiated");
        EventLogger.PrintLog();
    }

    [Button]
    public void ProgressGameplay()
    {
        if (questionPack != null)
            GameplayManager.Get.ProgressGameplay();
    }

    public void Save()
    {
        SaveManager.BackUpData();
    }

    [Button]
    public void CondoneR2Answer()
    {
        if (GameplayManager.Get.currentStage == GameplayManager.GameplayStage.CalculateR2Points)
            StrapManager.Get.CondoneAnAnswer();
    }

    [Button]
    public void ResendQuestionToCurrentFinalist()
    {
        if (!R4PodiumManager.Get.resendPossible)
            return;
        R4PodiumManager.Get.SendQuestion();
    }

    [Button]
    public void CondoneR4Answer()
    {
        if (GameplayManager.Get.currentStage == GameplayManager.GameplayStage.MarkR4Finalist)
            R4PodiumManager.Get.OverrideResponse();
    }

    [Button]
    public void ForceToRoundX()
    {
        if(GameplayManager.Get.currentStage == GameplayManager.GameplayStage.LoadNextRound || GameplayManager.Get.currentStage == GameplayManager.GameplayStage.LockLobby)
        {
            GameplayManager.Get.currentConfig = GameplayManager.Get.roundConfigs[roundToForceTo];
            GameplayManager.Get.currentRound = (GameplayManager.Round)roundToForceTo;
            switch(roundToForceTo)
            {
                case 1:
                    GameplayManager.Get.questionsPlayed = 3;
                    break;

                case 2:
                    GameplayManager.Get.questionsPlayed = 6;
                    break;

                case 3:
                    GameplayManager.Get.questionsPlayed = 9;
                    break;
            }
            DebugLog.Print($"NEXT ROUND WILL BE ROUND {roundToForceTo + 1}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
        }
    }
}
