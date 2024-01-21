using Control;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class R2PlayerPrefab : MonoBehaviour
{
    public PlayerObject containedPlayer;
    public TextMeshProUGUI textMesh;
    public Animator anim;
    public Image background;
    public Color[] backgroundColors;
    //0 = Standby (blue)
    //1 = Answer in (orange)
    //2 = Correct (dark green)
    //3 = Current Answer (bright green)
    //4 = Incorrect (Red)

    public void Init(PlayerObject pl)
    {
        containedPlayer = pl;
        textMesh.text = pl.playerName;
        background.color = backgroundColors[0];
    }

    public void Hit()
    {
        anim.SetTrigger("hit");
        background.color = backgroundColors[1];
    }

    public void OnPressButton()
    {
        if (GameplayManager.Get.currentStage == GameplayManager.GameplayStage.CalculateR2Points && !containedPlayer.wasCorrect || string.IsNullOrEmpty(containedPlayer.submittedAnswers.FirstOrDefault()))
            StrapManager.Get.FocusPlayer = containedPlayer;
    }

    public void OnSetWrong()
    {
        AudioManager.Get.PlayUnique(AudioManager.OneShotClip.WrongAnswerReveal);
        anim.SetTrigger("hit");
        background.color = backgroundColors[4];
        textMesh.text = $"{containedPlayer.playerName} - {(containedPlayer.submittedAnswers.Count == 0 ? "NO ANSWER" : containedPlayer.submittedAnswers.FirstOrDefault())} [0]";
    }

    public void OnSetHighlighted(int points)
    {
        anim.SetTrigger("hit");
        background.color = backgroundColors[3];
        textMesh.text = $"{containedPlayer.playerName} - {containedPlayer.submittedAnswers.FirstOrDefault()} [{points}]";
        Invoke("OnSetCorrect", 4f);
    }

    public void OnSetCorrect()
    {
        background.color = backgroundColors[2];
    }

    public void OnCondone(int answerCondoneIndex)
    {
        containedPlayer.wasCorrect = true;
        anim.SetTrigger("hit");
        background.color = backgroundColors[1];
        textMesh.text = containedPlayer.playerName;

        var ax = ColumnManager.Get.singleQuestionColumn.answerBoxes[answerCondoneIndex];
        string response = string.IsNullOrEmpty(containedPlayer.submittedAnswers.FirstOrDefault()) ? "NO ANSWER" : containedPlayer.submittedAnswers.FirstOrDefault();
        containedPlayer.submittedAnswers.Clear();
        containedPlayer.submittedAnswers.Add(ax.validAnswers.FirstOrDefault());
        PlayerManager.Get.SendMessageToPlayer($"<color=green>CORRECT</color>\nYour answer <i>({response})</i> matched with <color=green><u>{ax.validAnswers.FirstOrDefault()}</u></color> which is on the list. Let's see how many points it's worth...", containedPlayer);

        StrapManager.Get.FocusPlayer = null;
    }
}
