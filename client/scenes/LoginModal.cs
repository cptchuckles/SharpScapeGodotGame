using Godot;
using Newtonsoft.Json;
using SharpScape.Game.Services;
using static SharpScape.Game.NodeExtensions;

public class LoginModal : Panel
{
    [Signal] delegate void LoginPayloadReady();

    private ApiPayloadSecurity _crypto;

    private LineEdit _username;
    private LineEdit _password;
    private Button _submit;
    public string SecurePayload;

    public override void _Ready()
    {
        _username = GetNode<LineEdit>("Username");
        _username.GrabFocus();

        _password = GetNode<LineEdit>("Password");

        _submit = GetNode<Button>("LoginSubmit");
        _submit.Connect("pressed", this, "_OnLoginSubmitPressed");

        _submit.Disabled = true;
        _crypto = this.GetTransient<ApiPayloadSecurity>();
        if (! _crypto.keyProvider.IsKeyReady())
            // Enable the _submit button once keyProvider has retrieved the public key from the API
            _crypto.keyProvider.Connect("KeyReady", _submit, "set", new Godot.Collections.Array { "disabled", false }, (uint)ConnectFlags.Oneshot);
        else
            _submit.Disabled = false;
    }

    private void _OnLoginSubmitPressed()
    {
        var payloadJson = new LoginPayload(_username.Text, _password.Text).ToString();
        SecurePayload = Utils.ToJson(_crypto.EncryptPayload(payloadJson));
        EmitSignal(nameof(LoginPayloadReady));
    }

    internal class LoginPayload
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public LoginPayload(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}