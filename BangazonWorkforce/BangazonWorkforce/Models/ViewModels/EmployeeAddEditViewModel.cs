using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace BangazonWorkforce.Models
{
    public class EmployeeAddEditViewModel
    {
        public Employee Employee { get; set; }
        public List<Department> AllDepartments { get; set; }
        public List<SelectListItem> AllDepartmentOptions
        {
            get
            {
                if (AllDepartments == null)
                {
                    return null;
                }

/*
                SelectListItem item = new SelectListItem{
                    Text = "Choose Depo",
                    Value = "0"
                }; */
               
               var allDepoList = AllDepartments
                        .Select((d, id) => new SelectListItem(d.Name, d.Id.ToString())).ToList();


                allDepoList.Insert(0, new SelectListItem {
                    Text = "Choose Depo",
                    Value = "0"
                });

                return allDepoList;

            }
        }

    }
}
