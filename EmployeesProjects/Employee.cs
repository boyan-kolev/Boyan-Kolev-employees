using System;
using System.Collections.Generic;
using System.Text;

namespace EmployeesProjects
{
    public class Employee
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
    }
}
