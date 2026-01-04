public interface IAltInteractable
{
    string GetAltHintText();
    bool CanAltInteract(Interactor interactor);
    void AltInteract(Interactor interactor);
}
