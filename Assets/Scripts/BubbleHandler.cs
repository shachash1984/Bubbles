using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BubbleHandler : MonoBehaviour {

    #region Public Fields
    static public BubbleHandler S;
    public Color[] colors = new Color[] { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.black };
    
    #endregion

    #region Private Fields
    private List<Bubble> _bubbles = new List<Bubble>();
    [SerializeField] private GameObject _bubblePrefab;
    private Dictionary<Vector2Int, Bubble> _bubbleMap = new Dictionary<Vector2Int, Bubble>();
    #endregion

    #region Constants
    private const float EVEN_ROW_START_COL = 2.4f;
    private const float ODD_ROW_START_COL = 2.65f;
    #endregion

    #region Properties
    public Bubble targetBubble { get; private set; }
    public Bubble this[int i]
    {
        get { return _bubbles[i]; }
        set { _bubbles[i] = value; }
    }
    public int Count
    {
        get { return _bubbles.Count; }
    }
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
    }

    private void Start()
    {
        ReadLevelLayout(Player.S.level - 1);
    }

    #endregion

    #region Public Methods
    public void SetTargetBubble(Bubble b)
    {
        targetBubble = b;
    }

    public bool HitBubble(Bubble hitter)
    {
        Bubble[] hitBubbles = GetSurroundingBubbles(hitter.gridPosition);
        bool hit = false;
        for (int i = 0; i < hitBubbles.Length; i++)
        {
            if (hitBubbles[i].type == hitter.type && IsPaired(hitBubbles[i], hitter))
            {
                hit = true;
                BlowBubble(hitBubbles[i], hitter.type, true);
            }
            else
            {
                hitBubbles[i].DoWobble(hitter.transform.position);
                if (!hit)
                    SoundManager.S.PlaySound(SoundType.Stick);
                if (hitter.transform.position.y >= Player.TOP_ROW_Y)
                    hitter.topRow = true;
            }
                
        }
        if (hit)
            BlowBubble(hitter, hitter.type, true);

        return hit;
    }

    public Vector3 GetWantedPositionSimple(Vector3 hitPoint, Bubble other)
    {
        if (!other)
            return GridToWorldPos(WorldToGridPos(hitPoint));

        Vector3 wantedPos = Vector3.zero;
        Vector2Int gridPosToCheck = other.gridPosition;

        bool rightSide = hitPoint.x >= other.transform.position.x;
        bool debug = false;
        //HIT ON RIGHT SIDE////
        if (rightSide)
        {
            if (other.gridPosition.y % 2 == 0)
            {
                Vector2Int wantedGridPos = new Vector2Int(gridPosToCheck.x + 1, gridPosToCheck.y);
                int counter = 1;
                while (ContainsKey(wantedGridPos))
                {
                    if (counter % 2 == 1)
                        wantedGridPos.y--;
                    else
                        wantedGridPos.x++;

                    counter++;
                }
                wantedPos = GridToWorldPos(wantedGridPos);
            }
            else
            {
                Vector2Int wantedGridPos = new Vector2Int(gridPosToCheck.x+1, gridPosToCheck.y);
                int counter = 1;
                while (ContainsKey(wantedGridPos))
                {
                    if (counter % 2 == 1)
                    {
                        wantedGridPos.y--;
                        if (counter == 1)
                        {
                            wantedGridPos.x--;
                            continue;
                        }
                    }
                    else
                        wantedGridPos.x++;

                    counter++;
                }
                wantedPos = GridToWorldPos(wantedGridPos);
            }
        }
        else
        {
            if (other.gridPosition.y % 2 == 0)
            {
                Vector2Int wantedGridPos = new Vector2Int(gridPosToCheck.x - 1, gridPosToCheck.y);
                int counter = 1;
                while (ContainsKey(wantedGridPos))
                {
                    if (counter % 2 == 1)
                    {
                        wantedGridPos.y--;
                        if(counter == 1)
                        {
                            wantedGridPos.x++;
                            continue;
                        }
                    }
                    else
                        wantedGridPos.x--;

                    counter++;
                }
                wantedPos = GridToWorldPos(wantedGridPos);
            }
            else
            {
                Vector2Int wantedGridPos = new Vector2Int(gridPosToCheck.x - 1, gridPosToCheck.y);
                int counter = 1;
                while (ContainsKey(wantedGridPos))
                {
                    if (counter % 2 == 1)
                        wantedGridPos.y--;
                    else
                        wantedGridPos.x--;

                    counter++;
                }
                wantedPos = GridToWorldPos(wantedGridPos);
            }
        }
            return wantedPos;
    }

    public Vector3 GetWantedPosition(Vector3 hitPoint, Bubble other)
    {
        if (!other)
            return GridToWorldPos(WorldToGridPos(hitPoint));

        Vector3 wantedPos = Vector3.zero;
        Vector2Int gridPosToCheck = other.gridPosition;
        
        bool rightSide = hitPoint.x >= other.transform.position.x;
        bool debug = false;
        //HIT ON RIGHT SIDE////
        if (rightSide)
        {
            
            //if hit is high
            if (hitPoint.y > (other.transform.position.y - Bubble.BUBBLE_SCALE / 4))
            {
                //even row
                if(other.gridPosition.y % 2 == 0)
                {
                    //if there isn't a bubble on the right
                    if(!ContainsKey(new Vector2Int(gridPosToCheck.x + 1, gridPosToCheck.y)))
                    {
                        //wanted pos is right
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>hit is high, even row ,free on right, setting right</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x + 1, gridPosToCheck.y));
                    }
                    //if there isn't a bubble on the bottom right
                    else if(!ContainsKey(new Vector2Int(gridPosToCheck.x+1, gridPosToCheck.y-1)))
                    {
                        //wanted pos is bottom right
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>hit is high, even row ,occupied on right, setting bottom right</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x+1, gridPosToCheck.y - 1));
                    }
                    else
                    {
                        //wanted pos is bottom right of right
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>hit is high, even row ,occupied on right&&bottomright, setting right->bottom right</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x + 2, gridPosToCheck.y - 1));
                    }

                }
                //odd row
                else
                {
                    //if there isn't a bubble on the right
                    if (!ContainsKey(new Vector2Int(gridPosToCheck.x + 1, gridPosToCheck.y)))
                    {
                        //wanted pos is right
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>hit is high, odd row ,free on right, setting right</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x + 1, gridPosToCheck.y));
                    }
                    //if there isn't a bubble on the bottom right
                    else if (!ContainsKey(new Vector2Int(gridPosToCheck.x, gridPosToCheck.y - 1)))
                    {
                        //wanted pos is bottom right
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>hit is high, odd row ,occupied on right, setting bottom right</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x, gridPosToCheck.y - 1));
                    }
                    else
                    {
                        //wanted pos is bottom right of right
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>hit is high, odd row ,occupied on right&&bottomright, setting right->bottom right</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x + 1, gridPosToCheck.y - 1));
                    }
                }
            }
            //if hit is low
            else
            {
                //even row
                if (other.gridPosition.y % 2 == 0)
                {
                    //if there isn't a bubble on the bottom right
                    if (!ContainsKey(new Vector2Int(gridPosToCheck.x+1, gridPosToCheck.y - 1)))
                    {
                        //wanted pos is bottom right of bottom right
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>hit is low, even row ,free on bottomright, setting bottom right</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x+1, gridPosToCheck.y - 1));
                    }
                    else
                    {
                        //wanted pos is bottom right of bottom right
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>hit is low, even row ,occupied on bottomright, setting bottom right->bottom right</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x + 1, gridPosToCheck.y - 2));
                    }
                }
                //odd row
                else
                {
                    //if there isn't a bubble on the bottom right
                    if (!ContainsKey(new Vector2Int(gridPosToCheck.x, gridPosToCheck.y - 1)))
                    {
                        //wanted pos is bottom right of bottom right
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>hit is low, odd row ,free on bottomright, setting bottom right</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x, gridPosToCheck.y - 1));
                    }
                    else
                    {
                        //wanted pos is bottom right of bottom right
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>hit is low, odd row ,occupied on bottomright, setting bottom right->bottom right</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x + 1, gridPosToCheck.y - 2));
                    }
                }
            }
        }

        //HIT ON LEFT SIDE////
        else
        {
            //if hit is high
            if (hitPoint.y > (other.transform.position.y - Bubble.BUBBLE_SCALE / 4))
            {
                //even row
                if (other.gridPosition.y % 2 == 0)
                {
                    //if there isn't a bubble on the left
                    
                    if (!ContainsKey(new Vector2Int(gridPosToCheck.x - 1, gridPosToCheck.y)))
                    {
                        //wanted pos is left
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>LEFT hit is high, even row ,free on left, setting left</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x - 1, gridPosToCheck.y));
                    }
                    //if there isn't a bubble on the bottom left
                    else if (!ContainsKey(new Vector2Int(gridPosToCheck.x, gridPosToCheck.y - 1)))
                    {
                        //wanted pos is bottom left
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>LEFT hit is high, even row ,free on  bottom left, setting bottom left</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x, gridPosToCheck.y - 1));
                    }
                    else
                    {
                        //wanted pos is bottom left of left
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>LEFT hit is high, even row ,occupied on  bottom left, setting bottom left->bottom left</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x -1, gridPosToCheck.y - 1));
                    }

                }
                //odd row
                else
                {
                    //if there isn't a bubble on the left
                    if (!ContainsKey(new Vector2Int(gridPosToCheck.x - 1, gridPosToCheck.y)))
                    {
                        //wanted pos is left
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>LEFT hit is high, odd row ,free on left, setting left</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x - 1, gridPosToCheck.y));
                    }
                    //if there isn't a bubble on the bottom left
                    else if (!ContainsKey(new Vector2Int(gridPosToCheck.x-1, gridPosToCheck.y - 1)))
                    {
                        //wanted pos is bottom left
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>LEFT hit is high, odd row ,free on  bottom left, setting bottom left</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x-1, gridPosToCheck.y - 1));
                    }
                    else
                    {
                        //wanted pos is bottom left of left
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>LEFT hit is high, odd row ,occupied on  bottom left, setting bottom left -> bottom left</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x-2, gridPosToCheck.y - 1));
                    }
                }
            }
            //if hit is low
            else
            {
                //even row
                if (other.gridPosition.y % 2 == 0)
                {
                    //if there isn't a bubble on the bottom left
                    if (!ContainsKey(new Vector2Int(gridPosToCheck.x, gridPosToCheck.y - 1)))
                    {
                        //wanted pos is  bottom left
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>LEFT hit is low, even row ,free on left, setting left</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x, gridPosToCheck.y - 1));
                    }
                    else
                    {
                        //wanted pos is bottom left of bottom left
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>LEFT hit is low, even row ,free on  bottom left, setting bottom left</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x -1, gridPosToCheck.y - 1));
                    }
                }
                //odd row
                else
                {
                    //if there isn't a bubble on the bottom left
                    if (!ContainsKey(new Vector2Int(gridPosToCheck.x-1, gridPosToCheck.y - 1)))
                    {
                        //wanted pos is  bottom left
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>LEFT hit is low, odd row ,free on left, setting left</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x-1, gridPosToCheck.y - 1));
                    }
                    else
                    {
                        //wanted pos is bottom left of bottom left
                        if (debug)
                            Debug.Log(other.gridPosition + "<color=yellow>LEFT hit is low, odd row ,occupied on bottom left, setting bottom left -> bottom left</color>");
                        wantedPos = GridToWorldPos(new Vector2Int(gridPosToCheck.x - 1, gridPosToCheck.y - 2));
                    }
                }
            }
        }
        return wantedPos;
    }

    public Bubble GetBubbleByGridPosition(Vector2Int gp)
    {
        if (!_bubbleMap.ContainsKey(gp))
            return null;
        return _bubbleMap[gp];
    }

    public void BlowBubble(Bubble b, BubbleType bt, bool addScore)
    {
        if (b.type != bt || b.popped)
            return;
        b.popped = true;
        if (addScore)
            Player.S.score++;
        SoundManager.S.PlaySound(SoundType.Pop);
        UIHandler.S.SetScoreText(Player.S.score);
        Vector2Int[] neighbours = GetSurroundingBubblesGridPositions(b);
        //Debug.Log("<color=cyan>neighbours: " + neighbours.Length + "</color>");
        foreach (Vector2Int v in neighbours)
        {
            if (_bubbleMap.ContainsKey(v) && _bubbleMap[v])
            {
                //Debug.Log("<color=cyan>" + _bubbleMap[v] + "</color>");
                
                BlowBubble(_bubbleMap[v], bt, addScore);
                
            }
                
        }
        RemoveBubble(b);
        Destroy(b.gameObject);
    }

    public Vector2Int[] GetSurroundingBubblesGridPositions(Vector2Int v)
    {
        Vector2Int topRight, right, bottomRight, bottomLeft, left, topLeft;
        if (v.y % 2 == 0)
        {
            topRight = new Vector2Int(v.x + 1, v.y + 1);
            right = new Vector2Int(v.x + 1, v.y);
            bottomRight = new Vector2Int(v.x + 1, v.y - 1);
            bottomLeft = new Vector2Int(v.x, v.y - 1);
            left = new Vector2Int(v.x - 1, v.y);
            topLeft = new Vector2Int(v.x, v.y + 1);
        }
        else
        {
            topRight = new Vector2Int(v.x, v.y + 1);
            right = new Vector2Int(v.x + 1, v.y);
            bottomRight = new Vector2Int(v.x, v.y - 1);
            bottomLeft = new Vector2Int(v.x - 1, v.y - 1);
            left = new Vector2Int(v.x - 1, v.y);
            topLeft = new Vector2Int(v.x - 1, v.y + 1);
        }

        List<Vector2Int> posList = new List<Vector2Int>(new Vector2Int[] { topRight, right, bottomRight, bottomLeft, left, topLeft });

        return posList.ToArray();
    }

    public Vector2Int[] GetSurroundingBubblesGridPositions(Bubble b)
    {
        return GetSurroundingBubblesGridPositions(b.gridPosition);
    }

    public Vector3[] GetSurroundingBubblesWorldPositions(Bubble b)
    {
        List<Vector2Int> posList = new List<Vector2Int>(GetSurroundingBubblesGridPositions(b));
        List<Vector3> worldPosList = new List<Vector3>();
        for (int i = 0; i < posList.Count; i++)
        {
            worldPosList.Add(GetBubbleByGridPosition(posList[i]).transform.position);
        }
        return worldPosList.ToArray();
    }

    public Bubble[] GetSurroundingBubbles(Vector2Int pos)
    {
        List<Bubble> surroundingBubbles = new List<Bubble>();
        Vector2Int[] vecs = GetSurroundingBubblesGridPositions(pos);
        foreach (Vector2Int v in vecs)
        {
            if (ContainsKey(v))
                surroundingBubbles.Add(_bubbleMap[v]);
        }

        return surroundingBubbles.ToArray();
    }

    public void AddBubble(Bubble b)
    {
        _bubbles.Add(b);
        _bubbleMap.Add(b.gridPosition, b);
    }

    public bool Contains(Bubble b)
    {
        return _bubbles.Contains(b) && _bubbleMap.ContainsValue(b);
    }

    public bool ContainsKey(Vector2Int v)
    {
        return _bubbleMap.ContainsKey(v);
    }

    public bool CheckIfBubbleShouldFall(Bubble b)
    {
        return !HasBubblesAbove(b) && !HasConnectedBubblesOnLeft(b) && !HasConnectedBubblesOnRight(b);
    }

    public bool HasBubblesAbove(Bubble b)
    {
        if (b == null)
            return false;
        if (b.topRow)
            return true;

        Vector2Int v = new Vector2Int(b.gridPosition.x, b.gridPosition.y + 1);
        Vector2Int v1 = new Vector2Int(b.gridPosition.x + 1, b.gridPosition.y + 1);
        Vector2Int v2 = new Vector2Int(b.gridPosition.x - 1, b.gridPosition.y + 1);

        if (b.gridPosition.y % 2 == 0)
            return (ContainsKey(v) || ContainsKey(v1));
        else
            return (ContainsKey(v) || ContainsKey(v2));
        
    }

    public void DropLonelyBubbles()
    {
        StartCoroutine(DropLonelyBubblesCoroutine());
    }

    public Vector2Int WorldToGridPos(Vector3 worldPos)
    {
        Vector2Int gridPosition = Vector2Int.zero;
        if(worldPos.y == Mathf.Floor(worldPos.y))
        {
            // odd row number
            //Debug.Log("odd");
            gridPosition.y = (int)(2 * (worldPos.y - 5f)-1);
            gridPosition.x = (int)(2 * (worldPos.x + ODD_ROW_START_COL));
        }
        else
        {
            //even row number
            //Debug.Log("even");
            gridPosition.y = (int)(2 * (worldPos.y - 5.5f));
            gridPosition.x = (int)(2 * (worldPos.x + EVEN_ROW_START_COL));
        }

        return gridPosition;
    }

    public Vector3 GridToWorldPos(Vector2Int gridPos)
    {
        Vector3 worldPos = Vector3.zero;
        if (Mathf.Abs(gridPos.y) % 2 == 1)
        {
            //new row is odd row
            worldPos.x = (gridPos.x - 5.3f) / 2f;
            worldPos.y = (gridPos.y + 11f) / 2f;
        }
        else
        {
            //new row is even row
            worldPos.y = (gridPos.y + 11f) / 2f;
            worldPos.x = (gridPos.x - 4.8f) / 2f;

        }
        
        
        return worldPos;
    }

    public void ReadLevelLayout(int level)
    {
        string[] linesRaw = LevelManager.S.levels[level].rows;
        string[] lines = new string[linesRaw.Length];

        for (int i = 0; i < linesRaw.Length; i++)
        {

            lines[lines.Length - i - 1] = linesRaw[i];
        }

        for (int i = 0; i < lines.Length; i++)
        {
            for (int j = 0; j < lines[i].Length; j++)
            {

                if (lines[i][j] == '*')
                {
                    GameObject go = Instantiate(_bubblePrefab, Vector3.zero, Quaternion.identity);
                    Bubble b = go.GetComponent<Bubble>();
                    b.Initialize();
                    b.gridPosition = new Vector2Int(j, i);
                    Vector3 spawnPos = GridToWorldPos(new Vector2Int(j, i));
                    go.transform.position = spawnPos;
                    AddBubble(b);
                    go.name = string.Format("Bubble{0}: {1},{2}", i, b.gridPosition.x, b.gridPosition.y);
                    if (i == lines.Length - 2)
                        b.topRow = true;
                }
            }
        }
    }

    public bool RemoveBubble(Bubble b)
    {
        return _bubbles.Remove(b) && _bubbleMap.Remove(b.gridPosition);
    }

    public bool DestroyedAllBubbles()
    {
        return Count == 0;
    }

    public bool IsPaired(Bubble b, Bubble hitter)
    {
        Bubble[] adjacentBubbles = GetSurroundingBubbles(b.gridPosition);
        foreach (Bubble other in adjacentBubbles)
        {
            if (b.type == other.type && other != hitter)
                return true;
        }
        int counter = 0;
        adjacentBubbles = GetSurroundingBubbles(hitter.gridPosition);
        foreach (Bubble other in adjacentBubbles)
        {
            if (other.type == hitter.type)
                counter++;
        }
        if (counter > 1)
            return true;
        return false;
    }
    #endregion

    #region Private Methods
    private IEnumerator DropLonelyBubblesCoroutine()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < _bubbles.Count; i++)
        {
            
            if (_bubbles[i] != null && CheckIfBubbleShouldFall(_bubbles[i]))
            {
                Bubble b = _bubbles[i];
                if (b.state != BubbleState.Falling)
                {
                    b.FallDown();
                    Player.S.score++;
                    SoundManager.S.PlaySound(SoundType.Drop);
                    RemoveBubble(b);
                }
            }
            if (_bubbles[i] != null)
                CheckIfBubbleIsOutOfBounds(_bubbles[i]);
        }
        //Check is run twice because of possible race condition
        yield return new WaitForEndOfFrame();
        for (int i = _bubbles.Count - 1; i >= 0; i--)
        {
            if (_bubbles[i] != null && CheckIfBubbleShouldFall(_bubbles[i]))
            {
                Bubble b = _bubbles[i];
                if (b.state != BubbleState.Falling)
                {
                    b.FallDown();
                    Player.S.score++;
                    SoundManager.S.PlaySound(SoundType.Drop);
                    RemoveBubble(b);
                }
            }
        }
    }

    private bool HasConnectedBubblesOnLeft(Bubble b)
    {
        if (b.topRow || HasBubblesAbove(b))
            return true;
        else
        {
            Vector2Int vL = new Vector2Int(b.gridPosition.x - 1, b.gridPosition.y);
            if (GetBubbleByGridPosition(vL) != null) 
                return HasConnectedBubblesOnLeft(GetBubbleByGridPosition(vL));
        }
        return false;
    }

    private void CheckIfBubbleIsOutOfBounds(Bubble b)
    {
        if (Mathf.Abs(b.transform.position.x) > Player.RIGHT_BOUND)
            BlowBubble(b, b.type, false);
    }

    private bool HasConnectedBubblesOnRight(Bubble b)
    {
        if (b.topRow || HasBubblesAbove(b))
            return true;
        else
        {
            Vector2Int vR = new Vector2Int(b.gridPosition.x + 1, b.gridPosition.y);
            if (GetBubbleByGridPosition(vR) != null)
                return HasConnectedBubblesOnRight(GetBubbleByGridPosition(vR));
        }
        return false;
    }
    #endregion
}
