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
        private readonly IConfiguration _config;

        public Employee Employee { get; set; }

        public List<SelectListItem> Computers { get; set; }

        public List<SelectListItem> Departments { get; set; }

        public List<SelectListItem> TrainingPrograms { get; set; }

        public EmployeeEditViewModel() { }

        public EmployeeEditViewModel(IConfiguration config)
        {

            using (IDbConnection conn = new SqlConnection(config.GetConnectionString("DefaultConnection")))
            {
                Computers = conn.Query<Computer>(@"
                    SELECT Id, Name FROM Computer;
                ")
                .Select(li => new SelectListItem
                {
                    Text = li.Make,
                    Value = li.Id.ToString()
                }).ToList();
                ;
                Computers = conn.Query<Department>(@"
                    SELECT Id, Name FROM Department;
                ")
                .Select(li => new SelectListItem
                {
                    Text = li.Name,
                    Value = li.Id.ToString()
                }).ToList();
                ;
                Computers = conn.Query<TrainingProgram>(@"
                    SELECT Id, 
                            Name,
                            StartDate
                     FROM TrainingProgram tp
                     WHERE StartDate >= CONVERT(DATETIME, {fn CURDATE()});
                ")
                .Select(li => new SelectListItem
                {
                    Text = li.Name,
                    Value = li.Id.ToString()
                }).ToList();
                ;
            }
        }
    }
}
