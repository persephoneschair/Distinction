using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Column : MonoBehaviour
{
    public Question containedQuestion;
    public CategoryBox categoryBox;
    public List<AnswerBox> answerBoxes = new List<AnswerBox>();

    public GameObject answerBoxToInstance;
    public GameObject categoryBoxToInstance;
    public Transform instanceTransformTarget;

    public void InitColumn(Question containedQuestion)
    {
        ClearPreviousColumn();

        this.containedQuestion = containedQuestion;
        int index = containedQuestion.answers.Count;
        List<int> bonusIndices = GenerateBonusIndices(GameplayManager.Get.currentConfig.bonusesPerBoard);
        List<string> reversedAnswers = ReverseList(containedQuestion.answers);

        foreach(string s in reversedAnswers)
        {
            var x = Instantiate(answerBoxToInstance, instanceTransformTarget);
            var y = x.GetComponent<AnswerBox>();
            y.InitAnswerBox(s, index, bonusIndices.Contains(index));
            index--;
            answerBoxes.Add(y);
        }

        var a = Instantiate(categoryBoxToInstance, instanceTransformTarget);
        var b = a.GetComponent<CategoryBox>();
        b.InitCatBox(containedQuestion.question);
        categoryBox = b;
    }

    [Button]
    public void RevealCategory()
    {
        if(categoryBox != null)
        {
            categoryBox.RevealCatBox();
            DebugLog.Print(containedQuestion.question, DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
            if (GameplayManager.Get.currentRound == GameplayManager.Round.Round2)
                foreach (AnswerBox a in answerBoxes)
                    DebugLog.Print(a.validAnswers.FirstOrDefault(), DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);
        }            
    }

    public void ClearPreviousColumn()
    {
        containedQuestion = null;

        if(categoryBox != null)
            Destroy(categoryBox.gameObject);
        categoryBox = null;

        foreach (AnswerBox a in answerBoxes)
            if(a != null)
                Destroy(a.gameObject);

        answerBoxes.Clear();
        answerBoxes = new List<AnswerBox>();
    }

    List<int> GenerateBonusIndices(int n)
    {
        if (n > containedQuestion.answers.Count)
            n = containedQuestion.answers.Count;

        System.Random random = new System.Random();
        HashSet<int> uniqueInts = new HashSet<int>();

        while (uniqueInts.Count < n)
        {
            int randomInt = random.Next(1, 11);
            uniqueInts.Add(randomInt);
        }

        return new List<int>(uniqueInts);
    }

    List<string> ReverseList(List<string> originalList)
    {
        List<string> reversedList = new List<string>(originalList);
        reversedList.Reverse();
        return reversedList;
    }
}
