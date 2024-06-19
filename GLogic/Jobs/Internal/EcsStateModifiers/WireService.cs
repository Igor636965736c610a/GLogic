using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GLogicECS.Api;
using GLogicECS.Components;
using GLogicECS.Components.Common;
using GLogicECS.Components.Init;
using GLogicGlobal.Common;

namespace GLogic.Jobs.Internal.EcsStateModifiers;

internal static class WireService
{
    private static Connection? _existingConnection;

    public static void Reset()
        => _existingConnection = _existingConnection = null;

    public static bool IsHookedUp([NotNullWhen(true)] out Connection? existingConnection)
    {
        existingConnection = _existingConnection;
        return _existingConnection is not null && EntityManager.IsAlive(_existingConnection.Value.Entity);
    }

    public static Entity? Create(Connection connection)
    {
        Debug.Assert(EntityManager.IsAlive(connection.Entity));
        Debug.Assert(ValidConnect(connection));

        switch (connection.ConnectionType)
        {
            case ConnectionType.Input:
                return ConnectToInput(connection);
            case ConnectionType.Output:
                return ConnectToOutput(connection);
        }

        Reset();

        return null;
    }

    public static HookInfo? GetHookInfo(IoType ioType, Vector2Int position,
        Vector2Int point)
    {
        var xDiff = point.X - position.X;
        var yDiff = point.Y - position.Y;
        switch (ioType)
        {
            case IoType.AND:
            case IoType.OR:
            case IoType.XOR:
            case IoType.NAND:
            case IoType.NOR:
            case IoType.XNOR:
                switch (xDiff)
                {
                    case > 0 and < 30 when yDiff is > 4 and < 24:
                        return new HookInfo(ConnectionType.Input, 0);
                    case > 0 and < 30 when yDiff is > 26 and < 46:
                        return new HookInfo(ConnectionType.Input, 1);
                    case > 80 and < 100 when yDiff is > 20 and < 30:
                        return new HookInfo(ConnectionType.Output, 0);
                }

                break;
            case IoType.Constant:
                if (xDiff is > 80 and < 100)
                {
                    if (yDiff is > 20 and < 30)
                    {
                        return new HookInfo(ConnectionType.Output, 0);
                    }
                }

                break;
            case IoType.LedOutput:
                if (xDiff is > 0 and < 30)
                {
                    switch (yDiff)
                    {
                        case > 4 and < 24:
                            return new HookInfo(ConnectionType.Input, 0);
                        case > 26 and < 46:
                            return new HookInfo(ConnectionType.Input, 1);
                    }
                }

                break;
            case IoType.NOT:
                switch (xDiff)
                {
                    case > 0 and < 30 when yDiff is > 20 and < 30:
                        return new HookInfo(ConnectionType.Input, 0);
                    case > 80 and < 100 when yDiff is > 20 and < 30:
                        return new HookInfo(ConnectionType.Output, 0);
                }

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

        for (var i = 0; i < inputs.Count; i++)
        {
            var wireComp = ComponentManager.GetWireComponent(inputs[i].Entity);

            Debug.Assert(ComponentManager.GetEntityTypeComponent(wireComp.Entity).Type == IoType.Wire);
            Debug.Assert(EntityManager.IsAlive(wireComp.Entity));

            wireComp = wireComp with { P1 = CalculateInputConnectionPoint(type, inputs[i].HookNumber, entityPos) };

            ComponentManager.UpdateWireComponent(wireComp);
            ComponentManager.UpdateTransformComponent(
                CalculateTransformComponentForWire(wireComp.P1, wireComp.P2).ToTransformComponent(wireComp.Entity)
            );
        }

        var outputComp = ComponentManager.GetOutputComponent(entity);
        var outputs = outputComp.Outputs;

        for (var i = 0; i < outputs.Count; i++)
        {
            var wireComp = ComponentManager.GetWireComponent(outputs[i].Entity);

            Debug.Assert(ComponentManager.GetEntityTypeComponent(wireComp.Entity).Type == IoType.Wire);
            Debug.Assert(EntityManager.IsAlive(wireComp.Entity));

            wireComp = wireComp with { P2 = CalculateOutputConnectionPoint(type, outputs[i].HookNumber, entityPos) };

            ComponentManager.UpdateWireComponent(wireComp);
            ComponentManager.UpdateTransformComponent(CalculateTransformComponentForWire(wireComp.P1, wireComp.P2)
                .ToTransformComponent(wireComp.Entity));
        }
    }

    public static void RemoveFromOtherOutputs(Entity entity)
    {
        Debug.Assert(ComponentManager.GetEntityTypeComponent(entity).Type == IoType.Wire);
        var inputComp = ComponentManager.GetInputComponent(entity);
        for (var i = 0; i < inputComp.Inputs.Count; i++)
        {
            var otherSideEntity = inputComp.Inputs[i].Entity;

            Debug.Assert(EntityManager.IsAlive(otherSideEntity));
            var otherSideEntityOutputComp = ComponentManager.GetOutputComponent(otherSideEntity);
            var success = otherSideEntityOutputComp.Outputs.Remove(new ConnectionInfo(entity, default));
            Debug.Assert(success);

            ComponentManager.UpdateOutputComponent(otherSideEntityOutputComp);
        }
    }

    public static void RemoveFromOtherInputs(Entity entity)
    {
        Debug.Assert(ComponentManager.GetEntityTypeComponent(entity).Type == IoType.Wire);

        var outputComp = ComponentManager.GetOutputComponent(entity);
        for (var i = 0; i < outputComp.Outputs.Count; i++)
        {
            var otherSideEntity = outputComp.Outputs[i].Entity;

            Debug.Assert(EntityManager.IsAlive(otherSideEntity));
            var otherSideEntityInputComp = ComponentManager.GetInputComponent(otherSideEntity);
            var success = otherSideEntityInputComp.Inputs.Remove(new ConnectionInfo(entity, default));
            Debug.Assert(success);

            ComponentManager.UpdateInputComponent(otherSideEntityInputComp);
        }
    }

    public static Vector2Int CalculateInputConnectionPoint(IoType ioType, int hookNumber, Vector2Int position)
    {
        switch (ioType)
        {
            case IoType.AND:
            case IoType.OR:
            case IoType.XOR:
            case IoType.NAND:
            case IoType.NOR:
            case IoType.XNOR:
            case IoType.LedOutput:
                return hookNumber switch
                {
                    0 => new Vector2Int(position.X, position.Y + (int)(EntityService.RectLGateSize.Y * 0.25)),
                    1 => new Vector2Int(position.X, position.Y + (int)(EntityService.RectLGateSize.Y * 0.75)),
                    _ => throw new InvalidProgramException("Wire hook number out of range")
                };
            case IoType.NOT:
                return new Vector2Int(position.X, position.Y + (int)(EntityService.RectLGateSize.Y * 0.5));
            case IoType.Wire:
            case IoType.Constant:
            default:
                throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null);
        }
    }

