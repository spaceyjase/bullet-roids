using System.Collections.Generic;
using Godot;
using Shared;

namespace UI;

public partial class HUD : CanvasLayer
{
    [Signal]
    public delegate void StartGameEventHandler();

    private List<TextureRect> lives = new();
    private Label messageLabel;
    private Timer messageTimer;
    private TextureButton startButton;
    private Label scoreLabel;

    public Timer MessageTimer => messageTimer;

    public override void _Ready()
    {
        base._Ready();

        foreach (
            var l in GetNode<Node>("MarginContainer/HBoxContainer/LivesContainer").GetChildren()
        )
        {
            lives.Add(l as TextureRect);
        }

        messageLabel = GetNode<Label>("MessageLabel");
        messageTimer = GetNode<Timer>("MessageTimer");
        messageTimer.Timeout += () =>
        {
            messageLabel.Hide();
            ShowMessage(string.Empty);
        };
        startButton = GetNode<TextureButton>("StartButton");
        startButton.Pressed += () =>
        {
            startButton.Hide();
            EmitSignal(SignalName.StartGame);
        };
        scoreLabel = GetNode<Label>("MarginContainer/HBoxContainer/ScoreLabel");

        EventBus.Instance.ScoreUpdated += OnScoreUpdated;
    }

    private void OnScoreUpdated(int newScore)
    {
        scoreLabel.Text = $"{newScore:D10}";
    }

    private void OnLivesUpdated(int livesRemaining)
    {
        for (var i = 0; i < lives.Count; ++i)
        {
            lives[i].Visible = livesRemaining > i;
        }
    }

    public void ShowMessage(string message)
    {
        messageLabel.Text = message;
    }

    public async void GameOver()
    {
        ShowMessage("Game Over!");
        await ToSignal(messageTimer, "timeout");
        startButton.Show();
    }
}
