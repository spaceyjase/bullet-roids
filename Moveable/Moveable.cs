using Godot;

namespace Moveable;

public partial class Moveable : Node2D
{
  private Camera2D camera;
  protected Vector2 screenSize;
  
	public override void _Ready()
	{
		camera = GetTree().GetNodesInGroup("Camera")[0] as Camera2D;
		if (camera is not null) screenSize = camera.GetViewportRect().Size / camera.Zoom;
	}

  public override void _Process(double delta)
  {
    base._Process(delta);
    
    ScreenWrapCheck();
  }

  private void ScreenWrapCheck()
  {
    var position = Position;
    if (position.X > screenSize.X)
    {
      position.X = 0;
    }

    if (position.X < 0)
    {
      position.X = screenSize.X;
    }

    if (position.Y > screenSize.Y)
    {
      position.Y = 0;
    }
    if (position.Y < 0)
    {
      position.Y = screenSize.Y;
    }
    Position = position;
  }
}