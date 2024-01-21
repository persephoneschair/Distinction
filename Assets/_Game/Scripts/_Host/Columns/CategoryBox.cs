using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CategoryBox : MonoBehaviour
{
    public Animator anim;
    public TextMeshPro categoryMesh;

    public void InitCatBox(string catText)
    {
        categoryMesh.text = catText;
    }

    public void RevealCatBox()
    {
        anim.SetTrigger("toggle");
    }
}
