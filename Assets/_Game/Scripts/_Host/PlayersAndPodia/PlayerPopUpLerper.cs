using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPopUpLerper : MonoBehaviour
{
    public RawImage profilePic;

    private float elapsedTime;
    private Vector3 startPos;
    private Vector3 startRot;
    private Vector3 endPos;
    private Vector3 endRot;
    private float duration = 2f;

    private bool isMoving = false;

    #region Public Functions

    public void InitPopUp(PlayerObject player)
    {
        profilePic.texture = player.profileImage;
        this.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(UnityEngine.Random.Range(-800, 800f), -800f, 0);
        PerformAnimation();
    }

    #endregion

    #region Private Functions

    private void PerformAnimation()
    {
        startPos = this.gameObject.GetComponent<RectTransform>().localPosition;
        startRot = this.gameObject.GetComponent<RectTransform>().localEulerAngles;

        endPos = new Vector3(UnityEngine.Random.Range(-1200, 1200f), 800f, 0);
        endRot = new Vector3(UnityEngine.Random.Range(-540, 540f), UnityEngine.Random.Range(-540, 540), UnityEngine.Random.Range(-540, 540));

        elapsedTime = 0;
        duration = UnityEngine.Random.Range(2.5f, 5f);
        isMoving = true;
        Invoke("EndLock", duration);
    }

    private void Update()
    {
        if (isMoving)
            PerformLerp();
    }

    private void PerformLerp()
    {
        elapsedTime += Time.deltaTime;

        float percentageComplete = elapsedTime / duration;

        this.gameObject.transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, percentageComplete));

        float x = Mathf.LerpAngle(startRot.x, endRot.x, Mathf.SmoothStep(0, 1, percentageComplete));
        float y = Mathf.LerpAngle(startRot.y, endRot.y, Mathf.SmoothStep(0, 1, percentageComplete));
        float z = Mathf.LerpAngle(startRot.z, endRot.z, Mathf.SmoothStep(0, 1, percentageComplete));
        this.gameObject.transform.localEulerAngles = new Vector3(x, y, z);//Vector3.Lerp(startRot, endRot, Mathf.SmoothStep(0, 1, percentageComplete));
    }

    private void EndLock()
    {
        Destroy(this.gameObject);
    }

    #endregion
}
