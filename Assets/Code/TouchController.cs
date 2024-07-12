using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TouchController : MonoBehaviour
{
    // E�ER B�R BUTONA DOKUNDUYSA VE B�RDEN FAZLA TOUCH VARSA EN SON TOUCH'I AL
    // E�ER B�R BUTONA DOKUNMADIYSA �LK TOUCH'I AL
    // Check if there is a touch

    public int myTouchCount;
    public void CheckTouch()
    {
        // Check if there is a touch
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Check if finger is over a UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // UI objesini bulmak i�in kullan�lacak olan Raycast i�lemi
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = Input.GetTouch(0).position;

                // Raycast i�lemi
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                // E�er bir UI objesi t�klanm��sa
                if (results.Count > 0)
                {
                    // �lk t�klanan objenin layer ad�n� al ve Debug.Log ile g�r�nt�le
                    int layer = results[0].gameObject.layer;
                    //Debug.Log(layerName);
                    if (layer == 2)
                    {
                        myTouchCount = 0;
                    }
                    //u�'a t�kl�yor
                    //E�er UI'a t�kl�yorsam ve birden fazla dokunmam var ise dokunmam ise
                    else if (layer == 5)
                    {
                        myTouchCount = 1;
                    }
                }
            }
        }
    }
}
