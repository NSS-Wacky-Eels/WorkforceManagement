using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class DepartmentDetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Budget { get; set; }

        public List<Employee> AllEmployees { get; set; } = new List<Employee>();

    }
}
