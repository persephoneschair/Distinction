using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using UnityEngine.PlayerLoop;
using System.Linq;

public static class QuestionManager
{
    public static Pack currentPack = null;

    public static void DecompilePack(TextAsset tx)
    {
        currentPack = JsonConvert.DeserializeObject<Pack>(tx.text);
        BuildInitialBoards();
    }

    public static void BuildInitialBoards()
    {
        List<Question> firstRound = new List<Question>();
        for (int i = 0; i < GameplayManager.Get.currentConfig.numberOfBoards; i++)
            firstRound.Add(GetQuestion(i));
        ColumnManager.Get.BuildMultiColumns(firstRound);
        GameplayManager.Get.questionsPlayed = firstRound.Count;
    }

    public static string GetSingleQuestionString()
    {
        return ColumnManager.Get.singleQuestionColumn.containedQuestion.question;
    }

    public static string GetMultiQuestionString()
    {
        return string.Join("\n---\n", ColumnManager.Get.multiQuestionColumns.Where(x => x.containedQuestion != null).Select(x => x.containedQuestion.question));
    }

    public static int GetRoundQCount()
    {
        switch (GameplayManager.Get.currentRound)
        {
            default:
                return 0;
        }
    }

    public static Question GetQuestion(int qNum)
    {
        GameplayManager.Get.questionsPlayed++;
        return currentPack.questions[qNum];
    }
}
