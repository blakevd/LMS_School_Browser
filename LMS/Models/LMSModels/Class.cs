using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrolleds = new HashSet<Enrolled>();
        }

        public uint? Year { get; set; }
        public string? Semester { get; set; }
        public string? Location { get; set; }
        public TimeOnly? Start { get; set; }
        public TimeOnly? End { get; set; }
        public int ClassId { get; set; }
        public int? CourseId { get; set; }
        public string ProfId { get; set; } = null!;

        public virtual Course? Course { get; set; }
        public virtual Professor Prof { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
    }
}
