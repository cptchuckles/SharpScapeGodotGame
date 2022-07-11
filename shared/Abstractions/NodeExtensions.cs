using Godot;
using System.Linq;
using SharpScape.Game.Services;
using System;

namespace SharpScape.Game
{
    public static class NodeExtensions
    {
        public static T GetSingleton<T>(this Node self) where T : ServiceNode
        {
            var singleton = self.GetTree().Root.GetChildren().OfType<T>().FirstOrDefault();
            if (singleton is null)
            {
                var constructor = typeof(T).GetConstructor(new Type[] {});
                if (constructor is null)
                {
                    return null;
                }
                singleton = (T) constructor.Invoke(new object[] {});
                singleton.Name = singleton.GetType().ToString();
                self.GetTree().Root.AddChild(singleton);
            }
            return singleton;
        }

        public static T GetScoped<T>(this Node self) where T : ServiceNode
        {
            var scoped = self.GetTree().CurrentScene.GetChildren().OfType<T>().FirstOrDefault();
            if (scoped is null)
            {
                var constructor = typeof(T).GetConstructor(new Type[] {});
                if (constructor is null)
                {
                    return null;
                }
                scoped = (T) constructor.Invoke(new object[] {});
                scoped.Name = scoped.GetType().ToString();
                self.GetTree().CurrentScene.AddChild(scoped);
            }
            return scoped;
        }
        
        public static T GetTransient<T>(this Node self) where T : ServiceNode
        {
            var transient = self.GetChildren().OfType<T>().FirstOrDefault();
            if (transient is null)
            {
                var constructor = typeof(T).GetConstructor(new Type[] {});
                if (constructor is null)
                {
                    return null;
                }
                transient = (T) constructor.Invoke(new object[] {});
                transient.Name = transient.GetType().ToString();
                self.AddChild(transient);
            }
            return transient;
        }
    }
}