
using CSR;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Data.SQLite;
using Org.BouncyCastle.Bcpg;

namespace login
{
    class scr
    {
        static void CreateDB()
        {
            string path = @"data\LoginData.sqlite";
            SQLiteConnection cn = new SQLiteConnection("data source=" + path);
            if (cn.State != System.Data.ConnectionState.Open)
            {
                cn.Open();
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = cn;
                cmd.CommandText = "CREATE TABLE name(id int primary key auto_increment,passwd text,playername text,xuid text)";
                cmd.ExecuteNonQuery();
            }
        }
        public static void litelogin(MCCSAPI api)
        {
            if (File.Exists("./plugins/litelogin/config.ini"))
            {
                Console.WriteLine("[litelogin]正在检测数据…");
            }
            else
            {
                Directory.CreateDirectory("plugins/litelogin");
                File.AppendAllText("./plugins/litelogin/config.ini", "use_MySQL=false\nSQL_server=\nUserID=\npassword=\ndatabase=\n", System.Text.Encoding.Default);
            }
            Dictionary<string, string> uuid = new Dictionary<string, string>();
            Dictionary<string, int> logintime = new Dictionary<string, int>();
            Dictionary<string, string> xuid = new Dictionary<string, string>();
            Dictionary<string, bool> login = new Dictionary<string, bool>();
            Dictionary<string, bool> login_zt = new Dictionary<string, bool>();
            api.addAfterActListener(EventKey.onLoadName, x =>
            {
                var a = BaseEvent.getFrom(x) as LoadNameEvent;
                uuid.Add(a.playername, a.uuid);
                xuid.Add(a.playername, a.xuid);
                logintime.Add(a.playername, 0);
                login.Add(a.playername, false);
                login_zt.Add(a.playername, true);
                Task task = Task.Run(async () =>
                {
                    await Task.Delay(60000);
                    try//防止玩家下线导致崩服
                    {
                        if (login_zt[a.playername])
                        {
                            api.runcmd("kick \"" + a.playername + "\" 登录超时！！！");
                        }
                    }
                    catch { }
                });
                return true;
            });
            api.addBeforeActListener(EventKey.onPlayerLeft, x =>
            {
                var a = BaseEvent.getFrom(x) as PlayerLeftEvent;
                uuid.Remove(a.playername);
                logintime.Remove(a.playername);
                xuid.Remove(a.playername);
                login.Remove(a.playername);
                login_zt.Remove(a.playername);
                return true;
            });
            api.addBeforeActListener(EventKey.onInputCommand, x =>
            {
                var a = BaseEvent.getFrom(x) as InputCommandEvent;
                if (a.cmd.StartsWith("/login ") & login[a.playername] == false)
                {
                    string password = string.Empty;
                    password = a.cmd.Substring(7);
                    if (File.Exists("./plugins/litelogin/" + xuid[a.playername] + ".txt"))
                    {
                        string[] config = File.ReadAllLines("./plugins/litelogin/" + xuid[a.playername] + ".txt", System.Text.Encoding.Default);
                        string rightword = config[0].Substring(9);
                        string rightxuid = config[1].Substring(5);
                        string rightplayername = config[2].Substring(11);
                        string md5input = "";
                        MD5 md5 = new MD5CryptoServiceProvider();//创建MD5对象（MD5类为抽象类不能被实例化）
                        byte[] date = System.Text.Encoding.Default.GetBytes(password);//将字符串编码转换为一个字节序列
                        byte[] date1 = md5.ComputeHash(date);//计算data字节数组的哈希值（加密）
                        md5.Clear();//释放类资源
                        for (int i = 0; i < date1.Length - 1; i++)//遍历加密后的数值到变量str2
                        {

                            md5input += date1[i].ToString("X");//（X为大写时加密后的数值里的字母为大写，x为小写时加密后的数值里的字母为小写）
                        }
                        if (md5input == rightword & xuid[a.playername] == rightxuid & a.playername == rightplayername)
                        {
                            login_zt[a.playername] = false;
                            api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§3登录成功！\"}]}");
                            login[a.playername] = true;
                        }
                        else
                        {
                            api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4登录失败！\"}]}");
                            logintime[a.playername] = logintime[a.playername] + 1;
                            if (logintime[a.playername] == 3)
                            {
                                api.runcmd("kick \"" + a.playername + "\" 达到最大登录次数！");
                            }
                        }
                    }
                    else
                    {
                        api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4请先注册！\"}]}");
                        api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4请输入/register <密码> 来注册！\"}]}");
                    }
                    return false;
                }
                if (a.cmd.StartsWith("/register "))
                {
                    if (File.Exists("./plugins/litelogin/" + xuid[a.playername] + ".txt"))
                    {
                        api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4你注册过了！\"}]}");
                    }
                    else
                    {
                        string passwordin = string.Empty;
                        passwordin = a.cmd.Substring(10);
                        Directory.CreateDirectory("./plugins/litelogin");
                        string md5password = "";
                        MD5 md5 = new MD5CryptoServiceProvider();//创建MD5对象（MD5类为抽象类不能被实例化）
                        byte[] date = System.Text.Encoding.Default.GetBytes(passwordin);//将字符串编码转换为一个字节序列
                        byte[] date1 = md5.ComputeHash(date);//计算data字节数组的哈希值（加密）
                        md5.Clear();//释放类资源
                        for (int i = 0; i < date1.Length - 1; i++)//遍历加密后的数值到变量str2
                        {
                            md5password += date1[i].ToString("X");//（X为大写时加密后的数值里的字母为大写，x为小写时加密后的数值里的字母为小写）
                        }
                        File.AppendAllText("./plugins/litelogin/" + xuid[a.playername] + ".txt", "password:" + md5password + "\n", System.Text.Encoding.Default);
                        File.AppendAllText("./plugins/litelogin/" + xuid[a.playername] + ".txt", "xuid:" + xuid[a.playername] + "\n", System.Text.Encoding.Default);
                        File.AppendAllText("./plugins/litelogin/" + xuid[a.playername] + ".txt", "playername:" + a.playername, System.Text.Encoding.Default);
                        Task tassss = Task.Run(async () =>
                        {
                            string[] config2 = File.ReadAllLines("./plugins/litelogin/config.ini", System.Text.Encoding.Default);
                            string ifusesql = config2[0].Substring(10);
                            string sqserver = config2[1].Substring(11);
                            string userid = config2[2].Substring(7);
                            string passwd = config2[3].Substring(9);
                            string database = config2[4].Substring(9);
                            Console.WriteLine("[litelogin]数据读取成功！！");
                            if (ifusesql == "true")
                            {
                                try
                                {
                                    MySqlBaseConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder();
                                    connectionStringBuilder.Server = sqserver;
                                    connectionStringBuilder.UserID = userid;
                                    connectionStringBuilder.Password = passwd;
                                    connectionStringBuilder.Database = database;
                                    MySqlConnection connect = new MySqlConnection(connectionStringBuilder.ConnectionString);
                                    connect.Open();
                                    Console.WriteLine("[litelogin]数据库连接成功！！！");
                                    try
                                    {
                                        MySqlCommand command = connect.CreateCommand();
                                        command.CommandText = "INSERT INTO name VALUES (0,\"" + md5password + "\",\"" + a.playername + "\",\"" + uuid[a.playername] + "\")";
                                        command.ExecuteNonQuery();
                                        Console.WriteLine("[litelogin]数据库写入成功！！！");
                                    }
                                    catch
                                    {
                                        Console.WriteLine("[litelogin]数据库写入失败！！！");
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("[litelogin]数据库连接失败！！！");
                                }
                            }
                            else
                            {
                                Console.WriteLine("[litelogin]未启用SQL存储，将保存数据到本地文件！！！");
                            }
                        });
                        login_zt[a.playername] = false;
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
                if (login[a.playername] == false)
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
            api.addBeforeActListener(EventKey.onUseItem, x =>
            {
                var a = BaseEvent.getFrom(x) as UseItemEvent;
                if (login[a.playername] == false)
                {
                    api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4请输入/login <密码> 来登录！\"}]}");
                    return false;
                }
                return true;
            });
            api.addBeforeActListener(EventKey.onInputCommand, x =>
            {
                var a = BaseEvent.getFrom(x) as InputCommandEvent;
                if (login[a.playername] == false)
                {
                    api.runcmd("tellraw \"" + a.playername + "\" {\"rawtext\":[{\"text\":\"§4请输入/login <密码> 来登录！\"}]}");
                    return false;
                }
                return true;
            });
            api.addBeforeActListener(EventKey.onStartOpenBarrel, x =>
            {
                var a = BaseEvent.getFrom(x) as StartOpenBarrelEvent;
                if (login[a.playername] == false)
                {
                    api.runcmd("kick \"" + a.playername + "\" 请登录后再进行操作！");
                    return false;
                }
                return true;
            });
            api.addBeforeActListener(EventKey.onSetSlot, x =>
            {
                var a = BaseEvent.getFrom(x) as SetSlotEvent;
                if (login[a.playername] == false)
                {
                    api.runcmd("kick \"" + a.playername + "\" 请登录后再进行操作！");
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
            login.scr.litelogin(api);
            Console.WriteLine("[litelogin]登录插件已加载！");
        }
    }
}
