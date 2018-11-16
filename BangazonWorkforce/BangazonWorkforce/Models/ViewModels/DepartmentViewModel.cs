using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace BangazonWorkforce.Models
{
    public class DepartmentViewModel
    {

        public Department department { get; set; }

        public int employeeCount { get; set; }

    }
}
