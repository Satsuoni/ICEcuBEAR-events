using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public enum TouchReactionMode
{
    Placing,
    Zooming,
    Off
}
[RequireComponent(typeof(ARRaycastManager))]
public class PlaceMultipleObjectsOnPlane : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    public ZoomFader zoom;
    public GenericFader[] tower = new GenericFader[1];
    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedObject { get; private set; }

    /// <summary>
    /// Invoked whenever an object is placed in on a plane.
    /// </summary>
    public static event Action onPlacedObject;

    ARRaycastManager m_RaycastManager;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }
    List<GameObject> instObjects = new List<GameObject>();
    public void clearObjects()
    {
        foreach (GameObject obj in instObjects)
        {
            Destroy(obj);
        }
        instObjects.Clear();
    }
    TouchReactionMode zmode;
    void PlaceUpdate()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch mtouch = Input.GetTouch(i);
                if (EventSystem.current.IsPointerOverGameObject(mtouch.fingerId))
                {
                    return;
                }
            }
            if (touch.phase == TouchPhase.Began)
            {
                if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = s_Hits[0].pose;

                    spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                    DOMController tcon = spawnedObject.GetComponent<DOMController>();
                    if (tcon != null)
                    {
                        tcon.zoomChange();
                        tcon.updateForce();
                    }
                    instObjects.Add(spawnedObject);
                    while (instObjects.Count > 1)
                    {
                        var obj = instObjects[0];
                        instObjects.RemoveAt(0);
                        Destroy(obj);
                    }
                    if (onPlacedObject != null)
                    {
                        onPlacedObject();
                    }
                }
            }
        }
    }
    bool prev1 = false;
    public delegate void SlideAction(Vector2 shft);
    public static event SlideAction FingerSliding;
    void ZoomUpdate()
    {
        if(Input.touchCount==1)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                prev1 = false;
                return;
            }
            if(zoom!=null)
            {
                zoom.Show();
            }
            if ( tower!=null)
            foreach(GenericFader fd in tower)
            {
                    fd?.Show();
            }
            if(prev1)
            {
                Touch touchZero = Input.GetTouch(0);
                Vector2 dlt = touchZero.deltaPosition;
                FingerSliding(dlt);
            }
            prev1 = true;
        }
        if (Input.touchCount != 1)
            prev1 = false;
        if (Input.touchCount == 2)
        {
            
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;
            float scrn = Mathf.Min(Screen.width, Screen.height);
            if (zoom != null)
            {
                zoom.Show();
                if (tower != null)
                    foreach (GenericFader fd in tower)
                    {
                        fd?.Show();
                    }
                zoom.UpdateZoom(deltaMagnitudeDiff/scrn);
            }
         
        }
    }
    public void SetMode(TouchReactionMode mod)
    {
        zmode = mod;
    }
    void Update()
    {
  
        switch (zmode)
        {
            case TouchReactionMode.Off: break;
            case TouchReactionMode.Placing: PlaceUpdate();break;
            case TouchReactionMode.Zooming: ZoomUpdate(); break;
        }

    }
}
