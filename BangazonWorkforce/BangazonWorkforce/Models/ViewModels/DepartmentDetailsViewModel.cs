using System;
using System.Collections.Generic;

namespace BangazonWorkforce.Models.ViewModels
{
    public class DepartmentDetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Budget { get; set; }

        public List<Employee> Employees { get; set; }
    }
}
