using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static GameplayManager;

public class TitlesManager : SingletonMonoBehaviour<TitlesManager>
{
    [TextArea(3,4)] public string[] topMeshTitleOptions;
    [TextArea(3,4)] public string[] bottomMeshTitleOptions;
    public TextMeshProUGUI titleMeshOne;
    public TextMeshProUGUI titleMeshTwo;
    public Animator anim;
    public float titlesDelay = 4f;

    [Button]
    public void RunTitleSequence()
    {
        if (Operator.Get.skipOpeningTitles)
            EndOfTitleSequence();
        else
        {
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.DoNothing;
            StartCoroutine(TitleSequence());
        }           
    }

    IEnumerator TitleSequence()
    {
        AudioManager.Get.Play(AudioManager.LoopClip.Titles, false, 0f);
        yield return new WaitForSeconds(titlesDelay);
        for (int i = 0; i < topMeshTitleOptions.Length -1; i++)
        {
            titleMeshOne.text = topMeshTitleOptions[i];
            titleMeshTwo.text = bottomMeshTitleOptions[i];
            anim.SetInteger("titlesState", 2);
            AudioManager.Get.Play(AudioManager.OneShotClip.Wheee);
            yield return new WaitForSeconds(titlesDelay);
            anim.SetInteger("titlesState", i % 2);
            AudioManager.Get.Play(AudioManager.OneShotClip.FlyIn);
            yield return new WaitForSeconds(titlesDelay);
        }
        titleMeshOne.text = topMeshTitleOptions.Last();
        titleMeshTwo.text = bottomMeshTitleOptions.Last();
        AudioManager.Get.Play(AudioManager.OneShotClip.Wheee);
        anim.SetInteger("titlesState", 2);
        yield return new WaitForSeconds(titlesDelay);
        AudioManager.Get.Play(AudioManager.OneShotClip.PointDrain);
        anim.SetTrigger("bounce");
        yield return new WaitForSeconds(titlesDelay);
        AudioManager.Get.Play(AudioManager.OneShotClip.FlyIn);
        anim.SetTrigger("collapse");
        yield return new WaitForSeconds(titlesDelay);
        EndOfTitleSequence();
    }

    void EndOfTitleSequence()
    {
        AudioManager.Get.Play(AudioManager.LoopClip.Lobby, true, 0f);
        GameplayManager.Get.currentStage = GameplayStage.OpenLobby;
        GameplayManager.Get.ProgressGameplay();
        CameraLerpManager.Get.ZoomToPosition(CameraLerpManager.CameraPosition.MergeToAudience, 70f, 5f);
        CameraLerpManager.Get.ForceSwitchToAudience(5f);
        this.gameObject.SetActive(false);
    }
}
