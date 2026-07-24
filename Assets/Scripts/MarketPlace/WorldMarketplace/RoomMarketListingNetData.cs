using Unity.Netcode;
using Unity.Collections;

public class RoomMarketListingNetData : INetworkSerializable
{
    public FixedString64Bytes listingId;
    public FixedString64Bytes fishId;

    public int quantity;
    public int price;

    public FixedString64Bytes sellerPlayerId;
    public FixedString64Bytes sellerName;

    public float remainingTime;
    public bool isSold;
    public bool isNpcListing;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref listingId);
        serializer.SerializeValue(ref fishId);

        serializer.SerializeValue(ref quantity);
        serializer.SerializeValue(ref price);

        serializer.SerializeValue(ref sellerPlayerId);
        serializer.SerializeValue(ref sellerName);

        serializer.SerializeValue(ref remainingTime);
        serializer.SerializeValue(ref isSold);
        serializer.SerializeValue(ref isNpcListing);
    }
}
