using UnityEngine;  
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class UIHandler : MonoBehaviour {

    #region Public Fields
    static public UIHandler S;
    public bool endLevelPanelIsDisplayed = false;
    #endregion

    #region Private Fields
    private Text _scoreText;
    private Text _levelText;
    private Button _switchButton;
    private CanvasGroup _endLevelPanel;
    private Button _playAgainButton;
    private Button _playNextButton;
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private Transform[] starPositions;
    #endregion

    #region Monobehaviour Callbacks
    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
        Initialize();
        

    }

    private void Start()
    {
        ToggleUIItem(_endLevelPanel, false, true);
        SetScoreText(Player.S.score);
        SetLevelText(Player.S.level);
        Player.OnGainedStar += AddStar;
        Player.OnLevelFinished += FinishLevel;
    }

    private void OnDisable()
    {
        Player.OnGainedStar -= AddStar;
        Player.OnLevelFinished -= FinishLevel;
    }
    #endregion

    #region Public Methods
    public void SetScoreText(int score)
    {
        _scoreText.text = score.ToString();
    }

    public void SetLevelText(int level)
    {
        _levelText.text = level.ToString();
    }

    public void AddStar()
    {
        GameObject go = Instantiate(starPrefab, new Vector3(transform.position.x, -20f, transform.position.z), Quaternion.identity, transform);
        Sequence seq = DOTween.Sequence();
        go.transform.GetChild(0).gameObject.SetActive(true);
        go.transform.localScale = Vector3.one;
        seq.Append(go.transform.DOMove(new Vector3(0f, 4f, transform.position.z + 5f), 1f));
        seq.Append(go.transform.DORotate(new Vector3(0f, 720f, 0f), 1.5f, RotateMode.FastBeyond360)).OnComplete(() =>
        {
            go.transform.DOScale(0.2f, 1f);
            go.transform.DOMove(starPositions[Player.S.stars - 1].position, 1f);
            go.transform.GetChild(0).GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        });
        seq.Play();

    }

    public void FinishLevel(int finishedLevel)
    {
        StartCoroutine(FinishLevelCoroutine(finishedLevel));
    }

    public void ToggleEndLevelPanel(bool on)
    {
        ToggleUIItem(_endLevelPanel, on, false);
        transform.DOMove(transform.position, 0.1f).OnComplete(() => endLevelPanelIsDisplayed = on);
        
    }
    #endregion

    #region Private Methods
    private IEnumerator FinishLevelCoroutine(int finishedLevel)
    {
        yield return null;
        Debug.Log("Finished Level");
        ToggleEndLevelPanel(true);
    }

    private void ToggleUIItem(CanvasGroup cv, bool show, bool immediate = false)
    {
        if (immediate)
        {
            if (show)
            {
                cv.gameObject.SetActive(true);
                cv.alpha = 1f;
                cv.blocksRaycasts = true;
            }
            else
            {
                cv.alpha = 0f;
                cv.blocksRaycasts = false;
                cv.gameObject.SetActive(false);
            }
        }
        else
        {
            if (show)
            {
                cv.gameObject.SetActive(true);
                cv.DOFade(1, 0.75f).SetEase(Ease.OutQuad);
                cv.blocksRaycasts = true;
            }
            else
            {
                cv.DOFade(0, 0.75f).SetEase(Ease.OutQuad);
                cv.blocksRaycasts = false;
                cv.gameObject.SetActive(false);
            }
        }

    }

    private void Initialize()
    {
        _scoreText = transform.GetChild(0).GetChild(3).GetComponent<Text>();
        _levelText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        _switchButton = GameObject.Find("SwitchButton").GetComponent<Button>();
        _switchButton.onClick.RemoveAllListeners();
        _switchButton.onClick.AddListener(() => Player.S.SwitchBubble());
        for (int i = 0; i < 3; i++)
        {
            starPositions[i] = transform.GetChild(3).GetChild(i).transform;
        }
        _endLevelPanel = transform.GetChild(4).GetComponent<CanvasGroup>();
        _playAgainButton = _endLevelPanel.transform.GetChild(0).GetComponent<Button>();
        _playNextButton = _endLevelPanel.transform.GetChild(1).GetComponent<Button>();
        _playAgainButton.onClick.RemoveAllListeners();
        _playAgainButton.onClick.AddListener(() =>
        {
            LevelManager.S.LoadNextLevel(Player.S.level - 2);
            ToggleEndLevelPanel(false);
        });

        _playNextButton.onClick.RemoveAllListeners();
        _playNextButton.onClick.AddListener(() =>
        {
            LevelManager.S.LoadNextLevel((Player.S.level - 1) % LevelManager.S.levels.Length);
            ToggleEndLevelPanel(false);
        });
    }
    #endregion
}
