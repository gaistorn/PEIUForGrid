﻿using FireworksFramework.Mqtt;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using Microsoft.AspNet.SignalR.Client;
using PEIU.GUI.CustomControls;
using PEIU.GUI.Subscriber;
using PEIU.GUI.ViewModel;
using PEIU.GUI.WebServices;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PEIU.GUI.View
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        MainWindowViewModel ViewModel => ViewModelLocator.MainStatic;
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        ReservedRegisterSubscriberWorker subscriberWorker = null;
         BackgroundWorker mqttBacgkroundWorker; 
        public MainWindow()
        {
            InitializeComponent();
            AbsMqttBase.SetDefaultLoggerName("DefaultLogger");
           
        }

        private void HamburgerMenuControl_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            HamburgerMenuControl.Content = e.InvokedItem;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ////HamburgerMenu hm;hm.ItemTemplateSelector
            //HamburgerMenuIconItem item = new HamburgerMenuIconItem();
            //item.Label = "Hei";
            //HamburgerMenuControl.Items.Add(item);

            LoadNewUser();
            InitializeMqtt();

            //GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<DialogMessage>(this, HandleDialogMessage);


        }

        private  void InitializeMqtt()
        {
            try
            {

                mqttBacgkroundWorker = new BackgroundWorker();
                mqttBacgkroundWorker.DoWork += MqttBacgkroundWorker_DoWork;
                mqttBacgkroundWorker.RunWorkerAsync();
                
            }
            catch(Exception ex)
            {

            }
        }

        private void MqttBacgkroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            subscriberWorker = new ReservedRegisterSubscriberWorker();
            subscriberWorker.MessageReceivedEvent += SubscriberWorker_MessageReceivedEvent;
            subscriberWorker.Initialize();

            Task t = subscriberWorker.MqttSubscribeAsync(cancellationTokenSource.Token);
            t.Wait();
        }

        private void SubscriberWorker_MessageReceivedEvent(object sender, MqttMessageReceivedEventArgs<RegisterViewModel> e)
        {
            if(HamburgerMenuControl.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                HamburgerMenuControl.Dispatcher.Invoke(() => { (HamburgerMenuControl.Items[1] as HamburgerBedgeMenuItem).Bedge++; });
            }
            else
                (HamburgerMenuControl.Items[1] as HamburgerBedgeMenuItem).Bedge++;
        }

        private void OnReceiveNotifyRegister(string Email, int AuthRole)
        {

        }

        private async void LoadNewUser()
        {
            try
            {
                var result = await ContractWebService.RequestCollectionGetMethod<ReservedAssetLocation>("api/contract/getreservedregisters");
                (HamburgerMenuControl.Items[1] as HamburgerBedgeMenuItem).Bedge = result.Count(x => x.RegisterTimestamp.Date == DateTime.Now.Date);
                ViewModel.WaitJoinCustomerCount = 150;
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Badged_BadgeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void Badged_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Badged baged = (Badged)sender;
            baged.Badge = 1;
        }

        private void StatusBarItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
            //connection.Start();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            cancellationTokenSource.Cancel();
            ViewModelLocator.Dispose();
        }
    }
}