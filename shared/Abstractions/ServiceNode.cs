using Godot;

namespace SharpScape.Game.Services
{
    public abstract class ServiceNode : Node
    {
    }

    public abstract class NetworkServiceNode : ServiceNode
    {
        public abstract void _OnWorldLoadingComplete();
        public abstract void _OnWorldAvatarSpawned(GameAvatar who);
    }
}