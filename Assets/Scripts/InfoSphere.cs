using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class InfoSphere : MonoBehaviour, IInputClickHandler
{
    public bool tap = false;

    public int presetno;

    public int nstar = 3;

    public int encodage = 1; //faudrait mettre toute les valeur des widget en pourcentage  //dictionnaire avec EventID , Value ? 

    public Dictionary<int, string> widgetValue;

    public void Start()
    {
        widgetValue = new Dictionary<int, string>();
    }

    public virtual void OnInputClicked(InputClickedEventData eventData)
    {
        tap = true;
    }


}
