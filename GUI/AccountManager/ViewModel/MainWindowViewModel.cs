using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using PEIU.GUI.CustomControls;
using PEIU.GUI.WebServices;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PEIU.GUI.ViewModel
{
    public  class MainWindowViewModel : ViewModelBase
    {
        private string _version;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public string Version { get => _version; set => this.Set(ref this._version, value); }
        public string BuildDate { get => _buildDate; set => this.Set(ref this._buildDate, value); }
        public string Status { get => _CommStatus; set => this.Set("Status", ref _CommStatus, value); }

        private string _buildDate;

        private int _waitJoinCustomerCount = 10;

        private string _CommStatus;

        private MenuItemBase selectedMenu;
        public MenuItemBase SelectedMenu
        {
            get
            {
                return selectedMenu;
            }
            set
            {
                this.Set("SelectedMenu", ref selectedMenu, value);
            }
        }

        private RelayCommand<ItemClickEventArgs> selectedMenuItemCommand;
        public RelayCommand<ItemClickEventArgs> SelectedMenuItemCommand

        {
            get
            {
                return selectedMenuItemCommand ?? (selectedMenuItemCommand = new RelayCommand<ItemClickEventArgs>((e) => SelectedMenu = (MenuItemBase)e.ClickedItem));
            }
        }

        private RelayCommand _updateViewModeCommand;
            
        public RelayCommand UpdateViewModelCommand
        {
            get
            {
                return _updateViewModeCommand ?? (_updateViewModeCommand = new RelayCommand(UpdateViewModel));
                    
            }
        }

        private IEnumerable<HamburgerMenuItem> baseModels;

        public IEnumerable<HamburgerMenuItem> Menus
        {
            get
            {
                return baseModels ?? (baseModels = CreateMenuItems());
            }
        }

        private IEnumerable<HamburgerMenuItem> CreateMenuItems()
        {
            var menus = new IMenuModel[] { SimpleIoc.Default.GetInstance<CandidateCustomerViewModel>(), SimpleIoc.Default.GetInstance<CustomerManagerViewModel>(), SimpleIoc.Default.GetInstance<StatusDashboardViewModel>() };
            foreach(ViewModelBase baseModel in menus)
            {
                MenuItemBase item = null;
                if (baseModel is IBedgeMenuModel)
                {
                    item = new CustomControls.BedgeMenuItem();
                    
                }
                else
                    item = new MenuItemBase();
                item.ViewModel = baseModel;
                               
                yield return item;
            }
        }


        private bool CanAccessWebServer()
        {
            return true;
        }

        private async void UpdateViewModel()
        {
            if(selectedMenu != null && selectedMenu.ViewModel != null && selectedMenu.ViewModel is IUpdateWebData)
            {
                IUpdateWebData webData = selectedMenu.ViewModel as IUpdateWebData;
                if (webData.CanUpdate)
                    await webData.StartUpdateAsync(cancellationTokenSource.Token);
            }
            //CancellationTokenSource source
            //foreach(IUpdateWebData updateWeb in  CommonServiceLocator.ServiceLocator.Current.GetAllInstances<IUpdateWebData>())
            //{
            //    await updateWeb.StartUpdateAsync()
            //}
        }


        public MainWindowViewModel()
        {
            DateTime buildDate = GetLinkerTimestampUtc(Assembly.GetExecutingAssembly());
            string versionString = "";
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                versionString = $"버전 {System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion} (배포버전)";
            }
            else
            {
                versionString = "개발자 버전";
            }

            //versionString = $"{versionString} 빌드 {buildDate}";
            Version = versionString;
            BuildDate = $"빌드 {buildDate.ToShortDateString()}";

            
        }

        public static DateTime GetLinkerTimestampUtc(Assembly assembly)
        {
            var location = assembly.Location;
            DateTime modification = File.GetLastWriteTime(location);
            return modification;
        }

        public static DateTime GetLinkerTimestampUtc(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var bytes = new byte[2048];

            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Read(bytes, 0, bytes.Length);
                
            }

            var headerPos = BitConverter.ToInt32(bytes, peHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(bytes, headerPos + linkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(secondsSince1970);
        }

        public async Task StartUpdateAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("MainWindowsViewModel Update");
            await Task.CompletedTask;
        }

        public object GetMenuIcon()
        {
            throw new NotImplementedException();
        }
    }
}
