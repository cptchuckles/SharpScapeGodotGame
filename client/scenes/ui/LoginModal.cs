using Godot;
using Newtonsoft.Json;
using SharpScape.Game.Dto;
using SharpScape.Game.Services;
using static SharpScape.Game.NodeExtensions;

public class LoginModal : Panel
{
    [Signal] delegate void LoginPayloadReady();

    private MPClientCrypto _crypto;

    private Label _statusline;
    private LineEdit _username;
    private LineEdit _password;
    private Button _submit;
    public string SecurePayload;

    public override void _Ready()
    {
        _statusline = GetNode<Label>("Statusline");

        _username = GetNode<LineEdit>("Username");
        _username.GrabFocus();

        _password = GetNode<LineEdit>("Password");

        _submit = GetNode<Button>("LoginSubmit");
        _submit.Connect("pressed", this, "_OnLoginSubmitPressed");
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