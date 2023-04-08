using Godot;

namespace Moveable;

public partial class Moveable : Node2D
{
  public Vector2 ScreenSize { get; set; }

  public override void _Process(double delta)
  {
    base._Process(delta);
    
    ScreenWrapCheck();
  }

  private void ScreenWrapCheck()
  {
    var position = Position;
    if (position.X > ScreenSize.X)
    {
      position.X = 0;
    }

    if (position.X < 0)
    {
      position.X = ScreenSize.X;
    }

    if (position.Y > ScreenSize.Y)
    {
      position.Y = 0;
    }
    if (position.Y < 0)
    {
      position.Y = ScreenSize.Y;
    }
    Position = position;
  }
}