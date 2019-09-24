using GalaSoft.MvvmLight;//
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PEIU.GUI.View;
using PEIU.GUI.WebServices;
using PEIU.Models;
using PEIU.Models.Database;
using PEIU.Models.ExchangeModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PEIU.GUI.ViewModel
{
    public class ContractorCustomerViewModel : ViewModelBase, IUpdateWebData, IBedgeMenuModel
    {
        public bool CanUpdate => true;

        public int BedgeCount { get => _bedgeCount; set => base.Set("BedgeCount", ref _bedgeCount, value); }
        private int _bedgeCount = 0;

        private AggregatorGroupBase AllGroup = new AggregatorGroupBase() { ID = "-1", AggName = "전체" };

        private ObservableCollection<AggregatorGroupBase> _aggregators = new ObservableCollection<AggregatorGroupBase>();

        public ObservableCollection<AggregatorGroupBase> Aggregators
        {
            get => _aggregators;
            set => base.Set("Aggregators", ref _aggregators, value);
        }

        private ObservableCollection<VwContractoruserBase> _contractors = new ObservableCollection<
            VwContractoruserBase>();
        public ObservableCollection<VwContractoruserBase> Contractors
        {
            get => _contractors;
            set => base.Set("Contractors", ref _contractors, value);
        }

        public string Title => "발전사업자";

        public string Tooltip => "발전사업자 관리 페이지";
       // <iconPacks:PackIconMaterial Kind = "AccountCardDetailsOutline" />
        public object Icon
        {
            get
            {
                MahApps.Metro.IconPacks.PackIconMaterial icon = new MahApps.Metro.IconPacks.PackIconMaterial();
                icon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.AccountCardDetailsOutline;
                return icon;
            }
        }

        RelayCommand<VwContractoruserBase> _showApprovalWindowCommand;
        public RelayCommand<VwContractoruserBase> ShowApprovalWindowCommand
        {
            get
            {
                return _showApprovalWindowCommand ?? (_showApprovalWindowCommand = new RelayCommand<VwContractoruserBase>(ExecuteShowApprovalWindow));
            }
        }

        //RelayCommand _showApprovalWindowCommand;
        //public RelayCommand ShowApprovalWindowCommand
        //{
        //    get
        //    {
        //        return _showApprovalWindowCommand ?? (_showApprovalWindowCommand = new RelayCommand(ExecuteShowApprovalWindow));
        //    }
        //}

        //private void ExecuteShowApprovalWindow()
        //{

        //}

        private void ExecuteShowApprovalWindow(VwContractoruserBase user)
        {
            if (user.ContractStatus == ContractStatusCodes.Activating || user.ContractStatus == ContractStatusCodes.Cancellations)
                return;
            ViewModelLocator.ContractorStatic.Contractor = user;
            ViewModelLocator.ContractorStatic.ShownContractorApprovalView = new ContractorApprovalView();
            ViewModelLocator.ContractorStatic.ShownContractorApprovalView.ShowDialog();
        }

        private RelayCommand<SelectionChangedEventArgs> selectedAggregroup;
        public RelayCommand<SelectionChangedEventArgs> SelectedAggregroupCommand

        {
            get
            {
                return selectedAggregroup ?? (selectedAggregroup = new RelayCommand<SelectionChangedEventArgs>((e) =>
                {
                    if (e.AddedItems.Count == 0)
                    {
                        Contractors.Clear();
                        return;
                    }
                    AggregatorGroupBase group = (e.AddedItems[0] as AggregatorGroupBase);
                    UpdateContractors(group.ID);
                }));
            }
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private ContractorCustomerView view;
        public object OwnerControl => view ?? (view = new ContractorCustomerView());

        private async void UpdateContractors(string groupId)
        {
            try
            {
                object parameter = new { aggregatorgroupid = groupId };
                if (groupId == "-1")
                {
                    parameter = new { };
                }
                var res = await ContractWebService.RequestCollectionGetMethod<VwContractoruserBase>(RequestErrorHandler, "/api/contractor/getcontractors", parameter);
                Contractors = new ObservableCollection<VwContractoruserBase>(res);
                
                var result_sites = await ContractWebService.RequestCollectionGetMethod<VwContractorsiteBase>(RequestErrorHandler, "/api/contractor/getallcontractorsites", new { onlyunspecifieduser = true });
                ViewModelLocator.ContractorStatic.Sites = result_sites.Where(x => x.UserId == null);

                //Contractors.Clear();
                //foreach (JObject jo in res)
                //{
                //    var line = jo.ToObject<VwContractoruserBase>();
                //    Contractors.Add(line);

                //}
            }
            catch (Exception ex)
            { }
        }

        private async Task UpdateAggregateGroups()
        {
            try
            {
                List<AggregatorGroupBase> res = await ContractWebService.RequestCollectionGetMethod<AggregatorGroupBase>(RequestErrorHandler, "/api/aggregator/getaggregatorgroups");
                
                Aggregators = new ObservableCollection<AggregatorGroupBase>(res);
                Aggregators.Insert(0, AllGroup);
               // _aggregators.Clear();
                //foreach (JObject jo in res)
                //{
                //    var line = jo.ToObject<AggregatorGroupBase>();
                //    Contractors.Add(line);

                //}
            }
            catch (Exception ex)
            { }
        }

        private void RequestErrorHandler(bool Result, IEnumerable<(string code,string description)> errors)
        {

        }

        public async Task StartUpdateAsync(CancellationToken cancellationToken)
        {
            try
            {
               

                var res = await ContractWebService.RequestGetMethod<JArray>(RequestErrorHandler, "/api/contractor/getcontractors", new { aggregatorgroupid = "bf25cd61-254b-4406-9d2e-c951fea67054" });
                //Contractors.Clear();

                //foreach (JObject jo in res)
                //{
                //    var line = jo.ToObject<VwContractoruserBase>();
                //    Contractors.Add(line);

                //}
                //var gp = Contractors.GroupBy(x => x.AggGroupId, v => v.AggName);
                //List<(string key, string name)> k = new List<(string, string)>();
                //k.Add(("123", "123"));
                //Contractors = new ObservableCollection<Models.Database.VwContractoruserBase>(xs);
                await UpdateAggregateGroups();
                 BedgeCount = _contractors.Count(x => x.SignInConfirm == false && x.ContractStatus == ContractStatusCodes.Signing && x.RegistDate.Date == DateTime.Now.Date);


                //var result = await ContractWebService.RequestCollectionGetMethod<Models.ExchangeModel.ContractorRegistModel>("/api/contractor/getcontractors", new { aggregatorgroupid = "bf25cd61-254b-4406-9d2e-c951fea67054" });
                //Contractors = new ObservableCollection<Models.ExchangeModel.ContractorRegistModel>(result);
                ////foreach (var register in result)
                //{
                //    for (int i = 0; i < 3; i++)
                //    {
                //        AssetLocation loc = new AssetLocation();
                //        loc.AssetName = $"Test {i}";
                //        //register.Assets.Add(loc);
                //    }
                //}
                //BedgeCount = result.Count(x => (AuthRoles)x.AuthRoles == AuthRoles.Candidator);
            }
            catch (Exception ex)
            {
                // System.Windows.MessageBox.Show(ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
