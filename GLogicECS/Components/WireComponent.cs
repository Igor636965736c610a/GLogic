using GLogicECS.Components;

namespace GLogicECS.Components;

public struct WireComponent
{
    public Entity Entity;
    public bool Increasing;
    public uint InputHookNumber;
    public uint OutputHookNumber;
}

/*

InputHookNumber0   ---|----+
                      |------+
                      |---------+
    Gate-->           |-----------+-  OutputHookNumber0
                      |---------+
                      |------+
InputHookNumber1   ---|----+  
*/