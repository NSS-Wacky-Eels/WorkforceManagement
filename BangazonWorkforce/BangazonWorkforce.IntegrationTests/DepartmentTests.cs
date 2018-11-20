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
            Department department = (await GetAllDepartments()).First();
            string url = "/department";

            string firstDepoName = department.Name;
            int firstDepoBudget = department.Budget;
            int firstDepoTotalEmp = department.TotalEmployees;

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
            IHtmlDocument indexPage = await HtmlHelpers.GetDocumentAsync(response);
            var firstRow = indexPage.QuerySelector("tbody tr:first-child");



            Assert.Contains(firstRow.QuerySelectorAll("td"),
            td => td.TextContent.Contains(firstDepoName));
            Assert.Contains(firstRow.QuerySelectorAll("td"),
            td => td.TextContent.Contains(firstDepoBudget.ToString()));
            Assert.Contains(firstRow.QuerySelectorAll("td"),
            td => td.TextContent.Contains(firstDepoTotalEmp.ToString()));


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

        private async Task<List<Department>> GetAllDepartments()
        {
            using (IDbConnection conn = new SqlConnection(Config.ConnectionSring))
            {
                IEnumerable<Department> allDepartments =
                    await conn.QueryAsync<Department>(@"SELECT Id, Name, Budget FROM Department");
                return allDepartments.ToList();
            }
        }

    }
}
