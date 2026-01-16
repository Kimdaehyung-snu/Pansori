using UnityEngine;
/// <summary>
/// 게임에서 사용하는 모든 상수를 중앙 관리하는 클래스
/// </summary>
public static class GameConstants
{
    // UI 관련 상수
    public static class UI
    {
        public static class SortingOrder
        {
            // 게임 월드 내의 요소 (플레이어보다 뒤에 그려져야 하는 배경 오브젝트 등)
            public const int WorldSpace = -10;

            // 일반적인 UI의 밑바닥 (게임 화면을 가리는 전체 배경 이미지 등)
            public const int Background = 0;

            // 상시 노출되는 정보창 (체력바, 미니맵, 스킬 쿨타임 아이콘 등)
            public const int HUD = 10;

            // 버튼 클릭으로 열리는 창 (인벤토리, 설정창, 캐릭터 정보 등)
            public const int MenusAndPanels = 20;

            // 유저의 확인이 필요한 강제 팝업 (아이템 획득 알림, 경고창, 구매 확인 등)
            public const int Popup = 30;

            // 시스템 최상위 레이어 (로딩 화면, 네트워크 끊김 알림, 토스트 메시지 등)
            public const int System = 100;
        }

    }

}
