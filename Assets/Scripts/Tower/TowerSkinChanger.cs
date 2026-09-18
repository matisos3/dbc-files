using UnityEngine;

public class TowerSkinChanger : MonoBehaviour
{
    public Transform modelSlot;

    private GameObject currentSkin;


    public void ChangeSkin(GameObject newSkin)
    {
        // usuwamy stary wygląd
        if (currentSkin != null)
        {
            Destroy(currentSkin);
        }


        // dodajemy nowy wygląd
        currentSkin = Instantiate(
            newSkin,
            modelSlot.position,
            modelSlot.rotation,
            modelSlot
        );
    }
}