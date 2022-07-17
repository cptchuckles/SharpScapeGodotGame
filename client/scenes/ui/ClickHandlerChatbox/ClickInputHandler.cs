using Godot;
using SharpScape.Game;
using SharpScape.Game.Dto;
using System;

public class ClickInputHandler : Control
{
    private Vector2 _destination = new Vector2();

    private Sprite _clickMarker;
    private SharpScapeClient _client;

    private bool _modalOpen = false;

    public override void _Ready()
    {
        _clickMarker = GetNode<Sprite>("ClickMarker");
        RemoveChild(_clickMarker);
        GetTree().CurrentScene.AddChild(_clickMarker);
        _clickMarker.SetAsToplevel(true);
        _clickMarker.Hide();

        _client = this.GetSingleton<SharpScapeClient>();
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (_modalOpen) return;

        if (inputEvent is InputEventMouseButton click && click.Pressed && click.ButtonIndex == 1)
        {
            var clientCamera = _client.GetGameAvatar().GetNode<Camera2D>("PlayerCamera");
            _destination = clientCamera.GetCameraScreenCenter() - GetViewport().Size/2 + click.Position;
            _clickMarker.GlobalPosition = _destination;
            _clickMarker.Show();
            GetTree().CreateTimer(0.1f).Connect("timeout", _clickMarker, "hide");

            var destData = GD.Var2Bytes(_destination);
            var moveRequest = new MessageDto(MessageEvent.Movement, Convert.ToBase64String(destData));
            _client.SendData(Utils.ToJson(moveRequest));
        }
    }
    public override void _Input(InputEvent inputEvent)
    {
        if (_modalOpen) return;

        if (inputEvent.IsActionPressed("ui_cancel"))
        {
            var modal = GD.Load<PackedScene>("res://client/scenes/ui/LogoutModal.tscn").Instance();
            modal.Connect("tree_exited", this, "set", new Godot.Collections.Array {"_modalOpen", false});
            AddChild(modal);
            _modalOpen = true;
        }
    }
}
