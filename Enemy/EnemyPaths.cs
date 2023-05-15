using System;
using Godot;

namespace Enemy;

public partial class EnemyPaths : Node
{
    public override void _Ready()
    {
        base._Ready();

        var camera = GetTree().GetNodesInGroup("Camera")[0] as Camera2D;
        if (camera is null)
        {
            throw new NullReferenceException("No camera found in group 'Camera'");
        }

        foreach (var path in GetChildren())
        {
            if (path is not Path2D follow)
                continue;

            follow.Scale /= camera.Zoom;
        }
    }
}
