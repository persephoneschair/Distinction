using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairEntrance : MonoBehaviour
{
    public Renderer[] renderers;
    public void Initiate(Texture tex)
    {
        foreach (Renderer r in renderers)
            r.material.mainTexture = tex;

        Destroy(this.gameObject, 6f);
    }
}
