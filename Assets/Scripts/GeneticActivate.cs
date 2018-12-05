using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class GeneticActivate : MonoBehaviour, IInputClickHandler
{
    public bool DNA = false;

    public virtual void OnInputClicked(InputClickedEventData eventData)
    {
        DNA = true;
    }
}
