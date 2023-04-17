using System.Collections.Generic;
using Godot;
using Shared;

namespace UI;

public partial class HUD : CanvasLayer
{
    [Signal]
    public delegate void StartGameEventHandler();

    private readonly List<TextureRect> lives = new();
    private Label messageLabel;
    private TextureButton startButton;
    private Label scoreLabel;
    private Label title;

    public Timer MessageTimer { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        foreach (
            var l in GetNode<Node>("MarginContainer/HBoxContainer/LivesContainer").GetChildren()
        )
        {
            lives.Add(l as TextureRect);
        }

        title = GetNode<Label>("Title");
        messageLabel = GetNode<Label>("MessageLabel");
        MessageTimer = GetNode<Timer>("MessageTimer");
        MessageTimer.Timeout += () =>
        {
            messageLabel.Hide();
            ShowMessage(string.Empty);
        };
        startButton = GetNode<TextureButton>("StartButton");
        startButton.Pressed += () =>
        {
            startButton.Hide();
            title.Hide();
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
        messageLabel.Show();
        MessageTimer.Start();
    }

    public async void GameOver()
    {
        ShowMessage("Game Over!");
        await ToSignal(MessageTimer, "timeout");
        startButton.Show();
        title.Show();
    }
}
