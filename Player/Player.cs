using Godot;

namespace Player;

public partial class Player : Moveable.Moveable
{
  [Export]
  private float MovementSpeed { get; set; }
  [Export]
  private float RotationSpeed { get; set; }

  private int collisionCount;
  private bool active;

  private int CollisionCount
  {
    get => collisionCount;
    set
    {
      // clamp to 0
      if (value < 0)
      {
        value = 0;
      }

      collisionCount = value;
    }
  }

  public override void _Ready()
  {
    base._Ready();
    
    IsActive = true;
    
    Position = screenSize / 2;
  }

  private void OnArea2d_Area_Entered(Area2D area)
  {
    // TODO: bullet check, set property if true
    ++CollisionCount;
    GD.Print($"{area} CollisionCount: {CollisionCount}");
  }

  private void OnArea2d_Area_Exited(Area2D area)
  {
    // TODO: bullet check, set property if true
    --CollisionCount;
    GD.Print($"{area} CollisionCount: {CollisionCount}");
  }

  public bool Colliding => CollisionCount > 0;
  public bool IsActive
  {
    get => active;
    private set
    {
      active = value;
      Visible = active;
    }
  }

  public void Move(Vector2 movement, float rotation, double delta)
  {
    Position += movement * MovementSpeed * (float)delta;
    Rotation += rotation * RotationSpeed * (float)delta;
  }
}