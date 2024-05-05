namespace GLogicECS.Components.Common;

public static class IdStructure
{
    public const uint EntityIndexBits = 23;
    public const uint EntityIndexMask = (1 << (int)EntityIndexBits) - 1;

    public const uint EntityGenerationBits = 8;
    public const uint EntityGenerationMask = (1 << (int)EntityGenerationBits) - 1;

    public static uint MakeEntityId(uint index)
    {
        if (index > EntityIndexMask)
        {
            throw new ArgumentOutOfRangeException("Index exceeds allowed bits");
        }

        return (index & EntityIndexMask) | 0U;
    }

    public static uint Index(uint entityId)
        => entityId & EntityIndexMask;

    public static byte Generation(uint entityId)
        => (byte)((entityId >> (int)EntityIndexBits) & EntityGenerationMask);

    public static uint IdWithNewGeneration(uint entityId)
    {
        var generation = (uint)Generation(entityId) + 1;

        return Index(entityId) | (generation << (int)EntityIndexBits);
    }

    public static bool IsValid(uint entityId)
        => entityId != uint.MaxValue;

    public static uint MakeInvalidId()
        => uint.MaxValue;
}