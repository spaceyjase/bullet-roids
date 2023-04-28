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
    private Label highScoreLabel;
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
        MessageTimer.Timeout += ClearMessage;
        startButton = GetNode<TextureButton>("StartButton");
        startButton.Pressed += () =>
        {
            startButton.Hide();
            title.Hide();
            highScoreLabel.Hide();
            EmitSignal(SignalName.StartGame);
        };
        scoreLabel = GetNode<Label>("MarginContainer/HBoxContainer/ScoreLabel");
        highScoreLabel = GetNode<Label>("MarginContainer2/HighScore");

        EventBus.Instance.ScoreUpdated += OnScoreUpdated;
        EventBus.Instance.HighScoreUpdated += OnHighScoreUpdated;
    }

    private void ClearMessage()
    {
        messageLabel.Text = string.Empty;
        messageLabel.Hide();
    }

    private void OnScoreUpdated(uint newScore)
    {
        scoreLabel.Text = $"{newScore:D10}";
    }

    private void OnHighScoreUpdated(uint newScore)
    {
        highScoreLabel.Text = $"High-Score: {newScore:D10}";
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
        if (string.IsNullOrEmpty(message))
        {
            ClearMessage();
            return;
        }

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
        highScoreLabel.Show();
    }
}
