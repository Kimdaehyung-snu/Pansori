using UnityEngine;

public static class DetectHelper
{
    /// <summary>
    /// UI전용
    /// 두 UI가 겹치는지 체크
    /// </summary>
    public static bool CheckCollisionEnterUI(RectTransform rectA, RectTransform rectB)
    {
        return GetWorldRect(rectA).Overlaps(GetWorldRect(rectB)); 
    }
    static Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners); // 월드 좌표계의 네 꼭짓점을 가져옴

        // 좌측 하단(Bottom-Left) 좌표와 폭/높이 계산
        Vector3 bottomLeft = corners[0];
        float width = Mathf.Abs(corners[2].x - corners[0].x);
        float height = Mathf.Abs(corners[2].y - corners[0].y);

        return new Rect(bottomLeft.x, bottomLeft.y, width, height);
    }
    
}