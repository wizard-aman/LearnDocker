using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeApp.Data;
using EmployeeApp.Models;

namespace EmployeeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;
    public EmployeesController(AppDbContext db) => _db = db;

    /// <summary>Get all employees with department name</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _db.Employees
            .Include(e => e.Department)
            .Select(e => new EmployeeResponseDto(
                e.Id, e.Name, e.Email, e.Position, e.Salary,
                e.JoinedAt, e.DepartmentId, e.Department!.Name))
            .ToListAsync();

        return Ok(employees);
    }

    /// <summary>Get employee by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var emp = await _db.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (emp is null)
            return NotFound(new { message = $"Employee {id} not found" });

        return Ok(new EmployeeResponseDto(
            emp.Id, emp.Name, emp.Email, emp.Position, emp.Salary,
            emp.JoinedAt, emp.DepartmentId, emp.Department!.Name));
    }

    /// <summary>Get all employees in a department</summary>
    [HttpGet("by-department/{departmentId:int}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        var exists = await _db.Departments.AnyAsync(d => d.Id == departmentId);
        if (!exists)
            return NotFound(new { message = $"Department {departmentId} not found" });

        var employees = await _db.Employees
            .Include(e => e.Department)
            .Where(e => e.DepartmentId == departmentId)
            .Select(e => new EmployeeResponseDto(
                e.Id, e.Name, e.Email, e.Position, e.Salary,
                e.JoinedAt, e.DepartmentId, e.Department!.Name))
            .ToListAsync();

        return Ok(employees);
    }

    /// <summary>Create new employee</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Name and Email are required" });

        if (dto.Salary <= 0)
            return BadRequest(new { message = "Salary must be greater than 0" });

        // Department exist karta hai check karo
        var deptExists = await _db.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
        if (!deptExists)
            return BadRequest(new { message = $"Department {dto.DepartmentId} does not exist" });

        // Email unique check
        var emailTaken = await _db.Employees.AnyAsync(e => e.Email == dto.Email);
        if (emailTaken)
            return Conflict(new { message = $"Email '{dto.Email}' already exists" });

        var emp = new Employee
        {
            Name         = dto.Name,
            Email        = dto.Email,
            Position     = dto.Position,
            Salary       = dto.Salary,
            DepartmentId = dto.DepartmentId
        };

        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        // Department naam ke saath response do
        var dept = await _db.Departments.FindAsync(emp.DepartmentId);
        return CreatedAtAction(nameof(GetById), new { id = emp.Id },
            new EmployeeResponseDto(emp.Id, emp.Name, emp.Email, emp.Position,
                emp.Salary, emp.JoinedAt, emp.DepartmentId, dept!.Name));
    }

    /// <summary>Update employee</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var emp = await _db.Employees.FindAsync(id);
        if (emp is null)
            return NotFound(new { message = $"Employee {id} not found" });

        var deptExists = await _db.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
        if (!deptExists)
            return BadRequest(new { message = $"Department {dto.DepartmentId} does not exist" });

        emp.Name         = dto.Name;
        emp.Email        = dto.Email;
        emp.Position     = dto.Position;
        emp.Salary       = dto.Salary;
        emp.DepartmentId = dto.DepartmentId;

        await _db.SaveChangesAsync();

        var dept = await _db.Departments.FindAsync(emp.DepartmentId);
        return Ok(new EmployeeResponseDto(emp.Id, emp.Name, emp.Email, emp.Position,
            emp.Salary, emp.JoinedAt, emp.DepartmentId, dept!.Name));
    }

    /// <summary>Delete employee</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var emp = await _db.Employees.FindAsync(id);
        if (emp is null)
            return NotFound(new { message = $"Employee {id} not found" });

        _db.Employees.Remove(emp);
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Employee '{emp.Name}' deleted" });
    }
}
