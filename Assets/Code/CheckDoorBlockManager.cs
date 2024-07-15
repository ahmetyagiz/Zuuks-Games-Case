using UnityEngine;

public class CheckDoorBlockManager : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask; // Ray'in �arp��ma kontrol edece�i layer'� se�mek i�in
    [SerializeField] private bool rightDirection;
    public bool isDoorBlocked;

    void Update()
    {
        RaycastHit hit;
        Vector3 rayDirection = rightDirection ? transform.right : -transform.right; // Karakterin y�n�ne g�re ray y�n�
        float rayDistance = 10f; // Ray'in gidece�i maksimum mesafe

        // Ray'i g�nder ve �arp��ma varsa hit de�i�kenine atama yap
        if (Physics.BoxCast(transform.position, transform.lossyScale / 2, rayDirection, out hit, Quaternion.identity, rayDistance, layerMask))
        {
            // E�er ray se�ilen layer'a �arparsa buras� �al��acak
            isDoorBlocked = true;
        }
        else
        {
            // E�er ray se�ilen layer'a �arpmazsa buras� �al��acak
            isDoorBlocked = false;
        }
    }
}