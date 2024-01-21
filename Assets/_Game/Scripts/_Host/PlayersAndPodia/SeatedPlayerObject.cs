using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatedPlayerObject : MonoBehaviour
{
    public PlayerObject containedPlayer = null;
    public Renderer[] renderers;

    public void OnActivate(PlayerObject pl)
    {
        pl.seat = this;
        containedPlayer = pl;
        foreach (Renderer r in renderers)
            r.material.mainTexture = pl.profileImage;

        Invoke("Activation", 5f);
    }

    private void Activation()
    {
        this.gameObject.SetActive(true);
        AudioManager.Get.Play(AudioManager.FindAnswerClip.Bwoing, 0f);
        AudioManager.Get.Play(AudioManager.OneShotClip.PlayerLandsInSeat, 0.75f);
    }
}
