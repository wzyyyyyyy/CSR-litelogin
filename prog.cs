using CSR;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace login
{
    class scr
    {
        public static void aaascr(MCCSAPI api)
        {
            Dictionary<string, string> uuid = new Dictionary<string, string>();
            api.addAfterActListener(EventKey.onLoadName, x =>
            {
                var a = BaseEvent.getFrom(x) as LoadNameEvent;
                uuid.Add(a.playername, a.uuid);
                return true;
            });
            api.addBeforeActListener(EventKey.onPlayerLeft, x =>
            {
                var a = BaseEvent.getFrom(x) as PlayerLeftEvent;
                uuid.Remove(a.playername);
                return true;
            });
            Dictionary<string, bool> login = new Dictionary<string, bool>();
            api.addAfterActListener(EventKey.onLoadName, x =>
            {
                var a = BaseEvent.getFrom(x) as LoadNameEvent;
                login.Add(a.playername, false);
                return true;
            });
            api.addBeforeActListener(EventKey.onPlayerLeft, x =>
            {
                var a = BaseEvent.getFrom(x) as PlayerLeftEvent;
                login.Remove(a.playername);
                return true;
            });
            api.addBeforeActListener(EventKey.onInputCommand, x =>
             {
                 var a = BaseEvent.getFrom(x) as InputCommandEvent;
                 if(a.cmd.StartsWith("/login") & login[a.playername] == false)
                 {
                     string password = string.Empty;
                     password = a.cmd.Substring(7);
                     if(File.Exists("./plugins/litelogin/" + a.playername + ".txt"))
                     {
                         string[] config = File.ReadAllLines("./plugins/litelogin/" + a.playername + ".txt", System.Text.Encoding.Default);
                         string rightword = config[0].Substring(4);
                         if (password == rightword) ;
                         {
                             api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§3登录成功！\"}]}");
                             login[a.playername] = true;
                         }
                     }
                     else
                     {
                         api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4请先注册！\"}]}");
                         api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4请输入/register <密码> 来注册！\"}]}");
                     }
                     return false;
                 }
                 if(a.cmd.StartsWith("/register"))
                 {
                     if (File.Exists("./plugins/litelogin/" + a.playername + ".txt"))
                     {
                         api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4你注册过了！\"}]}");
                     }
                     else
                     {
                         string passwordin = string.Empty;
                         passwordin = a.cmd.Substring(10);
                         Directory.CreateDirectory("./plugins/litelogin");
                         File.AppendAllText("./plugins/litelogin/" + a.playername + ".txt","密码:"+passwordin, System.Text.Encoding.Default);
                         api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§3注册成功！\"}]}");
                         api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§3请输入/login <密码> 来登录！\"}]}");
                     }
                     return false;
                 }
                 return true;
             });
            api.addBeforeActListener(EventKey.onDestroyBlock, x =>
             {
                 var a = BaseEvent.getFrom(x) as DestroyBlockEvent;
                 if(login[a.playername] == false )
                 {
                     api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4请输入/login <密码> 来登录！\"}]}");
                     return false;
                 }
                 return true;
             });
            api.addBeforeActListener(EventKey.onPlacedBlock, x =>
            {
                var a = BaseEvent.getFrom(x) as PlacedBlockEvent;
                if (login[a.playername] == false)
                {
                    api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4请输入/login <密码> 来登录！\"}]}");
                    return false;
                }
                return true;
            });
            api.addBeforeActListener(EventKey.onAttack, x =>
            {
                var a = BaseEvent.getFrom(x) as AttackEvent;
                if (login[a.playername] == false)
                {
                    api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4请输入/login <密码> 来登录！\"}]}");
                    return false;
                }
                return true;
            });
            api.addBeforeActListener(EventKey.onStartOpenChest, x =>
            {
                var a = BaseEvent.getFrom(x) as StartOpenChestEvent;
                if (login[a.playername] == false)
                {
                    api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4请输入/login <密码> 来登录！\"}]}");
                    return false;
                }
                return true;
            });
        }
    }
}
namespace CSR
{
    partial class Plugin
    {

        public static void onStart(MCCSAPI api)
        {
            // TODO 此接口为必要实现
            login.scr.aaascr(api);
            Console.WriteLine("登录插件已加载！");
        }
    }
}