using System;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;

public class ServerUI : Control
{
    public SharpScapeServer _server;

    public SpinBox _port;
    public LineEdit _lineEdit;
    public OptionButton _writeMode;
    public RichTextLabel _logDest;

    public override void _Ready()
    {
        _server = GetNode<SharpScapeServer>("SharpScapeServer");
        _port = GetNode<SpinBox>("Panel/VBoxContainer/HBoxContainer/Port");
        _lineEdit = GetNode<LineEdit>("Panel/VBoxContainer/HBoxContainer3/LineEdit");
        _writeMode = GetNode<OptionButton>("Panel/VBoxContainer/HBoxContainer2/WriteMode");
        _logDest = GetNode<RichTextLabel>("Panel/VBoxContainer/RichTextLabel");

        _writeMode.Clear();
        _writeMode.AddItem("BINARY");
        _writeMode.SetItemMetadata(0, WebSocketPeer.WriteMode.Binary);
        _writeMode.AddItem("TEXT");
        _writeMode.SetItemMetadata(1, WebSocketPeer.WriteMode.Text);
        _writeMode.Select(0);
    }

    public void _OnListenToggled(bool pressed)
    {
        if(pressed)
        {
            string[] supportedProtocols = {"my-protocol", "binary"};
            var port = (int)_port.Value;
            if(_server.Listen(port, supportedProtocols) == Error.Ok)
            {
                Utils.Log(_logDest, $"Listing on port {port}");
            }
            else
            {
                Utils.Log(_logDest, $"Error  listening on port {port}");
            }
        }
        else
        {
            _server.Stop();
            _writeMode.Disabled = false;
            Utils.Log(_logDest, "SharpScapeServer stopped");
        }
    }

    public void _OnSendPressed()
    {
        if(_lineEdit.Text == "")
        {
            return;
        }
        Utils.Log(_logDest, $"Sending data {_lineEdit.Text}");
        _server.SendData(_lineEdit.Text);
        _lineEdit.Text = "";
    }

    public void _OnWriteModeItemSelected(int _id)
    {
        _server.SetWriteMode((WebSocketPeer.WriteMode)_writeMode.GetSelectedMetadata());
    }
}
