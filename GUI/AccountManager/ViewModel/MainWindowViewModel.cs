using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.GUI.ViewModel
{
    public  class MainWindowViewModel : ViewModelBase
    {
        private string _version; 

        public string Version { get => _version; set => this.Set(ref this._version, value); }
        public string BuildDate { get => _buildDate; set => this.Set(ref this._buildDate, value); }
        public int WaitJoinCustomerCount { get => _waitJoinCustomerCount; set => this.Set(ref this._waitJoinCustomerCount, value); }
        public string CommStatus { get => _CommStatus; set => this.Set(ref _CommStatus, value); }

        private string _buildDate;

        private int _waitJoinCustomerCount = 10;

        private string _CommStatus;


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
            WaitJoinCustomerCount = 25;
            

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


    }
}
