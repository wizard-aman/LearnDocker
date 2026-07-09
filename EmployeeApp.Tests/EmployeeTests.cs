using System.Net;
using System.Net.Http.Json;
using EmployeeApp.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EmployeeApp.Tests;

public class EmployeeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EmployeeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ── GET ALL ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithEmployees()
    {
        var response = await _client.GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeResponseDto>>();
        employees.Should().NotBeNull();
        employees!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAll_ResponseContainsDepartmentName()
    {
        var response = await _client.GetAsync("/api/employees");
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeResponseDto>>();

        // Response mein DepartmentName bhi aana chahiye (join ka result)
        employees!.All(e => !string.IsNullOrEmpty(e.DepartmentName)).Should().BeTrue();
    }

    // ── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingEmployee_ReturnsEmployee()
    {
        var response = await _client.GetAsync("/api/employees/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var emp = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>();
        emp!.Name.Should().Be("Aman Sharma");
        emp.DepartmentName.Should().Be("Engineering");
    }

    [Fact]
    public async Task GetById_InvalidId_Returns404()
    {
        var response = await _client.GetAsync("/api/employees/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET BY DEPARTMENT ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetByDepartment_ValidDept_ReturnsEmployees()
    {
        var response = await _client.GetAsync("/api/employees/by-department/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeResponseDto>>();
        employees!.All(e => e.DepartmentId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GetByDepartment_InvalidDept_Returns404()
    {
        var response = await _client.GetAsync("/api/employees/by-department/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidEmployee_Returns201()
    {
        var dto = new CreateEmployeeDto(
            "Rahul Verma", "rahul@company.com", "Designer", 60000, 1);

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var emp = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>();
        emp!.Name.Should().Be("Rahul Verma");
        emp.DepartmentName.Should().Be("Engineering");
    }

    [Fact]
    public async Task Create_InvalidDepartment_Returns400()
    {
        var dto = new CreateEmployeeDto("Test", "test@test.com", "Dev", 50000, 9999);

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_DuplicateEmail_Returns409()
    {
        // aman@company.com already exists in seed data
        var dto = new CreateEmployeeDto("Another Aman", "aman@company.com", "Dev", 50000, 1);

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_NegativeSalary_Returns400()
    {
        var dto = new CreateEmployeeDto("Test", "newtest@test.com", "Dev", -1000, 1);

        var response = await _client.PostAsJsonAsync("/api/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingEmployee_ReturnsUpdated()
    {
        var created = await _client.PostAsJsonAsync("/api/employees",
            new CreateEmployeeDto("Update Me", "updateme@test.com", "Tester", 45000, 1));
        var emp = await created.Content.ReadFromJsonAsync<EmployeeResponseDto>();

        var response = await _client.PutAsJsonAsync($"/api/employees/{emp!.Id}",
            new UpdateEmployeeDto("Updated Name", "updateme@test.com", "Sr. Tester", 55000, 2));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>();
        updated!.Position.Should().Be("Sr. Tester");
        updated.Salary.Should().Be(55000);
        updated.DepartmentName.Should().Be("HR"); // moved to dept 2
    }

    // ── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingEmployee_ReturnsOk_ThenNotFound()
    {
        var created = await _client.PostAsJsonAsync("/api/employees",
            new CreateEmployeeDto("Delete Me", "deleteme@test.com", "Intern", 30000, 1));
        var emp = await created.Content.ReadFromJsonAsync<EmployeeResponseDto>();

        // Delete karo
        var deleteResponse = await _client.DeleteAsync($"/api/employees/{emp!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Ab get karo — 404 aana chahiye
        var getResponse = await _client.GetAsync($"/api/employees/{emp.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
