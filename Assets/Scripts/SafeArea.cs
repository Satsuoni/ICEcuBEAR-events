using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SafeArea : MonoBehaviour
{
    RectTransform Panel;
    // Rect LastSafeArea = new Rect(0, 0, 0, 0);
    public SimplexCalc simplink;
    void Awake()
    {
        Panel = GetComponent<RectTransform>();
        Refresh();
    }

    void LateUpdate()
    {
        Refresh();
    }

    public void Refresh()
    {
        Rect safeArea = GetSafeArea();
        Rect LastSafeArea = new Rect(Panel.anchorMin, Panel.anchorMax - Panel.anchorMin);
        if (safeArea != LastSafeArea)
        {
            Debug.LogFormat("Rect changed from {0} to {1}", LastSafeArea,safeArea);
            ApplySafeArea(safeArea);
        }
    }

    Rect GetSafeArea()
    {
        Rect rs= Screen.safeArea;
        Vector2 anchorMin = rs.position;
        Vector2 anchorMax = rs.position + rs.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        return new Rect(anchorMin, anchorMax - anchorMin);

    }

    void ApplySafeArea(Rect r)
    {
     

        // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
        Panel.anchorMin = r.position;
        Panel.anchorMax = r.position+r.size;
        Debug.Log(Panel);
        Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
            name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
        simplink?.triggerRebuild();
    }
}