using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreboardManager : SingletonMonoBehaviour<ScoreboardManager>
{
    public ScoreboardStrap[] straps;
    public float holdTime = 4f;
    public float delayBetweenBoards = 1f;

    private void Start()
    {
        AssignStraps();
    }

    void AssignStraps()
    {
        for (int i = 0; i < straps.Length; i++)
            straps[i].anim.SetInteger("position", i + 1);
    }

    public List<PlayerObject> GetOrderedPlayers(List<PlayerObject> testList = null)
    {
        //This is horrible and forced me to split the points per round into individual variables
        //May be possible to `ThenByDescending` over a List<int> but couldn't work it out :(
        //Not the scoreboard will still display `=4th` for anybody with the same points

        return testList == null ? PlayerManager.Get.players
            .OrderByDescending(x => x.points)
            .ThenByDescending(x => x.r4Points)
            .ThenByDescending(x => x.r3Points)
            .ThenByDescending(x => x.r2Points)
            .ThenByDescending(x => x.r1Points)
            .ThenBy(x => x.playerName)
            .ToList()
            : testList.OrderByDescending(x => x.points).ThenBy(x => x.playerName).ToList();
    }

    public void DisplayScoreboard(List<PlayerObject> testList = null)
    {
        List<PlayerObject> pl = GetOrderedPlayers(testList);

        int currentPos = 1;
        int posExcess = 0;
        for(int i = 0; i < pl.Count; i++)
        {
            if(i == 0)
                pl[i].currentPositionString = Extensions.AddOrdinal(currentPos);
            else
            {
                if (pl[i - 1].points != pl[i].points)
                {
                    currentPos = currentPos + posExcess + 1;
                    posExcess = 0;
                    pl[i].currentPositionString = Extensions.AddOrdinal(currentPos);
                }                    
                else
                {
                    pl[i - 1].currentPositionString = $"={Extensions.AddOrdinal(currentPos)}";
                    pl[i].currentPositionString = $"={Extensions.AddOrdinal(currentPos)}";
                    posExcess++;
                }
            }
        }
        pl.Reverse();

        StartCoroutine(ScoreboardAnim(pl));
    }

    IEnumerator ScoreboardAnim(List<PlayerObject> pl)
    {
        int overlap = pl.Count % straps.Length;
        int gone = 0;
        if (overlap > 0)
        {
            for (int i = overlap; i > 0; i--)
            {
                straps[straps.Length - 1 - (overlap - i)].TriggerStrap(pl[overlap - i]);
                AudioManager.Get.Play(AudioManager.OneShotClip.BoardTurn);
                AudioManager.Get.Play(AudioManager.OneShotClip.PlayerLandsInSeat, 0.5f);
                yield return new WaitForSeconds(0.45f);
            }
            yield return new WaitForSeconds(holdTime);

            //Transition cam back to audience at end of board sequence
            if (overlap >= pl.Count)
                GameplayManager.Get.OnScoreboardFinished();
        }
        for (int i = overlap; i > 0; i--)
            straps[straps.Length - i].ClearStrap();

        yield return new WaitForSeconds(delayBetweenBoards);
        gone += overlap;

        while(gone < pl.Count)
        {
            for (int i = straps.Length; i > 0; i--)
            {
                straps[i - 1].TriggerStrap(pl[straps.Length - i + gone]);
                AudioManager.Get.Play(AudioManager.OneShotClip.BoardTurn);
                AudioManager.Get.Play(AudioManager.OneShotClip.PlayerLandsInSeat, 0.5f);
                yield return new WaitForSeconds(0.45f);
            }
            yield return new WaitForSeconds(holdTime);

            //Transition cam back to audience at end of board sequence
            if (gone + straps.Length >= pl.Count)
                GameplayManager.Get.OnScoreboardFinished();

            foreach (ScoreboardStrap s in straps)
                s.anim.SetTrigger("clear");

            yield return new WaitForSeconds(delayBetweenBoards);
            gone += straps.Length;
        }
    }

    [Button]
    public void TestScoreboard()
    {
        List<PlayerObject> testList = new List<PlayerObject>();
        for(int i = 0; i < testPlayers.Length; i++)
            testList.Add(new PlayerObject(testPlayers[i]));

        DisplayScoreboard(testList);
    }

    public string[] testPlayers;
}
