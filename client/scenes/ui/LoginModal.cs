using Godot;
using Newtonsoft.Json;
using SharpScape.Game.Dto;
using SharpScape.Game.Services;
using static SharpScape.Game.NodeExtensions;

public class LoginModal : PanelContainer
{
    [Signal] delegate void LoginPayloadReady();

    private MPClientCrypto _crypto;

    [Export] private NodePath StatusLineNode;
    [Export] private NodePath UsernameInputNode;
    [Export] private NodePath PasswordInputNode;
    [Export] private NodePath SubmitButtonNode;

    private Label _statusline;
    private LineEdit _username;
    private LineEdit _password;
    private Button _submit;
    public string SecurePayload;

    public override void _Ready()
    {
        _statusline = GetNode<Label>(StatusLineNode);

        _username = GetNode<LineEdit>(UsernameInputNode);
        _username.GrabFocus();

        _password = GetNode<LineEdit>(PasswordInputNode);

        _submit = GetNode<Button>(SubmitButtonNode);
        _submit.Connect("pressed", this, "_OnLoginSubmitPressed");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && key.Scancode == (uint)KeyList.Enter)
        {
            if (_password.HasFocus())
            {
                _submit.GrabFocus();
                _OnLoginSubmitPressed();
            }
        }
    }

    private void _OnLoginSubmitPressed()
    {
        _submit.Disabled = true;
        _submit.Text = "Logging in...";
        _crypto = this.GetTransient<MPClientCrypto>();
        _crypto.KeyProvider.Connect("KeyReady", this, nameof(_OnCryptoKeyReady), flags: (uint)ConnectFlags.Oneshot);
    }

    private void _OnCryptoKeyReady(bool success)
    {
        if (!success)
        {
            ErrorRetry("Connection error. Retry");
            return;
        }
        var payloadJson = Utils.ToJson(new LoginPayload(_username.Text, _password.Text));
        SecurePayload = _crypto.EncryptPayload(payloadJson);
        EmitSignal(nameof(LoginPayloadReady));
    }

    public void ErrorRetry(string message)
    {
        _statusline.Text = message;
        _username.GrabFocus();
        _password.Text = string.Empty;
        _submit.Text = "Retry";
        _submit.Disabled = false;
    }

    internal class LoginPayload : JsonSerializable
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public LoginPayload(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
