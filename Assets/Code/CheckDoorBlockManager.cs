using UnityEngine;

public class CheckDoorBlockManager : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask; // Ray'in çarpýþma kontrol edeceði layer'ý seçmek için
    [SerializeField] private bool rightDirection;
    public bool isDoorBlocked;

    void Update()
    {
        RaycastHit hit;
        Vector3 rayDirection = rightDirection ? transform.right : -transform.right; // Karakterin yönüne göre ray yönü
        float rayDistance = 10f; // Ray'in gideceði maksimum mesafe

        // Ray'i gönder ve çarpýþma varsa hit deðiþkenine atama yap
        if (Physics.BoxCast(transform.position, transform.lossyScale / 2, rayDirection, out hit, Quaternion.identity, rayDistance, layerMask))
        {
            // Eðer ray seçilen layer'a çarparsa burasý çalýþacak
            isDoorBlocked = true;
        }
        else
        {
            // Eðer ray seçilen layer'a çarpmazsa burasý çalýþacak
            isDoorBlocked = false;
        }
    }
}