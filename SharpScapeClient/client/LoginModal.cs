using Godot;
using System;

public class LoginModal : Panel
{
    [Signal] delegate void LoginPayloadReady(string securePayload);

    private LineEdit _username;
    private LineEdit _password;
    private Button _submit;

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
        var payload = new Godot.Collections.Dictionary() {
            ["Username"] = _username.Text,
            ["Password"] = _password.Text
        };

        var payloadJson = JSON.Print(payload);
        var securePayload = new ApiPayloadSecurity().EncryptPayload(payloadJson);
        EmitSignal(nameof(LoginPayloadReady), securePayload);

        QueueFree();
    }
}