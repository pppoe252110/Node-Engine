public interface ISerializableVariable
{
    string SerializeValue();
    void DeserializeValue(string serializedValue);
}