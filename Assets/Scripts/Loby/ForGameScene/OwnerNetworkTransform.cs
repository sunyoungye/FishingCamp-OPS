using Unity.Netcode.Components;

public class OwnerNetworkTransform : NetworkTransform
{
    // this script means i play my character on my own
    // and makes cannot control other's character
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
