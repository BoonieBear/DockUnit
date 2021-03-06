﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BoonieBear.DeckUnit.ACMP;
using BoonieBear.DeckUnit.Mov4500UI.Core;
using BoonieBear.DeckUnit.Mov4500UI.Events;
using BoonieBear.DeckUnit.Mov4500UI.Helpers;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Ribbon.Customization;
using DevExpress.XtraCharts.Native;
using DevExpress.XtraRichEdit.Utils.NumberConverters;
using MahApps.Metro.Controls.Dialogs;
using TinyMetroWpfLibrary.Events;
using TinyMetroWpfLibrary.Utility.Schedular;
using TinyMetroWpfLibrary.ViewModel;
using System.Windows.Media.Media3D;
using System.Net.NetworkInformation;
using System.Globalization;
using TinyMetroWpfLibrary.EventAggregation;
using System.Collections;
using System.IO;
using System.Windows.Media.Imaging;
using BoonieBear.DeckUnit.Mov4500UI.Models;
using System.Threading;
using System.Windows;
using FILETIME = System.Runtime.InteropServices.FILETIME;
using HelixToolkit.Wpf;

namespace BoonieBear.DeckUnit.Mov4500UI.ViewModel
{
    public class LiveCaptureViewModel : ViewModelBase, IHandleMessage<MovDataEvent>, IHandleMessage<USBLEvent>, IHandleMessage<SailEvent>, IHandleMessage<DspFeedbackComEvent>, IHandleMessage<DSPReqEvent>
    {
        private DispatcherTimer t, timer_DATALINKTIMER, timer_BREAKTIMER;
        private NetworkInterface currentInterface = null;
        private NetworkInterface[] networkInterfaces = null;
        double bytesFormerReceived;
        double bytesFormerSent;
        private bool InInitial = false;
		public int bpcount = 0;
        public int adcpcount = 0;
        public bool RecvData = false;
		
        private List<Point3D> USBLPath = new List<Point3D>();//track
        private List<Point3D> UWVPath = new List<Point3D>();//track
        private Point3D LastUsblPoint = new Point3D(0, 0, 0);
        private Point3D LastUWVPoint = new Point3D(0, 0, 0);
        public override void Initialize()
        {

            LinkStatus = "未连接通信机";
            LinkBoxColor = new SolidColorBrush(Colors.Crimson);
            SendMessageBoxreadonly=true;

            xVel = new ObservableCollection<AdcpInfo>();
            yVel = new ObservableCollection<AdcpInfo>();
            zVel = new ObservableCollection<AdcpInfo>();
            USBLUsed = true;
            //ShipHeading = 0.0F;
            //MovHeading = 45.0F;
            AlarmList = new ObservableCollection<string>();
            UsblPositionCollection = new ObservableCollection<Sysposition>();
            SubPositionCollection = new ObservableCollection<Subposition>();
            BpCollection = new ObservableCollection<Bpdata>();
            CTDCollection = new ObservableCollection<Ctddata>();
            LifesupplyCollection = new ObservableCollection<Lifesupply>();
            EnergysysCollection = new ObservableCollection<Energysys>();
            AlertdataCollection = new ObservableCollection<Alertdata>();
            AdcpdataCollection = new ObservableCollection<Adcpdata>();
            ImgCollection =new ObservableCollection<BitmapImage>();

            AddFHCMD = RegisterCommand(ExecuteAddFHCMD, CanExecuteAddFHCMD, true);
            AddImgCMD = RegisterCommand(ExecuteAddImgCMD, CanExecuteAddImgCMD, true);
            xVel.CollectionChanged+=xVel_CollectionChanged;
            yVel.CollectionChanged +=yVel_CollectionChanged;
            zVel.CollectionChanged+=zVel_CollectionChanged;
            AlarmList.CollectionChanged +=AlarmList_CollectionChanged;
            UsblPositionCollection.CollectionChanged +=UsblPositionCollection_CollectionChanged;
            SubPositionCollection.CollectionChanged += SubPositionCollection_CollectionChanged;
            BpCollection.CollectionChanged += BpCollection_CollectionChanged;
            CTDCollection.CollectionChanged += CTDCollection_CollectionChanged;
            LifesupplyCollection.CollectionChanged += LifesupplyCollection_CollectionChanged;
            EnergysysCollection.CollectionChanged += EnergysysCollection_CollectionChanged;
            AlertdataCollection.CollectionChanged += AlertdataCollection_CollectionChanged;
            AdcpdataCollection.CollectionChanged += AdcpdataCollection_CollectionChanged;
            ImgCollection.CollectionChanged += ImgCollection_CollectionChanged;
        }

        private void AlertdataCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnPropertyChanged(() => AlertdataCollection);
        }

