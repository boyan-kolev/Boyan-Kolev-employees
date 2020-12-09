using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace EmployeesProjects
{
    public class Program
    {
        static void Main(string[] args)
        {
            string pathOfTextFile = "";
            var employees = GetDataFromTextFile(pathOfTextFile);

            var groupByProjects = employees
                .GroupBy(x => x.ProjectId)
                .Select(x => x.OrderBy(d => d.DateFrom).ToList())
                .Where(x => x.Count > 1)
                .ToList();

            var allCoupleWork = new List<CoupleWork>();

            foreach (var employeesPerProject in groupByProjects)
            {
                var groupById = employeesPerProject
                    .GroupBy(x => x.Id)
                    .Select(x => x.ToList())
                    .ToList();

                for (int i = 0; i < groupById.Count; i++)
                {
                    var employeeWork1 = groupById[i];

                    foreach (var emp1 in employeeWork1)
                    {
                        var dateFrom1 = emp1.DateFrom;
                        var dateTo1 = emp1.DateTo;

                        for (int j = i + 1; j < groupById.Count; j++)
                        {
                            var employeeWork2 = groupById[j];

                            foreach (var emp2 in employeeWork2)
                            {
                                var dateFrom2 = emp2.DateFrom;
                                var dateTo2 = emp2.DateTo;

                                if (dateFrom1 > dateTo2 || dateTo1 < dateFrom2)
                                {
                                    continue;
                                }

                                var daysWorkTogether = CalculateDaysWorkTogether(dateFrom1, dateTo1, dateFrom2, dateTo2);

                                if (daysWorkTogether > 0)
                                {
                                    var id1 = 0;
                                    var id2 = 0;

                                    if (emp1.Id < emp2.Id)
                                    {
                                        id1 = emp1.Id;
                                        id2 = emp2.Id;
                                    }
                                    else
                                    {
                                        id1 = emp2.Id;
                                        id2 = emp1.Id;
                                    }

                                    var coupleWork = new CoupleWork()
                                    {
                                        Id1 = id1,
                                        Id2 = id2,
                                        ProjectId = emp1.ProjectId,
                                        Days = daysWorkTogether
                                    };

                                    allCoupleWork.Add(coupleWork);
                                }
                            }
                        }
                    }
                }
            }

            var groupByWorkCouplePerProject = allCoupleWork
                .GroupBy(x => new { x.Id1, x.Id2, x.ProjectId })
                .Select(x => new CoupleWork()
                {
                    Id1 = x.Key.Id1,
                    Id2 = x.Key.Id2,
                    Days = x.Sum(d => d.Days),
                    ProjectId = x.First().ProjectId
                }).OrderByDescending(x => x.Days).ToList();

            var workInATeamForTheLongestTime = groupByWorkCouplePerProject
                .GroupBy(x => new { x.Id1, x.Id2 })
                .Select(x => new
                {
                    Id1 = x.Key.Id1,
                    Id2 = x.Key.Id2,
                    Days = x.Sum(d => d.Days),
                    ProjectIds = string.Join(",", x.Select(p => p.ProjectId))
                }).OrderByDescending(x => x.Days).ToList();

            StringBuilder sb = new StringBuilder();

            sb.Append("Двойката служители, които най-дълго време са работили в екип");
            sb.AppendLine($" са служиители с ID:{workInATeamForTheLongestTime.First().Id1} и {workInATeamForTheLongestTime.First().Id2}.");
            sb.Append($"Работили са общо {workInATeamForTheLongestTime.First().Days} дни");
            sb.Append($", по проекти с ID:{workInATeamForTheLongestTime.First().ProjectIds}");

            Console.WriteLine(sb.ToString());
        }

        private static List<Employee> GetDataFromTextFile(string pathOfTextFile)
        {
            var text = File.ReadAllText(pathOfTextFile);
            var lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var employees = new List<Employee>();

            for (int i = 1; i < lines.Length; i++)
            {
                var tokens = lines[i].Split(", ", StringSplitOptions.RemoveEmptyEntries);

                var empId = int.Parse(tokens[0]);
                var projectId = int.Parse(tokens[1]);
                var dateFrom = DateTime.ParseExact(tokens[2], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var dateTo = DateTime.UtcNow;

                if (tokens[3] != "NULL")
                {
                    dateTo = DateTime.ParseExact(tokens[3], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }


                var employee = new Employee()
                {
                    Id = empId,
                    ProjectId = projectId,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                employees.Add(employee);
            }

            return employees;
        }

        private static int CalculateDaysWorkTogether(DateTime dateFrom1, DateTime dateTo1, DateTime dateFrom2, DateTime dateTo2)
        {
            int days = 0;

            if (dateFrom1 <= dateFrom2 && dateTo1 <= dateTo2)
            {
                days = (int)(dateTo1 - dateFrom2).TotalDays;
            }
            else if (dateFrom1 >= dateFrom2 && dateTo1 >= dateTo2)
            {
                days = (int)(dateTo2 - dateFrom1).TotalDays;
            }
            else if (dateFrom1 >= dateFrom2 && dateTo1 <= dateTo2)
            {
                days = (int)(dateTo1 - dateFrom1).TotalDays;
            }
            else if (dateFrom1 <= dateFrom2 && dateTo1 >= dateTo2)
            {
                days = (int)(dateTo2 - dateFrom2).TotalDays;
            }

            return days;
        }
    }
}
