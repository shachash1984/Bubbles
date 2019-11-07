using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    static public LevelManager S;

    public Level[] levels;

    private void Awake()
    {
        S = this;
    }

    public bool EntitledToStar(int currentScore, int level, int stars)
    {
        switch (stars)
        {
            case 0:
                return currentScore >= levels[level].firstStarScoreAmount;
            case 1:
                return currentScore >= levels[level].secondStarScoreAmount;
            case 2:
                return currentScore >= levels[level].thirdStarScoreAmount;
            default:
                return false;
        }
    }

    public void LoadNextLevel(int newLevel)
    {
        StartCoroutine(LoadNextLevelCoroutine(newLevel));
    }

    private IEnumerator LoadNextLevelCoroutine(int newLevel)
    {
        yield return null;
        BubbleHandler.S.ReadLevelLayout(newLevel);
        Player.S.SetCurrentBubble(Player.S.CreateCurrentBubble(true));
    }
}
