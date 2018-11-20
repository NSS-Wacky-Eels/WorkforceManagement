using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonWorkforce.Models;
using BangazonWorkforce.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace BangazonWorkforce.Controllers
{
    public class EmployeeController : Controller
    {
        private IConfiguration _config;
        private IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public EmployeeController(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            using (IDbConnection conn = Connection)
            {
                string sql = @"
                    SELECT e.Id, 
                        e.FirstName,
                        e.LastName, 
                        e.IsSupervisor,
                        e.DepartmentId,
                        d.Id,
                        d.Name,
                        d.Budget
                    FROM Employee e JOIN Department d on e.DepartmentId = d.Id
                    ORDER BY e.Id";
                IEnumerable<Employee> employees = await conn.QueryAsync<Employee, Department, Employee>(
                    sql,
                    (employee, department) => {
                        employee.Department = department;
                        return employee;
                    });

                return View(employees);
            }
        }
        //Author: Kayla Reid
            //Description:
            //Returns a employee with department, computer, and training programs details from the database
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee employee = await GetById(id.Value);
            if (employee == null)
            {
                return NotFound();
            }
            using (IDbConnection conn = Connection)
            {
                string sql = $@"
                     SELECT
                            e.Id,
                            e.FirstName,
                            e.LastName,
                            e.DepartmentId,
                            e.IsSuperVisor,
                            c.Id,
                            c.Manufacturer,
                            c.Make,
                            c.PurchaseDate,
                            c.DecomissionDate,
                            d.Id,
                            d.[Name],
                            tp.Id,
                            tp.[Name],
                            tp.StartDate,
                            tp.EndDate,
                            tp.MaxAttendees
                        FROM Employee e
                        LEFT JOIN ComputerEmployee ce ON ce.EmployeeId = e.Id
                        LEFT JOIN Computer c ON c.Id = ce.ComputerId
                        LEFT JOIN EmployeeTraining empT ON empT.EmployeeId = e.Id 
                        LEFT JOIN TrainingProgram tp ON tp.Id = empT.TrainingProgramId
                        JOIN Department d ON d.Id = e.DepartmentId 
                        WHERE e.Id = {id}                   
                    ";

                EmployeeDetailsViewModel model = new EmployeeDetailsViewModel();

                IEnumerable<Employee> queriedEmployee = await conn.QueryAsync<Employee, Computer, Department, TrainingProgram, Employee>(
                    sql,
                    (emp, computer, department, trainingProgram) =>
                    {

                        if (model.DepartmentName == null)
                        {
                            model.Id = emp.Id;
                            model.FirstName = emp.FirstName;
                            model.LastName = emp.LastName;
                            model.DepartmentName = department.Name;
                        }

                        if (computer != null)
                        {
                            model.ComputerMake = computer.Make;
                            model.ComputerManufacturer = computer.Manufacturer;
                        }
                  
                        if (!model.TrainingPrograms.Contains(trainingProgram))
                        {
                            model.TrainingPrograms.Add(trainingProgram);
                        }
          
                    return emp;
                    });
          
                return View(model);
            }
        }

        // GET: Employee/Create
        public async Task<IActionResult> Create()
        {
            List<Department> allDepartments = await GetAllDepartments();
            EmployeeAddEditViewModel viewmodel = new EmployeeAddEditViewModel
            {
                AllDepartments = allDepartments
            };
            return View(viewmodel);
        }

        // POST: Employee/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeAddEditViewModel viewmodel)
        {
            if (!ModelState.IsValid)
            {
                List<Department> allDepartments = await GetAllDepartments();
                viewmodel.AllDepartments = allDepartments;
                return View(viewmodel);
            }

            Employee employee = viewmodel.Employee;

            using (IDbConnection conn = Connection)
            {
                string sql = $@"INSERT INTO Employee (
                                    FirstName, LastName, IsSupervisor, DepartmentId
                                ) VALUES (
                                    '{employee.FirstName}', '{employee.LastName}',
                                    {(employee.IsSupervisor ? 1 : 0)}, {employee.DepartmentId}
                                );";

                await conn.ExecuteAsync(sql);
                return RedirectToAction(nameof(Index));
            }
        }

        /*
            Author: Kayla Reid and Taylor Gulley
            Description: Gets the relavent infomation for the Employee to Edit. This includes the departments, training programs computers.
        */

        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            List<Department> allDepartments = await GetAllDepartments();

            List<Computer> allComputers = await GetAllAvailableComputers();

            List<TrainingProgram> allTrainingPrograms = await GetAllTrainingPrograms();

            List<TrainingProgram> employeeTrainingPrograms = await GetAllChosenTrainingPrograms(id.Value);

            Employee employee = await GetById(id.Value);

            Computer computer = await GetEmployeeComputer(id.Value);

            if (employee == null)
            {
                return NotFound();
            }

            EmployeeEditViewModel viewmodel = new EmployeeEditViewModel
            {
                Employee = employee,
                AllDepartments = allDepartments,
                AllComputers = allComputers,
                AllTrainingPrograms = allTrainingPrograms
            };

            if (computer != null)
            {
                viewmodel.Computer = computer;
            }

            if (employeeTrainingPrograms != null)
            {
                viewmodel.EmployeeChosenTrainingPrograms = employeeTrainingPrograms;
            }

            return View(viewmodel);
        }

        // POST: Employee/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeEditViewModel viewmodel)
        {
            if (id != viewmodel.Employee.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                List<Department> allDepartments = await GetAllDepartments();
                viewmodel.AllDepartments = allDepartments;
                return View(viewmodel);
            }
            //getting employee from view and setting it to this employee instance
            Employee employee = viewmodel.Employee;

            List<int> employeeTrainingPrograms = viewmodel.SelectedTrainingProgramsIds;

            //Get the selected computer
            Computer computer = viewmodel.Computer;

            //get the employee computer
            Computer employeeComputer = await GetEmployeeComputer(employee.Id);

            using (IDbConnection conn = Connection)
            {
                string sql = $@"UPDATE Employee 
                                   SET FirstName = '{employee.FirstName}', 
                                       LastName = '{employee.LastName}', 
                                       IsSupervisor = {(employee.IsSupervisor ? 1 : 0)},
                                       DepartmentId = {employee.DepartmentId}
                                 WHERE id = {id}";
                 //setting empty var
                string computerResetSql = "";
                //setting empty var
                string computerAddSql = "";

                if (computer.Id != 0 && computer.Id != employee.Id)
                {   
                    //building up var based on if statement
                    computerResetSql = $@"DELETE FROM ComputerEmployee WHERE EmployeeId = {employee.Id};";
                    //building up var based on if statement
                    computerAddSql = $@"INSERT INTO ComputerEmployee
                                      (ComputerId, EmployeeId, AssignDate)
                                      VALUES
                                      ({ computer.Id }, { employee.Id }, '{DateTime.Now}');";
                }

                string trainingResetSql = $@"DELETE FROM EmployeeTraining WHERE EmployeeId = {employee.Id};";
                //setting empty var
                string trainingAddSql = "";

                if (employeeTrainingPrograms != null)
                { 
                    foreach (int num in employeeTrainingPrograms)
                    {
                        //foreach training Program selected build up this var to hold all the needed slq insert statments
                        trainingAddSql = trainingAddSql + $@"
                        INSERT INTO EmployeeTraining
                        (EmployeeId, TrainingProgramId)
                        VALUES 
                        ({ employee.Id }, { num });";
                    }
                }

                //add up all the sql statements and send them off in one go to the db
                sql = sql + computerResetSql + computerAddSql + trainingResetSql + trainingAddSql;

                await conn.ExecuteAsync(sql);

                return RedirectToAction(nameof(Index));
            }
        }


        // GET: Employee/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee employee = await GetById(id.Value);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"DELETE FROM Employee WHERE id = {id}";
                await conn.ExecuteAsync(sql);
                return RedirectToAction(nameof(Index));
            }
        }


        private async Task<Employee> GetById(int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"SELECT e.Id, 
                                       e.FirstName,
                                       e.LastName, 
                                       e.IsSupervisor,
                                       e.DepartmentId,
                                       d.Id,
                                       d.Name,
                                       d.Budget
                                  FROM Employee e JOIN Department d on e.DepartmentId = d.Id
                                 WHERE e.id = {id}";
                IEnumerable<Employee> employees = await conn.QueryAsync<Employee, Department, Employee>(
                    sql,
                    (employee, department) => {
                        employee.Department = department;
                        return employee;
                    });

                return employees.SingleOrDefault();
            }
        }

        private async Task<List<Department>> GetAllDepartments()
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"SELECT Id, Name, Budget FROM Department";

                IEnumerable<Department> departments = await conn.QueryAsync<Department>(sql);
                return departments.ToList();
            }
        }

        /*
            Author: Kayla Reed and Taylor Gulley
            Description: A private function using a sql statment to return computers that are not assigned to an employee
        */
        private async Task<List<Computer>> GetAllAvailableComputers()
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"SELECT c.Id,
                                        c.Make
                                FROM Computer c
                                LEFT JOIN ComputerEmployee ce ON c.Id = ce.ComputerId
                                WHERE ce.ComputerId IS NULL
                                AND c.DecomissionDate IS NULL";

                IEnumerable<Computer> computers = await conn.QueryAsync<Computer>(sql);
                return computers.ToList();
            }
        }

        /*
            Author: Kayla Reed and Taylor Gulley
            Description: A private function using a sql statment to return the computer assigned to the employee
        */
        private async Task<Computer> GetEmployeeComputer(int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"SELECT c.Id,
                                        c.Make
                                FROM Employee e 
                                LEFT JOIN ComputerEmployee ce ON ce.EmployeeId = e.Id
                                LEFT JOIN Computer c ON c.Id = ce.ComputerId
                                WHERE e.Id = {id}";

                Computer computer = await conn.QueryFirstAsync<Computer>(sql);
                return computer;
            }
        }

        /*private async Task<Computer> GetComputerById(int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"SELECT c.Id,
                                        c.Make,
                                FROM Computer c
                                WHERE c.Id = {id}";

                Computer computer = await conn.QueryFirstAsync<Computer>(sql);
                return computer;
            }
        }*/

        /*
            Author: Kayla Reed and Taylor Gulley
            Description: A private function using a sql statment to return all training programs
        */
        private async Task<List<TrainingProgram>> GetAllTrainingPrograms()
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"SELECT tp.Id, 
                                        tp.Name, 
                                        tp.StartDate
                                FROM TrainingProgram tp";

                IEnumerable<TrainingProgram> trainingPrograms = await conn.QueryAsync<TrainingProgram>(sql);
                return trainingPrograms.ToList();
            }
        }

        /*
            Author: Kayla Reed and Taylor Gulley
            Description: A private function using a sql statment to return training programs that the employee has chosen
        */
        private async Task<List<TrainingProgram>> GetAllChosenTrainingPrograms(int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"SELECT tp.Id, 
                                        tp.Name, 
                                        tp.StartDate
                                FROM TrainingProgram tp
                                JOIN EmployeeTraining et ON et.TrainingProgramId = tp.Id
                                JOIN Employee e ON e.Id = et.EmployeeId
                                WHERE e.Id = {id}";

                IEnumerable<TrainingProgram> trainingPrograms = await conn.QueryAsync<TrainingProgram>(sql);
                return trainingPrograms.ToList();
            }
        }
    }
}