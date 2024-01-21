using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreboardStrap : MonoBehaviour
{
    public TextMeshPro positionMesh;
    public TextMeshPro nameMesh;
    public TextMeshPro scoreMesh;
    public Renderer profilePicRend;
    public Animator anim;

    public void TriggerStrap(PlayerObject pl)
    {
        profilePicRend.material.mainTexture = pl.profileImage;
        nameMesh.text = pl.playerName;
        scoreMesh.text = pl.points.ToString();
        positionMesh.text = pl.currentPositionString;

        anim.SetTrigger("appear");
    }

    public void ClearStrap()
    {
        anim.SetTrigger("clear");
    }
}
