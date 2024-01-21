using Control;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class R4PlayerPrefab : MonoBehaviour
{
    public PlayerObject containedPlayer;
    public TextMeshProUGUI nameMesh;
    public Animator anim;
    public Image background;
    public Color[] backgroundColors;
    public Image[] pips;
    //0 = Standby (blue)
    //1 = Answer in (orange)
    //2 = Correct (dark green)
    //3 = Current Answer (bright green)
    //4 = Incorrect (Red)

    public void Init(PlayerObject pl)
    {
        containedPlayer = pl;
        nameMesh.text = pl.playerName;
        background.color = backgroundColors[0];
        foreach (Image i in pips)
            i.color = backgroundColors[2];
    }

    public void Hit()
    {
        anim.SetTrigger("hit");
        pips[containedPlayer.submittedAnswers.Count - 1].color = backgroundColors[3];
        background.color = backgroundColors[2];
        //Flick to orange upon completion or flick back to default
        if (containedPlayer.submittedAnswers.Count == GameplayManager.Get.currentConfig.maximumAnswers)
            Invoke("OnSetCompleted", 1f);
        else
            Invoke("OnSetStandby", 1f);
    }

    public void OnSetHighlighted()
    {
        anim.SetTrigger("hit");
        background.color = backgroundColors[3];
        Invoke("OnSetCompleted", 4f);
    }

    public void OnSetCompleted()
    {
        background.color = backgroundColors[1];
    }

    public void OnSetStandby()
    {
        background.color = backgroundColors[0];
    }
}
