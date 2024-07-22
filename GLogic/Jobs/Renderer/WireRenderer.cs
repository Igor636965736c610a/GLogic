using System.Diagnostics;
using GLogic.Data;
using GLogic.Imports;
using GLogic.Jobs.Internal.EcsStateModifiers;
using GLogicECS.Api;
using GLogicGlobal.Common;
using SDL2;

namespace GLogic.Jobs.Renderer;

public sealed class WireRenderer
{
    private readonly IntPtr _renderer;

    private readonly IRendererStateAccess _rendererStateAccess;
    //private readonly TextureStorage _textureStorage;

    public WireRenderer(IRendererStateAccess rendererStateAccess, IntPtr renderer, TextureStorage textureStorage)
    {
        _rendererStateAccess = rendererStateAccess;
        _renderer = renderer;
        //_textureStorage = textureStorage;
    }

    public void RenderStaticWire(WireRendererInfo info)
    {
        SDL.SDL_Color color;
        if (info.State)
        {
            color = new SDL.SDL_Color
            {
                r = 255,
                g = 0,
                b = 0,
                a = 255
            };
        }
        else
        {
            color = new SDL.SDL_Color
            {
                r = 52,
                g = 56,
                b = 64,
                a = 255
            };
        }
        
        SDL2Gfx.AaLineRgba(_renderer, info.P1.X, info.P1.Y, info.P2.X, info.P2.Y, color);
        
        // double angle = Math.Atan2(info.P2.Y - info.P1.Y, info.P2.X - info.P1.X);
        // int dx = (int)(2 / 2.0 * Math.Sin(angle));
        // int dy = (int)(2 / 2.0 * Math.Cos(angle));
        //
        // int[] vx = new int[4] { info.P1.X + dx, info.P2.X + dx, info.P2.X - dx, info.P1.X - dx };
        // int[] vy = new int[4] { info.P1.Y - dy, info.P2.Y - dy, info.P2.Y + dy, info.P1.Y + dy };
        // GCHandle handleVx = GCHandle.Alloc(vx, GCHandleType.Pinned);
        // GCHandle handleVy = GCHandle.Alloc(vy, GCHandleType.Pinned);
        //
        // try
        // {
        //     IntPtr vxPtr = handleVx.AddrOfPinnedObject();
        //     IntPtr vyPtr = handleVy.AddrOfPinnedObject();
        //
        //     SDL2Gfx.filledPolygonRGBA(_renderer, vxPtr, vyPtr, 4, 
        //         color.r, 
        //         color.g, 
        //         color.b, 
        //         color.a);
        // }
        // finally
        // {
        //     handleVx.Free();
        //     handleVy.Free();
        // }

        //SDL2Gfx.thickLineRGBA(_renderer, info.P1.X, info.P1.Y, info.P2.X, info.P2.Y, 2, color.r, color.g, color.b,
        //    color.a);
    }

    public void RenderChosenWireFromMenuOption()
    {
        if (!WireService.IsHookedUp(out var outExistingConnection))
        {
            return;
        }

        Debug.Assert(outExistingConnection is not null, "WireService.ExistingConnection == null");
        var existingConnection = outExistingConnection.Value;

        SDL.SDL_GetMouseState(out var x, out var y);

        var ioTypeOutgoingLGate = ComponentManager.GetEntityTypeComponent(existingConnection.Entity).Type;
        var outgoingLGatePosition = ComponentManager.GetTransformComponent(existingConnection.Entity).Position;

        var p1 = _rendererStateAccess.GetRelativeShiftedCursor(new Vector2Int(
            RendererApi.CalculateShiftRelatively(x, _rendererStateAccess.Zoom, _rendererStateAccess.CameraShift.X),
            RendererApi.CalculateShiftRelatively(y, _rendererStateAccess.Zoom, _rendererStateAccess.CameraShift.Y)
        ));

        (var p2, var value) = existingConnection.ConnectionType switch
        {
            ConnectionType.Input => (WireService.CalculateInputConnectionPoint(
                ioTypeOutgoingLGate,
                existingConnection.HookNumber, outgoingLGatePosition
            ), false),

            ConnectionType.Output => (WireService.CalculateOutputConnectionPoint(
                ioTypeOutgoingLGate,
                existingConnection.HookNumber, outgoingLGatePosition
            ), ComponentManager.GetStateComponent(existingConnection.Entity).State),

            _ => throw new ArgumentOutOfRangeException()
        };
        p2 = new Vector2Int
        {
            X = RendererApi.CalculateShiftRelatively(p2.X, _rendererStateAccess.Zoom,
                _rendererStateAccess.CameraShift.X),
            Y = RendererApi.CalculateShiftRelatively(p2.Y, _rendererStateAccess.Zoom,
                _rendererStateAccess.CameraShift.Y)
        };

        RenderStaticWire(new WireRendererInfo(p1, p2, value));
    }
}

public readonly record struct WireRendererInfo(Vector2Int P1, Vector2Int P2, bool State);