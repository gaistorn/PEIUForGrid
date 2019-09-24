using GalaSoft.MvvmLight;
using PEIU.GUI.WebServices;
using PEIU.Models.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.GUI.Models
{
    public class CommonServiceModel : ViewModelBase
    {
        private AggregatorGroupBase AllGroup = new AggregatorGroupBase() { ID = "-1", AggName = "전체" };

        private ObservableCollection<AggregatorGroupBase> _aggregatorGroups = new ObservableCollection<AggregatorGroupBase>();

        public ObservableCollection<AggregatorGroupBase> AggregatorGroups
        {
            get => _aggregatorGroups;
            set => base.Set(nameof(AggregatorGroups), ref _aggregatorGroups, value);
        }

        private ObservableCollection<VwContractoruserBase> _contractors = new ObservableCollection<VwContractoruserBase>();
        public ObservableCollection<VwContractoruserBase> Contractors
        {
            get => _contractors;
            set { base.Set(nameof(Contractors), ref _contractors, value); }
        }

        private List<IUpdateWebData> ViewModels = new List<IUpdateWebData>();

        public void RegistViewModels(params IUpdateWebData[] viewModelBases )
        {
            ViewModels.AddRange(viewModelBases);
        }

        public async void UpdateModelFromWebServer()
        {
            try
            {
                List<AggregatorGroupBase> res = await ContractWebService.RequestCollectionGetMethod<AggregatorGroupBase>(RequestErrorHandler, "/api/aggregator/getaggregatorgroups");
                AggregatorGroups = new ObservableCollection<AggregatorGroupBase>(res);
                AggregatorGroups.Insert(0, AllGroup);
                // _aggregators.Clear();
                //foreach (JObject jo in res)
                //{
                //    var line = jo.ToObject<AggregatorGroupBase>();
                //    Contractors.Add(line);

                //}
            }
            catch (Exception ex)
            { }
            foreach (IUpdateWebData modelBase in ViewModels)
            {
                //modelBase.UpdateDatasource("AggregatorGroups");
                //modelBase.UpdateDatasource("Contractors");
            }
        }


        private void RequestErrorHandler(bool Result, IEnumerable<(string code, string description)> errors)
        {

        }
    }
}
