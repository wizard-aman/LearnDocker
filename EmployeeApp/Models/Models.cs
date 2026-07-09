namespace EmployeeApp.Models;

// ── ENTITIES ──────────────────────────────────────────────────────────────────

public class Department
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = string.Empty;
    public string Location  { get; set; } = string.Empty;

    // Navigation — ek department ke andar multiple employees
    public List<Employee> Employees { get; set; } = new();
}

public class Employee
{
    public int    Id           { get; set; }
    public string Name         { get; set; } = string.Empty;
    public string Email        { get; set; } = string.Empty;
    public string Position     { get; set; } = string.Empty;
    public decimal Salary      { get; set; }
    public DateTime JoinedAt   { get; set; } = DateTime.UtcNow;

    // Foreign key — Employee belongs to a Department
    public int    DepartmentId { get; set; }
    public Department? Department { get; set; }
}

// ── DTOs — entity seedha expose mat karo ──────────────────────────────────────

public record CreateDepartmentDto(string Name, string Location);
public record UpdateDepartmentDto(string Name, string Location);

public record CreateEmployeeDto(
    string Name,
    string Email,
    string Position,
    decimal Salary,
    int DepartmentId
);

public record UpdateEmployeeDto(
    string Name,
    string Email,
    string Position,
    decimal Salary,
    int DepartmentId
);

// Response DTO — Department ke saath Employee count dikhao
public record DepartmentResponseDto(
    int Id,
    string Name,
    string Location,
    int EmployeeCount
);

// Response DTO — Employee ke saath Department naam dikhao
public record EmployeeResponseDto(
    int Id,
    string Name,
    string Email,
    string Position,
    decimal Salary,
    DateTime JoinedAt,
    int DepartmentId,
    string DepartmentName
);
