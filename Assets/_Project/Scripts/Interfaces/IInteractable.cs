public interface IInteractable
{
    //TODO: If-Bedingung mit return vor die Interact-Methoden, um nur die gezielte Interact auszulösen
    public string GetInteractionName();
    public void Interact();
}