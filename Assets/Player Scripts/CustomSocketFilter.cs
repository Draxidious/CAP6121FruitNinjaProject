using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CustomSocketFilter : XRSocketInteractor
{
    [SerializeField] public string allowedTag = "Untagged"; // Change this to match your desired tag

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        // Check if the object has the correct tag
        return interactable.transform.CompareTag(allowedTag) && base.CanSelect(interactable);
    }
}
