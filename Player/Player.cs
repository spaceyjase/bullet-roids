using Godot;

namespace Player;

public partial class Player : Node2D
{
  [Export]
  public float MovementSpeed { get; set; }
  [Export]
  public float RotationSpeed { get; set; }
  
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
  }

  private void OnArea2d_Area_Entered(Area2D area)
  {
    // TODO: bullet check, set property if true
    ++CollisionCount;
  }

  private void OnArea2d_Area_Exited(Area2D area)
  {
    // TODO: bullet check, set property if true
    --CollisionCount;
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
    // TODO: check for screen wrap
  }

}