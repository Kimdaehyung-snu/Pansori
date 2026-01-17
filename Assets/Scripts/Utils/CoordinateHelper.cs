using UnityEngine;

public static class CoordinateHelper
{
    public static Vector3 GetCanvasWorldPos(Vector3 worldInputPos, RectTransform canvasRect)
    {
        // 1. 입력받은 월드 좌표를 스크린 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldInputPos);
        
        Vector3 finalCanvasPos;

        // 2. 스크린 좌표를 Canvas(RectTransform) 평면 위의 월드 좌표로 변환
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect,           // 기준 RectTransform (Canvas)
            screenPos,            // 변환된 스크린 좌표
            null,                 // Overlay 모드이므로 카메라는 null
            out finalCanvasPos    // 결과 저장
        );

        return finalCanvasPos;
    }
}