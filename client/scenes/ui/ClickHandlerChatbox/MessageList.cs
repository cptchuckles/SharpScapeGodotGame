using Godot;
using System;

public class MessageList : VBoxContainer
{
    private bool _dragging = false;

    [Export] private NodePath VerticalSizerNode;
    [Export] private NodePath HorizontalSizerNode;
    private VSplitContainer _verticalSizer;
    private HSplitContainer _horizontalSizer;

    public override void _Ready()
    {
        if (VerticalSizerNode != null)
        {
            _verticalSizer = GetNode<VSplitContainer>(VerticalSizerNode);
        }
        if (HorizontalSizerNode != null)
        {
            _horizontalSizer = GetNode<HSplitContainer>(HorizontalSizerNode);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton e && e.ButtonIndex == 1)
        {
            _dragging = e.Pressed;
        }
        else if (@event is InputEventMouseMotion motion && _dragging)
        {
            _verticalSizer.SplitOffset += (int) motion.Relative.y;
            _horizontalSizer.SplitOffset += (int) motion.Relative.x;
        }
    }
}
