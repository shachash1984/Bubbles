using UnityEngine;
using System;
using DG.Tweening;

public class PlayerInput : MonoBehaviour {

    #region Public Fields
    static public PlayerInput S;
    public Action OnTouchUp;
    #endregion

    #region Private Fields
    private Camera _camera;
    private Vector3 _currentWorldPosition;

    #endregion

    #region Constants
    private const float MIN_TOUCH_HEIGHT = 0.25f;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (S != null)
            Destroy(gameObject);
        S = this;
        _camera = Camera.main;
        
    }

    private void Update()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (Input.GetMouseButton(0) &&
            Player.S.GetCurrentBubble().state != BubbleState.Shooting &&
            Input.mousePosition.y > Screen.height * MIN_TOUCH_HEIGHT &&
            Player.S.GetCurrentBubble() &&
            !UIHandler.S.endLevelPanelIsDisplayed &&
            !DOTween.IsTweening(Player.S.GetCurrentBubble().transform))
        {
            Player.S.DrawBeam(Player.S.Aim(GetCurrentWorldPosition()));
        }
        else if (Input.GetMouseButtonUp(0) &&
            Player.S.GetCurrentBubble().state != BubbleState.Shooting &&
            Input.mousePosition.y > Screen.height * MIN_TOUCH_HEIGHT &&
            Player.S.GetCurrentBubble() &&
            !UIHandler.S.endLevelPanelIsDisplayed &&
            !DOTween.IsTweening(Player.S.GetCurrentBubble().transform)) 
        {
            if (OnTouchUp != null)
                OnTouchUp();
        }
        else if (Input.GetMouseButton(0) && Input.mousePosition.y <= Screen.height * MIN_TOUCH_HEIGHT)
            Beam.S.ResetBeam();

#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 1 && Player.S.GetCurrentBubble().state != BubbleState.Shooting &&
            Input.GetTouch(0).position.y > Screen.height * MIN_TOUCH_HEIGHT &&
            Player.S.GetCurrentBubble() &&
            !UIHandler.S.endLevelPanelIsDisplayed &&
            !DOTween.IsTweening(Player.S.GetCurrentBubble().transform))
        {
            Player.S.DrawBeam(Player.S.Aim(GetCurrentWorldPosition()));
            if (Input.GetTouch(0).phase == TouchPhase.Ended && Player.S.GetCurrentBubble().state != BubbleState.Shooting && !DOTween.IsTweening(Player.S.GetCurrentBubble().transform))
            {
                if (OnTouchUp != null)
                OnTouchUp();
            }
                
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).position.y <= Screen.height * MIN_TOUCH_HEIGHT)
            Beam.S.ResetBeam();
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
#endif
    }
    #endregion

    #region Public Methods
    public Vector3 GetCurrentWorldPosition()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        _currentWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
#elif UNITY_ANDROID || UNITY_IOS
        _currentWorldPosition = _camera.ScreenToWorldPoint(Input.GetTouch(0).position);
#endif
        _currentWorldPosition.z = Player.S.initialPosition.z;
        return _currentWorldPosition;
    }
#endregion

#region Private Methods

#endregion
}
