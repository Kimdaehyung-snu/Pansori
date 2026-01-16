using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupTest : UIPopup
{
    enum Buttons
    {
        Target
    }

    private void Start()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Button TargetButton = Get<Button>((int)Buttons.Target);
        BindEvent(TargetButton.gameObject,OnBeginDragTargetButton,GameEvents.UIEvent.BeginDrag);
        BindEvent(TargetButton.gameObject,OnDragTargetButton,GameEvents.UIEvent.Drag);
        BindEvent(TargetButton.gameObject,OnEndDragTargetButton,GameEvents.UIEvent.EndDrag);
    }

    private void OnBeginDragTargetButton(PointerEventData eventData)
    {
        
    }
    private void OnDragTargetButton(PointerEventData eventData)
    {
        
    }
    private void OnEndDragTargetButton(PointerEventData eventData)
    {
        
    }
}