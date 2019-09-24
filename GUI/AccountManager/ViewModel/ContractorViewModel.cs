using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json.Linq;
using PEIU.GUI.WebServices;
using PEIU.Models;
using PEIU.Models.Database;
using PEIU.Models.ExchangeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.GUI.ViewModel
{
    public class ContractorViewModel : ViewModelBase
    {
        private PEIU.Models.Database.VwContractoruserBase _contractor;
        public View.ContractorApprovalView ShownContractorApprovalView;
        private string _commitReason;
        private string _comment;
        private bool _allNotify = false;
        public VwContractoruserBase Contractor { get => _contractor; set => this.Set("Contractor", ref _contractor, value); }
        public string CommitReason { get => _commitReason; set => this.Set("CommitReason", ref  _commitReason, value); }
        public string Comment { get => _comment; set => this.Set("Comment", ref _comment, value); }
        public bool AllNotify { get => _allNotify; set => this.Set("AllNotify", ref _allNotify, value); }
        public IEnumerable<VwContractorsiteBase> Sites { get => _sites; set => this.Set("Sites", ref  _sites, value); }

        private VwContractorsiteBase _selectedItem;

        public VwContractorsiteBase SelectedItem { get => _selectedItem; set => this.Set("SelectedItem", ref _selectedItem, value); }

        List<object> _submitCodes;
        public List<object> SubmitCodes => _submitCodes ?? (_submitCodes = new List<object>()
        {
            new { Value = 1, Label = "승인 (다음 단계로)" },
           new { Value = 2, Label = "보결 (이전 단계로)" },
           new { Value = 3, Label = "취소 (다음 단계로)" },
        });

        private IEnumerable<VwContractorsiteBase> _sites;
        



        private RelayCommand<int> _submitCommand;
        public RelayCommand<int> SubmitCommand => _submitCommand ?? (_submitCommand = new RelayCommand<int>(Submit));

        

        public ContractorViewModel()
        {
            
        }


        private async void Submit(int submitCode)
        {
            try
            {
                if (_contractor.ContractStatus == ContractStatusCodes.Explorating)
                {
                    if (_selectedItem == null)
                        return;
                    await ContractWebService.RequestGetMethod<JObject>(null, "/api/contractor/connectsitetocustomer", new { userid = Contractor.UserId, siteid = _selectedItem.SiteId });
                }
                if (submitCode == 0)
                    return;
                ContractStatusCodes newstatus = ContractStatusCodes.None;
                if (submitCode == 1)
                    newstatus = (ContractStatusCodes)((int)_contractor.ContractStatus) + 1;
                else if (submitCode == 2)
                    newstatus = (ContractStatusCodes)((int)_contractor.ContractStatus) - 1;
                else if(submitCode == 3)
                    newstatus = ContractStatusCodes.Cancellations;
                await ContractWebService.RequestPostMethod(null, "/api/aggregator/submituserstatus", 
                    new { comment = Comment, notification = AllNotify,userid = _contractor.UserId, reason = CommitReason, status = newstatus });
                _contractor.ContractStatus = newstatus;
               


                if (ShownContractorApprovalView != null)
                    ShownContractorApprovalView.Close();
            }
            catch(Exception ex)
            {

            }
                
        }
    }
}
