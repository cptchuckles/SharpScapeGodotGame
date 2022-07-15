using Godot;
using SharpScape.Game;
using SharpScape.Game.Dto;
using UsernameColors = System.Collections.Generic.Dictionary<string, Godot.Color>;

public class Chatbox : PanelContainer
{
    private ScrollContainer _messageRegion;
    private VBoxContainer _messageList;
    private PackedScene _chatMessage;
    private LineEdit _input;
    private Button _submitButton;
    private CheckBox _autoScroll;
    private CheckButton _showChat;

    private SharpScapeClient _client;
    private UsernameColors _usernameColors = new UsernameColors();

    public override void _Ready()
    {
        _client = this.GetSingleton<SharpScapeClient>();
        _client.Connect("ChatMessageReceived", this, nameof(_OnChatMessageReceived));

        _messageRegion = GetNode<ScrollContainer>("MarginContainer/Interface/MessageRegion");
        _messageList = GetNode<VBoxContainer>("MarginContainer/Interface/MessageRegion/MessageList");
        _chatMessage = GD.Load<PackedScene>("res://client/scenes/ui/Chatbox/ChatMessage.tscn");

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
        _input.GrabFocus();
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
        if (! _usernameColors.ContainsKey(username))
        {
            _usernameColors[username] = Color.FromHsv(
                hue: GD.Randf(),
                saturation: (float) GD.RandRange(0.3f, 0.6f),
                value: 0.95f
                );
        }
        var chatMessage = _chatMessage.Instance();
        var usernameLabel = chatMessage.GetNode<Label>("Username");
        usernameLabel.Text = $"<{username}>";
        usernameLabel.AddColorOverride("font_color", _usernameColors[username]);
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