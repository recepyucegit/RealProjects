namespace TeknoRoma.Application.DTOs;

public class DepartmentDto
{
    public int Id { get; set; }
    public string DepartmentName { get; set; } = null!;
    public string? Description { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateDepartmentDto
{
    public string DepartmentName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateDepartmentDto
{
    public int Id { get; set; }
    public string DepartmentName { get; set; } = null!;
    public string? Description { get; set; }
}
