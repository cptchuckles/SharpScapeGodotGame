using Godot;
using System.Linq;

namespace SharpScape
{
    public static class NodeExtensions
    {
        public static T GetSingleton<T>(this Node self) where T : Node, new()
        {
            var singleton = self.GetTree().Root.GetChildren().OfType<T>().FirstOrDefault();
            if (singleton is null)
            {
                singleton = new T();
                singleton.Name = singleton.GetType().ToString();
                self.GetTree().Root.AddChild(singleton);
            }
            return singleton;
        }

        public static T GetScoped<T>(this Node self) where T : Node, new()
        {
            var scoped = self.GetTree().CurrentScene.GetChildren().OfType<T>().FirstOrDefault();
            if (scoped is null)
            {
                scoped = new T();
                scoped.Name = scoped.GetType().ToString();
                self.GetTree().CurrentScene.AddChild(scoped);
            }
            return scoped;
        }
    }
}