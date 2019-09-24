using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public enum ContractStatusCodes : int
    {
        None = 0,
        /// <summary>
        /// 가입신청중
        /// </summary>
        Signing = 1, 

        /// <summary>
        /// 고객과 상담중
        /// </summary>
        Cantacting = 2,

        /// <summary>
        /// 현장 답사 상태
        /// </summary>
        Explorating = 3,

        /// <summary>
        /// 최종승인 대기
        /// </summary>
        WaitingForApprval = 4,


        /// <summary>
        /// 활동중 (가입된 상태, 유효상태)
        /// </summary>
        Activating = 5,

        /// <summary>
        /// 정지(경고 및 개인 사유로 인한 거래 정지)
        /// </summary>
        Deactivating = 128,

        /// <summary>
        /// 취소, 탈퇴
        /// </summary>
        Cancellations = 1024
    }

    public enum AssetTypeCodes : int
    {
        PCS,
        BMS,
        PV
    }

    public enum RegisterType : int
    {
        
        Contrator,
        Aggregator,
        Supervisor,
        Candidate
    }


    public enum ServiceCodes : int
    {
        NoService = 0,
        DR = 1,
        Schedule = 2,
    }
}
