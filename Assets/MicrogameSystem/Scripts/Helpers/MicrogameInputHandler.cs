using System;
using UnityEngine;

namespace Pansori.Microgames
{
    /// <summary>
    /// 미니게임 입력 처리 헬퍼 컴포넌트
    /// 표준화된 입력 처리를 제공합니다.
    /// </summary>
    public class MicrogameInputHandler : MonoBehaviour
    {
        [Header("입력 설정")]
        [SerializeField] private bool enableKeyboardInput = true;
        [SerializeField] private bool enableMouseInput = true;
        
        /// <summary>
        /// 키가 눌렸을 때 발생하는 이벤트
        /// </summary>
        public event Action<KeyCode> OnKeyPressed;
        
        /// <summary>
        /// 키가 떼어졌을 때 발생하는 이벤트
        /// </summary>
        public event Action<KeyCode> OnKeyReleased;
        
        /// <summary>
        /// 마우스 클릭 이벤트 (버튼, 월드 위치)
        /// </summary>
        public event Action<int, Vector3> OnMouseClick;
        
        /// <summary>
        /// 마우스 드래그 이벤트 (시작 위치, 현재 위치)
        /// </summary>
        public event Action<Vector3, Vector3> OnMouseDrag;
        
        /// <summary>
        /// 마우스 드래그 시작 이벤트
        /// </summary>
        public event Action<Vector3> OnMouseDragStart;
        
        /// <summary>
        /// 마우스 드래그 종료 이벤트
        /// </summary>
        public event Action<Vector3> OnMouseDragEnd;
        
        private bool isDragging = false;
        private Vector3 dragStartPosition;
        
        private void Update()
        {
            if (enableKeyboardInput)
            {
                HandleKeyboardInput();
            }
            
            if (enableMouseInput)
            {
                HandleMouseInput();
            }
        }
        
        /// <summary>
        /// 키보드 입력 처리
        /// </summary>
        private void HandleKeyboardInput()
        {
            // 모든 키 코드를 확인 (주요 키만 체크)
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    OnKeyPressed?.Invoke(keyCode);
                }
                
                if (Input.GetKeyUp(keyCode))
                {
                    OnKeyReleased?.Invoke(keyCode);
                }
            }
        }
        
        /// <summary>
        /// 마우스 입력 처리
        /// </summary>
        private void HandleMouseInput()
        {
            // 마우스 클릭 처리
            for (int i = 0; i < 3; i++) // 0: 왼쪽, 1: 오른쪽, 2: 가운데
            {
                if (Input.GetMouseButtonDown(i))
                {
                    Vector3 worldPos = GetMouseWorldPosition();
                    OnMouseClick?.Invoke(i, worldPos);
                    
                    // 드래그 시작
                    isDragging = true;
                    dragStartPosition = worldPos;
                    OnMouseDragStart?.Invoke(worldPos);
                }
                
                if (Input.GetMouseButtonUp(i))
                {
                    if (isDragging)
                    {
                        Vector3 worldPos = GetMouseWorldPosition();
                        OnMouseDragEnd?.Invoke(worldPos);
                        isDragging = false;
                    }
                }
            }
            
            // 마우스 드래그 처리
            if (isDragging && (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)))
            {
                Vector3 currentPos = GetMouseWorldPosition();
                OnMouseDrag?.Invoke(dragStartPosition, currentPos);
            }
        }
        
        /// <summary>
        /// 마우스의 월드 좌표를 가져옵니다.
        /// </summary>
        /// <returns>마우스 월드 좌표</returns>
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main != null ? Camera.main.nearClipPlane + 1f : 10f;
            return Camera.main != null ? Camera.main.ScreenToWorldPoint(mousePos) : mousePos;
        }
        
        /// <summary>
        /// 입력 활성화/비활성화
        /// </summary>
        /// <param name="enable">활성화 여부</param>
        public void SetInputEnabled(bool enable)
        {
            enableKeyboardInput = enable;
            enableMouseInput = enable;
            
            if (!enable && isDragging)
            {
                // 드래그 중이면 종료 처리
                Vector3 worldPos = GetMouseWorldPosition();
                OnMouseDragEnd?.Invoke(worldPos);
                isDragging = false;
            }
        }
        
        /// <summary>
        /// 컴포넌트가 비활성화될 때 드래그 상태 초기화
        /// </summary>
        private void OnDisable()
        {
            if (isDragging)
            {
                Vector3 worldPos = GetMouseWorldPosition();
                OnMouseDragEnd?.Invoke(worldPos);
                isDragging = false;
            }
        }
    }
}
