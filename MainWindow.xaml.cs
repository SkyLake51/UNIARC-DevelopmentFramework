using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Windows;
using Microsoft.VisualBasic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MinecraftLaunch;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Utilities;
using static System.Net.Mime.MediaTypeNames;
using MinecraftLaunch.Components.Watcher;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Checker;
using System.Linq;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Components.Installer;
using TC.OpenWindow;

namespace TC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public  MainWindow()
        {
            InitializeComponent();
            if (Environment.Is64BitOperatingSystem)
            { }
            else
            {
                System.Windows.MessageBox.Show("您的操作系统不支持此软件");
                Close();
            }
            DirectoryInfo corefolder = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory + @".minecraft");
            if (!corefolder.Exists)
            {
                corefolder.Create();
            }
            else { }
            DirectoryInfo msfolder = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"Option");
            if (!msfolder.Exists)
            {
                msfolder.Create();
            }
            else { }

            DirectoryInfo Accountfolder = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"account");
            if (!Accountfolder.Exists)
            {
                Accountfolder.Create();
            }
            else { }
            DirectoryInfo MSfolder = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"account\MS");
            if (!MSfolder.Exists)
            {
                MSfolder.Create();
            }
            else { }
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\mem.opt"))
            {
                RuntimeMem.Text = File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\mem.opt");
            }
            else { }
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\JL.opt"))
            {
                JavaLocation.Text = File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\JL.opt");
            }
            else { }
            var resolver = new GameResolver(".minecraft");
            Ver1.ItemsSource = resolver.GetGameEntitys();
            string directoryPath = System.AppDomain.CurrentDomain.BaseDirectory + @"account\MS";
            string[] subdirectoryEntries = Directory.GetDirectories(directoryPath);
            SaveAct.ItemsSource = subdirectoryEntries;
        }
        Cache.Cache1 cache1 = new Cache.Cache1();
        public int launchMode = 1;
        private void Button_Launch(object sender, RoutedEventArgs e)
        {
            GameStart();
        }

        private void Button_OffLogin(object sender, RoutedEventArgs e)
        {
            launchMode = 1;
        }

        private async void Button_MSLoad(object sender, RoutedEventArgs e)
        {
            try
            {
                MicrosoftAuthenticator authenticator = new("YOUR CLIENT ID",false);
                await authenticator.DeviceFlowAuthAsync(async dc =>
                {
                    var Usercode = dc.UserCode;
                    DeviceInfo.Text = Usercode;
                    System.Diagnostics.Process.Start("explorer.exe", dc.VerificationUrl);
                });
                var userProfile = await authenticator.AuthenticateAsync();
                DirectoryInfo MS1folder = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory + @"account\MS\" + userProfile.Name);
                if (!MS1folder.Exists)
                {
                    MS1folder.Create();
                }
                else
                {
                    MessageBox.Show("此账号已存在");
                }
                System.IO.File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"account\MS\" + userProfile.Name + @"\name", userProfile.RefreshToken);
                string directoryPath = System.AppDomain.CurrentDomain.BaseDirectory + @"account\MS";
                string[] subdirectoryEntries = Directory.GetDirectories(directoryPath);
                SaveAct.ItemsSource = subdirectoryEntries;
            }
            catch
            {
                MessageBox.Show("无法连接至登陆服务器");
            }

        }



        private void Button_ThirdLogin(object sender, RoutedEventArgs e)
        {
            launchMode = 2;
        }

        private void Button_Locate(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Java|javaw.exe";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                JavaLocation.Text = openFileDialog.FileName;
            }

        }

        private void Button_Reg(object sender, RoutedEventArgs e)
        {
            if (JavaLocation.Text != string.Empty)
            {
                try
                {
                    var javaEmpty = JavaUtil.GetJavaInfo(JavaLocation.Text);
                    MessageBox.Show("Java版本:" + javaEmpty.JavaVersion);
                }
                catch
                {
                    MessageBox.Show("0x0005 无效的Java版本");
                }
            }
            else
            {
                MessageBox.Show("0x0004 无效的Java路径");
            }


        }

        private void Button_DownVer(object sender, RoutedEventArgs e)
        {
            if (DownLoadVer.Text != string.Empty)
            {
                Download(DownLoadVer.Text, 0);
            }
            else
            {
                MessageBox.Show("0x0007 请输入有效信息");
            }


        }
        public async void Download(string version, int type)
        {
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @".minecraft\versions\" + DownLoadVer.Text + @"\" + DownLoadVer.Text + @".json"))
            {
                MessageBox.Show("0x0010 已存在同名文件");
            }
            else
            {
                try
                {
                    cache1.Vercache.Text = string.Empty;
                    cache1.Vercache.Text = DownLoadVer.Text;
                    DownloadStatus.Text = @"正在下载" + cache1.Vercache.Text;
                    var resolver = new GameResolver(".minecraft");
                    var vanlliaInstaller = new VanlliaInstaller(resolver, cache1.Vercache.Text);
                    var result = await vanlliaInstaller.InstallAsync();
                    DownloadStatus.Text = cache1.Vercache.Text + @"下载完成";
                    cache1.Vercache.Text = string.Empty;
                    var resolver2 = new GameResolver(".minecraft");
                    Ver1.ItemsSource = resolver2.GetGameEntitys();
                    MessageBox.Show("已更新主页");
                }
                catch
                {
                    MessageBox.Show("无法连接网络");
                }

            }
        }

        private void Button_Refresh(object sender, RoutedEventArgs e)
        {
            var resolver = new GameResolver(".minecraft");
            Ver1.ItemsSource = resolver.GetGameEntitys();
            MessageBox.Show("已更新主页");
        }



        private void Button_SaveLog(object sender, RoutedEventArgs e)
        {
            try
            {
                cache1.AccountCache.Text = string.Empty;
                string cache2 = File.ReadAllText(SaveAct.Text + @"\name");
                cache1.AccountCache.Text = cache2;
                launchMode = 3;
            }
            catch
            {
                MessageBox.Show("Cannot Login");
            }
        }

        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            try
            {
                File.Delete(SaveAct.Text + @"\name");
                Directory.Delete(SaveAct.Text);
                string directoryPath = System.AppDomain.CurrentDomain.BaseDirectory + @"account\MS";
                string[] subdirectoryEntries = Directory.GetDirectories(directoryPath);
                SaveAct.ItemsSource = subdirectoryEntries;
            }
            catch
            {
                MessageBox.Show("权限异常");
            }
        }


        public async void GameStart()
        {

            switch (launchMode)
            {
                case 1:
                    if (OffName.Text != string.Empty)
                    {
                        try
                        {
                            if (RuntimeMem.Text != string.Empty && JavaLocation.Text != string.Empty && width.Text != string.Empty && height.Text != string.Empty)
                            {
                                var account = new OfflineAuthenticator(OffName.Text).Authenticate();
                                Account1.Text = OffName.Text;
                                var resolver = new GameResolver(".minecraft");
                                var GameWindowConfig = new GameWindowConfig
                                {
                                    Width = Convert.ToInt32(width.Text),
                                    Height = Convert.ToInt32(height.Text),
                                    IsFullscreen = false
                                };
                                var config = new LaunchConfig
                                {
                                    Account = account,
                                    IsEnableIndependencyCore = true,
                                    JvmConfig = new(JavaLocation.Text)
                                    {
                                        MinMemory = Convert.ToInt32(RuntimeMem.Text),
                                        MaxMemory = Convert.ToInt32(RuntimeMem.Text),
                                    }
                                };
                                Launcher launcher = new(resolver, config);
                                var gameProcessWatcher = await launcher.LaunchAsync(Ver1.Text);
                                VerRun.Text = Ver1.Text + @"运行中";
                                if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\mem.opt"))
                                { }
                                else
                                {
                                    File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\mem.opt", RuntimeMem.Text);
                                }
                                if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\JL.opt"))
                                { }
                                else
                                {
                                    File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\JL.opt", JavaLocation.Text);
                                }
                            }
                            else
                            {
                                MessageBox.Show("启动配置异常");
                            }
                        }


                        catch
                        {
                            MessageBox.Show("异常退出");
                        }


                    }
                    else
                    {
                        MessageBox.Show("登陆配置异常");
                    }
                    break;
                case 2:
                    if (ThirdMail.Text != string.Empty && ThirdPwd.Text != string.Empty && ThirdSvr.Text != string.Empty)
                    {
                        try
                        {
                            if (RuntimeMem.Text != string.Empty && JavaLocation.Text != string.Empty && width.Text != string.Empty && height.Text != string.Empty)
                            {
                                YggdrasilAuthenticator authenticator3 = new(ThirdSvr.Text, ThirdMail.Text, ThirdPwd.Text);
                                var userProfile3 = await authenticator3.AuthenticateAsync();

                                Account1.Text = ThirdMail.Text;
                                var resolver = new GameResolver(".minecraft");
                                var GameWindowConfig = new GameWindowConfig
                                {
                                    Width = Convert.ToInt32(width.Text),
                                    Height = Convert.ToInt32(height.Text),
                                    IsFullscreen = false
                                };
                                var config = new LaunchConfig
                                {
                                    Account = userProfile3.FirstOrDefault(),
                                    IsEnableIndependencyCore = true,
                                    JvmConfig = new(JavaLocation.Text)
                                    {
                                        MinMemory = Convert.ToInt32(RuntimeMem.Text),
                                        MaxMemory = Convert.ToInt32(RuntimeMem.Text),
                                    }
                                };

                                Launcher launcher = new(resolver, config);
                                var gameProcessWatcher = await launcher.LaunchAsync(Ver1.Text);
                                VerRun.Text = Ver1.Text + @"运行中";


                                if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\mem.opt"))
                                { }
                                else
                                {
                                    File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\mem.opt", RuntimeMem.Text);
                                }
                                if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\JL.opt"))
                                { }
                                else
                                {
                                    File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\JL.opt", JavaLocation.Text);
                                }
                            }
                            else
                            {
                                MessageBox.Show("启动配置异常");
                            }
                        }
                        catch
                        {
                            MessageBox.Show("异常退出");
                        }

                    }
                    else
                    {
                        MessageBox.Show("登陆配置异常");
                    }
                    break;
                case 3:

                    try
                    {
                        if (RuntimeMem.Text != string.Empty && JavaLocation.Text != string.Empty && width.Text != string.Empty && height.Text != string.Empty)
                        {
                            string s = cache1.AccountCache.Text;
                            MicrosoftAccount account = new MicrosoftAccount();
                            account.RefreshToken = s;
                            MicrosoftAuthenticator authenticator2 = new(account, "YOUR CLIENT ID", false);
                            MessageBox.Show(cache1.AccountCache.Text);
                            var userProfile2 = await authenticator2.AuthenticateAsync();
                            Account1.Text = userProfile2.Name;
                            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"account\MS\" + userProfile2.Name + @"\name")) 
                            {
                                File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + @"account\MS\" + userProfile2.Name + @"\name");
                                File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"account\MS\" + userProfile2.Name + @"\name",userProfile2.RefreshToken);
                            }
                            var resolver = new GameResolver(".minecraft");
                            var GameWindowConfig = new GameWindowConfig
                            {
                                Width = Convert.ToInt32(width.Text),
                                Height = Convert.ToInt32(height.Text),
                                IsFullscreen = false
                            };
                            var config = new LaunchConfig
                            {
                                Account = userProfile2,
                                IsEnableIndependencyCore = true,
                                JvmConfig = new(JavaLocation.Text)
                                {
                                    MinMemory = Convert.ToInt32(RuntimeMem.Text),
                                    MaxMemory = Convert.ToInt32(RuntimeMem.Text),
                                }
                            };

                            Launcher launcher = new(resolver, config);

                            var gameProcessWatcher = await launcher.LaunchAsync(Ver1.Text);
                            VerRun.Text = Ver1.Text + @"运行中";


                            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\mem.opt"))
                            { }
                            else
                            {
                                File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\mem.opt", RuntimeMem.Text);
                            }
                            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\JL.opt"))
                            { }
                            else
                            {
                                File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\JL.opt", JavaLocation.Text);
                            }
                        }
                        else
                        {
                            MessageBox.Show("启动配置异常");
                        }
                    }

                    catch
                    {
                        MessageBox.Show("异常退出");
                    }

                    break;

            }

        }

        private void Button_Clear2(object sender, RoutedEventArgs e)
        {
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\mem.opt"))
            {
                File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\mem.opt");
            }
            else { }
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\JL.opt"))
            {
                File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + @"Option\JL.opt");
            }
            else { }
        }

        private void Button_InfV(object sender, RoutedEventArgs e)
        {
            var window = new OpenWindow.Window1();
            window.Owner = this;
            window.ShowDialog();
        }

        private void Button_MLInf(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/Blessing-Studio/MinecraftLaunch");
        }

        private void Button_MeInf(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://mahapps.com/");
        }

        private void Button_GitPage(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/SkyLake51/UNIARC-Development-Framework");
        }

        private void Button_License(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "http://mitsloan.mit.edu/licensing");
        }
    }
    
}