using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class ComputerEmployee
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int ComputerId { get; set; }

        public List<Employee> EmployeeList { get; set; } = new List<Employee>();

        public List<Computer> ComputerList { get; set; } = new List<Computer>();
    }
}
