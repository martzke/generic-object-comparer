using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compare.Tests.Entities
{
    public class WorkInfo
    {
        public Occupation Occupation { set; get; }
        public int YearsOfExperience { set; get; }
        public int CurrentSalary { set; get; }
        public Company Employer { set; get; }
    }
}
