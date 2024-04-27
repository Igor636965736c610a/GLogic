using System.Diagnostics;
using GLogic.Components.Common;
using GLogic.Jobs.Renderer;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicECS.Components.Init;

namespace GLogic.Jobs;

public static class WireService
{
    private static Connection? _existingConnection;

    public static void Reset()
        => _existingConnection = _existingConnection = null;

    public static void Create(Connection connection)
    {
        Debug.Assert(EntityManager.IsAlive(connection.Entity));
        Debug.Assert(ValidConnect(connection));

        switch (connection.ConnectionType)
        {
            case ConnectionType.Input:
                ConnectToInput(connection);
                
                return;
            case ConnectionType.Output:
                ConnectToOutput(connection);

                return;
        }
        
        Reset();
    }

    public static (ConnectionType connectionType, int hookNumber)? GetHookInfo(IoType ioType, Vector2Int position, Vector2Int point)
    {
        switch (ioType)
        {
            case IoType.AND:
            case IoType.OR:
            case IoType.XOR:
            case IoType.NAND:
            case IoType.NOR:
            case IoType.XNOR:
                var xDiff = point.X - position.X;
                var yDiff = point.Y - position.Y;
                switch (xDiff)
                {
                    case > 0 and < 30 when yDiff is > 5 and < 23:
                        return (ConnectionType.Input, 0);
                    case > 0 and < 30 when yDiff is > 27 and < 45:
                        return (ConnectionType.Input, 1);
                    case > 80 and < 100 when yDiff is > 20 and < 30:
                        return (ConnectionType.Output, 0);
                }
                break;
            case IoType.Input:
                break;
            case IoType.Output:
                break;
            case IoType.NOT:
                break;
            case IoType.Wire:
                throw new InvalidProgramException("Invalid argument");
            default:
                throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null); 
        }
        return null;
    }

    public static void UpdateConnectedWiresPosition(Entity entity)
    {
        Debug.Assert(EntityManager.IsAlive(entity));

        var type = ComponentManager.GetEntityTypeComponent(entity).Type;
        
        Debug.Assert(type != IoType.Wire);
        
        var entityPos = ComponentManager.GetTransformComponent(entity).Position;
        
        var inputComp = ComponentManager.GetInputComponent(entity);
        var inputs = inputComp.Inputs;

        for (int i = 0; i < inputs.Count; i++)
        {
            var wireComp = ComponentManager.GetWireComponent(inputs[i].entity);
            
            Debug.Assert(ComponentManager.GetEntityTypeComponent(wireComp.Entity).Type == IoType.Wire);
            
            wireComp = wireComp with { P1 = CalculateInputConnectionPoint(type, inputs[i].hookNumber, entityPos) };
            
            ComponentManager.UpdateWireComponent(wireComp);
            ComponentManager.UpdateTransformComponent(CalculateTransformComponentForWire(wireComp.P1, wireComp.P2).ToTransformComponent(wireComp.Entity));
        }
        
        var outputComp = ComponentManager.GetOutputComponent(entity);
        var outputs = outputComp.Outputs;

        for (int i = 0; i < outputs.Count; i++)
        {
            var wireComp = ComponentManager.GetWireComponent(outputs[i].entity);
            
            Debug.Assert(ComponentManager.GetEntityTypeComponent(wireComp.Entity).Type == IoType.Wire);
            
            wireComp = wireComp with { P2 = CalculateInputConnectionPoint(type, outputs[i].hookNumber, entityPos) };
            
            ComponentManager.UpdateWireComponent(wireComp);
            ComponentManager.UpdateTransformComponent(CalculateTransformComponentForWire(wireComp.P1, wireComp.P2).ToTransformComponent(wireComp.Entity));
        }
    }

    private static void ConnectToInput(Connection connection)
    {
        InputComponent inputComp;
        if (_existingConnection is null || !EntityManager.IsAlive(_existingConnection.Value.Entity))
        {
            inputComp = ComponentManager.GetInputComponent(connection.Entity);
            
            if (!CanConnectToInput(connection, inputComp))
            {
                return;
            }
            
            _existingConnection = connection;
            
            return;
        }
        
        if (connection.ConnectionType == _existingConnection.Value.ConnectionType)
        {
            return;
        }
        
        if (_existingConnection.Value.Entity.Id == connection.Entity.Id)
        {
            return;
        }
        
        inputComp = ComponentManager.GetInputComponent(connection.Entity);
            
        if (!CanConnectToInput(connection, inputComp))
        {
            return;
        }

        Entity wire;
        if (_existingConnection.Value.ConnectionType == ConnectionType.Input)
        {
            wire = CreateWire(_existingConnection.Value, connection);
        }
        else
        {
            wire = CreateWire(connection, _existingConnection.Value);
        }
        
        inputComp.Inputs.Add(new (wire, connection.HookNumber));

        var outputComp = ComponentManager.GetOutputComponent(_existingConnection.Value.Entity);
        outputComp.Outputs.Add(new (wire, connection.HookNumber));
        
        ComponentManager.UpdateInputComponent(inputComp);
        ComponentManager.UpdateOutputComponent(outputComp);
        
        Reset();
    }

    private static void ConnectToOutput(Connection connection)
    {
        if (_existingConnection is null || !EntityManager.IsAlive(_existingConnection.Value.Entity))
        {
            _existingConnection = connection;
            
            return;
        }
        
        if (connection.ConnectionType == _existingConnection.Value.ConnectionType)
        {
            return;
        }
        
        if (_existingConnection.Value.Entity.Id == connection.Entity.Id)
        {
            return;
        }

        var outputComp = ComponentManager.GetOutputComponent(connection.Entity);
        var inputComp = ComponentManager.GetInputComponent(_existingConnection.Value.Entity);
        
        if (_existingConnection.Value.ConnectionType == ConnectionType.Input)
        {
            CreateWire(_existingConnection.Value, connection);
        }
        else
        {
            CreateWire(connection, _existingConnection.Value);
        }

        outputComp.Outputs.Add(new (connection.Entity, connection.HookNumber));
        inputComp.Inputs.Add(new (_existingConnection.Value.Entity, connection.HookNumber));
        
        ComponentManager.UpdateInputComponent(inputComp);
        ComponentManager.UpdateOutputComponent(outputComp);
        
        Reset();
    }
    
    private static bool CanConnectToInput(Connection connection, InputComponent inputComp)
    {
        Debug.Assert(EntityManager.IsAlive(inputComp.Entity));
        var inputs = inputComp.Inputs;
        for (int i = 0; i < inputs.Count; i++)
        {
            if (!EntityManager.IsAlive(inputs[i].entity))
            {
                continue;
            }

            if (inputs[i].hookNumber == connection.HookNumber)
            {
                return false;
            }
        }
        return true;
    }

    private static bool ValidConnect(Connection connection)
    {
        var ioType = ComponentManager.GetEntityTypeComponent(connection.Entity).Type;
        return connection.ConnectionType switch
        {
            ConnectionType.Input => connection.HookNumber < InputComponent.GetInputsCount(ioType),
            ConnectionType.Output => connection.HookNumber < OutputComponent.GetOutputsCount(ioType),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static Entity CreateWire(Connection input, Connection output)
    {
        var iType = ComponentManager.GetEntityTypeComponent(input.Entity).Type;
        var oType = ComponentManager.GetEntityTypeComponent(output.Entity).Type;
        var iTransform = ComponentManager.GetTransformComponent(input.Entity);
        var oTransform = ComponentManager.GetTransformComponent(output.Entity);

        var p1 = CalculateInputConnectionPoint(iType, input.HookNumber, iTransform.Position);
        var p2 = CalculateOutputConnectionPoint(oType, output.HookNumber, oTransform.Position);
        
        return EntityManager.CreateEntity(new InitWire(
            CalculateTransformComponentForWire(p1, p2),
            new InitWireComponent(p1, p2),
            new ConnectionInfo(input.Entity, input.HookNumber),
            new ConnectionInfo(output.Entity, output.HookNumber)
        ));
    }

    private static Vector2Int CalculateInputConnectionPoint(IoType ioType, int hookNumber, Vector2Int position)
    {
        switch (ioType)
        {
            case IoType.AND:
            case IoType.OR:
            case IoType.XOR:
            case IoType.NAND:
            case IoType.NOR:
            case IoType.XNOR:
            case IoType.Output:
                return hookNumber switch
                {
                    0 => new Vector2Int(position.X, position.Y + (int)(EntityService.RectLGateSize.Y * 0.25)),
                    1 => new Vector2Int(position.X, position.Y + (int)(EntityService.RectLGateSize.Y * 0.75)),
                    _ => throw new InvalidProgramException("Wire hook number out of range")
                };
            case IoType.NOT:
                return new Vector2Int(position.X, position.Y + (int)(EntityService.RectLGateSize.Y * 0.5));
            case IoType.Wire:
                //TODO
                throw new Exception();
            case IoType.Input:
            default:
                throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null);
        }
    }
    
    private static Vector2Int CalculateOutputConnectionPoint(IoType ioType, int hookNumber, Vector2Int position)
    {
        switch (ioType)
        {
            case IoType.AND:
            case IoType.OR:
            case IoType.XOR:
            case IoType.NAND:
            case IoType.NOR:
            case IoType.XNOR:
            case IoType.NOT:
            case IoType.Input:
                return new Vector2Int(position.X + EntityService.RectLGateSize.X, (int)(position.Y + EntityService.RectLGateSize.Y * 0.5));
            case IoType.Wire:
                //TODO
                throw new Exception();
            case IoType.Output:
            default:
                throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null);
        }
    }

    private static InitTransformComponent CalculateTransformComponentForWire(Vector2Int p1, Vector2Int p2)
    {
        var position = new Vector2Int(Math.Min(p1.X, p2.X) - 4, Math.Min(p1.Y, p2.Y) - 4);
        var size = new Vector2Int(Math.Abs(p1.X - p2.X) + 4, Math.Abs(p1.Y - p2.Y) + 4);

        return new InitTransformComponent(position, size);
    }
}

public readonly record struct Connection(Entity Entity, ConnectionType ConnectionType, int HookNumber);

public readonly record struct HookInfo(ConnectionType ConnectionType, int HookNumber);

public enum ConnectionType
{
    Input,
    Output
}