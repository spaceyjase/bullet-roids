using Godot;

namespace Player.PlayerBullet;

public partial class PlayerBullet : Node2D
{
  [Export] private int speed = 1000;

  private Vector2 velocity;
  private AudioStreamPlayer2D audioStreamPlayer;

  public override void _Ready()
  {
    base._Ready();
    audioStreamPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer");
  }

  public void Start(Vector2 position, float direction)
  {
    Position = position;
    Rotation = direction;
    velocity = new Vector2(0, -speed).Rotated(direction);
  }

  public override void _Process(double delta)
  {
    Position += velocity * (float)delta;
  }
  
  private async void OnVisibleOnScreenNotifier2d_ScreenExited()
  {
    if (audioStreamPlayer.Playing)
    {
      await ToSignal(audioStreamPlayer, "finished");
    }
    QueueFree();
  }

  private void OnArea2d_BodyEntered(Node2D body)
  {
    if (body.IsInGroup(nameof(Roid)))
    {
      // TODO: implement this, remove check for "Rocks" group - any body should be able to explode (layers)
      // body as Asteroid.Explode();
      QueueFree();
    }
  }
}