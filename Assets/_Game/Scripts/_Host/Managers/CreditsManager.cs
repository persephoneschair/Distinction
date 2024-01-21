using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsManager : SingletonMonoBehaviour<CreditsManager>
{
    public Animator creditsAnim;
    public TextMeshProUGUI creditsMesh;
    public GameObject endCard;

    public List<Texture> storedScreenshots = new List<Texture>();
    public RawImage[] photoRends;

    private void Start()
    {
        this.gameObject.SetActive(false);
    }

    [Button]
    public void RollCredits()
    {
        this.gameObject.SetActive(true);
        AudioManager.Get.Play(AudioManager.LoopClip.Credits, false);
        CreditsBuilder();
        StartCoroutine(Credits());
    }

    IEnumerator Credits()
    {
        yield return new WaitForSeconds(6f);
        creditsAnim.SetTrigger("toggle");
        yield return new WaitForSeconds(52.75f);
        endCard.SetActive(true);
    }

    private int currentCol = 0;
    private string[] colorSequence = new string[7]
    {
        "<color=red>",
        "<color=green>",
        "<color=blue>",
        "<color=yellow>",
        "<color=orange>",
        "<color=purple>",
        "<color=white>"
    };

    [TextArea(4, 5)] public string[] headerOptions;
    [TextArea(7, 8)] public string[] creditOptions;

    [Button]
    public void CreditsBuilder()
    {
        currentCol = 0;
        string creds = "<size=175%>";
        for (int i = 0; i < headerOptions.Length; i++)
        {
            if (i != 0)
                creds += "<size=75%>";

            char[] chars = headerOptions[i].ToCharArray();
            foreach(char c in chars)
            {
                creds += colorSequence[currentCol] + c;
                currentCol = (currentCol + 1) % colorSequence.Length;
                if (i > 0 && currentCol == 6)
                    currentCol = 0;
            }
            if (i == 0)
                creds += "</size>\n\n\n";
            else
            {
                creds += $"</size>\n<color=white>{creditOptions[i]}";
                if (i > 6 && i < 9)
                    creds += "\n";
                else
                    creds += "\n\n";

            }                
        }
        currentCol = 0;
        creds += "\n\n\n\n<size=175%><color=red>D<color=green>I<color=blue>S<color=yellow>T<color=orange>I<color=purple>N<color=white>C<color=red>T<color=green>I<color=blue>O<color=yellow>N<color=orange>!<color=white>";
        creditsMesh.text = creds;

        if (storedScreenshots.Count >= photoRends.Length)
            foreach (RawImage ri in photoRends)
            {
                var x = Extensions.PickRandom(storedScreenshots);
                ri.texture = x;
                storedScreenshots.Remove(x);
            }
    }
}
