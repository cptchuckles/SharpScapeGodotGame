using Godot;
using System;

public class GameAvatar : KinematicBody2D
{
    public int UserId;

    [Export]
    private float _speed = 200.0f;

    public Vector2 Destination;
    public bool InMotion { get; private set; }

    private Label _username;
    public string Username
    {
        get => _username.Text;
        set => _username.Text = value;
    }

    public override void _Ready()
    {
        _username = GetNode<Label>("Username");
        Destination = GlobalPosition;
    }

    public void MoveTo(Vector2 destination)
    {
        Destination = destination;
        InMotion = true;
    }

    public void FocusMe()
    {
        GetNode<Camera2D>("PlayerCamera").Current = true;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (! InMotion) return;

        var movement = GlobalPosition.MoveToward(Destination, _speed * delta) - GlobalPosition;
        var result = MoveAndCollide(movement);

        if (result != null && result.Remainder.IsEqualApprox(Vector2.Zero))
        {
            InMotion = false;
        }
    }
}