using UnityEngine;

public class ColorPersister : JsonPersisterBase<Color>
{
    protected override Color DefaultValue => Color.white;
}