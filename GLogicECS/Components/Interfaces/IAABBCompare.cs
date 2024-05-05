using GLogic.Components.Common;
using GLogicECS.Components.Common;

namespace GLogicECS.Components.Interfaces;

public interface IAABBCompare
{
    bool Compare(EcsArea area);
    bool Compare(Vector2Int point);
    Entity GetEntity();
}