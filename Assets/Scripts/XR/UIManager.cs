using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The ARCameraManager which will produce frame events.")]
    ARCameraManager m_CameraManager;

    /// <summary>
    /// Get or set the <c>ARCameraManager</c>.
    /// </summary>
    public ARCameraManager cameraManager
    {
        get { return m_CameraManager; }
        set
        {
            if (m_CameraManager == value)
                return;

            if (m_CameraManager != null)
                m_CameraManager.frameReceived -= FrameChanged;

            m_CameraManager = value;

            if (m_CameraManager != null & enabled)
                m_CameraManager.frameReceived += FrameChanged;
        }
    }

    const string k_FadeOffAnim = "FadeOff";
    const string k_FadeOnAnim = "FadeOn";

    [SerializeField]
    ARPlaneManager m_PlaneManager;

    public ARPlaneManager planeManager
    {
        get { return m_PlaneManager; }
        set { m_PlaneManager = value; }
    }

    public PlaceMultipleObjectsOnPlane objectPlacer;
    public void SetPlaneDetection(bool state)
    {
        m_PlaneManager.enabled = state;// !m_PlaneManager.enabled;

        if (m_PlaneManager.enabled)
        {
           // planeDetectionMessage = "Disable Plane Detection and Hide Existing";
            SetAllPlanesActive(true);
        }
        else
        {
           // planeDetectionMessage = "Enable Plane Detection and Show Existing";
            SetAllPlanesActive(false);
        }

    }

    void SetAllPlanesActive(bool value)
    {
        foreach (var plane in m_PlaneManager.trackables)
            plane.gameObject.SetActive(value);
    }
    int _state = 0;
    void setState(int state)
    {
        switch (state)
        {
            case 0: //detection state
                {
                    if(!m_ShowingMoveDevice)
                    {
                        if (moveDeviceAnimation)
                            moveDeviceAnimation.SetTrigger(k_FadeOnAnim);
                    }
                    m_ShowingMoveDevice = true;
                    if (m_ShowingTapToPlace)
                    {
                        if (tapToPlaceAnimation)
                            tapToPlaceAnimation.SetTrigger(k_FadeOffAnim);
                    }
                    m_ShowingTapToPlace = false;
                    SetPlaneDetection(true);
                    if (objectPlacer != null) objectPlacer.clearObjects();
                    objectPlacer.SetMode(TouchReactionMode.Off);
                    _state = 0;
                    break;
                }
                case 1: //placing phase
                {
                    if (m_ShowingMoveDevice)
                    {
                        if (moveDeviceAnimation)
                            moveDeviceAnimation.SetTrigger(k_FadeOffAnim);
                    }
                    m_ShowingMoveDevice = false;
                    if (!m_ShowingTapToPlace)
                    {
                        if (tapToPlaceAnimation)
                            tapToPlaceAnimation.SetTrigger(k_FadeOnAnim);
                    }
                    m_ShowingTapToPlace = true;
                    SetPlaneDetection(true);
                    if (objectPlacer != null&&state==2) objectPlacer.clearObjects();
                    objectPlacer.SetMode(TouchReactionMode.Placing);
                    _state = 1;
                    break;
                }
            case 2:
                {
                    if (m_ShowingMoveDevice)
                    {
                        if (moveDeviceAnimation)
                            moveDeviceAnimation.SetTrigger(k_FadeOffAnim);
                    }
                    m_ShowingMoveDevice = false;
                    if (m_ShowingTapToPlace)
                    {
                        if (tapToPlaceAnimation)
                            tapToPlaceAnimation.SetTrigger(k_FadeOffAnim);
                    }
                    m_ShowingTapToPlace = false;
                    SetPlaneDetection(false);
                    objectPlacer.SetMode(TouchReactionMode.Zooming);
                    _state = 2;
                    break;
                }
            default:
                {
                    Debug.LogFormat("Unknown state {0}",state);
                    break;
                }
        }

    }

    public void PushReset()
    {
        setState(0);
    }
    [SerializeField]
    Animator m_MoveDeviceAnimation;

    public Animator moveDeviceAnimation
    {
        get { return m_MoveDeviceAnimation; }
        set { m_MoveDeviceAnimation = value; }
    }

    [SerializeField]
    Animator m_TapToPlaceAnimation;

    public Animator tapToPlaceAnimation
    {
        get { return m_TapToPlaceAnimation; }
        set { m_TapToPlaceAnimation = value; }
    }

    static List<ARPlane> s_Planes = new List<ARPlane>();

    bool m_ShowingTapToPlace = false;

    bool m_ShowingMoveDevice = true;

    void OnEnable()
    {
        if (m_CameraManager != null)
            m_CameraManager.frameReceived += FrameChanged;

        PlaceMultipleObjectsOnPlane.onPlacedObject += PlacedObject;
    }

    void OnDisable()
    {
        if (m_CameraManager != null)
            m_CameraManager.frameReceived -= FrameChanged;

        PlaceMultipleObjectsOnPlane.onPlacedObject -= PlacedObject;
    }

    void FrameChanged(ARCameraFrameEventArgs args)
    {
        /* if (PlanesFound() && m_ShowingMoveDevice)
         {
             if (moveDeviceAnimation)
                 moveDeviceAnimation.SetTrigger(k_FadeOffAnim);

             if (tapToPlaceAnimation)
                 tapToPlaceAnimation.SetTrigger(k_FadeOnAnim);

             m_ShowingTapToPlace = true;
             m_ShowingMoveDevice = false;
         }*/
         if(_state==0&&PlanesFound())
           setState(1);
    }

    bool PlanesFound()
    {
        if (planeManager == null)
            return false;

        return planeManager.trackables.count > 0;
    }

    void PlacedObject()
    {
       /*  if (m_ShowingTapToPlace)
         {
             if (tapToPlaceAnimation)
                 tapToPlaceAnimation.SetTrigger(k_FadeOffAnim);

             m_ShowingTapToPlace = false;
         }*/
        setState(2);
    }
}