        private void ImgCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnPropertyChanged(() => ImgCollection);
        }

        private void AdcpdataCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            base.OnPropertyChanged(() => AdcpdataCollection);
        }

        private void EnergysysCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            base.OnPropertyChanged(() => EnergysysCollection);
        }

        private void LifesupplyCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            base.OnPropertyChanged(() => LifesupplyCollection);
        }

        private void CTDCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            base.OnPropertyChanged(() => CTDCollection);
        }

        private void BpCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            base.OnPropertyChanged(() => BpCollection);
        }

        private void SubPositionCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            base.OnPropertyChanged(() => SubPositionCollection);
        }

        private void UsblPositionCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        { 
            base.OnPropertyChanged(() => UsblPositionCollection);
        }

        private void AlarmList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnPropertyChanged(()=>AlarmList);
        }

        private void zVel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnPropertyChanged(()=>zVel);
        }

        private void yVel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnPropertyChanged(()=>yVel);
        }

        private void xVel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnPropertyChanged(()=>xVel);
        }

        //将x,y,z参数转化为3D图上的相对坐标
        private void Transfromxyz(float x, float y,float z)
        {
            X = -x+80;
            Y = y-120;
            Z = -z;
        }
        private void UpdateUSBLTrack(float x, float y, float z)
        {
            USBLPath.Add(new Point3D(-x, y, -z));
            if (ShowUSBLTrack)
            {
                // create the WPF3D model
                var m = new Model3DGroup();
                var gm = new MeshBuilder();
                //gm.AddTube(USBLPath, 18, 10, false);
                for (int i = 0; i < USBLPath.Count; i++)
                {
                    gm.AddSphere(USBLPath[0],18);
                    m.Children.Add(new GeometryModel3D(gm.ToMesh(), Materials.Gold));
                }
                
                USBLTrackModel = m;
            }
            
        }
        private void UpdateUWVLTrack(float x, float y, float z)
        {
            UWVPath.Add(new Point3D(-x, y, -z));
            if (ShowUWVTrack)
            {
                // create the WPF3D model
                var m = new Model3DGroup();
                var gm = new MeshBuilder();
                for (int i = 0; i < UWVPath.Count; i++)
                {
                    gm.AddSphere(UWVPath[0], 18);
                    m.Children.Add(new GeometryModel3D(gm.ToMesh(), Materials.Red));
                }
                UWVTrackModel = m;
            }
            
        }
        public override void InitializePage(object extraData)
        {
            SetShipHeadingFront = false;
            InInitial = true;
            BtnShowShip = (UnitCore.Instance.WorkMode == MonitorMode.SHIP) ? true : false;
            if (t==null)
                t = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, Tick, Dispatcher.CurrentDispatcher);
            if (networkInterfaces==null)
                networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            if (networkInterfaces.Length == 0)
            {
                UnitCore.Instance.EventAggregator.PublishMessage(new LogEvent("没有找到可用网卡！",LogType.Both));
            }
            else
            {
                t.IsEnabled = true;
            }
            XMTValue = float.Parse(UnitCore.Instance.MovConfigueService.GetXmtAmp());
            XMTIndex = int.Parse(UnitCore.Instance.MovConfigueService.GetXmtChannel()) - 1;
            SelectGMode = (int)UnitCore.Instance.MovConfigueService.GetGMode();
            Gain = int.Parse(UnitCore.Instance.MovConfigueService.GetGain());
            InInitial = false;
            RefreshLifeInfos();
            if (UnitCore.Instance.MovConfigueService.GetMode() == MonitorMode.SUBMARINE)
            {
                if (timer_DATALINKTIMER == null)
                    timer_DATALINKTIMER = new DispatcherTimer(TimeSpan.FromSeconds(6), DispatcherPriority.Normal, timer_DATALINKTIMER_Tick, Dispatcher.CurrentDispatcher);
                if (timer_BREAKTIMER == null)
                    timer_BREAKTIMER = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, timer_BREAKTIMER_Tick, Dispatcher.CurrentDispatcher);            
            }
            else
            {
                if (timer_DATALINKTIMER != null)
                    timer_DATALINKTIMER.Stop();
                if (timer_BREAKTIMER != null)
                    timer_BREAKTIMER.Stop();
            }
            
            //ShipHeading = 90.0F;
            //Transfromxyz(1000, 600, 800);
        }

        private void RefreshLifeInfos()
        {
            if(LiveInfos==null)
                LiveInfos = new ObservableCollection<CollectionInfo>();
            LiveInfos.Clear();
            var o2info = new CollectionInfo();
            var co2info = new CollectionInfo();
            var other = new CollectionInfo();
            o2info.Category = "氧气";
            o2info.Number = Oxygen;
            co2info.Category = "二氧化碳";
            co2info.Number = Co2;
            other.Category = "其他";
            other.Number = 100 - Oxygen - Co2;
            LiveInfos.Add(o2info);
            LiveInfos.Add(co2info);
            LiveInfos.Add(other);
        }

        private void Tick(object sender, EventArgs e)
        {
            double receiveByte, SentBype, bytesReceived, bytesSent;
            Time = DateTime.Now.ToLongTimeString();
            if (UnitCore.Instance.NetCore.IsTCPWorking)
            {
                LinkStatus = "已连接通信机";
                LinkBoxColor = new SolidColorBrush(Colors.White);
            }
            else
            {
                LinkStatus = "未连接通信机";
                LinkBoxColor = new SolidColorBrush(Colors.Crimson);
            }
            receiveByte = 0;
            SentBype = 0;
            NumberFormatInfo numberFormat = NumberFormatInfo.CurrentInfo;
            foreach (var currentInterface in networkInterfaces)
            {
                IPv4InterfaceStatistics ipStats = currentInterface.GetIPv4Statistics();

                receiveByte += ipStats.BytesReceived / 1024;
                SentBype += ipStats.BytesSent / 1024;
                
            }
            bytesReceived = receiveByte - bytesFormerReceived;
            bytesSent = SentBype - bytesFormerSent;

            if (bytesFormerReceived == 0 && bytesFormerSent == 0) //第一次
            {
                bytesFormerReceived = receiveByte;
                bytesFormerSent = SentBype;
                return;
            }
            bytesFormerReceived = receiveByte;
            bytesFormerSent = SentBype;
            if (bytesReceived < 1024)
            {
                LinkRxSpeed = bytesReceived.ToString("N1", numberFormat) + " KB/s";
            }
            else
            {
                LinkRxSpeed = (bytesReceived/1024).ToString("N1", numberFormat) + " MB/s";
            }
            if (bytesSent < 1024)
            {
                LinkTxSpeed = bytesSent.ToString("N1", numberFormat) + " KB/s";
            }
            else
            {
                LinkTxSpeed = (bytesSent/1024).ToString("N1", numberFormat) + " MB/s";
            }
            
        }

		void timer_DATALINKTIMER_Tick(object sender, EventArgs e)
        {
            if (UnitCore.Instance.CommCore.IsWorking)
            {
                if (RecvData)//有接收数据
                {
                    RecvData = false;
                }
                else
                {
                    UnitCore.Instance.CommCore.Sendbreak();
                    timer_DATALINKTIMER.Stop();
                    timer_BREAKTIMER.Start();
                }
            }
            else
            {
                timer_DATALINKTIMER.Stop();
            }


        }
        void timer_BREAKTIMER_Tick(object sender, EventArgs e)
        {
            if (UnitCore.Instance.CommCore.IsWorking)
            {
                UnitCore.Instance.CommCore.Sendcs();
                timer_BREAKTIMER.Stop();
                timer_DATALINKTIMER.Start();
            }
            else
            {
                timer_BREAKTIMER.Stop();
            }


        }
        //show btn or control based on the workmode 
        //if workmode == ship tnshow = true, or btnshow=false
        public bool BtnShowShip
        {
            get { return GetPropertyValue(() => BtnShowShip); }
            set { SetPropertyValue(() => BtnShowShip, value); }
        }

        #region data binding
        public bool USBLUsed
        {
            get { return GetPropertyValue(() => USBLUsed); }
            set
            {
                if (value == true)
                {
                    Transfromxyz((float)LastUsblPoint.X, (float)LastUsblPoint.Y, (float)LastUsblPoint.Z);
                }
                SetPropertyValue(() => USBLUsed, value); 
                
            }
        }
        public bool UWVUsed
        {
            get { return GetPropertyValue(() => UWVUsed); }
            set
            {
                if (value == true)
                {
                   
                    Transfromxyz((float)LastUWVPoint.X, (float)LastUWVPoint.Y, (float)LastUWVPoint.Z);
                }
                SetPropertyValue(() => UWVUsed, value); 
                
            }
        }
        public bool ShowUSBLTrack
        {
            get { return GetPropertyValue(() => ShowUSBLTrack); }
            set
            {

                if (value == false)
                    USBLTrackModel = null;
                else
                {
                    // create the WPF3D model
                    var m = new Model3DGroup();
                    var gm = new MeshBuilder();
                    //gm.AddTube(USBLPath, 18, 10, false);
                    for (int i = 0; i < USBLPath.Count; i++)
                    {
                        gm.AddSphere(USBLPath[i], 18);
                        m.Children.Add(new GeometryModel3D(gm.ToMesh(), Materials.Gold));
                    }
                    //m.Children.Add(new GeometryModel3D(gm.ToMesh(), Materials.Gold));

                    USBLTrackModel = m;
                }
                SetPropertyValue(() => ShowUSBLTrack, value);

            }
        }
        public bool ShowUWVTrack
        {
            get { return GetPropertyValue(() => ShowUWVTrack); }
            set
            {

                if (value == false)
                    UWVTrackModel = null;
                else
                {
                    // create the WPF3D model
                    var m = new Model3DGroup();
                    var gm = new MeshBuilder();
                    for (int i = 0; i < UWVPath.Count; i++)
                    {
                        gm.AddSphere(UWVPath[i], 18);
                        m.Children.Add(new GeometryModel3D(gm.ToMesh(), Materials.Red));
                    }
                    UWVTrackModel = m;
                }
                SetPropertyValue(() => ShowUWVTrack, value);

            }
        }
		public bool SendMessageBoxreadonly
        {
            get { return GetPropertyValue(() => SendMessageBoxreadonly); }
            set { SetPropertyValue(() => SendMessageBoxreadonly, value); }
        }
		
        public bool SetShipHeadingFront
        {
            get { return GetPropertyValue(() => SetShipHeadingFront); }
            set { SetPropertyValue(() => SetShipHeadingFront, value); }
        }
        public int XMTIndex
        {
            get { return GetPropertyValue(() => XMTIndex); }
            set
            {
                if (value == XMTIndex)
                {
                    return;
                }
                SetPropertyValue(() => XMTIndex, value);
                if (!InInitial)
                    ChangeXMTIndex(XMTIndex);
            }
        }
        public float XMTValue
        {
            get { return GetPropertyValue(() => XMTValue); }
            set
            {
                if (value == XMTValue)
                {
                    return;
                }
                SetPropertyValue(() => XMTValue, value);
                /*if (!InInitial)
                    ChangeXMTValue(XMTValue);*/
            }
        }

        public int SelectGMode
        {
            get { return GetPropertyValue(() => SelectGMode); }
            set
            {
                if (value == SelectGMode)
                {
                    return;
                }
                SetPropertyValue(() => SelectGMode, value);
                if (!InInitial)
                    ChangeSelectGMode(SelectGMode);
            }
        }

        public int Gain
        {
            get { return GetPropertyValue(() => Gain); }
            set
            {
                if (value == Gain)
                {
                    return;
                }
                SetPropertyValue(() => Gain, value);
                if (!InInitial)
                    ChangeGain(Gain);
            }
        }

       /* private void ChangeXMTValue(float XMTValue)
        {
            var cmd = "a " + XMTValue.ToString("F03");
            UnitCore.Instance.NetCore.SendConsoleCMD(cmd);
            LogHelper.WriteLog("发射幅度设置为" + XMTValue.ToString("F03"));
            var ans = UnitCore.Instance.MovConfigueService.SetXmtAmp(XMTValue);//保存幅度
            if (ans == false)
            {
                EventAggregator.PublishMessage(new LogEvent("保存参数出错", LogType.Both));
                return;
            }
        }*/

        private async void ChangeXMTIndex(int index)
        {
            if (UnitCore.Instance.NetCore.IsTCPWorking)
            {
                var cmd = "channel " + (index + 1).ToString() + " -w";
                await UnitCore.Instance.NetCore.SendConsoleCMD(cmd);
                LogHelper.WriteLog("发射换能器设置为" + (index + 1).ToString());
                cmd = "opent " + (index + 1).ToString() + " -w";
                await UnitCore.Instance.NetCore.SendConsoleCMD(cmd);
                LogHelper.WriteLog("接收换能器设置为" + (index + 1).ToString());
            }
            var ans = UnitCore.Instance.MovConfigueService.SetXmtChannel(index + 1);//保存通道
            if (ans == false)
            {
                EventAggregator.PublishMessage(new LogEvent("保存参数出错", LogType.Both));
                return;
            }
        }

        private async void ChangeSelectGMode(int SelectGMode)
        {
            if (UnitCore.Instance.NetCore.IsTCPWorking)
            {
                if (SelectGMode == (int)(MonitorGMode.HAND))
                {
                    var cmd = "gm 1";
                    await UnitCore.Instance.NetCore.SendConsoleCMD(cmd);
                    cmd = "g " + Gain;
                    await UnitCore.Instance.NetCore.SendConsoleCMD(cmd);
                    LogHelper.WriteLog("接收增益设置为" + Gain);
                }
                else
                {
                    var cmd = "gm 2";
                    await UnitCore.Instance.NetCore.SendConsoleCMD(cmd);
                    LogHelper.WriteLog("接收增益设置为自动增益模式");
                }
                
            }
            var ans = UnitCore.Instance.MovConfigueService.SetGMode((MonitorGMode)Enum.Parse(typeof(MonitorGMode), SelectGMode.ToString()));
            if (ans == false)
            {
                EventAggregator.PublishMessage(new LogEvent("保存运行模式出错！", LogType.Both));
                return;
            }
        }

        private async void ChangeGain(int Gain)
        {
            if (UnitCore.Instance.NetCore.IsTCPWorking)
            {
                if (SelectGMode == (int)(MonitorGMode.HAND))
                {
                    var cmd = "g " + Gain;
                    await UnitCore.Instance.NetCore.SendConsoleCMD(cmd);
                    LogHelper.WriteLog("接收增益设置为" + Gain);
                }
            }
            var ans = UnitCore.Instance.MovConfigueService.SetGain(Gain);//保存通道
            if (ans == false)
            {
                EventAggregator.PublishMessage(new LogEvent("保存参数出错", LogType.Both));
                return;
            }
        }
        
        #region status
        public string Time
        {
            get { return GetPropertyValue(() => Time); }
            set { SetPropertyValue(() => Time, value); }
        }
        public string LinkRxSpeed
        {
            get { return GetPropertyValue(() => LinkRxSpeed); }
            set { SetPropertyValue(() => LinkRxSpeed, value); }
        }
        public string LinkTxSpeed
        {
            get { return GetPropertyValue(() => LinkTxSpeed); }
            set { SetPropertyValue(() => LinkTxSpeed, value); }
        }
        public string LinkStatus
        {
            get { return GetPropertyValue(() => LinkStatus); }
            set
            {
                SetPropertyValue(() => LinkStatus, value);
            }
        }

        public SolidColorBrush LinkBoxColor
        {
            get { return GetPropertyValue(() => LinkBoxColor); }
            set { SetPropertyValue(() => LinkBoxColor, value); }
        }
        #endregion

        #region position
        public string PosFromUwvTime
        {
            get { return GetPropertyValue(() => PosFromUwvTime); }
            set { SetPropertyValue(() => PosFromUwvTime, value); }
        }
        public string PosFromUSBLTime
        {
            get { return GetPropertyValue(() => PosFromUSBLTime); }
            set { SetPropertyValue(() => PosFromUSBLTime, value); }
        }
        //from usbl/gps/PlaneS1
        public string ShipLongUsbl
        {
            get { return GetPropertyValue(() => ShipLongUsbl); }
            set { SetPropertyValue(() => ShipLongUsbl, value); }
        }
        public string ShipLatUsbl
        {
            get { return GetPropertyValue(() => ShipLatUsbl); }
            set { SetPropertyValue(() => ShipLatUsbl, value); }
        }
        public string MovLongUsbl
        {
            get { return GetPropertyValue(() => MovLongUsbl); }
            set { SetPropertyValue(() => MovLongUsbl, value); }
        }
        public string MovLatUsbl
        {
            get { return GetPropertyValue(() => MovLatUsbl); }
            set { SetPropertyValue(() => MovLatUsbl, value); }
        }
        public float MovDepthUsbl
        {
            get { return GetPropertyValue(() => MovDepthUsbl); }
            set { SetPropertyValue(() => MovDepthUsbl, value); }
        }
        public float ShipHeading
        {
            get { return GetPropertyValue(() => ShipHeading); }
            set { SetPropertyValue(() => ShipHeading, value); }
        }

        public float ShipPitch
        {
            get { return GetPropertyValue(() => ShipPitch); }
            set { SetPropertyValue(() => ShipPitch, value); }
        }

        public float ShipRoll
        {
            get { return GetPropertyValue(() => ShipRoll); }
            set { SetPropertyValue(() => ShipRoll, value); }
        }

        public float ShipSpeed
        {
            get { return GetPropertyValue(() => ShipSpeed); }
            set { SetPropertyValue(() => ShipSpeed, value); }
        }
        //from mov
        public string MovLong
        {
            get { return GetPropertyValue(() => MovLong); }
            set { SetPropertyValue(() => MovLong, value); }
        }
        public string MovLat
        {
            get { return GetPropertyValue(() => MovLat); }
            set { SetPropertyValue(() => MovLat, value); }
        }
        public float MovDepth
        {
            get { return GetPropertyValue(() => MovDepth); }
            set { SetPropertyValue(() => MovDepth, value); }
        }
        public float MovHeading
        {
            get { return GetPropertyValue(() => MovHeading); }
            set { SetPropertyValue(() => MovHeading, value); }
        }

        public float MovPitch
        {
            get { return GetPropertyValue(() => MovPitch); }
            set { SetPropertyValue(() => MovPitch, value); }
        }

        public float MovRoll
        {
            get { return GetPropertyValue(() => MovRoll); }
            set { SetPropertyValue(() => MovRoll, value); }
        }

        public float MovHeight
        {
            get { return GetPropertyValue(() => MovHeight); }
            set { SetPropertyValue(() => MovHeight, value); }
        }
        //根据水下回传的位置信息计算出来的相对距离
        public float XDistanceFromUWV
        {
            get { return GetPropertyValue(() => XDistanceFromUWV); }
            set { SetPropertyValue(() => XDistanceFromUWV, value); }
        }
        public float YDistanceFromUWV
        {
            get { return GetPropertyValue(() => YDistanceFromUWV); }
            set { SetPropertyValue(() => YDistanceFromUWV, value); }
        }
        public float ZDistanceFromUWV
        {
            get { return GetPropertyValue(() => ZDistanceFromUWV); }
            set { SetPropertyValue(() => ZDistanceFromUWV, value); }
        }
        
        public float XDistanceFromUSBL
        {
            get { return GetPropertyValue(() => XDistanceFromUSBL); }
            set { SetPropertyValue(() => XDistanceFromUSBL, value); }
        }
        public float YDistanceFromUSBL
        {
            get { return GetPropertyValue(() => YDistanceFromUSBL); }
            set { SetPropertyValue(() => YDistanceFromUSBL, value); }
        }
        public float ZDistanceFromUSBL
        {
            get { return GetPropertyValue(() => ZDistanceFromUSBL); }
            set { SetPropertyValue(() => ZDistanceFromUSBL, value); }
        }
        //pos binding by map, can be calced by usbl or mov stats
        public float X
        {
            get { return GetPropertyValue(() => X); }
            set { SetPropertyValue(() => X, value); }
        }
        public float Y
        {
            get { return GetPropertyValue(() => Y); }
            set { SetPropertyValue(() => Y, value); }
        }
        public float Z
        {
            get { return GetPropertyValue(() => Z); }
            set { SetPropertyValue(() => Z, value); }
        }
        public Model3D USBLTrackModel
        {
            get { return GetPropertyValue(() => USBLTrackModel); }
            set { SetPropertyValue(() => USBLTrackModel, value); }
        }
        public Model3D UWVTrackModel
        {
            get { return GetPropertyValue(() => UWVTrackModel); }
            set { SetPropertyValue(() => UWVTrackModel, value); }
        }
        #endregion
        #region alarm

        public string AlarmTime
        {
            get { return GetPropertyValue(() => AlarmTime); }
            set { SetPropertyValue(() => AlarmTime, value); }
        }

        public float Leak
        {
            get { return GetPropertyValue(() => Leak); }
            set { SetPropertyValue(() => Leak, value); }
        }
        public float Cable
        {
            get { return GetPropertyValue(() => Cable); }
            set { SetPropertyValue(() => Cable, value); }
        }
        public float AlertTemp
        {
            get { return GetPropertyValue(() => AlertTemp); }
            set { SetPropertyValue(() => AlertTemp, value); }
        }
        public ObservableCollection<string> AlarmList
        {
            get { return GetPropertyValue(() => AlarmList); }
            set { SetPropertyValue(() => AlarmList, value); }
        }
        #endregion
        #region energy
        public string EnergyTime
        {
            get { return GetPropertyValue(() => EnergyTime); }
            set { SetPropertyValue(() => EnergyTime, value); }
        }

        public float HeadmainV
        {
            get { return GetPropertyValue(() => HeadmainV); }
            set { SetPropertyValue(() => HeadmainV, value); }
        }
        public float HeadmainI
        {
            get { return GetPropertyValue(() => HeadmainI); }
            set { SetPropertyValue(() => HeadmainI, value); }
        }
        public float Headmainconsume
        {
            get { return GetPropertyValue(() => Headmainconsume); }
            set { SetPropertyValue(() => Headmainconsume, value); }
        }
        public float HeadmainMaxTemp
        {
            get { return GetPropertyValue(() => HeadmainMaxTemp); }
            set { SetPropertyValue(() => HeadmainMaxTemp, value); }
        }
        public float HeadmainMaxExpand
        {
            get { return GetPropertyValue(() => HeadmainMaxExpand); }
            set { SetPropertyValue(() => HeadmainMaxExpand, value); }
        }
        public float TailmainV
        {
            get { return GetPropertyValue(() => TailmainV); }
            set { SetPropertyValue(() => TailmainV, value); }
        }
        public float TailmainI
        {
            get { return GetPropertyValue(() => TailmainI); }
            set { SetPropertyValue(() => TailmainI, value); }
        }
        public float Tailmainconsume
        {
            get { return GetPropertyValue(() => Tailmainconsume); }
            set { SetPropertyValue(() => Tailmainconsume, value); }
        }
        public float TailmainMaxTemp
        {
            get { return GetPropertyValue(() => TailmainMaxTemp); }
            set { SetPropertyValue(() => TailmainMaxTemp, value); }
        }
        public float TailmainMaxExpand
        {
            get { return GetPropertyValue(() => TailmainMaxExpand); }
            set { SetPropertyValue(() => TailmainMaxExpand, value); }
        }
        public float LeftsubV
        {
            get { return GetPropertyValue(() => LeftsubV); }
            set { SetPropertyValue(() => LeftsubV, value); }
        }
        public float LeftsubI
        {
            get { return GetPropertyValue(() => LeftsubI); }
            set { SetPropertyValue(() => LeftsubI, value); }
        }
        public float Leftsubconsume
        {
            get { return GetPropertyValue(() => Leftsubconsume); }
            set { SetPropertyValue(() => Leftsubconsume, value); }
        }
        public float LeftsubMaxTemp
        {
            get { return GetPropertyValue(() => LeftsubMaxTemp); }
            set { SetPropertyValue(() => LeftsubMaxTemp, value); }
        }
        public float LeftsubMaxExpand
        {
            get { return GetPropertyValue(() => LeftsubMaxExpand); }
            set { SetPropertyValue(() => LeftsubMaxExpand, value); }
        }
        public float RightsubV
        {
            get { return GetPropertyValue(() => RightsubV); }
            set { SetPropertyValue(() => RightsubV, value); }
        }
        public float RightsubI
        {
            get { return GetPropertyValue(() => RightsubI); }
            set { SetPropertyValue(() => RightsubI, value); }
        }
        public float Rightsubconsume
        {
            get { return GetPropertyValue(() => Rightsubconsume); }
            set { SetPropertyValue(() => Rightsubconsume, value); }
        }
        public float RightsubMaxTemp
        {
            get { return GetPropertyValue(() => RightsubMaxTemp); }
            set { SetPropertyValue(() => RightsubMaxTemp, value); }
        }
        public float RightsubMaxExpand
        {
            get { return GetPropertyValue(() => RightsubMaxExpand); }
            set { SetPropertyValue(() => RightsubMaxExpand, value); }
        }
        #endregion
        #region adcp
        public string ADCPTime
        {
            get { return GetPropertyValue(() => ADCPTime); }
            set { SetPropertyValue(() => ADCPTime, value); }
        }
        
        public ObservableCollection<AdcpInfo> xVel
        {
            get { return GetPropertyValue(() => xVel); }
            set { SetPropertyValue(() => xVel, value); }
        }
        public ObservableCollection<AdcpInfo> yVel
        {
            get { return GetPropertyValue(() => yVel); }
            set { SetPropertyValue(() => yVel, value); }
        }
        public ObservableCollection<AdcpInfo> zVel
        {
            get { return GetPropertyValue(() => zVel); }
            set { SetPropertyValue(() => zVel, value); }
        }

        public float BottomTrack
        {
            get { return GetPropertyValue(() => BottomTrack); }
            set { SetPropertyValue(() => BottomTrack, value); }
        }
        public float ADCPHeight
        {
            get { return GetPropertyValue(() => ADCPHeight); }
            set { SetPropertyValue(() => ADCPHeight, value); }
        }
        #endregion
        #region life supply
        public string LifeTime
        {
            get { return GetPropertyValue(() => LifeTime); }
            set { SetPropertyValue(() => LifeTime, value); }
        }

        public float Oxygen
        {
            get { return GetPropertyValue(() => Oxygen); }
            set { SetPropertyValue(() => Oxygen, value); }
        }
        public float Co2
        {
            get { return GetPropertyValue(() => Co2); }
            set { SetPropertyValue(() => Co2, value); }
        }

        public ObservableCollection<CollectionInfo> LiveInfos
        {
            get { return GetPropertyValue(() => LiveInfos); }
            set { SetPropertyValue(() => LiveInfos, value); }
        }
        public float Pressure
        {
            get { return GetPropertyValue(() => Pressure); }
            set { SetPropertyValue(() => Pressure, value); }
        }
        public float Temperature
        {
            get { return GetPropertyValue(() => Temperature); }
            set { SetPropertyValue(() => Temperature, value); }
        }
        public float Humidity
        {
            get { return GetPropertyValue(() => Humidity); }
            set { SetPropertyValue(() => Humidity, value); }
        }
        #endregion
        #region ctd
        public string CTDTime
        {
            get { return GetPropertyValue(() => CTDTime); }
            set { SetPropertyValue(() => CTDTime, value); }
        }
        public float CTDWaterTemp
        {
            get { return GetPropertyValue(() => CTDWaterTemp); }
            set { SetPropertyValue(() => CTDWaterTemp, value); }
        }
        public float CTDDepth
        {
            get { return GetPropertyValue(() => CTDDepth); }
            set { SetPropertyValue(() => CTDDepth, value); }
        }

        public float CTDWatercond
        {
            get { return GetPropertyValue(() => CTDWatercond); }
            set { SetPropertyValue(() => CTDWatercond, value); }
        }
        public float CTDSoundvec
        {
            get { return GetPropertyValue(() => CTDSoundvec); }
            set { SetPropertyValue(() => CTDSoundvec, value); }
        }
        #endregion
        #region bp
        public string BpTime
        {
            get { return GetPropertyValue(() => BpTime); }
            set { SetPropertyValue(() => BpTime, value); }
        }
        public float BpFrontUp
        {
            get { return GetPropertyValue(() => BpFrontUp); }
            set { SetPropertyValue(() => BpFrontUp, value); }
        }
        public float BpLeft
        {
            get { return GetPropertyValue(() => BpLeft); }
            set { SetPropertyValue(() => BpLeft, value); }
        }
        public float BpRight
        {
            get { return GetPropertyValue(() => BpRight); }
            set { SetPropertyValue(() => BpRight, value); }
        }
        public float BpFront
        {
            get { return GetPropertyValue(() => BpFront); }
            set { SetPropertyValue(() => BpFront, value); }
        }
        public float BpFrontDown
        {
            get { return GetPropertyValue(() => BpFrontDown); }
            set { SetPropertyValue(() => BpFrontDown, value); }
        }
        public float BpBottom
        {
            get { return GetPropertyValue(() => BpBottom); }
            set { SetPropertyValue(() => BpBottom, value); }
        }
        public float BpBottomBack
        {
            get { return GetPropertyValue(() => BpBottomBack); }
            set { SetPropertyValue(() => BpBottomBack, value); }
        }

        #endregion
        #region bsss
        public string BsssTime
        {
            get { return GetPropertyValue(() => BsssTime); }
            set { SetPropertyValue(() => BsssTime, value); }
        }
        public int BsssHeight
        {
            get { return GetPropertyValue(() => BsssHeight); }
            set { SetPropertyValue(() => BsssHeight, value); }
        }
        #endregion
        

        #endregion
        //所有历史数据集合，可以从里面找出有意义的数据进行显示 假设1分钟一个数据 10小时数据大小~=2K*60*10 = 1.2M字节 
        //UDP数据包太多，所以集合只存储最近的10个数据包
        #region all stats collections
        public ObservableCollection<Sysposition> UsblPositionCollection
        {
            get { return GetPropertyValue(() => UsblPositionCollection); }
            set { SetPropertyValue(() => UsblPositionCollection, value); }
        }
        public ObservableCollection<Subposition> SubPositionCollection
        {
            get { return GetPropertyValue(() => SubPositionCollection); }
            set { SetPropertyValue(() => SubPositionCollection, value); }
        }
        public ObservableCollection<Bpdata> BpCollection
        {
            get { return GetPropertyValue(() => BpCollection); }
            set { SetPropertyValue(() => BpCollection, value); }
        }
        public ObservableCollection<Ctddata> CTDCollection
        {
            get { return GetPropertyValue(() => CTDCollection); }
            set { SetPropertyValue(() => CTDCollection, value); }
        }
        public ObservableCollection<Lifesupply> LifesupplyCollection
        {
            get { return GetPropertyValue(() => LifesupplyCollection); }
            set { SetPropertyValue(() => LifesupplyCollection, value); }
        }
        public ObservableCollection<Energysys> EnergysysCollection
        {
            get { return GetPropertyValue(() => EnergysysCollection); }
            set { SetPropertyValue(() => EnergysysCollection, value); }
        }
        public ObservableCollection<Alertdata> AlertdataCollection
        {
            get { return GetPropertyValue(() => AlertdataCollection); }
            set { SetPropertyValue(() => AlertdataCollection, value); }
        }
        public ObservableCollection<Adcpdata> AdcpdataCollection
        {
            get { return GetPropertyValue(() => AdcpdataCollection); }
            set { SetPropertyValue(() => AdcpdataCollection, value); }
        }
        public ObservableCollection<BitmapImage> ImgCollection
        {
            get { return GetPropertyValue(() => ImgCollection); }
            set { SetPropertyValue(() => ImgCollection, value); }
        }
        #endregion

        #region Command

        public ICommand AddFHCMD
        {
            get { return GetPropertyValue(() => AddFHCMD); }
            set { SetPropertyValue(() => AddFHCMD, value); }
        }
        public void CanExecuteAddFHCMD(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }


        public async void ExecuteAddFHCMD(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if (!UnitCore.Instance.NetCore.IsTCPWorking)
            {
                var md = new MetroDialogSettings();
                md.AffirmativeButtonText = "确定";

                await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMessageAsync(MainFrameViewModel.pMainFrame,"无法发送信息",
                    "尚未连接通信机网络或连接出错", MessageDialogStyle.Affirmative, md);
                return;
                
            }
            var dialog = (BaseMetroDialog)App.Current.MainWindow.Resources["SendFhDialog"];
            await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMetroDialogAsync(MainFrameViewModel.pMainFrame,
                dialog);
        }
        public ICommand AddImgCMD
        {
            get { return GetPropertyValue(() => AddImgCMD); }
            set { SetPropertyValue(() => AddImgCMD, value); }
        }
        public void CanExecuteAddImgCMD(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }


        public async void ExecuteAddImgCMD(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if(UnitCore.Instance.WorkMode==MonitorMode.SHIP)
                return;
            if (!UnitCore.Instance.NetCore.IsTCPWorking)
            {
                var md = new MetroDialogSettings();
                md.AffirmativeButtonText = "确定";

                await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMessageAsync(MainFrameViewModel.pMainFrame, "无法发送图像",
                    "尚未连接通信机网络或连接出错", MessageDialogStyle.Affirmative, md);
                return;

            }
            
            var dialog = (BaseMetroDialog)App.Current.MainWindow.Resources["SendImgDialog"];
            await MainFrameViewModel.pMainFrame.DialogCoordinator.ShowMetroDialogAsync(MainFrameViewModel.pMainFrame,
                dialog);
        }
        #endregion

        #region Data Handle
        public void Handle(MovDataEvent message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Status.LastUpdateTime = DateTime.Now.ToString();
                Status.ReceiveMsgCount++;
                string chartstring = null;
                System.Windows.Controls.Image ImgContainer = null;
                if (message.Data.ContainsKey(MovDataType.ADCP))
                {
                    var adcp = message.Data[MovDataType.ADCP] as Adcpdata;
                    if (adcp != null)
                    {
                        ADCPTime = adcp.Time;
                        xVel.Clear();
                        yVel.Clear();
                        zVel.Clear();
                        for (int i = 0; i < 10; i++)
                        {
                            xVel.Add(new AdcpInfo(i + 1, adcp.FloorX[i]));
                            yVel.Add(new AdcpInfo(i + 1, adcp.FloorY[i]));
                            zVel.Add(new AdcpInfo(i + 1, adcp.FloorZ[i]));
                        }
                        BottomTrack = (float) adcp.BottomTrack;
                        ADCPHeight = (float) adcp.Height;
                        AdcpdataCollection.Add(adcp);
                        if (AdcpdataCollection.Count > 10)
                            AdcpdataCollection.RemoveAt(0);
                    }
                }
                if (message.Data.ContainsKey(MovDataType.ALERT))
                {
                    var alt = message.Data[MovDataType.ALERT] as Alertdata;
                    if (alt != null)
                    {
                        AlarmTime = alt.Time;
                        Leak = alt.Leak;
                        Cable = alt.Cable;
                        AlertTemp = alt.Temperature;
                        var lst = BitConverter.GetBytes(alt.Alert);
                        BitArray ba = new BitArray(lst);
                        int i = 0;
                        foreach (bool a in ba)
                        {
                            if (a == true)
                            {
                                AlarmList.Add(Alarm.Name[i]);
                            }
                            i++;
                        }
                        AlertdataCollection.Add(alt);
                        if (AlertdataCollection.Count > 10)
                            AlertdataCollection.RemoveAt(0);
                        //广播
                        UnitCore.Instance.MovTraceService.Save("ACOUSTICTOSAIL", alt.Pack());
                        byte[] posBytes = new byte[22];
                        posBytes[0] = 0x15;
                        posBytes[1] = 0x20;
                        Buffer.BlockCopy(alt.Pack(), 0, posBytes, 2, 20);
                        UnitCore.Instance.NetCore.BroadCast(posBytes);
                    }
                }
                if (message.Data.ContainsKey(MovDataType.ALLPOST))
                {

                    var allpos = message.Data[MovDataType.ALLPOST] as Sysposition;
                    if (allpos != null)
                    {
                        PosFromUSBLTime = allpos.Time;
                        
                        ShipLongUsbl = allpos.ShipLong;
                        ShipLatUsbl = allpos.ShipLat;
                        ShipHeading = allpos.Shipheading;
                        ShipPitch = allpos.Shippitch;
                        ShipRoll = allpos.Shiproll;
                        //ShipSpeed = allpos.Shipvel/1000/0.5144444;//mm/s->(knot=每秒0.5144444米（m/s）)
                        MovLongUsbl = allpos.SubLong;
                        MovLatUsbl = allpos.SubLat;
                        MovDepthUsbl = allpos.Subdepth;
                        XDistanceFromUSBL = allpos.RelateX;
                        YDistanceFromUSBL = allpos.RelateY;
                        ZDistanceFromUSBL = allpos.RelateZ;
                        LastUsblPoint = new Point3D(XDistanceFromUSBL, YDistanceFromUSBL, ZDistanceFromUSBL);
                        UpdateUSBLTrack(XDistanceFromUSBL, YDistanceFromUSBL, ZDistanceFromUSBL);
                        if(USBLUsed)
                            Transfromxyz(XDistanceFromUSBL, YDistanceFromUSBL, ZDistanceFromUSBL);
                        UsblPositionCollection.Add(allpos);
                        if (UsblPositionCollection.Count > 10)
                            UsblPositionCollection.RemoveAt(0);
                        //广播
                        //UnitCore.Instance.MovTraceService.Save("ACOUSTICTOSAIL", allpos.Pack());
                        //byte[] posBytes = new byte[42];
                        //posBytes[0] = 0x01;
                        //posBytes[1] = 0x20;
                        //Buffer.BlockCopy(allpos.Pack(), 0, posBytes, 2, 40);
                        //UnitCore.Instance.NetCore.BroadCast(posBytes);
                    }
                }
                if (message.Data.ContainsKey(MovDataType.BP))
                {
                    var bp = message.Data[MovDataType.BP] as Bpdata;
                    if (bp != null)
                    {
                        BpTime = bp.Time;
                        BpBottom = bp.Down;
                        BpBottomBack = bp.Behinddown;
                        BpFront = bp.Front;
                        BpFrontDown = bp.Frontdown;
                        BpFrontUp = bp.Frontup;
                        BpLeft = bp.Left;
                        BpRight = bp.Right;
                        BpCollection.Add(bp);
                        if (BpCollection.Count > 10)
                            BpCollection.RemoveAt(0);
                        //广播
                        //byte[] posBytes = new byte[18];
                        //UnitCore.Instance.MovTraceService.Save("ACOUSTICTOSAIL", bp.Pack());//
                        //byte[] posBytes = new byte[18];
                        //posBytes[0] = 0x16;
                        //posBytes[1] = 0x20;
                        //Buffer.BlockCopy(ctd.Pack(), 0, posBytes, 2, 16);
                        //UnitCore.Instance.NetCore.BroadCast(posBytes);
                    }
                }
                /*
            if (message.Data.ContainsKey(MovDataType.BSSS))
            {
                var bsss = message.Data[MovDataType.BSSS] as Bsssdata;
                if (bsss != null)
                {
                    BsssTime = DateTime.FromFileTime(bsss.Itime).ToShortTimeString();
                    BsssHeight = bsss.Height;

                }
            }*/
                if (message.Data.ContainsKey(MovDataType.CTD))
                {
                    var ctd = message.Data[MovDataType.CTD] as Ctddata;
                    if (ctd != null)
                    {
                        CTDTime = ctd.Time;
                        CTDSoundvec = ctd.Soundvec;
                        CTDDepth = ctd.Depth;
                        CTDWaterTemp = ctd.Watertemp;
                        CTDWatercond = ctd.Watercond;
                        CTDCollection.Add(ctd);
                        if (CTDCollection.Count > 10)
                            CTDCollection.RemoveAt(0);
                        //广播
                        UnitCore.Instance.MovTraceService.Save("ACOUSTICTOSAIL", ctd.Pack());
                        byte[] posBytes = new byte[18];
                        posBytes[0] = 0x12;
                        posBytes[1] = 0x20;
                        Buffer.BlockCopy(ctd.Pack(), 0, posBytes, 2, 16);
                        UnitCore.Instance.NetCore.BroadCast(posBytes);
                    }
                }
                if (message.Data.ContainsKey(MovDataType.ENERGY))
                {
                    var eng = message.Data[MovDataType.ENERGY] as Energysys;
                    if (eng != null)
                    {
                        EnergyTime = eng.Time;
                        HeadmainV = eng.HeadmainV;
                        HeadmainI = eng.HeadmainI;
                        Headmainconsume = eng.Headmainconsume;
                        HeadmainMaxTemp = eng.HeadmainMaxTemp;
                        HeadmainMaxExpand = eng.HeadmainMaxExpand;
                        TailmainV = eng.TailmainV;
                        TailmainI = eng.TailmainI;
                        Tailmainconsume = eng.Tailmainconsume;
                        TailmainMaxTemp = eng.TailmainMaxTemp;
                        TailmainMaxExpand = eng.TailmainMaxTemp;
                        LeftsubV = eng.LeftsubV;
                        LeftsubI = eng.LeftsubI;
                        Leftsubconsume = eng.Leftsubconsume;
                        LeftsubMaxTemp = eng.LeftsubMaxTemp;
                        LeftsubMaxExpand = eng.LeftsubMaxExpand;
                        RightsubV = eng.RightsubV;
                        RightsubI = eng.RightsubI;
                        Rightsubconsume = eng.Rightsubconsume;
                        RightsubMaxTemp = eng.RightsubMaxTemp;
                        RightsubMaxExpand = eng.RightsubMaxExpand;
                        EnergysysCollection.Add(eng);
                        if (EnergysysCollection.Count > 10)
                            EnergysysCollection.RemoveAt(0);
                        //广播
                        UnitCore.Instance.MovTraceService.Save("ACOUSTICTOSAIL", eng.Pack());
                        byte[] posBytes = new byte[36];
                        posBytes[0] = 0x14;
                        posBytes[1] = 0x20;
                        Buffer.BlockCopy(eng.Pack(), 0, posBytes, 2, 34);
                        UnitCore.Instance.NetCore.BroadCast(posBytes);
                    }
                }
                if (message.Data.ContainsKey(MovDataType.IMAGE))
                {
                    byte[] bytes = message.Data[MovDataType.IMAGE] as byte[];
                    if (bytes != null)
                    {
                        MemoryStream ms = new MemoryStream(bytes);
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = ms;
                        image.EndInit();
                        ImgContainer = new System.Windows.Controls.Image();
                        ImgContainer.Source = image;
                        ImgCollection.Add(image);
                        //转发给水面航控
                    }
                }
                if (message.Data.ContainsKey(MovDataType.LIFESUPPLY))
                {
                    var life = message.Data[MovDataType.LIFESUPPLY] as Lifesupply;
                    if (life != null)
                    {
                        LifeTime = life.Time;
                        
                        Oxygen = life.Oxygen;
                        Co2 = life.Co2;
                        Pressure = life.Pressure;
                        Temperature = life.Temperature;
                        Humidity = life.Humidity;
                        LifesupplyCollection.Add(life);
                        if (LifesupplyCollection.Count > 10)
                            LifesupplyCollection.RemoveAt(0);
                        //广播
                        UnitCore.Instance.MovTraceService.Save("ACOUSTICTOSAIL", life.Pack());
                        byte[] posBytes = new byte[16];
                        posBytes[0] = 0x13;
                        posBytes[1] = 0x20;
                        Buffer.BlockCopy(life.Pack(), 0, posBytes, 2, 14);
                        UnitCore.Instance.NetCore.BroadCast(posBytes);
                        RefreshLifeInfos();
                    }
                }
                if (message.Data.ContainsKey(MovDataType.SUBPOST))
                {

                    var subpos = message.Data[MovDataType.SUBPOST] as Subposition;
                    if (subpos != null)
                    {
                        PosFromUwvTime = subpos.Time;
                        
                        MovDepth = subpos.Subdepth;
                        MovLat = subpos.SubLat;
                        MovLong = subpos.SubLong;
                        MovHeading = subpos.Subheading;
                        MovHeight = subpos.Subheight;
                        MovPitch = subpos.Subpitch;
                        MovRoll = subpos.Subroll;
                        
                        ZDistanceFromUWV = MovDepth;
                        float DistanceX, DistanceY,movlng,movlat,shiplng,shiplat;
                        movlng = subpos._subLong/60 + subpos._subLong%60/100;
                        movlat = subpos._subLat/60 + subpos._subLat%60/100;
                        if (UsblPositionCollection.Count > 0)
                        {
                            var lastshippos = UsblPositionCollection[UsblPositionCollection.Count - 1];
                            shiplng = lastshippos._shipLong / 60 + lastshippos._shipLong % 60 / 100;
                            shiplat = lastshippos._shipLat / 60 + lastshippos._shipLat % 60 / 100;
                            CalcRelativePostion(ShipHeading, movlng, movlat, shiplng, shiplat, out DistanceX,
                                out DistanceY);
                            XDistanceFromUWV = DistanceX;
                            YDistanceFromUWV = DistanceY;
                            UpdateUWVLTrack(XDistanceFromUWV, YDistanceFromUWV, ZDistanceFromUWV);
                            LastUWVPoint = new Point3D(XDistanceFromUWV, YDistanceFromUWV, ZDistanceFromUWV);
                            if (UWVUsed)
                                Transfromxyz(XDistanceFromUWV, YDistanceFromUWV, ZDistanceFromUWV);
                        }
                        
                        SubPositionCollection.Add(subpos);
                        if (SubPositionCollection.Count > 10)
                            SubPositionCollection.RemoveAt(0);
                        //广播
                        UnitCore.Instance.MovTraceService.Save("ACOUSTICTOSAIL", subpos.Pack());
                        byte[] posBytes = new byte[28];
                        posBytes[0] = 0x11;
                        posBytes[1] = 0x20;
                        Buffer.BlockCopy(subpos.Pack(), 0, posBytes, 2, 26);
                        UnitCore.Instance.NetCore.BroadCast(posBytes);
                    }
                }
                if (message.Data.ContainsKey(MovDataType.WORD))
                {
                    var word = message.Data[MovDataType.WORD] as string;
                    if (word != null)
                    {

                        if (message.Type == ModuleType.FH)
                        {
                            if (UnitCore.Instance.WorkMode == MonitorMode.SHIP)
                            {
                                UnitCore.Instance.MovTraceService.Save("FH", "（潜器）" + word);
                                UnitCore.Instance.MovTraceService.Save("Chart", "（潜器-跳频）" + word);
                                MainFrameViewModel.pMainFrame.MsgLog.Add(DateTime.Now.ToLongTimeString() + ":" +
                                                                         "（潜器-跳频）" + word);
                            }
                            else
                            {
                                UnitCore.Instance.MovTraceService.Save("FH", "（母船）" + word);
                                UnitCore.Instance.MovTraceService.Save("Chart", "（母船-跳频）" + word);
                                MainFrameViewModel.pMainFrame.MsgLog.Add(DateTime.Now.ToLongTimeString() + ":" +
                                                                         "（母船-跳频）" + word);
                            }
                        }
                        else
                        {
                            if (UnitCore.Instance.WorkMode == MonitorMode.SHIP)
                            {
                                UnitCore.Instance.MovTraceService.Save("Chart", "（潜器）" + word);
                                MainFrameViewModel.pMainFrame.MsgLog.Add(DateTime.Now.ToLongTimeString() + ":" +
                                                                         "（潜器）" + word);
                            }
                            else
                            {
                                UnitCore.Instance.MovTraceService.Save("Chart", "（母船）" + word);
                                MainFrameViewModel.pMainFrame.MsgLog.Add(DateTime.Now.ToLongTimeString() + ":" +
                                                                         "（母船）" + word);
                            }
                        }
                        chartstring = word;
                    }

                }
                UnitCore.Instance.LiveHandle(message.Type, chartstring,ImgContainer);
            }));
        }

        private void CalcRelativePostion(float head, float movlng, float movlat, float shiplng, float shiplat, out float DistanceX, out float DistanceY)
        {
            float lngdist = 0;
            double xdist = 0;
            float latdist = 0;
            double ydist = 0;
            if (movlng <= 0 && shiplng >= 0)
            {
                if (shiplng - movlng >= 180)
                {
                    lngdist = 360 - (shiplng - movlng);
                }
                else
                {
                    lngdist = -(shiplng - movlng);
                }
            }
            else if (shiplng <= 0 && movlng >= 0)
            {
                if (movlng - shiplng >= 180)
                {
                    lngdist = (movlng - shiplng) - 360;
                }
                else
                {
                    lngdist = movlng - shiplng;
                }
            }
            else//两者同时为正，同时为负
            {
                lngdist = movlng - shiplng;
            }
            latdist = movlat - shiplat;
            xdist = ((lngdist - (int)lngdist) / 100 + (int)lngdist) * 60 * 1852;
            xdist = xdist*Math.Cos(movlat/180*Math.PI);
            ydist = ((latdist - (int)latdist) / 100 + (int)latdist) * 60 * 1852;
            DistanceX = (float)(xdist * Math.Cos(head / 180 * Math.PI) - ydist * Math.Sin(head / 180 * Math.PI));
            DistanceY = (float)(xdist * Math.Sin(head / 180 * Math.PI) + ydist * Math.Cos(head / 180 * Math.PI));
        }

        public void Handle(USBLEvent message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var allpos = message.Position as Sysposition;
                if (allpos != null)
                {
                    PosFromUSBLTime = DateTime.FromFileTime(allpos.Ltime).ToShortTimeString();
                    ShipLongUsbl = allpos.ShipLong;
                    ShipLatUsbl = allpos.ShipLat;
                    ShipHeading = allpos.Shipheading;
                    ShipPitch = allpos.Shippitch;
                    ShipRoll = allpos.Shiproll;
                    ShipSpeed = allpos.Shipvel;
                    MovLongUsbl = allpos.SubLong;
                    MovLatUsbl = allpos.SubLat;
                    MovDepthUsbl = allpos.Subdepth;
                    XDistanceFromUSBL = allpos.RelateX;
                    YDistanceFromUSBL = allpos.RelateY;
                    ZDistanceFromUSBL = allpos.RelateZ;
                    Transfromxyz(XDistanceFromUSBL, YDistanceFromUSBL, ZDistanceFromUSBL);
                }
            }));
        }
        #endregion


        public void Handle(SailEvent message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (message.Type == ExchageType.ADCP)
                {
                    RecvData = true;
                    adcpcount++;
                    var adcp = message.SailData as Adcpdata;
                    if (adcp != null)
                    {
                        ADCPTime = adcp.Time;
                        xVel.Clear();
                        yVel.Clear();
                        zVel.Clear();
                        for (int i = 0; i < 10; i++)
                        {
                            xVel.Add(new AdcpInfo(i + 1, adcp.FloorX[i]));
                            yVel.Add(new AdcpInfo(i + 1, adcp.FloorY[i]));
                            zVel.Add(new AdcpInfo(i + 1, adcp.FloorZ[i]));
                        }
                        BottomTrack = (float) adcp.BottomTrack;
                        ADCPHeight = (float) adcp.Height;
                        AdcpdataCollection.Add(adcp);
                        if (AdcpdataCollection.Count > 10)
                            AdcpdataCollection.RemoveAt(0);
                    }
                }
                if (message.Type == ExchageType.ALERT)
                {
                    var alt = message.SailData as Alertdata;
                    if (alt != null)
                    {
                        AlarmTime = alt.Time;
                        Leak = alt.Leak;
                        Cable = alt.Cable;
                        AlertTemp = alt.Temperature;
                        var lst = BitConverter.GetBytes(alt.Alert);
                        BitArray ba = new BitArray(lst);
                        int i = 0;
                        foreach (bool a in ba)
                        {
                            if (a == true)
                            {
                                AlarmList.Add(Alarm.Name[i]);
                            }
                            i++;
                        }
                        AlertdataCollection.Add(alt);
                        if (AlertdataCollection.Count > 10)
                            AlertdataCollection.RemoveAt(0);
                        
                    }
                }
                
                if (message.Type == ExchageType.BP)
                {
                    UnitCore.Instance.PostMsgEvent_BPsend.Set();
                    bpcount++;
                    var bp = message.SailData as Bpdata;
                    if (bp != null)
                    {
                        BpTime = bp.Time;
                        BpBottom = bp.Down;
                        BpBottomBack = bp.Behinddown;
                        BpFront = bp.Front;
                        BpFrontDown = bp.Frontdown;
                        BpFrontUp = bp.Frontup;
                        BpLeft = bp.Left;
                        BpRight = bp.Right;
                        BpCollection.Add(bp);
                        if (BpCollection.Count > 10)
                            BpCollection.RemoveAt(0);
                    }
                }
               
                if (message.Type == ExchageType.CTD)
                {
                    var ctd = message.SailData as Ctddata;
                    if (ctd != null)
                    {
                        CTDTime = ctd.Time;
                        CTDSoundvec = ctd.Soundvec;
                        CTDDepth = ctd.Depth;
                        CTDWaterTemp = ctd.Watertemp;
                        CTDWatercond = ctd.Watercond;
                        CTDCollection.Add(ctd);
                        if (CTDCollection.Count > 10)
                            CTDCollection.RemoveAt(0);
                        
                    }
                }
                if (message.Type == ExchageType.ENERGY)
                {
                    var eng = message.SailData as Energysys;
                    if (eng != null)
                    {
                        EnergyTime = eng.Time;
                        HeadmainV = eng.HeadmainV;
                        HeadmainI = eng.HeadmainI;
                        Headmainconsume = eng.Headmainconsume;
                        HeadmainMaxTemp = eng.HeadmainMaxTemp;
                        HeadmainMaxExpand = eng.HeadmainMaxExpand;
                        TailmainV = eng.TailmainV;
                        TailmainI = eng.TailmainI;
                        Tailmainconsume = eng.Tailmainconsume;
                        TailmainMaxTemp = eng.TailmainMaxTemp;
                        TailmainMaxExpand = eng.TailmainMaxTemp;
                        LeftsubV = eng.LeftsubV;
                        LeftsubI = eng.LeftsubI;
                        Leftsubconsume = eng.Leftsubconsume;
                        LeftsubMaxTemp = eng.LeftsubMaxTemp;
                        LeftsubMaxExpand = eng.LeftsubMaxExpand;
                        RightsubV = eng.RightsubV;
                        RightsubI = eng.RightsubI;
                        Rightsubconsume = eng.Rightsubconsume;
                        RightsubMaxTemp = eng.RightsubMaxTemp;
                        RightsubMaxExpand = eng.RightsubMaxExpand;
                        EnergysysCollection.Add(eng);
                        if (EnergysysCollection.Count > 10)
                            EnergysysCollection.RemoveAt(0);
                        
                    }
                }
                
                if (message.Type == ExchageType.LIFESUPPLY)
                {
                    var life = message.SailData as Lifesupply;
                    if (life != null)
                    {
                        LifeTime = life.Time;
                        
                        Oxygen = life.Oxygen;
                        Co2 = life.Co2;
                        Pressure = life.Pressure;
                        Temperature = life.Temperature;
                        Humidity = life.Humidity;
                        LifesupplyCollection.Add(life);
                        if (LifesupplyCollection.Count > 10)
                            LifesupplyCollection.RemoveAt(0);
                        
                    }
                }
                if (message.Type == ExchageType.SUBPOST)
                {

                    var subpos = message.SailData as Subposition;
                    if (subpos != null)
                    {
                        PosFromUwvTime = subpos.Time;
                        
                        MovDepth = subpos.Subdepth;
                        MovLat = subpos.SubLat;
                        MovLong = subpos.SubLong;
                        MovHeading = subpos.Subheading;
                        MovHeight = subpos.Subheight;
                        MovPitch = subpos.Subpitch;
                        MovRoll = subpos.Subroll;
                        SubPositionCollection.Add(subpos);
                        if (SubPositionCollection.Count > 10)
                            SubPositionCollection.RemoveAt(0);
                    }
                }
            }));
        }
        public void Handle(DspFeedbackComEvent message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (message.Type == ModuleType.FeedBack)
                    {
                        Gain=BitConverter.ToInt16(message.Data, 0);
                    }

                }));

        }

        public void Handle(DSPReqEvent message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                UnitCore.Instance.MFSK_LeftSize = 20;
                if (SendMessageBoxreadonly==true)
                {
                    SendMessageBoxreadonly = false;//产生一个状态切换，让LiveCaptureView产生触发
                }
                SendMessageBoxreadonly = true;
                
            }));

        }
    }
}
