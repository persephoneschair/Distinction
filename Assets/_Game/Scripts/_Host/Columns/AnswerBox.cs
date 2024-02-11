using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AnswerBox : MonoBehaviour
{
    public Animator anim;
    public TextMeshPro hiddenAnswerMesh;
    public TextMeshPro revealedAnswerMesh;
    public TextMeshPro revealedAnswerMeshNoPic;
    public Renderer playerPictureRend;
    public GameObject[] playerPictureObj;

    public Renderer frontAnswerRend;
    public Renderer backAnswerRend;
    public Material[] backAnswerMats;
    //0 = Incorrect (red)
    //1 = Correct (green)
    //2 = Bonus (blue)

    public bool isBonus;
    public List<string> validAnswers = new List<string>();

    public bool flaggedForReveal = false;
    public bool revealed = false;

    public int playersWhoGaveThisAnswer = 0;
    public int r2PointsBoxIsWorth = 0;

    public void InitAnswerBox(string allValidAnswers, int index, bool bonus)
    {
        validAnswers.Clear();
        validAnswers = allValidAnswers.Split(',').ToList();

        if(GameplayManager.Get.currentRound == GameplayManager.Round.Round2)
        {
            revealedAnswerMesh.text = "";
            revealedAnswerMeshNoPic.text = validAnswers.FirstOrDefault();
            foreach (GameObject go in playerPictureObj)
                go.SetActive(false);
        }            
        else
        {
            revealedAnswerMeshNoPic.text = "";
            revealedAnswerMesh.text = validAnswers.FirstOrDefault();
        }
            

        hiddenAnswerMesh.text = $"Answer #{index}";
        isBonus = bonus;
    }

    public void FlagAnswerAsFound(PlayerObject finder = null)
    {
        //Check player hasn't already said this
        if (finder.submittedAnswers.Contains(validAnswers.FirstOrDefault()))
            return;

        //Check answer hasn't already been revealed
        if (revealed)
            return;
        DebugLog.Print($"{validAnswers.FirstOrDefault()} found by {(finder == null ? "UNKNOWN" : finder.playerName)}", !flaggedForReveal ? DebugLog.StyleOption.Bold : DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);

        //Apply consolation points
        if (flaggedForReveal)
        {
            finder.points += GameplayManager.Get.currentConfig.pointsForSecondary;
            finder.closeSubmittedAnswers.Add(validAnswers.FirstOrDefault());
            return;
        }
        else
        {
            //Flag for reveal and iterate base/bonus points
            finder.points += isBonus ? GameplayManager.Get.currentConfig.pointsForBonus : GameplayManager.Get.currentConfig.basePoints;
            finder.submittedAnswers.Add(validAnswers.FirstOrDefault());
            flaggedForReveal = true;
            StartCoroutine(DelayReveal(finder));
        }        
    }

    private IEnumerator DelayReveal(PlayerObject finder)
    {
        yield return new WaitForSeconds(GameplayManager.Get.currentConfig.secondaryAnswerDelay);
        RevealAnswerBox(finder);
    }

    [Button]
    public void RevealAnswerBox(PlayerObject finder = null)
    {
        revealed = true;
        backAnswerRend.material = finder == null ? backAnswerMats[0] : isBonus ? backAnswerMats[2] : backAnswerMats[1];
        anim.SetTrigger("toggle");
        if (finder != null)
        {
            AudioManager.Get.Play(AudioManager.OneShotClip.BoardTurn);
            playerPictureRend.material.mainTexture = finder.profileImage;
            if (isBonus)
                AudioManager.Get.Play(AudioManager.FindAnswerClip.Wow);
            else
                AudioManager.Get.Play((AudioManager.FindAnswerClip)UnityEngine.Random.Range(1, 11));
        }
        else
            playerPictureRend.material.mainTexture = ColumnManager.Get.incorrectTexture;
    }

    public void RevealR2AnswerBox()
    {
        revealed = true;
        revealedAnswerMeshNoPic.text += $" [{r2PointsBoxIsWorth}]";
        bool wasFound = r2PointsBoxIsWorth != 0;
        backAnswerRend.material = wasFound ? backAnswerMats[1] : backAnswerMats[0];
        anim.SetTrigger("toggle");
        AudioManager.Get.Play(AudioManager.OneShotClip.BoardTurn);
        if (wasFound)
            AudioManager.Get.Play(AudioManager.OneShotClip.Correct);
        else
            AudioManager.Get.Play(AudioManager.FindAnswerClip.Bwoing);

        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => x.submittedAnswers.FirstOrDefault() == validAnswers.FirstOrDefault()))
        {
            pl.points += r2PointsBoxIsWorth;
            PlayerManager.Get.UpdatePlayerScore(pl);
            PlayerManager.Get.SendMessageToPlayer($"<color=green><u>{validAnswers.FirstOrDefault()}</u></color> scored you {r2PointsBoxIsWorth} points!", pl);
            if(StrapManager.Get.r2PlayerPrefabs.FirstOrDefault(x => x.containedPlayer == pl) != null)
            {
                var strap = StrapManager.Get.r2PlayerPrefabs.FirstOrDefault(x => x.containedPlayer == pl);
                strap.OnSetHighlighted(r2PointsBoxIsWorth);
            }
        }
    }

    public void RevealR4AnswerBox(string response)
    {
        revealed = true;
        revealedAnswerMeshNoPic.text = response;
        AudioManager.Get.Play(AudioManager.OneShotClip.BoardTurn);
        AudioManager.Get.Play(AudioManager.FindAnswerClip.Bwoing);
        anim.SetTrigger("toggle");
    }

    public void SetR4BoxColor(bool correct)
    {
        backAnswerRend.material = correct ? backAnswerMats[1] : backAnswerMats[0];
    }

    public void SetR4BoxDistinct()
    {
        backAnswerRend.material = backAnswerMats[2];
    }
}
