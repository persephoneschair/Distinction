using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StrapManager : SingletonMonoBehaviour<StrapManager>
{
    private PlayerObject _focusPlayer;
    public PlayerObject FocusPlayer
    {
        get
        {
            return _focusPlayer;
        }
        set
        {
            _focusPlayer = value;
            if (_focusPlayer != null)
            {
                focusPlayer = _focusPlayer.playerName;
                Operator.Get.focusPlayer = _focusPlayer.playerName;
            }                
            else
            {
                focusPlayer = "NO PLAYER IN FOCUS";
                Operator.Get.focusPlayer = "NO PLAYER IN FOCUS";
            }
        }
    }

    [ShowOnly] public string focusPlayer = "NO PLAYER IN FOCUS";

    public Animator anim;
    public List<R2PlayerPrefab> r2PlayerPrefabs = new List<R2PlayerPrefab>();
    public List<R4PlayerPrefab> r4PlayerPrefabs = new List<R4PlayerPrefab>();

    public GameObject r2StrapToInstance;
    public GameObject r4StrapToInstance;
    public Transform instanceTarget;

    private void DestroyAllExistingStraps()
    {
        foreach (R2PlayerPrefab p in r2PlayerPrefabs)
            Destroy(p.gameObject);
        r2PlayerPrefabs.Clear();

        foreach (R4PlayerPrefab p in r4PlayerPrefabs)
            Destroy(p.gameObject);
        r4PlayerPrefabs.Clear();
    }

    public void InstanceR2Straps()
    {
        DestroyAllExistingStraps();

        foreach (PlayerObject pl in PlayerManager.Get.players)
        {
            var x = Instantiate(r2StrapToInstance, instanceTarget);
            var y = x.GetComponent<R2PlayerPrefab>();
            y.Init(pl);
            r2PlayerPrefabs.Add(y);
        }
        Invoke("AnimationToggle", 1f);
    }

    public void InstanceR4Straps()
    {
        DestroyAllExistingStraps();

        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => !x.finalist))
        {
            var x = Instantiate(r4StrapToInstance, instanceTarget);
            var y = x.GetComponent<R4PlayerPrefab>();
            y.Init(pl);
            r4PlayerPrefabs.Add(y);
        }
        Invoke("AnimationToggle", 1f);
    }

    public void PlayerResponse(PlayerObject pl)
    {
        if (r2PlayerPrefabs.FirstOrDefault(x => x.containedPlayer == pl) != null)
            r2PlayerPrefabs.FirstOrDefault(x => x.containedPlayer == pl).Hit();
    }

    public void AnimationToggle()
    {
        anim.SetTrigger("toggle");
    }

    public void RevealIncorrectPlayers()
    {
        foreach (PlayerObject pl in PlayerManager.Get.players.Where(x => !x.wasCorrect))
        {
            r2PlayerPrefabs.FirstOrDefault(x => x.containedPlayer == pl).OnSetWrong();
            DebugLog.Print($"{pl.playerName} said {(string.IsNullOrEmpty(pl.submittedAnswers.FirstOrDefault()) ? "NO ANSWER" : pl.submittedAnswers.FirstOrDefault())}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
        }            
    }

    public void CondoneAnAnswer()
    {
        //Correct gameplay stage
        if (GameplayManager.Get.currentStage == GameplayManager.GameplayStage.CalculateR2Points)
            if (FocusPlayer != null && Operator.Get.answerToCondoneAsIndex < ColumnManager.Get.singleQuestionColumn.answerBoxes.Count)
                r2PlayerPrefabs.FirstOrDefault(x => x.containedPlayer == FocusPlayer).OnCondone(Operator.Get.answerToCondoneAsIndex);
    }

    public void ClearStraps()
    {
        Invoke("Clearance", 1f);
    }

    void Clearance()
    {
        foreach (R2PlayerPrefab pr in r2PlayerPrefabs)
            Destroy(pr.gameObject);
        r2PlayerPrefabs.Clear();
        AnimationToggle();
    }
}
