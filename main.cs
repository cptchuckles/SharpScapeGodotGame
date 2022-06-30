using Godot;
using System;

public class main : Node2D
{
    public override void _Ready()
    {
        if (OS.GetName() == "HTML5")
            GetTree().ChangeScene("res://client/scenes/ClientUI.tscn");
        else if (new File().FileExists("res://development.tscn"))
            GetTree().ChangeScene("res://development.tscn");
        else
            GetTree().ChangeScene("res://server/scenes/server.tscn");
    }
}
