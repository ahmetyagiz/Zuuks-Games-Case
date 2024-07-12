using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TouchController : MonoBehaviour
{
    // EÐER BÝR BUTONA DOKUNDUYSA VE BÝRDEN FAZLA TOUCH VARSA EN SON TOUCH'I AL
    // EÐER BÝR BUTONA DOKUNMADIYSA ÝLK TOUCH'I AL
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
                // UI objesini bulmak için kullanýlacak olan Raycast iþlemi
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = Input.GetTouch(0).position;

                // Raycast iþlemi
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                // Eðer bir UI objesi týklanmýþsa
                if (results.Count > 0)
                {
                    // Ýlk týklanan objenin layer adýný al ve Debug.Log ile görüntüle
                    int layer = results[0].gameObject.layer;
                    //Debug.Log(layerName);
                    if (layer == 2)
                    {
                        myTouchCount = 0;
                    }
                    //uý'a týklýyor
                    //Eðer UI'a týklýyorsam ve birden fazla dokunmam var ise dokunmam ise
                    else if (layer == 5)
                    {
                        myTouchCount = 1;
                    }
                }
            }
        }
    }
}
