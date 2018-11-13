using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class EmployeeDetailsViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Department department { get; set; }
    }
}
