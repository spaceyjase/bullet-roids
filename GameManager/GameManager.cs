using Godot;
using Player.PlayerBullet;

namespace GameManager;

public partial class GameManager : Node2D
{
  public override void _Ready()
  {
    base._Ready();

    SetProcess(false);
  }

  private void OnPlayer_Shoot(PackedScene bulletScene, Vector2 position, float direction)
  {
    var bulletInstance = bulletScene.Instantiate<PlayerBullet>();
    bulletInstance.Start(position, direction);
    AddChild(bulletInstance);
  }
}