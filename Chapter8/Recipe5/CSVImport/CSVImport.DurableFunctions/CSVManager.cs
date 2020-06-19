using System;
using System.Collections.Generic;



namespace CSVImport.DurableFunctions
{
  	class CSVManager
	    {
	       
	        public static List<Employee> ReadEmployeeData(string employeesListContent)
	        {
	            List<Employee> employees = new List<Employee>();
	            var employeesList = employeesListContent.Split(Environment.NewLine);
	         
	            for (int employeeIndex = 1; employeeIndex<employeesList.Length; employeeIndex++)
	            {
	                var employee = employeesList[employeeIndex];
	                if (employee != null & employee.Length > 1)
	                {
	                    var employeeColumns = employee.Split(",");
	                    employees.Add(
	                    new Employee()
	                    {
	                        EmpId = employeeColumns[0],
	                        Name = employeeColumns[1],
	                        Email = employeeColumns[2],
	                        PhoneNumber = employeeColumns[3],
	                    });
	                }
	            }
	            return employees;
	        }
    }
	public class Employee
	{
		public string EmpId { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
	}


}
