using Godot;
using Newtonsoft.Json;
using System;

public class LoginModal : Panel
{
    [Signal] delegate void LoginPayloadReady();

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
    }

    private void _OnLoginSubmitPressed()
    {
        var payloadJson = new LoginPayload(_username.Text, _password.Text).ToString();
        SecurePayload = new ApiPayloadSecurity().EncryptPayload(payloadJson);
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