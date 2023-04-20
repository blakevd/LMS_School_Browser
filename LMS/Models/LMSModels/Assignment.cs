using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignment
    {
        public Assignment()
        {
            Submissions = new HashSet<Submission>();
        }

        public string? Name { get; set; }
        public string? Contents { get; set; }
        public uint? Points { get; set; }
        public DateTime? Due { get; set; }
        public int CategoryId { get; set; }
        public int AssignmentId { get; set; }

        public virtual AssignmentCategory Category { get; set; } = null!;
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
