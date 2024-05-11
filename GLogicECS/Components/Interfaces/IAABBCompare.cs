using GLogicECS.Components.Common;
using GLogicGlobal.Common;

namespace GLogicECS.Components.Interfaces;

public interface IAABBCompare
{
    bool Compare(EcsArea area);
    bool Compare(Vector2Int point);
    Entity GetEntity();
}