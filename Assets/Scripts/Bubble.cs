using UnityEngine;
using System;
using DG.Tweening;

[Flags] public enum BubbleType { Red, Green, Blue, Cyan, Magenta, Black}

public enum BubbleState { Idle, Aiming, Shooting, OnGrid, Falling}

public class Bubble : MonoBehaviour {

    #region Public Fields
    public BubbleState state { get; private set; }
    public BubbleType type { get { return _type; } private set { _type = value; } }
    public Vector2Int gridPosition;
    public bool popped = false;
    public bool topRow = false;
    #endregion

    #region Private Fields
    [SerializeField] private BubbleType _type;
    private Rigidbody _rigidBody;
    #endregion

    #region Constants
    public const float BUBBLE_SCALE = 0.5f;
    
    #endregion

    #region MonoBehaviour Callbacks
    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void OnBecameInvisible()
    {
        if (BubbleHandler.S.Contains(this))
            BubbleHandler.S.RemoveBubble(this);
        /*if (this == Player.S.GetCurrentBubble() && Mathf.Abs(transform.position.x) > 2.8f)
            Player.S.SetCurrentBubble(Player.S.CreateCurrentBubble(false));*/
        Destroy(gameObject);
    }


    #endregion

    #region Public Methods
    public void Initialize()
    {
        int rand = UnityEngine.Random.Range(0, BubbleHandler.S.colors.Length);
        GetComponent<Renderer>().material.color = BubbleHandler.S.colors[rand];
        SetType((BubbleType)rand);
        if (!_rigidBody)
            _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        SetState(BubbleState.OnGrid);
    }

    public void SetGridPosition(int x, int y)
    {
        SetGridPosition(new Vector2Int(x, y));
    }

    public void SetGridPosition(Vector2Int vec)
    {
        gridPosition = vec;
    }

    public void StopVelocity()
    {
        if (!_rigidBody)
            _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.velocity = Vector3.zero;
    }

    public void SetLayer(int layerNum)
    {
        gameObject.layer = layerNum;
    }

    public void SetState(BubbleState newState)
    {
        state = newState;
    }

    public void SetType(BubbleType newType)
    {
        _type = newType;
    }

    public void SnapToPosition(Vector3 newPos)
    {
        transform.DOMove(newPos, 0.1f).OnComplete(() =>
        {
            if (transform.position != newPos)
                transform.position = newPos;
        }).SetEase(Ease.Linear);
    }

    public void FallDown()
    {
        if (!_rigidBody)
            _rigidBody.GetComponent<Rigidbody>();
        _rigidBody.constraints = RigidbodyConstraints.None;
        _rigidBody.useGravity = true;
    }

    public void DoWobble(Vector3 hitPos)
    {
        Vector3 currentPos = transform.position;
        transform.DOPath(new Vector3[] { (currentPos + (currentPos - hitPos) * 0.1f) , currentPos }, 0.25f);
    }


    
    #endregion

}
