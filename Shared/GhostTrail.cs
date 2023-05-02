using Godot;

namespace Shared;

public partial class GhostTrail : Sprite2D
{
    [Export]
    private PackedScene fadingSpriteScene { get; set; }

    public bool IsEmitting
    {
        get => isEmitting;
        set
        {
            isEmitting = value;
            if (value)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }
    }

    private void SpawnGhost()
    {
        var ghost = fadingSpriteScene.Instantiate<FadingSprite>();
        ghost.Offset = Offset;
        ghost.Texture = Texture;
        ghost.FlipH = FlipH;
        ghost.FlipV = FlipV;
        ghost.RegionEnabled = RegionEnabled;
        ghost.RegionRect = RegionRect;
        ghost.Rotation = GlobalRotation;
        AddChild(ghost);
        ghost.TopLevel = true;
        ghost.GlobalPosition = GlobalPosition;
    }

    private Timer timer;
    private bool isEmitting;

    public override void _Ready()
    {
        base._Ready();
        timer = GetNode<Timer>("Timer");
        timer.Timeout += SpawnGhost;
    }
}
