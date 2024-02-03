using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public void OnSelect(BaseEventData eventData)
    {
        SoundManager.Instance.PlaySoundOnSelect();
        Debug.Log("Should play soundi");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySoundOnHover();
        Debug.Log("Should play soundi");
    }
}
