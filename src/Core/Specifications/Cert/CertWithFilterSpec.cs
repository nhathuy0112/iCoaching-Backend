using Core.Entities;
using Core.Entities.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Specifications.Cert
{
    public class CertWithFilterSpec : Specification<CertificateSubmission>
    {
        public CertWithFilterSpec(PaginationParam param, CertStatus? status, bool isFilter) : 
            base(c => 
                (string.IsNullOrEmpty(param.Search) ||
                c.Coach.UserName.ToLower().Contains(param.Search) ||
                c.Coach.Fullname.ToLower().Contains(param.Search) ||
                c.Coach.Email.ToLower().Contains(param.Search)) && 
                (!status.HasValue || c.Status == status))
        {
            AddInclude(c => c.Coach);
            if (!isFilter) return;
            AddOrderBy(c => c.Id);
            ApplyPaging(param.PageSize * (param.PageIndex - 1),
                param.PageSize);
            if (string.IsNullOrEmpty(param.Sort)) return;
            switch (param.Sort)
            {
                case "fullnameAsc":
                    AddOrderBy(u => u.Coach.Fullname);
                    break;
                case "fullnameDesc":
                    AddOrderByDescending(u => u.Coach.Fullname);
                    break;
                default:
                    AddOrderBy(u => u.Coach.Fullname);
                    break;
            }
        }
    }
}
