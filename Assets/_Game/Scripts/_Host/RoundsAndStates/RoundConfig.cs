using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundConfig
{
    public RoundConfig(int boards, int bonuses, bool firstAnswer, float secondaryDelay, int max, float time, int first, int second, int bonus, int finalReduction, int finalMaximum)
    {
        numberOfBoards = boards;
        bonusesPerBoard = bonuses;
        firstAnswerOnly = firstAnswer;
        secondaryAnswerDelay = secondaryDelay;
        maximumAnswers = max;
        roundTime = time;
        basePoints = first;
        pointsForSecondary = second;
        pointsForBonus = bonus;
        finalReductionPerAnswer = finalReduction;
        finalMaximumLoss = finalMaximum;
    }

    public int numberOfBoards { get; set; }
    public int bonusesPerBoard { get; set; } = 1;
    public bool firstAnswerOnly { get; set; } = false;
    public float secondaryAnswerDelay { get; set; } = 0f;
    public int maximumAnswers { get; set; } = 3;
    public float roundTime { get; set; } = 60f;
    public int basePoints { get; set; } = 20;
    public int pointsForSecondary { get; set; } = 10;
    public int pointsForBonus { get; set; } = 30;
    public int finalReductionPerAnswer { get; set; } = 5;
    public int finalMaximumLoss { get; set; } = 100;


    public void PrintRoundConfig()
    {
        DebugLog.Print($"{numberOfBoards} boards this round", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Yellow);
        
        if(GameplayManager.Get.currentRound == GameplayManager.Round.Round1 || GameplayManager.Get.currentRound == GameplayManager.Round.Round3)
            DebugLog.Print($"First answerer scores {basePoints} points", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Yellow);
        else if(GameplayManager.Get.currentRound == GameplayManager.Round.Round2)
            DebugLog.Print($"Most unique answer scores {basePoints * ColumnManager.Get.singleQuestionColumn.containedQuestion.answers.Count} points, with subsequent answers scoring {basePoints} points fewer", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Yellow);

        if (!firstAnswerOnly)
            DebugLog.Print($"Subsequent answerers score {pointsForSecondary} points if submitted within {secondaryAnswerDelay} seconds of first answerer", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Yellow);

        if (bonusesPerBoard > 0)
            DebugLog.Print($"{bonusesPerBoard} bonuses per board, worth {pointsForBonus} points", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Yellow);

        DebugLog.Print($"You may give {maximumAnswers} answer(s) in total and have {roundTime} seconds", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Yellow);
    }
}