    public static Vector2Int CalculateOutputConnectionPoint(IoType ioType, int hookNumber, Vector2Int position)
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
            case IoType.Constant:
                return new Vector2Int(position.X + EntityService.RectLGateSize.X,
                    (int)(position.Y + EntityService.RectLGateSize.Y * 0.5));
            case IoType.Wire:
            case IoType.LedOutput:
            default:
                throw new ArgumentOutOfRangeException(nameof(ioType), ioType, null);
        }
    }

    private static Entity? ConnectToInput(Connection connection)
    {
        InputComponent inputComp;
        if (_existingConnection is null || !EntityManager.IsAlive(_existingConnection.Value.Entity))
        {
            inputComp = ComponentManager.GetInputComponent(connection.Entity);

            if (!CanConnectToInput(connection, inputComp))
            {
                Reset();
                return null;
            }

            _existingConnection = connection;

            return null;
        }

        if (_existingConnection.Value.ConnectionType == connection.ConnectionType)
        {
            Reset();
            return null;
        }

        if (_existingConnection.Value.Entity.Id == connection.Entity.Id)
        {
            Reset();
            return null;
        }

        inputComp = ComponentManager.GetInputComponent(connection.Entity);

        if (!CanConnectToInput(connection, inputComp))
        {
            Reset();
            return null;
        }

        var wire = _existingConnection.Value.ConnectionType == ConnectionType.Input
            ? CreateWire(_existingConnection.Value, connection)
            : CreateWire(connection, _existingConnection.Value);

        inputComp.Inputs.Add(new ConnectionInfo(wire, connection.HookNumber));

        var outputComp = ComponentManager.GetOutputComponent(_existingConnection.Value.Entity);
        outputComp.Outputs.Add(new ConnectionInfo(wire, _existingConnection.Value.HookNumber));

        ComponentManager.UpdateInputComponent(inputComp);
        ComponentManager.UpdateOutputComponent(outputComp);

        Reset();

        return wire;
    }

    private static Entity? ConnectToOutput(Connection connection)
    {
        if (_existingConnection is null || !EntityManager.IsAlive(_existingConnection.Value.Entity))
        {
            _existingConnection = connection;

            return null;
        }

        if (_existingConnection.Value.ConnectionType == connection.ConnectionType)
        {
            Reset();
            return null;
        }

        if (_existingConnection.Value.Entity.Id == connection.Entity.Id)
        {
            Reset();
            return null;
        }

        var outputComp = ComponentManager.GetOutputComponent(connection.Entity);
        var inputComp = ComponentManager.GetInputComponent(_existingConnection.Value.Entity);

        var wire = _existingConnection.Value.ConnectionType == ConnectionType.Input
            ? CreateWire(_existingConnection.Value, connection)
            : CreateWire(connection, _existingConnection.Value);

        outputComp.Outputs.Add(new ConnectionInfo(wire, connection.HookNumber));
        inputComp.Inputs.Add(new ConnectionInfo(wire, _existingConnection.Value.HookNumber));

        ComponentManager.UpdateInputComponent(inputComp);
        ComponentManager.UpdateOutputComponent(outputComp);

        Reset();

        return wire;
    }

    private static bool CanConnectToInput(Connection connection, InputComponent inputComp)
    {
        Debug.Assert(EntityManager.IsAlive(inputComp.Entity));
        var inputs = inputComp.Inputs;
        for (var i = 0; i < inputs.Count; i++)
        {
            if (!EntityManager.IsAlive(inputs[i].Entity))
            {
                continue;
            }

            if (inputs[i].HookNumber == connection.HookNumber)
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

        var state = ComponentManager.GetStateComponent(output.Entity).State;

        return EntityManager.CreateEntity(new InitWire(
            CalculateTransformComponentForWire(p1, p2),
            new InitWireComponent(p1, p2, state),
            new ConnectionInfo(output.Entity, output.HookNumber),
            new ConnectionInfo(input.Entity, input.HookNumber)
        ));
    }

    private static InitTransformComponent CalculateTransformComponentForWire(Vector2Int p1, Vector2Int p2)
    {
        const int magnification = 4;
        var position = new Vector2Int(Math.Min(p1.X, p2.X) - magnification, Math.Min(p1.Y, p2.Y) - magnification);
        var size = new Vector2Int(Math.Abs(p1.X - p2.X) + magnification, Math.Abs(p1.Y - p2.Y) + magnification);

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