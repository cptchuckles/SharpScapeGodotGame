using Godot;
using System;

public class Chatbox : PanelContainer
{
    private ScrollContainer _messageRegion;
    private VBoxContainer _messageList;
    private Label _username;
    private LineEdit _input;
    private Button _submitButton;
    private CheckBox _autoScroll;
    private CheckButton _showChat;

    public override void _Ready()
    {
        _messageRegion = GetNode<ScrollContainer>("MarginContainer/Interface/MessageRegion");
        _messageList = GetNode<VBoxContainer>("MarginContainer/Interface/MessageRegion/MessageList");
        _username = GetNode<Label>("MarginContainer/Interface/UserControls/Username");
        _input = GetNode<LineEdit>("MarginContainer/Interface/UserControls/Input");
        _submitButton = GetNode<Button>("MarginContainer/Interface/UserControls/SubmitButton");
        _autoScroll = GetNode<CheckBox>("MarginContainer/Interface/UserControls/AutoScroll");
        _showChat = GetNode<CheckButton>("MarginContainer/Interface/UserControls/ShowChat");
        _showChat.Connect("toggled", this, nameof(_OnShowChatToggled));

        _input.GrabFocus();
        _submitButton.Connect("pressed", this, nameof(AddChatMessage));
    }

    private void _OnShowChatToggled(bool enabled)
    {
        if (enabled)
        {
            MarginTop = -300f;
        }
        else
        {
            MarginTop = -66f;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ChatSendMessage"))
        {
            AddChatMessage();
            _input.GrabFocus();
        }
    }

    private void AddChatMessage()
    {
        if (_input.Text.Length == 0)
            return;

        var chatMessage = GD.Load<PackedScene>("res://client/scenes/ui/Chatbox/ChatMessage.tscn").Instance();
        chatMessage.GetNode<Label>("Username").Text = _username.Text;
        chatMessage.GetNode<Label>("Content").Text = _input.Text;
        _messageList.AddChild(chatMessage);
        _input.Text = string.Empty;

        if (_autoScroll.Pressed)
        {
            GetTree().Connect("idle_frame", this, nameof(UpdateMessageRegionScroll), flags: (uint)ConnectFlags.Oneshot);
        }
    }
    private void UpdateMessageRegionScroll()
    {
        _messageRegion.ScrollVertical = (int) _messageList.RectSize.y;
    }
}