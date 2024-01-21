using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColumnManager : SingletonMonoBehaviour<ColumnManager>
{
    public Column singleQuestionColumn;
    public List<Column> multiQuestionColumns;
    public Texture incorrectTexture;

    public void BuildSingleColumn(Question questionToBuild)
    {
        StartCoroutine(DelayBuild(questionToBuild));
    }

    IEnumerator DelayBuild(Question questionToBuild)
    {

        yield return new WaitForSeconds(1f);
        singleQuestionColumn.InitColumn(questionToBuild);
        foreach (Column c in multiQuestionColumns)
            c.ClearPreviousColumn();

        //Only print config before first board
        if(GameplayManager.Get.questionsPlayed - GameplayManager.Get.roundConfigs[0].numberOfBoards == 1)
            GameplayManager.Get.currentConfig.PrintRoundConfig();
        DebugLog.Print($"===BOARD #{GameplayManager.Get.questionsPlayed - GameplayManager.Get.roundConfigs[0].numberOfBoards}===", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
    }

    public void BuildMultiColumns(List<Question> questionsToBuild)
    {
        singleQuestionColumn.ClearPreviousColumn();

        if (questionsToBuild.Count > multiQuestionColumns.Count)
        {
            DebugLog.Print($"ERROR: The build can only handle {multiQuestionColumns.Count} columns in a single round. Please check your data/settings and then try again.");
            return;
        }
        for (int i = 0; i < questionsToBuild.Count; i++)
            multiQuestionColumns[i].InitColumn(questionsToBuild[i]);
    }

    [Button]
    public void RevealSingleCategory()
    {
        singleQuestionColumn.RevealCategory();
    }

    [Button]
    public void RevealMultipleCategories()
    {
        foreach (Column c in multiQuestionColumns)
            c.RevealCategory();
    }

    [Button]
    public void RevealAllAnswersMultiple()
    {
        AudioManager.Get.Play(AudioManager.FindAnswerClip.Bwoing);
        AudioManager.Get.Play(AudioManager.OneShotClip.BoardTurn);
        foreach (Column c in multiQuestionColumns)
            foreach (AnswerBox a in c.answerBoxes.Where(x => !x.revealed))
                a.RevealAnswerBox();
    }

    public void TallyR2Points()
    {
        //A more concise way that I THINK works
        foreach (AnswerBox ax in singleQuestionColumn.answerBoxes)
            ax.playersWhoGaveThisAnswer += PlayerManager.Get.players
                .Count(pl => pl.submittedAnswers.FirstOrDefault() == ax.validAnswers.FirstOrDefault());

        List<AnswerBox> orderedBoxes = singleQuestionColumn.answerBoxes.OrderBy(x => x.playersWhoGaveThisAnswer == 0 ? int.MaxValue : x.playersWhoGaveThisAnswer).ToList();
        int currentPoints = GameplayManager.Get.currentConfig.basePoints * orderedBoxes.Count;

        for(int i = 0; i < orderedBoxes.Count(); i++)
        {
            if (orderedBoxes[i].playersWhoGaveThisAnswer == 0)
                orderedBoxes[i].r2PointsBoxIsWorth = 0;

            else if (orderedBoxes[i].r2PointsBoxIsWorth != 0)
                continue;

            else
            {
                int reduction = 0;
                foreach (AnswerBox a in orderedBoxes.Where(x => x.playersWhoGaveThisAnswer == orderedBoxes[i].playersWhoGaveThisAnswer))
                {
                    a.r2PointsBoxIsWorth = currentPoints;
                    reduction++;
                }
                currentPoints -= (GameplayManager.Get.currentConfig.basePoints * reduction);
            }
        }

        foreach (AnswerBox ax in singleQuestionColumn.answerBoxes.OrderBy(x => x.r2PointsBoxIsWorth == 0 ? int.MaxValue : x.r2PointsBoxIsWorth))
            DebugLog.Print($"{ax.playersWhoGaveThisAnswer} said {ax.validAnswers.FirstOrDefault()} for {ax.r2PointsBoxIsWorth} points", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);

        foreach(PlayerObject pl in PlayerManager.Get.players.Where(x => !x.wasCorrect))
            PlayerManager.Get.SendMessageToPlayer($"<color=red>BAD LUCK</color>\nNo points for this board...", pl);
    }

    public void RevealAnswerSingle()
    {
        singleQuestionColumn.answerBoxes.Where(x => !x.revealed).OrderBy(x => x.r2PointsBoxIsWorth == 0 ? int.MaxValue : x.r2PointsBoxIsWorth).FirstOrDefault().RevealR2AnswerBox();
    }

    [Button]
    public void TestBuild()
    {
        BuildMultiColumns(new List<Question>() { QuestionManager.currentPack.questions[0], QuestionManager.currentPack.questions[1], QuestionManager.currentPack.questions[2] });
    }
}
