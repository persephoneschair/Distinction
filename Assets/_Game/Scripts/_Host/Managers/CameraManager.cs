using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Api.Core.Extensions.System;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : SingletonMonoBehaviour<CameraManager>
{
    public RawImage transitionImage;
    public Animator transitionAnim;

    public enum CameraAngle
    {
        Floating,
        Audience,
        MultiBox,
        SingleBox,
        Scores,
        Final
    }

    public Camera[] cameras;
    public KeyCode[] toggles;
    public CameraAngle startingAngle;
    private CameraAngle _currentCam = CameraAngle.Floating;
    public CameraAngle CurrentCam
    {
        get
        {
            return _currentCam;
        }
        set
        {
            ToggleCam(_currentCam, value);
            _currentCam = value;
        }
    }

    private void Start()
    {
        CurrentCam = startingAngle;
    }

    IEnumerator RecordFrame(CameraAngle newAngle)
    {
        /*yield return new WaitForEndOfFrame();
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        transitionImage.texture = texture;*/

        transitionAnim.SetTrigger("toggle");
        yield return new WaitForSeconds(1f);
        CurrentCam = newAngle;

        //CreditsManager.Get.storedScreenshots.Add(texture);


        // cleanup
        //Object.Destroy(texture);
    }

    public void TransitionCam(CameraAngle newAngle)
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.PlayerLandsInSeat, 0.25f);
        AudioManager.Get.Play(AudioManager.OneShotClip.PointDrain, 1f);
        //AudioManager.Get.Play(AudioManager.OneShotClip.FlyIn, 0.5f);
        AudioManager.Get.Play(AudioManager.OneShotClip.Wheee, 2f);
        StartCoroutine(RecordFrame(newAngle));
    }

    private void ToggleCam(CameraAngle oldAngle, CameraAngle newAngle)
    {
        cameras[(int)oldAngle].enabled = false;
        cameras[(int)newAngle].enabled = true;
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.RightControl))
        {
            for (int i = 0; i < toggles.Length; i++)
                if (Input.GetKeyDown(toggles[i]))
                    CurrentCam = (CameraAngle)i;
        }        
    }

    [Button]
    public void PreviousCamera()
    {
        CurrentCam = (CameraAngle)(((int)CurrentCam + toggles.Length - 1) % toggles.Length);
    }

    [Button]
    public void NextCamera()
    {
        CurrentCam = (CameraAngle)(((int)CurrentCam + 1) % toggles.Length);
    }
}
