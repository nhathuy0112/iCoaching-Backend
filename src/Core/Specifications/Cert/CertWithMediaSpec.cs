using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Specifications.Cert
{
    public class CertWithMediaSpec : Specification<CertificateSubmission>
    {
        public CertWithMediaSpec(string userId) : base(c => c.CoachId == userId)
        {
            AddInclude(c => c.MediaAssets.Where(m => m.CertificateSubmissionId.HasValue));
        }
        public CertWithMediaSpec(int certId) : base(c => c.Id == certId)
        {
            AddInclude("Coach.MediasAssets");
            AddInclude(c => c.MediaAssets);
        }
    }
}
