namespace GLogic.Components.Common;

public static class IdStructure
{
    public const uint EntityIndexBits = 24;
    public const uint EntityIndexMask = (1 << (int)EntityIndexBits) - 1;
    
    public const uint EntityGenerationBits = 8;
    public const uint EntityGenerationMask = (1 << (int)EntityGenerationBits) - 1;
    
    public static uint Index(uint entityId)
    {
        return entityId & IdStructure.EntityIndexMask;
    }

    public static byte Generation(uint entityId)
    {
        return (byte)((entityId >> (int)IdStructure.EntityIndexBits) & IdStructure.EntityGenerationMask);
    }

    public static uint IdWithNewGeneration(uint entityId)
    {
        var generation = (uint)Generation(entityId) + 1;
        return Index(entityId) | (generation << (int)EntityIndexBits);
    }
}