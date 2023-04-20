using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public DateTime? Time { get; set; }
        public uint? Score { get; set; }
        public string? Contents { get; set; }
        public string? UId { get; set; }
        public int? AssignmentId { get; set; }
        public int SubmissionId { get; set; }

        public virtual Assignment? Assignment { get; set; }
        public virtual Student? UIdNavigation { get; set; }
    }
}
