using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour {

    #region Public Fields
    static public Beam S;
    public Vector3[] beamPath;
    public const int MAX_DOTS = 60;
    #endregion

    #region Private Fields
    [SerializeField] private GameObject _dotPrefab;
    private List<GameObject> dots = new List<GameObject>();
    private LineRenderer _line;
    private Material _lineMaterial;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
        _line = GetComponent<LineRenderer>();
        _lineMaterial = _line.material;
    }
    #endregion

    #region Public Methods
    public void SetBeamPositions(Vector3[] newPositions)
    {
        beamPath = newPositions;
        _line.SetPositions(beamPath);
        CreateDots();
        SpreadDots();
        
    }

    public void ResetBeam()
    {
        for (int i = 0; i < _line.positionCount; i++)
        {
            _line.SetPosition(i, Player.S.initialPosition);
        }
        HideDots();
        Player.S.GetCurrentBubble().SetState(BubbleState.Idle);
    }

    #endregion

    #region Private Methods
    private int CalculateDotAmount()
    {
        float dotScale = _dotPrefab.transform.localScale.x;
        int lineDistance = (int)(Vector3.Distance(_line.GetPosition(0), _line.GetPosition(1)) + Vector3.Distance(_line.GetPosition(1), _line.GetPosition(2)) + Vector3.Distance(_line.GetPosition(2), _line.GetPosition(3)));
        int dotAmount = lineDistance*4 / (int)dotScale;
        return dotAmount;
    }

    private void CreateDots()
    {
        int neededDotAmount = CalculateDotAmount();
        if(dots.Count < neededDotAmount && dots.Count < MAX_DOTS)
        {
            Vector3 hidePos = new Vector3(0f, -10f, 0f);
            while (dots.Count < MAX_DOTS)
            {
                GameObject go = Instantiate(_dotPrefab, hidePos, Quaternion.identity);
                dots.Add(go);
            }
        }
    }
    
    private void SpreadDots()
    {
        List<GameObject> displayedDots = new List<GameObject>();
        int neededDotAmount = CalculateDotAmount();
        neededDotAmount = Mathf.Clamp(neededDotAmount, 0, MAX_DOTS);
        int j = 0;
        int k = 0;
        Vector3 dotPos = Vector3.zero;
        for (int i = 0; i < neededDotAmount; i++)
        {
            dotPos = _line.GetPosition(0) + _line.GetPosition(1).normalized * (float)i / 2;
            dots[i].SetActive(true);

            if (dotPos.y >= _line.GetPosition(1).y)
            {
                dotPos = _line.GetPosition(1) + (_line.GetPosition(2) - _line.GetPosition(1)).normalized * (float)j++ / 2;

                if (dotPos.y >= _line.GetPosition(2).y)
                    dotPos = _line.GetPosition(2) + (_line.GetPosition(3) - _line.GetPosition(2)).normalized * (float)k++ / 2;
            }
            dots[i].transform.position = dotPos;
            if (i % 2 == 0)
                dots[i].GetComponent<Animator>().Play("LineDotReverseAnimation");
            else
                dots[i].GetComponent<Animator>().Play("LineDotAnimation");
            displayedDots.Add(dots[i]);
            
        }
        Vector3 hidePos = new Vector3(0f, -10f, 0f);
        for (int i = 0; i < dots.Count; i++)
        {
            if (!displayedDots.Contains(dots[i]))
            {
                dots[i].SetActive(false);
                dots[i].transform.position = hidePos;
            }
                
        }
    }

    private void HideDots()
    {
        Vector3 hidePos = new Vector3(0f, -10f, 0f);
        for (int i = 0; i < dots.Count; i++)
        {
            dots[i].SetActive(false);
            dots[i].transform.position = hidePos;
        }
    }
    #endregion

}
