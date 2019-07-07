using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

//#undef UNITY_EDITOR
[ExecuteInEditMode]
public class Landscaper : MonoBehaviour
{
    public Landscaped[] landscaped;
    // Start is called before the first frame update
    RectTransform mine;
    [SerializeField]
    bool _isPortrait = false;
    [SerializeField]
    Canvas me;
    float m_PrevScaleFactor = 0;
    protected void SetScaleFactor(float scaleFactor)
    {
        if (scaleFactor == m_PrevScaleFactor)
            return;

        me.scaleFactor = scaleFactor;
        m_PrevScaleFactor = scaleFactor;
    }
    public bool isPortrait

    {
        get { return _isPortrait; }
    }
    void Start()
    {
        mine = gameObject.GetComponent<RectTransform>();
        me = gameObject.GetComponent<Canvas>();
    }
    public void OnEnabled()
    {
        landscaped = gameObject.GetComponentsInChildren<Landscaped>(true);
        mine = gameObject.GetComponent<RectTransform>();
        if(mine!=null)
        {
            Vector2 wh = mine.offsetMax - mine.offsetMin;
            if (wh.x > wh.y)
            {
                _isPortrait = false;
            }
            else
            {
                _isPortrait = true;
            }

            foreach (Landscaped l in landscaped)
            {
                l.Switch(_isPortrait);
            }
        }
    }
    void beSwitched(Vector2 wh)
    {
        if(me==null)
            me = gameObject.GetComponent<Canvas>();
        bool sw = false;
        if (wh.x > wh.y)
        {
            //Debug.Log("Landscape");
            if (_isPortrait) sw = true;
            _isPortrait = false;
        }
        else
        {
            if (!_isPortrait) sw = true;
            _isPortrait = true;
        }
        if (sw)
        {
            me.enabled = false;
            foreach (Landscaped l in landscaped)
            {

                l.Switch(_isPortrait);
            }
            me.enabled = true;
            //  Canvas.ForceUpdateCanvases();
        }
    }
    void OnRectTransformDimensionsChange()
    {
       
        landscaped = gameObject.GetComponentsInChildren<Landscaped>(true);
        if(mine==null)
         mine = gameObject.GetComponent<RectTransform>();
       // Debug.LogFormat("Tmrw: scale {0}", me.scaleFactor);
        // Debug.LogFormat("Changed {0} {1}",mine.offsetMin,mine.offsetMax);
        Vector2 wh = mine.offsetMax - mine.offsetMin;
        beSwitched(wh);
        beenFlipped = false;
       
    }
    // Update is called once per frame
    DeviceOrientation old=DeviceOrientation.LandscapeLeft;
    Vector2 sz;
    ScreenOrientation so;
    Vector2 arage;


#if UNITY_ANDROID && !UNITY_EDITOR
  private static AndroidJavaClass unityPlayerClass = null;
     private static AndroidJavaClass metricsClass = null;
  /// <summary>    
  /// <para> Gets the current UnityActivity used on Android. </para>
  /// It will store the AndroidJavaClass for later use ensuring it is not creating a new
  /// class in memory every call.
  /// </summary>
  /// <returns> The AndroidActivity with the UnityPlayer running in it. </returns>
  public static AndroidJavaObject GetUnityActivity() {  // Worth noting I have separated this out into a class for common Android calls :/
    if (unityPlayerClass == null) {
      unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            metricsClass = new AndroidJavaClass("android.util.DisplayMetrics");
    }
    return unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
  }
    public static float Density { get; protected set; }

    // The screen density expressed as dots-per-inch
    public static int DensityDPI { get; protected set; }

    // The absolute height of the display in pixels
    public static int HeightPixels { get; protected set; }

    // The absolute width of the display in pixels
    public static int WidthPixels { get; protected set; }
#endif
    bool beenFlipped = false;
    void Update()
    {
      
       /* if(Input.deviceOrientation!=old)
        {
            Debug.LogFormat("Tmr- {0} {1}", Time.frameCount, Time.time);
            old = Input.deviceOrientation;
            beenFlipped = true;
        }
        if(beenFlipped)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (
                  AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
                  activityInstance = GetUnityActivity(),
                  windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager"),
                  displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
              )
            {
                displayInstance.Call("getRealMetrics", metricsInstance);
                Density = metricsInstance.Get<float>("density");
                DensityDPI = metricsInstance.Get<int>("densityDpi");
                int hp= metricsInstance.Get<int>("heightPixels");
                int wp= metricsInstance.Get<int>("widthPixels");
                if(HeightPixels!=hp||WidthPixels!=wp)
                {
            float scaleFactor=(float)wp / (float)2160;
            SetScaleFactor(scaleFactor);
             Debug.LogFormat("Tmri: scale {0}",scaleFactor);
                    beSwitched(new Vector2((float)wp,(float) hp));
                    beenFlipped = false;
                }
                HeightPixels = hp;
                WidthPixels = wp;

                Debug.LogFormat("Tmri: {0} {1} ", HeightPixels, WidthPixels);
            }
#endif
        }
        Vector2 ss = new Vector2((float)Screen.width, (float)Screen.height);*/
        
    }
}
