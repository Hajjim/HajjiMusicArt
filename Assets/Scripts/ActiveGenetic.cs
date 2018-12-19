using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ActiveGenetic : MonoBehaviour, IInputClickHandler
{
    public bool act = false;

    public virtual void OnInputClicked(InputClickedEventData eventData)
    {
        act = true;
    }
}
