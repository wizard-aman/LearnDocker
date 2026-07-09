using System.Net;
using System.Net.Http.Json;
using EmployeeApp.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EmployeeApp.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// INTEGRATION TESTS — WebApplicationFactory
//
// WebApplicationFactory kya karta hai?
// → Real ASP.NET Core app memory mein spin up karta hai
// → Actual HTTP calls karo — controllers, middleware, DB sab real hai
// → Koi mock nahi — actual code test hota hai
// → Koi server start karne ki zaroorat nahi — tests mein hi sab hota hai
//
// IClassFixture<WebApplicationFactory<Program>>
// → Ek hi factory instance share hoti hai sabhi tests mein (performance)
// ─────────────────────────────────────────────────────────────────────────────
public class DepartmentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DepartmentTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ── GET ALL ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk_WithSeededData()
    {
        var response = await _client.GetAsync("/api/departments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var departments = await response.Content.ReadFromJsonAsync<List<DepartmentResponseDto>>();
        departments.Should().NotBeNull();
        departments!.Count.Should().BeGreaterThan(0); // seed data hai
    }

    // ── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_ReturnsDepartment()
    {
        // Seed data mein Id=1 Engineering hai
        var response = await _client.GetAsync("/api/departments/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dept = await response.Content.ReadFromJsonAsync<DepartmentResponseDto>();
        dept!.Name.Should().Be("Engineering");
    }

    [Fact]
    public async Task GetById_InvalidId_Returns404()
    {
        var response = await _client.GetAsync("/api/departments/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidDepartment_Returns201()
    {
        var dto = new CreateDepartmentDto("Marketing", "Bangalore");

        var response = await _client.PostAsJsonAsync("/api/departments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<DepartmentResponseDto>();
        created!.Name.Should().Be("Marketing");
        created.Location.Should().Be("Bangalore");
        created.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var dto = new CreateDepartmentDto("", "Bangalore");

        var response = await _client.PostAsJsonAsync("/api/departments", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingDepartment_ReturnsUpdated()
    {
        // Pehle create karo
        var created = await _client.PostAsJsonAsync("/api/departments",
            new CreateDepartmentDto("OldName", "OldCity"));
        var dept = await created.Content.ReadFromJsonAsync<DepartmentResponseDto>();

        // Phir update karo
        var response = await _client.PutAsJsonAsync($"/api/departments/{dept!.Id}",
            new UpdateDepartmentDto("NewName", "NewCity"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<DepartmentResponseDto>();
        updated!.Name.Should().Be("NewName");
        updated.Location.Should().Be("NewCity");
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_EmptyDepartment_ReturnsOk()
    {
        // Naya empty department banao
        var created = await _client.PostAsJsonAsync("/api/departments",
            new CreateDepartmentDto("ToDelete", "Nowhere"));
        var dept = await created.Content.ReadFromJsonAsync<DepartmentResponseDto>();

        var response = await _client.DeleteAsync($"/api/departments/{dept!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_DepartmentWithEmployees_Returns400()
    {
        // Engineering (Id=1) mein seed employees hain — delete nahi hona chahiye
        var response = await _client.DeleteAsync("/api/departments/1");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── HEALTH CHECK ─────────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
