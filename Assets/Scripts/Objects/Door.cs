using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Door : MonoBehaviour
{
    public Transform door;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public Vector3 openOffset = new Vector3(0.5f, 0f, 0f);

    public AudioClip doorOpenClip;
    public AudioSource audioSource;

    private XRBaseInteractable interactable;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        if (door != null)
        {
            closedRotation = door.rotation;
            openRotation = closedRotation * Quaternion.Euler(0f, 0f, openAngle);
            closedPosition = door.position;
            openPosition = closedPosition + openOffset;
        }
        else
        {
            Debug.LogError("Nie przypisano drzwi do przycisku!", this);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                Debug.LogWarning("Brak AudioSource! Efekty dźwiękowe nie będą działać.");
        }

        interactable.selectEntered.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed(SelectEnterEventArgs args)
    {
        if (door != null)
        {
            isOpen = !isOpen;
            PlaySound(isOpen);
            StopAllCoroutines();
            StartCoroutine(MoveAndRotateDoor(
                isOpen ? openPosition : closedPosition,
                isOpen ? openRotation : closedRotation
            ));
        }
    }

    private System.Collections.IEnumerator MoveAndRotateDoor(Vector3 targetPosition, Quaternion targetRotation)
    {
        while (Vector3.Distance(door.position, targetPosition) > 0.01f ||
               Quaternion.Angle(door.rotation, targetRotation) > 0.1f)
        {
            door.position = Vector3.Lerp(door.position, targetPosition, openSpeed * Time.deltaTime);
            door.rotation = Quaternion.Slerp(door.rotation, targetRotation, openSpeed * Time.deltaTime);
            yield return null;
        }

        door.position = targetPosition;
        door.rotation = targetRotation;
    }

    private void PlaySound(bool opening)
    {
        if (audioSource != null)
        {
            AudioClip clip = doorOpenClip;
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    private void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnButtonPressed);
    }
}