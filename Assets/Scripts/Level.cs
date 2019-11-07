using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName ="Level", menuName = "New Level",order = 0)][Serializable]
public class Level : ScriptableObject, ISerializationCallbackReceiver
{

    public string[] rows;
    public int firstStarScoreAmount = 100;
    public int secondStarScoreAmount = 100;
    public int thirdStarScoreAmount = 100;
    [Space][Space]
    public int bubbleCount;

    void OnValidate()
    {
        bubbleCount = 0;
        if (!rows[0].Equals("___________"))
            rows[0] = "___________";
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i].Length > 11)
                rows[i] = rows[i].Substring(0, 11);
            else if(rows[i].Length < 11)
            {
                int missingChars = 11 - rows[i].Length;
                while(missingChars > 0)
                {
                    rows[i] += "_";
                    missingChars--;
                }
            }
            for (int j = 0; j < rows[i].Length; j++)
            {
                if (rows[i][j] == '*')
                    bubbleCount++;
            }    
        }
    }

    public void OnAfterDeserialize()
    {
        return;
    }

    public void OnBeforeSerialize()
    {
        return;
    }
}
