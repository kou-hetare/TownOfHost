using System.IO;
using Hazel;
using HarmonyLib;
using System.Linq;
using System;
using static TownOfHost.Translator;
using System.Collections.Generic;

namespace TownOfHost
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    class ChatCommands
    {
        public static bool Prefix(ChatController __instance)
        {
            var text = __instance.TextArea.text;
            string[] args = text.Split(' ');
            var canceled = false;
            var cancelVal = "";
            main.isChatCommand = true;
            Logger.info(text, "SendChat");
            switch (args[0])
            {
                case "/dump":
                    canceled = true;
                    string t = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
                    string filename = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/TownOfHost-v{main.PluginVersion}-{t}.log";
                    FileInfo file = new FileInfo(@$"{System.Environment.CurrentDirectory}/BepInEx/LogOutput.log");
                    file.CopyTo(@filename);
                    System.Diagnostics.Process.Start(@$"{System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}");
                    Logger.info($"{filename}にログを保存しました。", "dump");
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "デスクトップにログを保存しました。バグ報告チケットを作成してこのファイルを添付してください。");
                    break;
                case "/v":
                case "/version":
                    canceled = true;
                    string version_text = "";
                    foreach (var kvp in main.playerVersion.OrderBy(pair => pair.Key))
                    {
                        version_text += $"{kvp.Key}:{Utils.getPlayerById(kvp.Key).getRealName()}:{kvp.Value.version}({kvp.Value.tag})\n";
                    }
                    if (version_text != "") HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, version_text);
                    break;
                default:
                    main.isChatCommand = false;
                    break;
            }
            if (AmongUsClient.Instance.AmHost)
            {
                main.isChatCommand = true;
                switch (args[0])
                {
                    case "/win":
                    case "/winner":
                        canceled = true;
                        Utils.SendMessage("Winner: " + string.Join(",", main.winnerList.Select(b => main.AllPlayerNames[b])));
                        break;

                    case "/l":
                    case "/lastroles":
                        canceled = true;
                        Utils.ShowLastRoles();
                        break;

                    case "/r":
                    case "/rename":
                        canceled = true;
                        if (args.Length > 1) { main.nickName = args[1]; }
                        break;

                    case "/n":
                    case "/now":
                        canceled = true;
                        Utils.ShowActiveSettings();
                        break;

                    case "/dis":
                        canceled = true;
                        if (args.Length < 2) { __instance.AddChat(PlayerControl.LocalPlayer, "crewmate | impostor"); cancelVal = "/dis"; }
                        switch (args[1])
                        {
                            case "crewmate":
                                ShipStatus.Instance.enabled = false;
                                ShipStatus.RpcEndGame(GameOverReason.HumansDisconnect, false);
                                break;

                            case "impostor":
                                ShipStatus.Instance.enabled = false;
                                ShipStatus.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                                break;

                            default:
                                __instance.AddChat(PlayerControl.LocalPlayer, "crewmate | impostor");
                                cancelVal = "/dis";
                                break;
                        }
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Admin, 0);
                        break;

                    case "/h":
                    case "/help":
                        canceled = true;
                        if (args.Length < 2)
                        {
                            Utils.ShowHelp();
                            break;
                        }
                        switch (args[1])
                        {
                            case "r":
                            case "roles":
                                if (args.Length < 3) { getRolesInfo(""); break; }
                                getRolesInfo(args[2]);
                                break;

                            case "att":
                            case "attributes":
                                if (args.Length < 3) { Utils.SendMessage("使用可能な引数(略称): lastimpostor(limp)"); break; }
                                switch (args[2])
                                {
                                    case "lastimpostor":
                                    case "limp":
                                        Utils.SendMessage(getString("LastImpostor") + getString("LastImpostorInfo"));
                                        break;

                                    default:
                                        Utils.SendMessage("使用可能な引数(略称): lastimpostor(limp)");
                                        break;
                                }
                                break;

                            case "m":
                            case "modes":
                                if (args.Length < 3) { Utils.SendMessage("使用可能な引数(略称): hideandseek(has), nogameend(nge), syncbuttonmode(sbm), randommapsmode(rmm)"); break; }
                                switch (args[2])
                                {
                                    case "hideandseek":
                                    case "has":
                                        Utils.SendMessage(getString("HideAndSeekInfo"));
                                        break;

                                    case "nogameend":
                                    case "nge":
                                        Utils.SendMessage(getString("NoGameEndInfo"));
                                        break;

                                    case "syncbuttonmode":
                                    case "sbm":
                                        Utils.SendMessage(getString("SyncButtonModeInfo"));
                                        break;

                                    case "randommapsmode":
                                    case "rmm":
                                        Utils.SendMessage(getString("RandomMapsModeInfo"));
                                        break;

                                    default:
                                        Utils.SendMessage("使用可能な引数(略称): hideandseek(has), nogameend(nge), syncbuttonmode(sbm), randommapsmode(rmm)");
                                        break;
                                }
                                break;


                            case "n":
                            case "now":
                                Utils.ShowActiveRoles();
                                break;

                            default:
                                Utils.ShowHelp();
                                break;
                        }
                        break;

                    default:
                        main.isChatCommand = false;
                        break;
                }
            }
            if (canceled)
            {
                Logger.info("Command Canceled");
                __instance.TextArea.Clear();
                __instance.TextArea.SetText(cancelVal);
                __instance.quickChatMenu.ResetGlyphs();
            }
            return !canceled;
        }

        public static void getRolesInfo(string role)
        {
            var roleList = new Dictionary<CustomRoles, string>
            {
                //Impostor陣営
                { CustomRoles.BountyHunter,"bo" },
                { CustomRoles.Mafia,"mf" },
                { CustomRoles.SerialKiller,"sk" },
                { CustomRoles.ShapeMaster,"sha" },
                { CustomRoles.Vampire,"va" },
                { CustomRoles.Witch,"wi" },
                { CustomRoles.Warlock,"wa" },
                { CustomRoles.Puppeteer,"pup" },
                //Madmate陣営
                { CustomRoles.MadGuardian,"mg" },
                { CustomRoles.Madmate,"mm" },
                { CustomRoles.MadSnitch,"msn" },
                { CustomRoles.SKMadmate,"sm" },
                //両陣営
                { CustomRoles.Watcher,"wat" },
                //Crewmate陣営
                { CustomRoles.Bait,"ba" },
                { CustomRoles.Dictator,"dic" },
                { CustomRoles.Doctor,"doc" },
                { CustomRoles.Lighter,"li" },
                { CustomRoles.Mayor,"my" },
                { CustomRoles.SabotageMaster,"sa" },
                { CustomRoles.Sheriff,"sh" },
                { CustomRoles.Snitch,"sn" },
                { CustomRoles.SpeedBooster,"sb" },
                { CustomRoles.Trapper,"tra" },
                //Neutral陣営
                { CustomRoles.Arsonist,"ar" },
                { CustomRoles.Egoist,"eg" },
                { CustomRoles.Executioner,"exe" },
                { CustomRoles.Jester,"je" },
                { CustomRoles.Opportunist,"op" },
                { CustomRoles.SchrodingerCat,"sc" },
                { CustomRoles.Terrorist,"te" },
                //HAS
                { CustomRoles.Fox,"fo" },
                { CustomRoles.Troll,"tr" },

            };
            var msg = "使用可能な引数(略称): \n";
            var rolemsg="";
            foreach (var r in roleList)
            {
                var roleName = r.Key.ToString();
                var roleShort = r.Value;
                if (String.Compare(role, roleName, true) == 0 || String.Compare(role, roleShort, true) == 0)
                {
                    Utils.SendMessage(getString(roleName) + getString($"{roleName}InfoLong"));
                    return;
                }
                var roleText = $"{roleName.ToLower()}({roleShort.ToLower()}), ";
                if (rolemsg.Length + roleText.Length > 40)
                {
                    msg += rolemsg + "\n";
                    rolemsg = roleText;
                    if (msg.Count(c=>c=='\n') == 3)
                    {
                        Utils.SendMessage(msg);
                        msg = "";
                    }
                }
                else
                {
                    rolemsg += roleText;
                }
            }
            msg += rolemsg;
            if (rolemsg != "")
            {
                Utils.SendMessage(msg);
            }
        }
    }
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    class ChatUpdatePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            float num = 3f - __instance.TimeSinceLastMessage;
            if (main.MessagesToSend.Count > 0 && num <= 0.0f)
            {
                (string, byte) msgData = main.MessagesToSend[0];
                string msg = msgData.Item1;
                byte sendTo = msgData.Item2;
                main.MessagesToSend.RemoveAt(0);
                __instance.TimeSinceLastMessage = 0.0f;
                if (sendTo == byte.MaxValue)
                {
                    PlayerControl.LocalPlayer.RpcSendChat(msg);
                }
                else
                {
                    PlayerControl target = Utils.getPlayerById(sendTo);
                    if (target == null) return;
                    int clientId = target.getClientId();
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SendChat, SendOption.Reliable, clientId);
                    writer.Write(msg);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    class AddChatPatch
    {
        public static void Postfix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
        {
            switch (chatText)
            {
                default:
                    break;
            }
            if (!AmongUsClient.Instance.AmHost) return;
        }
    }
}
