﻿using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Professor
    {
        public Professor()
        {
            Classes = new HashSet<Class>();
        }

        public string UId { get; set; } = null!;
        public string? FName { get; set; }
        public string? LName { get; set; }
        public DateOnly? Dob { get; set; }
        public string Department { get; set; } = null!;

        public virtual Department DepartmentNavigation { get; set; } = null!;
        public virtual ICollection<Class> Classes { get; set; }
    }
}
