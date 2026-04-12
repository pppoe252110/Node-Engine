using System;
using NodeEngine.Input;

public class MouseButtonPersister : IValuePersister
{
    public bool CanPersist(Type type) => type == typeof(MouseButton);

    public string Serialize(object value)
    {
        if (value is MouseButton button)
        {
            return button.ToString();
        }
        return MouseButton.Left.ToString();
    }

    public object Deserialize(string data, Type targetType)
    {
        if (string.IsNullOrEmpty(data))
            return MouseButton.Left;

        try
        {
            return Enum.Parse(typeof(MouseButton), data);
        }
        catch
        {
            return MouseButton.Left;
        }
    }
}