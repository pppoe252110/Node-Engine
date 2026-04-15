/// <summary>
/// Allows a node to save/load custom data beyond basic ports and position.
/// </summary>
public interface IGraphSerializable
{
    string SerializeCustomData();
    void DeserializeCustomData(string data);
}