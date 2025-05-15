using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Door : MonoBehaviour
{
    public Transform door; // Przypisz drzwi w Inspectorze
    public float openAngle = 90f; // Kat obrotu w stopniach
    public float openSpeed = 2f;

    private XRBaseInteractable interactable;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        if (door != null)
        {
            closedRotation = door.rotation;
            openRotation = closedRotation * Quaternion.Euler(0f, 0f, openAngle);
        }
        else
        {
            Debug.LogError("Nie przypisano drzwi do przycisku!", this);
        }

        interactable.selectEntered.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed(SelectEnterEventArgs args)
    {
        if (door != null)
        {
            isOpen = !isOpen;
            StopAllCoroutines();
            StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));
        }
    }

    private System.Collections.IEnumerator RotateDoor(Quaternion targetRotation)
    {
        while (Quaternion.Angle(door.rotation, targetRotation) > 0.1f)
        {
            door.rotation = Quaternion.Slerp(door.rotation, targetRotation, openSpeed * Time.deltaTime);
            yield return null;
        }

        door.rotation = targetRotation;
    }

    private void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnButtonPressed);
    }
}
