using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace LinuxTShockServerStop
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        public override string Name => "LinuxTShockServerStop";
        public override string Author => "oli";
        public override Version Version => new("1.0");

        public Plugin(Main game) : base(game) { }
        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
        }
        private void OnPostInitialize(EventArgs args)
        {
            if (OperatingSystem.IsLinux())
            {
                static void AddCommand(Command c)
                {
                    Commands.ChatCommands.RemoveAll(c2 => c2.Names.Any(n => c.Names.Contains(n)));
                    Commands.ChatCommands.Add(c);
                }
                AddCommand(new Command(Permissions.maintenance, Stop, "stop", "exit", "off"));
                AddCommand(new Command(Permissions.maintenance, StopNoSave, "stop-nosave", "exit-nosave", "off-nosave"));
                Console.CancelKeyPress += OnConsoleCancel;
            }
        }

        private void OnConsoleCancel(object? sender, ConsoleCancelEventArgs e)
        {
            StopServer(true, "");
        }
        private static void Stop(CommandArgs e)
        {
            StopServer(true, e.Parameters.Count == 0 ? "Server shutting down!" : "Server shutting down: " + string.Join(" ", e.Parameters));
        }
        private static void StopNoSave(CommandArgs e)
        {
            StopServer(false, e.Parameters.Count == 0 ? "Server shutting down!" : "Server shutting down: " + string.Join(" ", e.Parameters));
        }
        private static void StopServer(bool saveWorld = true, string reason = "Server shutting down!")
        {
            TShock.ShuttingDown = true;
            if (saveWorld)
                TShock.Utils.SaveWorld();
            foreach (var player in TShock.Players.Where(p => p != null))
            {
                if (Main.ServerSideCharacter && player.IsLoggedIn)
                    player.SaveServerCharacter();
                player.Disconnect(reason);
            }
            TShock.Utils.Broadcast(reason, 255, 0, 0);
            Environment.Exit(-1); // Fuck Linux
        }
    }
}
