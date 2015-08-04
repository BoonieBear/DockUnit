﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoonieBear.DeckUnit.CommLib;
using BoonieBear.DeckUnit.Core;
using BoonieBear.DeckUnit.Events;
using BoonieBear.DeckUnit.JsonUtils;
using BoonieBear.DeckUnit.Models;
using Microsoft.Win32;
using TinyMetroWpfLibrary.Controller;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using System.IO;
using BoonieBear.DeckUnit.ViewModels;
namespace BoonieBear.DeckUnit.Views
{

    public partial class MainFrame
    {
        public MainFrame()
        {
            InitializeComponent();
            MainFrameViewModel.pMainFrame.DialogCoordinator = DialogCoordinator.Instance;
            //DataContext = MainFrameViewModel.pMainFrame;
            
            Kernel.Instance.Controller.SetRootFrame(ContentFrame);
            this.DebugLog.Text = MainFrameViewModel.pMainFrame.Shellstring;
            var strlStringses = new List<string[]>
            {
                new[] {"0", "块数", "3", ""},
                new[] {"0", "块数1", "33", ""},
                new[] {"1", "块数2", "33", ""},
                new[] {"1", "块数3", "53", ""},
                new[] {"1", "块数4", "43", ""},
                new[] {"2", "块数5", "3", ""},
                new[] {"3", "块数6", "32", ""},
                new[] {"4", "块数7", "23", ""},
                new[] {"3", "块数8", "3", ""},
                new[] {"4", "块数9", "13", ""},
                new[] {"2", "块数", "31", ""}
            };
            var tree = StringListToTree.TransListToNodeWriteLineic(strlStringses);
            this._tree.Model = new DataTreeModel(tree);
        }

        private void ContentFrame_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            ProgressDialogController remote = null;
            UnitCore.Instance.Start();
            UnitCore.Instance.EventAggregator.PublishMessage(new GoAcousticViewNavigationEvent());
            /*
            var remoteTask = this.ShowProgressAsync("请稍候...", "正在初始化系统");
            Task.Factory.StartNew(() => Thread.Sleep(2000)).ContinueWith(x => Dispatcher.Invoke(new Action(() =>
            {
                remote = remoteTask.Result;
                
            }))).ContinueWith(obj =>
            {
                //UnitCore.Instance.Start();
                remote.SetIndeterminate();
                //remote.SetCancelable(true);
                Dispatcher.Invoke(new Action(() =>
                {
                    if (UnitCore.Instance.ServiceOK)
                    {
                        remote.SetMessage("初始化成功!");
                    }
                    else
                    {
                        remote.SetMessage("初始化失败,详细错误信息请查看系统日志");
                    }
                }));
                Thread.Sleep(3000);
                Dispatcher.Invoke(new Action(() => remote.CloseAsync().ContinueWith(x =>
                {
                    if (!UnitCore.Instance.ServiceOK)
                    {
                        //导航到Home界面，下面的是示例
                        UnitCore.Instance.EventAggregator.PublishMessage(new GoHomePageNavigationEvent());
                        
                    }
                })));
            });*/

        }

        private void FlyOutView(int index)
        {
            var flyout = this.flyoutsControl.Items[index] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }
        
        private void LaunchDebugView(object sender, System.Windows.RoutedEventArgs e)
        {
            var flyout = this.flyoutsControl.Items[2] as Flyout;
            flyout.IsOpen = false;
            flyout = this.flyoutsControl.Items[1] as Flyout;
            flyout.IsOpen = false;
            FlyOutView(0);
        }

        private void LaunchDataView(object sender, System.Windows.RoutedEventArgs e)
        {
            var flyout = this.flyoutsControl.Items[2] as Flyout;
            flyout.IsOpen = false;
            flyout = this.flyoutsControl.Items[0] as Flyout;
            flyout.IsOpen = false;
            FlyOutView(1);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as MainFrameViewModel;
            if(vm!=null)
                if (vm.ModeType)
                {
                    this.DebugLog.Text = vm.Serialstring;
                }
                else
                {
                    this.DebugLog.Text = vm.Shellstring;
                }
        }

        private async void ToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
             
            if (LoadcheckButton.IsChecked==true)
            {
                
                var LoaderCheck = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "确定",
                    NegativeButtonText = "取消",
                    //FirstAuxiliaryButtonText = "Cancel",
                    ColorScheme = MetroDialogOptions.ColorScheme
                };
                MessageDialogResult result = await this.ShowMessageAsync("串口模式", "进入Loader模式？",
                MessageDialogStyle.AffirmativeAndNegative, LoaderCheck);
                if (result == MessageDialogResult.Affirmative)
                {
                    UnitCore.Instance.CommEngine.SerialService.ChangeMode(SerialServiceMode.LoaderMode);
                }
                else
                {
                    LoadcheckButton.IsChecked = false;
                }
                return;
            }
            if (LoadcheckButton.IsChecked==false)
            {
                
                var LoaderCheck = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "确定",
                    NegativeButtonText = "取消",
                    //FirstAuxiliaryButtonText = "Cancel",
                    ColorScheme = MetroDialogOptions.ColorScheme
                };
                MessageDialogResult result = await this.ShowMessageAsync("串口模式", "退出Loader模式？",
                MessageDialogStyle.AffirmativeAndNegative, LoaderCheck);
                if (result == MessageDialogResult.Affirmative)
                {
                    UnitCore.Instance.CommEngine.SerialService.ChangeMode(SerialServiceMode.HexMode);
                }
                else
                {
                    LoadcheckButton.IsChecked = true;
                }
                return;
            }
            
        }

        private async void SelectFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog OpenMspFile =new OpenFileDialog();
            if (OpenMspFile.ShowDialog() == true)
            {
                try
                {
                    if (UnitCore.Instance.CommEngine.IsWorking)
                    {
                        await UnitCore.Instance.CommEngine.SendFile(OpenMspFile.OpenFile());
                        UnitCore.Instance.EventAggregator.PublishMessage(new LogEvent("下载数据完成", LogType.OnlyInfo));
                    }
                    else
                    {
                        UnitCore.Instance.EventAggregator.PublishMessage(new LogEvent("串口进程未正常工作", LogType.OnlyInfo));
                    }
                    
                }
                catch (Exception MyEx)
                {

                    UnitCore.Instance.EventAggregator.PublishMessage(new ErrorEvent(MyEx, LogType.Both));
                }
            }
        }

        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
             var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "确定",
                NegativeButtonText = "取消",
                AnimateShow = true,
                AnimateHide = false
            };

            var result = await this.ShowMessageAsync("退出",
                "确信要退出程序吗？",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);

            

            if (result == MessageDialogResult.Affirmative)
            {
                UnitCore.Instance.Stop();
                System.Windows.Application.Current.Shutdown();
            }
                
        }

        private void FilterableListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var flyout = this.flyoutsControl.Items[2] as Flyout;
            flyout.IsOpen = true;

        }
    }
}
