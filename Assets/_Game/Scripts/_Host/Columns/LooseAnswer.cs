using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LooseAnswer
{
    public LooseAnswer(string s)
    {
        validAnswers = s.Split(',').ToList();
    }

    public List<string> validAnswers = new List<string>();

    public bool flaggedForReveal = false;
    public bool revealed = false;

    public int playersWhoGaveThisAnswer = 0;
    public int pointsBoxIsWorth = 0;
}
