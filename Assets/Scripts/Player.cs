using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour {


    #region Public Fields
    static public Player S;
    public Vector3 initialPosition;
    public int score;
    public int level = 1;
    public int stars
    {
        get
        {
            return _stars;
        }
        set
        {
            if (value >= 0 && value <= 3)
                _stars = value;
            else
                Debug.LogError("Cannot have less than 0 or more than 3 stars");
        }
    }

    #endregion

    #region Private Fields
    [SerializeField] private int _stars;
    private int _score;
    private int _level;
    private int _movesLeft;
    private bool _reachedGoal = false;
    private List<Boost> _boosts;
    private Boost _currentBoost;
    private List<Coin> _coins;
    private Goal _goal;
    [SerializeField] private Bubble _currentBubble;
    [SerializeField] private Bubble _nextBubble;
    private Camera _camera;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private GameObject _bubblePrefab;

    //Helper vectors to calculate the intersection point
    //between the beam and the ceiling/walls. called only if there is no bubble in the way
    private Vector2 leftUpperBound = new Vector2(LEFT_BOUND, UPPER_BOUND);
    private Vector2 rightUpperBound = new Vector2(RIGHT_BOUND, UPPER_BOUND);
    private Vector2 lowerRightBound = new Vector2(RIGHT_BOUND, 0);
    private Vector2 upperRightBound = new Vector2(RIGHT_BOUND, UPPER_BOUND);
    private Vector2 lowerLeftBound = new Vector2(LEFT_BOUND, 0);
    private Vector2 upperLeftBound = new Vector2(LEFT_BOUND, UPPER_BOUND);
    #endregion

    #region Constants
    public const float RIGHT_BOUND = 2.75f;
    public const float LEFT_BOUND = -2.75f;
    public const float UPPER_BOUND = 9.0f;
    public const float TOP_ROW_Y = 8f;
    public const float MIN_CLOSEST_BUBBLE_DISTANCE = 0.5f;
    #endregion

    #region Events and Delegates
    public static event System.Action OnGainedStar;
    public static event System.Action<int> OnLevelFinished;
    #endregion

    #region Monobehaviour Callbacks
    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
        _camera = Camera.main;
    }

    private void Start()
    {
        PlayerInput.S.OnTouchUp += Shoot;
        PlayerInput.S.OnTouchUp += SetCurrentBubbleStateToShooting;
        SetCurrentBubble(CreateCurrentBubble(true));
        UIHandler.S.SetLevelText(level);
    }

    private void OnDisable()
    {
        PlayerInput.S.OnTouchUp -= Shoot;
        PlayerInput.S.OnTouchUp -= SetCurrentBubbleStateToShooting;
    }

    #endregion

    #region Public Methods
    public Bubble GetCurrentBubble()
    {
        return _currentBubble;
    }

    public void SetCurrentBubbleStateToShooting()
    {
        _currentBubble.SetState(BubbleState.Shooting);
    }

    public void DrawBeam(Vector3[] beamPositions)
    {
        Beam.S.SetBeamPositions(beamPositions);
    }

    public Vector3[] Aim(Vector3 touchPos)
    {
        //setting the state of the current bubble
        _currentBubble.SetState(BubbleState.Aiming);

        //calculating the second ray point
        Vector3 angleCalculationVector = new Vector3(touchPos.x, 0f, 0f);
        float angle = Vector3.SignedAngle(angleCalculationVector, touchPos, Vector3.forward);
        float cos = Mathf.Cos(Mathf.Deg2Rad * angle);
        float xBound = touchPos.x >= initialPosition.x ? RIGHT_BOUND : LEFT_BOUND;
        float hypotenuse = xBound / cos;
        float wallY = Mathf.Sqrt(Mathf.Pow(hypotenuse, 2) - Mathf.Pow(xBound, 2));
        Vector3 secondRayPoint = new Vector3(xBound, wallY, initialPosition.z);
        RaycastHit hit = new RaycastHit();

        //if hit a bubble
        if (Physics.Raycast(initialPosition, secondRayPoint, out hit, 12f, _layerMask))
        {
            secondRayPoint = hit.point;
            BubbleHandler.S.SetTargetBubble(hit.collider.GetComponent<Bubble>());

            return new Vector3[] { initialPosition, secondRayPoint, secondRayPoint, secondRayPoint };
        }
        //if didnt hit a bubble and is exceeding the upper bound
        else if (secondRayPoint.y > UPPER_BOUND)
        {
            Vector2 firstPoint2D = new Vector2(initialPosition.x, initialPosition.y);
            Vector2 secondPoint2D = new Vector2(secondRayPoint.x, secondRayPoint.y);
            bool found = false;
            Vector2 intersection = GetIntersectionPointCoordinates(firstPoint2D, secondPoint2D, leftUpperBound, rightUpperBound, out found);
            if(found)
            {
                secondRayPoint = new Vector3(intersection.x, intersection.y, 0);
            }
            else
            {
                secondRayPoint = Vector3.ClampMagnitude(secondRayPoint, UPPER_BOUND);
            }
            
            return new Vector3[] { initialPosition, secondRayPoint, secondRayPoint, secondRayPoint };
            
        }
        
        //calculating the third ray point
        Vector3 thirdRayPoint = secondRayPoint;

        //if didnt hit a bubble
        if (hit.point == Vector3.zero)
        {
            Vector3 angleCalculationVector2 = new Vector3(-xBound, 0f, 0f);
            float cos2 = Mathf.Cos(Mathf.Deg2Rad * angle);
            float hypotenuse2 = 2*xBound / cos2;
            float oppositeY2 = Mathf.Sqrt(Mathf.Pow(hypotenuse2, 2) - Mathf.Pow(2*xBound, 2));
            thirdRayPoint = new Vector3(-xBound, oppositeY2, initialPosition.z);
            Ray ray = new Ray(secondRayPoint, thirdRayPoint - secondRayPoint);
            //if hit a bubble
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 12f, _layerMask))
            {
                thirdRayPoint = hit.point;
                BubbleHandler.S.SetTargetBubble(hit.collider.GetComponent<Bubble>());
                return new Vector3[] { initialPosition, secondRayPoint, thirdRayPoint, thirdRayPoint };
            }
            //if didnt hit a bubble and exceeding the upper bound
            else if (thirdRayPoint.y > UPPER_BOUND)
            {
                Vector2 firstPoint2D = new Vector2(secondRayPoint.x, secondRayPoint.y);
                Vector2 secondPoint2D = new Vector2(thirdRayPoint.x, thirdRayPoint.y);
                bool found = false;
                Vector2 intersection = GetIntersectionPointCoordinates(firstPoint2D, secondPoint2D, leftUpperBound, rightUpperBound, out found);
                if (found)
                {
                    thirdRayPoint = new Vector3(intersection.x, intersection.y, 0);
                }
                else
                    thirdRayPoint = Vector3.ClampMagnitude(thirdRayPoint, UPPER_BOUND);

                return new Vector3[] { initialPosition, secondRayPoint, thirdRayPoint, thirdRayPoint };
            }
        }

        Vector3 fourthRayPoint = thirdRayPoint;
        //if didnt hit a bubble
        if(hit.point == Vector3.zero)
        {
            float reflectionAngle = xBound == RIGHT_BOUND ? -1 * (90f + angle) : 90f - angle;
            float ceilY = UPPER_BOUND - thirdRayPoint.y;
            float tan = Mathf.Tan(Mathf.Deg2Rad * reflectionAngle);
            float ceilX = xBound == RIGHT_BOUND ? xBound + Mathf.Abs(tan * ceilY) : xBound - Mathf.Abs(tan * ceilY);
            fourthRayPoint = new Vector3(ceilX, UPPER_BOUND, initialPosition.z);
            Ray ray = new Ray(thirdRayPoint, fourthRayPoint - thirdRayPoint);
            //if hit bubble
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 12f, _layerMask))
            {
                fourthRayPoint = hit.point;
                BubbleHandler.S.SetTargetBubble(hit.collider.GetComponent<Bubble>());
            }
            //if exceeding upper bound or side bound
            else if (fourthRayPoint.y > UPPER_BOUND || Mathf.Abs(fourthRayPoint.x) > RIGHT_BOUND)
            {
                //if exceeding upper bound
                if(fourthRayPoint.y > UPPER_BOUND)
                {
                    Vector2 firstPoint2D = new Vector2(thirdRayPoint.x, thirdRayPoint.y);
                    Vector2 secondPoint2D = new Vector2(fourthRayPoint.x, fourthRayPoint.y);
                    bool found = false;
                    Vector2 intersection = GetIntersectionPointCoordinates(firstPoint2D, secondPoint2D, leftUpperBound, rightUpperBound, out found);
                    if (found)
                    {
                        fourthRayPoint = new Vector3(intersection.x, intersection.y, 0);
                    }
                    else
                        fourthRayPoint = Vector3.ClampMagnitude(fourthRayPoint, UPPER_BOUND);
                }
                //if exceeding side bound
                if(Mathf.Abs(fourthRayPoint.x) > RIGHT_BOUND)
                {
                    Vector2 firstPoint2D = new Vector2(thirdRayPoint.x, thirdRayPoint.y);
                    Vector2 secondPoint2D = new Vector2(fourthRayPoint.x, fourthRayPoint.y);
                    bool isOnRightSide = touchPos.x > 0;
                    bool found = false;
                    //if exceeding right side
                    if (isOnRightSide)
                    {
                        Vector2 intersection = GetIntersectionPointCoordinates(firstPoint2D, secondPoint2D, lowerRightBound, upperRightBound, out found);
                        if (found)
                        {
                            fourthRayPoint = new Vector3(intersection.x, intersection.y, 0);
                        }
                        else
                            fourthRayPoint = Vector3.ClampMagnitude(fourthRayPoint, RIGHT_BOUND);
                    }
                    //if exceeding left side
                    else
                    {
                        Vector2 intersection = GetIntersectionPointCoordinates(firstPoint2D, secondPoint2D, lowerLeftBound, upperLeftBound, out found);
                        if (found)
                        {
                            fourthRayPoint = new Vector3(intersection.x, intersection.y, 0);
                        }
                        else
                            fourthRayPoint = Vector3.ClampMagnitude(fourthRayPoint, LEFT_BOUND);
                    }
                }
            }
        }
        Debug.DrawRay(initialPosition, secondRayPoint, Color.blue);
        Debug.DrawRay(secondRayPoint, thirdRayPoint - secondRayPoint, Color.blue);
        Debug.DrawRay(thirdRayPoint, fourthRayPoint - thirdRayPoint, Color.blue);
        return new Vector3[] { initialPosition, secondRayPoint, thirdRayPoint, fourthRayPoint };
    }

    public Bubble GetClosestBubble(Vector3 pos, bool limit)
    {
        float minDistance = float.PositiveInfinity;
        float distance = 0f;
        Bubble b = null;
        for (int i = 0; i < BubbleHandler.S.Count; i++)
        {
            if (BubbleHandler.S[i] == null)
                continue;
            distance = Vector3.Distance(BubbleHandler.S[i].transform.position, pos);
            if (distance < minDistance)
            {
                minDistance = distance;
                b = BubbleHandler.S[i];
            }
        }
        if (limit)
        {
            if (minDistance <= MIN_CLOSEST_BUBBLE_DISTANCE)
                return b;
            else
                return null;
        }
        else
            return b;
        
    }

    public void Shoot()
    {
        Shoot(Beam.S.beamPath);
        Beam.S.ResetBeam();
    }

    public void Shoot(Vector3[] path)
    {
        //getting the closest bubble to the hit point
        Bubble closestBubble = GetClosestBubble(path[path.Length - 1], true);
        Vector3 hitPosition = path[path.Length - 1];

        //if the closest bubble is not null
        if (closestBubble)
        {
            path[path.Length - 1] = BubbleHandler.S.GetWantedPositionSimple(path[path.Length - 1], BubbleHandler.S.targetBubble);
            _currentBubble.SetState(BubbleState.Shooting);
            _currentBubble.transform.DOPath(path, 0.5f).OnComplete(() =>
            {
                _currentBubble.gridPosition = BubbleHandler.S.WorldToGridPos(path[path.Length - 1]);
                _currentBubble.gameObject.layer = 9;
                _currentBubble.GetComponent<Rigidbody>().velocity = Vector3.zero;
                _currentBubble.SetState(BubbleState.OnGrid);
                BubbleHandler.S.AddBubble(_currentBubble);
                _currentBubble.SnapToPosition(path[path.Length - 1]);

                if (BubbleHandler.S.HitBubble(_currentBubble))
                {
                    BubbleHandler.S.DropLonelyBubbles();
                    CheckForStar(score, level);
                }
                StartCoroutine(CheckLevelStatus());
                
            }).SetEase(Ease.Linear);
        }
        else //if the hit was far from any bubbles
        {
            bool isLonely = false;
            _currentBubble.SetState(BubbleState.Shooting);
            if (path[path.Length - 1].y >= TOP_ROW_Y)
            {
                path[path.Length - 1].y = TOP_ROW_Y;
                _currentBubble.topRow = true;
                Bubble closest = GetClosestBubble(path[path.Length - 1], false);
                Vector2Int closestGridPos = closest.gridPosition;
                
                while (BubbleHandler.S.Contains(BubbleHandler.S.GetBubbleByGridPosition(new Vector2Int(closestGridPos.x, closestGridPos.y))))
                {
                    path[path.Length - 1] = BubbleHandler.S.GetWantedPositionSimple(path[path.Length - 1], closest);
                    closestGridPos = BubbleHandler.S.WorldToGridPos(path[path.Length - 1]);
                    closest = BubbleHandler.S.GetBubbleByGridPosition(closestGridPos);
                }
                path[path.Length - 1] = BubbleHandler.S.GridToWorldPos(closestGridPos);
            }
            if (path[path.Length - 1].y < TOP_ROW_Y)
            {
                _currentBubble.topRow = false;
                isLonely = true;
            }
            
            _currentBubble.transform.DOPath(path, 0.5f).OnComplete(() =>
            {
                _currentBubble.SetLayer(9);
                _currentBubble.StopVelocity();
                _currentBubble.SetState(BubbleState.OnGrid);
                _currentBubble.gridPosition = BubbleHandler.S.WorldToGridPos(path[path.Length - 1]);
                BubbleHandler.S.AddBubble(_currentBubble);
                if (isLonely)
                    BubbleHandler.S.BlowBubble(_currentBubble, _currentBubble.type, false);
                Bubble closest = GetClosestBubble(path[path.Length - 1], true);
                if (closest)
                {
                    if (_currentBubble.topRow && BubbleHandler.S.IsPaired(closest, _currentBubble))
                        BubbleHandler.S.BlowBubble(_currentBubble, _currentBubble.type, true);
                    StartCoroutine(CheckLevelStatus());
                }
                else
                {
                    if (_currentBubble.topRow && BubbleHandler.S.IsPaired(_currentBubble, _currentBubble))
                        BubbleHandler.S.BlowBubble(_currentBubble, _currentBubble.type, true);
                    StartCoroutine(CheckLevelStatus());
                }
            });
        }
    }

    public void SetCurrentBubble(Bubble b)
    {
        _currentBubble = b;
    }

    public Bubble CreateNextBubble()
    {
        GameObject go = Instantiate(_bubblePrefab, new Vector3(0.95f, -0.75f, 0f), Quaternion.identity);
        go.transform.localScale = Vector3.one * 0.25f;
        int rand = Random.Range(0, BubbleHandler.S.colors.Length);
        go.GetComponent<Renderer>().material.color = BubbleHandler.S.colors[rand];
        Bubble b = go.GetComponent<Bubble>();
        b.SetType((BubbleType)rand);
        b.SetState(BubbleState.Idle);
        return b;
    }

    public Bubble CreateCurrentBubble(bool start)
    {
        if (start)
        {
            if (_currentBubble)
                Destroy(_currentBubble.gameObject);
            GameObject go = Instantiate(_bubblePrefab, Vector3.zero, Quaternion.identity);
            int rand = Random.Range(0, BubbleHandler.S.colors.Length);
            go.GetComponent<Renderer>().material.color = BubbleHandler.S.colors[rand];
            Bubble b = go.GetComponent<Bubble>();
            b.SetType((BubbleType)rand);
            b.SetState(BubbleState.Idle);
            _nextBubble = CreateNextBubble();
            return b;
        }

        _nextBubble.transform.DOMove(Vector3.zero, 0.5f);
        _nextBubble.transform.DOScale(0.5f, 0.5f);
        _currentBubble = _nextBubble;
        _nextBubble = CreateNextBubble();
        return _currentBubble;
    }

    public void SwitchBubble()
    {
        _nextBubble.transform.DOMove(Vector3.zero, 0.5f);
        _nextBubble.transform.DOScale(0.5f, 0.5f);
        _currentBubble.transform.DOMove(new Vector3(0.95f, -0.75f, 0f), 0.5f);
        _currentBubble.transform.DOScale(0.25f, 0.25f);
        Bubble temp = _currentBubble;
        _currentBubble = _nextBubble;
        _nextBubble = temp;
    }

    /// <summary>
    /// Gets the coordinates of the intersection point of two lines.
    /// </summary>
    /// <param name="A1">A point on the first line.</param>
    /// <param name="A2">Another point on the first line.</param>
    /// <param name="B1">A point on the second line.</param>
    /// <param name="B2">Another point on the second line.</param>
    /// <param name="found">Is set to false of there are no solution. true otherwise.</param>
    /// <returns>The intersection point coordinates. Returns Vector2.zero if there is no solution.</returns>
    public Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
    {
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

        if (tmp == 0)
        {
            // No solution!
            found = false;
            return Vector2.zero;
        }

        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        found = true;

        return new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
        );
    }
    #endregion

    #region Private Methods
    private void AddScore(int scoreToAdd)
    {
        _score += scoreToAdd;
    }

    private void SetBoost(Boost b)
    {
        _currentBoost = b;
    }

    private void CheckForStar(int currentScore, int currentLevel)
    {
        if (stars < 3)
        {
            if (LevelManager.S.EntitledToStar(currentScore, currentLevel-1, stars))
            {
                if (OnGainedStar != null)
                    OnGainedStar();
                stars++;
                //UIHandler:
                    //show new star in the center
                    //animate new star to position
                    //add star
            }
        }
    }

    private IEnumerator CheckLevelStatus()
    {
        yield return new WaitForSeconds(0.1f);
        if (!CheckIfLevelIsFinished())
        {
            SetCurrentBubble(CreateCurrentBubble(false));
            BubbleHandler.S.DropLonelyBubbles();
            yield return new WaitForSeconds(0.1f);
            if (CheckIfLevelIsFinished())
            {
                if (OnLevelFinished != null)
                    OnLevelFinished(level);
                level++;
            }
        }
        else
        {
            if (OnLevelFinished != null)
                OnLevelFinished(level);
            level++;
        }
    }

    private bool CheckIfLevelIsFinished()
    {
        return BubbleHandler.S.DestroyedAllBubbles();
    }
    #endregion
}
