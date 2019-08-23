using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models
{
    public class CommonClaimTypes
    {
        public const string Issuer = "LOCAL AUTHORITY";
        public const string CREATE_CUSTOMER_INFO_CLAIM = "https://www.peiu.co.kr/claims/2019/08/create/customer_info";
        public const string READ_CUSTOMER_INFO_CLAIM = "https://www.peiu.co.kr/claims/2019/08/read/customer_info";
        public const string UPDATE_CUSTOMER_INFO_CLAIM = "https://www.peiu.co.kr/claims/2019/08/update/customer_info";
        public const string DELETE_CUSTOMER_INFO_CLAIM = "https://www.peiu.co.kr/claims/2019/08/delete/customer_info";
    }

    public enum CrudClaims
    {
        NONE = 0,
        CREATE = 8,
        READ = 1,
        UPDATE = 2,
        DELETE = 4
    }
}
