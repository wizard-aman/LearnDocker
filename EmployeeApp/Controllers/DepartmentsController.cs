using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeApp.Data;
using EmployeeApp.Models;

namespace EmployeeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public DepartmentsController(AppDbContext db) => _db = db;

    /// <summary>Get all departments with employee count</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _db.Departments
            .Include(d => d.Employees)
            .Select(d => new DepartmentResponseDto(
                d.Id, d.Name, d.Location, d.Employees.Count))
            .ToListAsync();

        return Ok(departments);
    }

    /// <summary>Get department by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dept = await _db.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dept is null)
            return NotFound(new { message = $"Department {id} not found" });

        return Ok(new DepartmentResponseDto(dept.Id, dept.Name, dept.Location, dept.Employees.Count));
    }

    /// <summary>Create a new department</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Department name is required" });

        var dept = new Department { Name = dto.Name, Location = dto.Location };
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = dept.Id },
            new DepartmentResponseDto(dept.Id, dept.Name, dept.Location, 0));
    }

    /// <summary>Update department</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept is null)
            return NotFound(new { message = $"Department {id} not found" });

        dept.Name     = dto.Name;
        dept.Location = dto.Location;
        await _db.SaveChangesAsync();

        return Ok(new DepartmentResponseDto(dept.Id, dept.Name, dept.Location, 0));
    }

    /// <summary>Delete department</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var dept = await _db.Departments.FindAsync(id);
        if (dept is null)
            return NotFound(new { message = $"Department {id} not found" });

        var hasEmployees = await _db.Employees.AnyAsync(e => e.DepartmentId == id);
        if (hasEmployees)
            return BadRequest(new { message = "Cannot delete department with employees. Reassign employees first." });

        _db.Departments.Remove(dept);
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Department '{dept.Name}' deleted" });
    }
}
