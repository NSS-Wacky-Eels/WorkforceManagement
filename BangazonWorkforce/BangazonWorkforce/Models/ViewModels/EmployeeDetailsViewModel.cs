﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


//Author: Kayla Reid
//This ViewModel show and employee with department, computer and training program details
namespace BangazonWorkforce.Models.ViewModels
{
    public class EmployeeDetailsViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DepartmentName { get; set; }

        public string ComputerMake { get; set; }

        public string ComputerManufacturer { get; set; }

        public List<TrainingProgram> TrainingPrograms = new List<TrainingProgram>();
    }
}
