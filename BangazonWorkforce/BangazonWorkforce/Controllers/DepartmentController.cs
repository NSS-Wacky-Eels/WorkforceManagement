﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using BangazonWorkforce.Models;
using BangazonWorkforce.Models.ViewModels;

namespace BangazonWorkforce.Controllers
{
    public class DepartmentController : Controller
    {
        private IConfiguration _config;
        private IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public DepartmentController(IConfiguration config)
        {
            _config = config;
        }

        
        public async Task<IActionResult> Index()
        {

            using (IDbConnection conn = Connection)
            {

                string sql = @"SELECT d.Id, 
                                        d.Name, 
                                        d.Budget, 
                                        count(e.Id) TotalEmployees
                                   FROM Department d
                                   left join Employee e on d.Id = e.DepartmentId 
                                   group by d.Id, d.Name, d.Budget
                                   ";


                IEnumerable<Department> depoWithEmpCount = await conn.QueryAsync<Department>(sql);
                DepartmentViewModel model = new DepartmentViewModel();

                model.departments = depoWithEmpCount.ToList();
                return View(model);
            }
        } 

        // Details Page
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (IDbConnection conn = Connection)
            {
                string sql = $@"
                     SELECT
                            d.Id,
                            d.[Name],
                            d.Budget,
                            e.Id,
                            e.FirstName,
                            e.LastName,
                            e.DepartmentId,
                            e.IsSuperVisor
                        FROM Department d
                        LEFT JOIN Employee e ON e.DepartmentId = d.Id 
                        WHERE d.Id = {id}                   
                    ";
                DepartmentDetailsViewModel model = new DepartmentDetailsViewModel();

                IEnumerable<DepartmentDetailsViewModel> queriedDepartment = await conn.QueryAsync<DepartmentDetailsViewModel, Employee, DepartmentDetailsViewModel>(
                    sql,
                    (dept, emp) =>
                    {

                        if (model.Name == null)
                        {
                            model.Name = dept.Name;
                            model.Budget = dept.Budget;
                        }

                        if (!model.AllEmployees.Contains(emp))
                        {
                            model.AllEmployees.Add(emp);
                        }

                        return dept;
                    });
                return View(model);
            }
        }

        // GET: Department/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Department/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name, Budget")] Department department)
        {
            if (!ModelState.IsValid)
            {
                return View(department);
            }

            using (IDbConnection conn = Connection)
            {
                string sql = $@"INSERT INTO Department (Name, Budget) 
                                     VALUES ('{department.Name}', {department.Budget});";

                await conn.ExecuteAsync(sql);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Department/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Department department = await GetById(id.Value);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Department/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(department);
            }

            using (IDbConnection conn = Connection)
            {
                string sql = $@"UPDATE Department 
                                   SET Name = '{department.Name}', 
                                       Budget = {department.Budget}
                                 WHERE id = {id}";

                await conn.ExecuteAsync(sql);
                return RedirectToAction(nameof(Index));
            }
        }


        // GET: Department/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Department department = await GetById(id.Value);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Department/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"DELETE FROM Department WHERE id = {id}";
                int rowsDeleted = await conn.ExecuteAsync(sql);
                
                if (rowsDeleted > 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                return NotFound();
            }
        }


        private async Task<Department> GetById(int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"SELECT Id, Name, Budget 
                                  FROM Department
                                 WHERE id = {id}";

                IEnumerable<Department> departments = await conn.QueryAsync<Department>(sql);
                return departments.SingleOrDefault();
            }
        }
    }
}
