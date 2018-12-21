using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class InfoSphere : MonoBehaviour, IInputClickHandler
{
    public bool tap = false;

    public int presetno;

    public string presetName = "";

    public int nstar = 3;

    public List<Vector2> widgetValue = new List<Vector2>();
    //mieux que dictionnaire pour mating + mutation

    public void Start()
    {
    }

    public virtual void OnInputClicked(InputClickedEventData eventData)
    {
        tap = true;
    }


}
