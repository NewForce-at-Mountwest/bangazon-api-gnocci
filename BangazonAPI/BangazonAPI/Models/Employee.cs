
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Employee
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int DepartmentId { get; set; }

        public bool IsSuperViser { get; set; }

        //------------------Referenct to Department Name Ticket #6--------------------//
        public Department CurrentDepartment { get; set; }

        //-------------------Reference to Most Recent Computer Ticket #6-----------------//
        public Computer AssignedComputer { get; set; }

    }
}
