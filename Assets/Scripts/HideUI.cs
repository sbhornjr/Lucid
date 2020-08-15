using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideUI : MonoBehaviour
{

    public void hideUIElement(GameObject UIElement)
    {
      UIElement.SetActive(false);
    }

    public void showUIElement(GameObject UIElement)
    {
      UIElement.SetActive(true);
    }
}
