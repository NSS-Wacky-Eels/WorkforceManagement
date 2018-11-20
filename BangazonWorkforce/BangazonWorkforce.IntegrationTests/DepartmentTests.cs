using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using BangazonWorkforce.IntegrationTests.Helpers;
using BangazonWorkforce.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BangazonWorkforce.IntegrationTests
{
    public class DepartmentTests :
        IClassFixture<WebApplicationFactory<BangazonWorkforce.Startup>>
    {
        private readonly HttpClient _client;

        public DepartmentTests(WebApplicationFactory<BangazonWorkforce.Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_IndexReturnsSuccessAndCorrectContentType()
        {
            // Arrange
            string url = "/department";
            
            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task Post_CreateAddsDepartment()
        {
            // Arrange
            string url = "/department/create";
            HttpResponseMessage createPageResponse = await _client.GetAsync(url);
            IHtmlDocument createPage = await HtmlHelpers.GetDocumentAsync(createPageResponse);

            string newDepartmentName = StringHelpers.EnsureMaxLength("Dept-" + Guid.NewGuid().ToString(), 55);
            string newDepartmentBudget = new Random().Next().ToString();


            // Act
            HttpResponseMessage response = await _client.SendAsync(
                createPage,
                new Dictionary<string, string>
                {
                    {"Name", newDepartmentName},
                    {"Budget", newDepartmentBudget}
                });


            // Assert
            response.EnsureSuccessStatusCode();

            IHtmlDocument indexPage = await HtmlHelpers.GetDocumentAsync(response);
            Assert.Contains(
                indexPage.QuerySelectorAll("td"), 
                td => td.TextContent.Contains(newDepartmentName));
            Assert.Contains(
                indexPage.QuerySelectorAll("td"), 
                td => td.TextContent.Contains(newDepartmentBudget));
        }

        // This gets all the employees and passes it to Get_DeptDisplayEmployees() below.
        private async Task<List<Employee>> AllEmployees()
        {
            using (IDbConnection conn = new SqlConnection(Config.ConnectionSring))
            {
                IEnumerable<Employee> allEmployees =
                    await conn.QueryAsync<Employee>(@"SELECT Id, FirstName, LastName, 
                                                              IsSupervisor, DepartmentId 
                                                         FROM Employee
                                                     ORDER BY Id");
                return allEmployees.ToList();
            }
        }

        [Fact]
        public async Task Get_DeptDisplayEmployees()
        {

            // Arrange
            // Creates variables to represent data to be tested

            Employee employee = (await AllEmployees()).Last();

            string url = $"/department/details/1";
            string employeeFirstName = employee.FirstName;
            string employeeLastName = employee.LastName;

            // Act
            // Gets HTTP response for data represented above

            HttpResponseMessage response = await _client.GetAsync(url);


            // Assert
            // Checks if there is any data represented on details 
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());


            // Checks if data displayed represents data in database
            IHtmlDocument detailPage = await HtmlHelpers.GetDocumentAsync(response);
            IHtmlCollection<IElement> viewData = detailPage.QuerySelectorAll("dd");
            Assert.Contains(
                viewData, 
                dd => dd.TextContent.Trim() == "Navy");
            IHtmlCollection<IElement> lis = detailPage.QuerySelectorAll("li");
            Assert.Contains(
                lis, 
                li => li.TextContent.Trim() == employee.FirstName + " " + employee.LastName);
        }

    }
}
