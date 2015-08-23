﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BoonieBear.DeckUnit.Core;
using BoonieBear.DeckUnit.DAL;
using TinyMetroWpfLibrary.Events;
using TinyMetroWpfLibrary.ViewModel;
using System.Windows.Threading;

namespace BoonieBear.DeckUnit.ViewModels
{

    public class HistoryDataPageViewModel : ViewModelBase
    {
        #region Overrides of ViewModelBase

        

        public override void Initialize()
        {
            GoBackCommand = RegisterCommand(ExecuteGoBackCommand, CanExecuteGoBackCommand, true);
            FetchingData = RegisterCommand(ExecuteFetchingData, CanExecuteFetchingData, true);
            DataCollMt = new List<CommandLog>();
            CMDCollMt = new List<CommandLog>();
            IsDataActive = true;
            IsCmdActive = false;
            IsFetching = false;
            SelectedFromDate = DateTime.Today;
            SelectedToDate = DateTime.Today;
            RefreshVisble = Visibility.Visible;
        }


        public override void InitializePage(object extraData)
        {
            AddPropertyChangedNotification(() => IsDataActive);
            AddPropertyChangedNotification(() => IsCmdActive);
            AddPropertyChangedNotification(() => SelectIndex);
            IsDataActive = true;
            IsCmdActive = false;
        }

        #endregion
        #region 绑定属性

        public DateTime SelectedFromDate
        {
            get { return GetPropertyValue(() => SelectedFromDate); }
            set { SetPropertyValue(() => SelectedFromDate, value); }
        }

        public DateTime SelectedToDate
        {
            get { return GetPropertyValue(() => SelectedToDate); }
            set { SetPropertyValue(() => SelectedToDate, value); }
        }
        
        public Visibility RefreshVisble
        {
            get { return GetPropertyValue(() => RefreshVisble); }
            set { SetPropertyValue(() => RefreshVisble, value); }
        }
        public bool IsCmdActive
        {
            get { return GetPropertyValue(() => IsCmdActive); }
            set
            {
                SetPropertyValue(() => IsCmdActive, value);
                if (value == true)
                {
                    IsDataActive = false;
                    SelectIndex = 1;
                }
                else
                {
                    if (IsDataActive == false)
                        IsCmdActive = true;
                }
            }
        }
        public bool IsDataActive
        {
            get { return GetPropertyValue(() => IsDataActive); }
            set
            {
                SetPropertyValue(() => IsDataActive, value);
                if (value == true)
                {
                    IsCmdActive = false;
                    SelectIndex = 0;
                }
                else
                {
                    if (IsCmdActive == false)
                        IsDataActive = true;
                }
            }
        }
        public List<CommandLog> DataCollMt
        {
            get { return GetPropertyValue(() => DataCollMt); }
            set { SetPropertyValue(() => DataCollMt, value); }
        }
        public List<CommandLog> CMDCollMt
        {
            get { return GetPropertyValue(() => CMDCollMt); }
            set { SetPropertyValue(() => CMDCollMt, value); }
        }
        public int SelectIndex
        {
            get { return GetPropertyValue(() => SelectIndex); }
            set { SetPropertyValue(() => SelectIndex, value); }
        }
        public bool IsFetching
        {
            get { return GetPropertyValue(() => IsFetching); }
            set { SetPropertyValue(() => IsFetching, value); }
        }
        #endregion
        #region cmd
        public ICommand GoBackCommand
        {
            get { return GetPropertyValue(() => GoBackCommand); }
            set { SetPropertyValue(() => GoBackCommand, value); }
        }


        private void CanExecuteGoBackCommand(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = !IsFetching;
        }


        private void ExecuteGoBackCommand(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            EventAggregator.PublishMessage(new GoBackNavigationRequest());
        }

        
        
        public ICommand FetchingData
        {
            get { return GetPropertyValue(() => FetchingData); }
            set { SetPropertyValue(() => FetchingData, value); }
        }


        public void CanExecuteFetchingData(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = UnitCore.Instance.UnitTraceService.IsOK;
        }


        public async void ExecuteFetchingData(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            IsFetching = true;
            RefreshVisble = Visibility.Collapsed;
            //fetch data...

            var from = new DateTime(SelectedFromDate.Year,SelectedFromDate.Month,SelectedFromDate.Day);
            var to = new DateTime(SelectedToDate.Year, SelectedToDate.Month, SelectedToDate.Day);
            var lst = UnitCore.Instance.UnitTraceService.GetCommandList(from, to);
            if (lst != null)
            {
                DataCollMt.Clear();
                CMDCollMt.Clear();
                foreach (var commandLog in lst)
                {
                    if (commandLog.Type == true) //收到
                    {
                        DataCollMt.Add(commandLog);
                    }
                    else
                    {
                        CMDCollMt.Add(commandLog);
                    }
                }
            }
            SelectIndex = 1;
            SelectIndex = 0;//控件不能自己刷新，需要切换一下，下一步需改进
            
            IsFetching = false;
            RefreshVisble = Visibility.Visible;
        }
        #endregion



    }
}