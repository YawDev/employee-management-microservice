using System.ComponentModel;

namespace Employee.Management.Models.Enums;

public enum Role
{
    [Description("sys-admin")]
    SystemAdmin = 0,
    [Description("company-admin")]
    CompanyAdmin = 1,
    [Description("manager")]
    Manager = 2,
    [Description("employee")]
    Employee = 3,
}