using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Newtonsoft.Json;

namespace ItemCleaner
{
    [ApiVersion(2, 1)]
    public class ItemCleanerPlugin : TerrariaPlugin
    {
        private System.Timers.Timer? cleanTimer;
        private string langFilePath = Path.Combine(TShock.SavePath, "Lixeiro.json");
        private Dictionary<string, string> lang = new();

        public override string Name => "Item Cleaner";
        public override string Author => "brasilzinhoz";
        public override string Description => "Limpa itens dropados no chão automaticamente.";
        public override Version Version => new Version(0, 0, 0, 1);

        public ItemCleanerPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            LoadLanguage();
            cleanTimer = new System.Timers.Timer(30 * 60 * 1000); // 30 m
            cleanTimer.Elapsed += OnCleanTimerElapsed;
            cleanTimer.AutoReset = true;
            cleanTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cleanTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void LoadLanguage()
        {
            if (!File.Exists(langFilePath))
            {
                lang = new Dictionary<string, string>
                {
                    { "clean_warning", "[c/ff0000:Os itens serão removidos do chão em 10 segundos!]" },
                    { "clean_done", "[c/ff0000:Todos os itens dropados foram removidos!]" }
                };
                File.WriteAllText(langFilePath, JsonConvert.SerializeObject(lang, Formatting.Indented));
            }
            else
            {
                lang = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(langFilePath)) ?? new();
            }
        }

        private void OnCleanTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            TShock.Utils.Broadcast(lang["clean_warning"], 255, 0, 0);
            System.Threading.Tasks.Task.Delay(10000).ContinueWith(t => CleanItems());
        }

        private void CleanItems()
        {
            for (int i = 0; i < Main.item.Length; i++)
            {
                if (Main.item[i].active && Main.item[i].noGrabDelay == 0)
                {
                    Main.item[i].active = false;
                }
            }
            TSPlayer.All.SendData(PacketTypes.UpdateItemDrop, "", -1);
            TShock.Utils.Broadcast(lang["clean_done"], 255, 0, 0);
        }
    }
}
