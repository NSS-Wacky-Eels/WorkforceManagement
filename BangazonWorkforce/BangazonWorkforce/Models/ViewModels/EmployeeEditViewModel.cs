using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonWorkforce.Controllers;

namespace BangazonWorkforce.Models.ViewModels
{
    public class EmployeeEditViewModel
    {

        public Employee Employee { get; set; }

        // List of all computer to select new computer
        public List<Computer> AllComputers { get; set; }
        //Currently assigned computer
        public Computer Computer { get; set; }

        public List<Department> AllDepartments { get; set; }
        //List of all training programs
        public List<TrainingProgram> AllTrainingPrograms { get; set; }
        //List of the training programs the employee has chosen
        public List<TrainingProgram> EmployeeChosenTrainingPrograms { get; set; }
        // List of the selected Training Programs Ids
        public List<int> SelectedTrainingProgramsIds { get; set; }
        // List of already chosen Training Programs Ids by the employee
        public List<int> PreSelectedTrainingPrograms { get; set; }

        //public EmployeeEditViewModel() { }

        public List<SelectListItem> AllDepartmentOptions
        {
            get
            {
                if (AllDepartments == null)
                {
                    return null;
                }

                return AllDepartments
                    .Select((d) => new SelectListItem(d.Name, d.Id.ToString()))
                    .ToList();
            }
        }

        public List<SelectListItem> AllComputerOptions
        {
            get
            {
                if (AllComputers == null)
                {
                    return null;
                }
                // Makes a new empty list 
                List<SelectListItem> ComputerOptions = new List<SelectListItem>();
                // Sets the data in the list to hold only Makes and Ids
                ComputerOptions = AllComputers
                    .Select((c) => new SelectListItem(c.Make, c.Id.ToString()))
                    .ToList();
                // If the Employee doesn't have a computer assigned then set position 0 to Assign a Computer
                if (Computer.Id == 0)
                {
                    ComputerOptions.Insert(0, new SelectListItem
                    {
                        Text = "Assign a Computer",
                        Value = "0"
                    });
                }

                List<string> SelectItemValues = new List<string>();

                foreach (SelectListItem sli in ComputerOptions)
                {
                    SelectItemValues.Add(sli.Value);
                }

                // If the Employee has a computer Id assigned place that computer in the first position
                if (!SelectItemValues.Contains(Computer.Id.ToString()))
                {
                    ComputerOptions.Insert(0, new SelectListItem
                    {
                        Text = Computer.Make,
                        Value = Computer.Id.ToString()
                    });
                }

                return ComputerOptions;
            }
        }


    }
}
