using Microsoft.EntityFrameworkCore;
using EmployeeApp.Models;

namespace EmployeeApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee>   Employees   => Set<Employee>();

    // Seed data — app start hote hi kuch dummy data
    public static void Seed(AppDbContext db)
    {
        if (db.Departments.Any()) return; // already seeded

        var engineering = new Department { Id = 1, Name = "Engineering",  Location = "Jaipur" };
        var hr          = new Department { Id = 2, Name = "HR",           Location = "Mumbai" };
        var finance     = new Department { Id = 3, Name = "Finance",      Location = "Delhi"  };

        db.Departments.AddRange(engineering, hr, finance);

        db.Employees.AddRange(
            new Employee { Id=1, Name="Aman Sharma",  Email="aman@company.com",  Position="Sr. Developer",  Salary=85000, DepartmentId=1 },
            new Employee { Id=2, Name="Priya Singh",  Email="priya@company.com", Position="HR Manager",     Salary=70000, DepartmentId=2 },
            new Employee { Id=3, Name="Raj Kumar",    Email="raj@company.com",   Position="Jr. Developer",  Salary=55000, DepartmentId=1 },
            new Employee { Id=4, Name="Neha Gupta",   Email="neha@company.com",  Position="Finance Analyst",Salary=65000, DepartmentId=3 }
        );

        db.SaveChanges();
    }
}
