using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FinalPodium : MonoBehaviour
{
    public PlayerObject containedPlayer;
    public Renderer avatarRend;
    public TextMeshPro nameMesh;
    public GameObject avatarObj;

    public Renderer[] podiumBodyRends;
    public Material[] podiumMats;

    public GameObject placeholderLogo;
    public TextMeshPro scoreMesh;
    public AnswerBox[] answerBoxes;

    private AnswerBox currentBox;

    public void ActivateForFinal(PlayerObject pl)
    {
        containedPlayer = pl;
        avatarRend.material.mainTexture = pl.profileImage;
        nameMesh.text = pl.playerName;
        avatarObj.SetActive(true);

        foreach (Renderer r in podiumBodyRends)
            r.material = podiumMats[0];

        placeholderLogo.SetActive(false);
        scoreMesh.text = pl.points.ToString();
    }

    public void SetPodiumLights(bool on)
    {
        foreach (Renderer r in podiumBodyRends)
            r.material = podiumMats[on ? 1 : 0];

        if(on)
        {
            currentBox = answerBoxes.FirstOrDefault(x => !x.revealed);

            if(currentBox != null)
                //Light up front of box upon selection
                currentBox.frontAnswerRend.material = currentBox.backAnswerMats[2];
        }
    }

    public void ReceiveAnswer(string response)
    {
        currentBox.RevealR4AnswerBox(response);
    }

    public void MarkAnswer(bool correct)
    {
        currentBox.SetR4BoxColor(correct);
    }

    public void SetDistinct()
    {
        currentBox.SetR4BoxDistinct();
    }

    public void PointDrain(int pointsToReduce, bool correct, int playersWhoSaidIt)
    {
        StartCoroutine(Drain(pointsToReduce, correct, playersWhoSaidIt));
    }

    IEnumerator Drain(int pointsToReduce, bool correct, int playersWhoSaidIt)
    {
        int target = containedPlayer.points - pointsToReduce;
        while(containedPlayer.points > target)
        {
            AudioManager.Get.Play(AudioManager.OneShotClip.Tick);
            containedPlayer.points--;
            scoreMesh.text = containedPlayer.points.ToString();
            yield return new WaitForSeconds(0.05f);
        }
        PlayerManager.Get.UpdatePlayerScore(containedPlayer);
        if (correct)
        {
            R4PodiumManager.Get.RegularAlert();
            PlayerManager.Get.SendMessageToPlayer($"<color=green>CORRECT</color>\n{playersWhoSaidIt} players said it and you lost {pointsToReduce} points", containedPlayer);
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.UnlockForR4Finalist;
            R4PodiumManager.Get.SwitchFocusPlayer();
        }
    }
}
