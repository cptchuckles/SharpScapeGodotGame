using Godot;
using SharpScape.Game;
using SharpScape.Game.Dto;
using System;

public class Chatbox : PanelContainer
{
    private ScrollContainer _messageRegion;
    private VBoxContainer _messageList;
    private LineEdit _input;
    private Button _submitButton;
    private CheckBox _autoScroll;
    private CheckButton _showChat;

    private SharpScapeClient _client;

    public override void _Ready()
    {
        _client = this.GetSingleton<SharpScapeClient>();
        _client.Connect("ChatMessageReceived", this, nameof(_OnChatMessageReceived));

        _messageRegion = GetNode<ScrollContainer>("MarginContainer/Interface/MessageRegion");
        _messageList = GetNode<VBoxContainer>("MarginContainer/Interface/MessageRegion/MessageList");
        _input = GetNode<LineEdit>("MarginContainer/Interface/UserControls/Input");
        _submitButton = GetNode<Button>("MarginContainer/Interface/UserControls/SubmitButton");
        _autoScroll = GetNode<CheckBox>("MarginContainer/Interface/UserControls/AutoScroll");
        _showChat = GetNode<CheckButton>("MarginContainer/Interface/UserControls/ShowChat");
        _showChat.Connect("toggled", this, nameof(_OnShowChatToggled));

        _input.GrabFocus();
        _submitButton.Connect("pressed", this, nameof(SendChatMessage));
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
            SendChatMessage();
        }
    }

    private void SendChatMessage()
    {
        if (_input.Text.Length == 0)
            return;

        var message = new MessageDto(MessageEvent.Message, _input.Text);
        _client.SendData(Utils.ToJson(message));

        _input.Text = string.Empty;
        _input.GrabFocus();
    }

    private void _OnChatMessageReceived(string username, string content)
    {
        var chatMessage = GD.Load<PackedScene>("res://client/scenes/ui/Chatbox/ChatMessage.tscn").Instance();
        chatMessage.GetNode<Label>("Username").Text = $"<{username}>";
        chatMessage.GetNode<Label>("Content").Text = content;
        _messageList.AddChild(chatMessage);

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