using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            bool exists = false;
            var query =
               from v in db.Departments
               select new { v.Subject, v.Name };

            foreach (var v in query)
                if (v.Subject == subject && v.Name == name)
                    exists = true;

            if (exists == true)
            {
                return Json(new { success = false });
            }
            Department d = new Department();
            d.Subject = subject;
            d.Name = name;
            db.Departments.Add(d);
            db.SaveChanges();
            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var query =
               from v in db.Courses
               where v.Subject == subject
               select new { number = v.Number, name = v.Name };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var query =
               from v in db.Professors
               where v.Department == subject
               select new { lname = v.LName, fname = v.FName, uid = v.UId };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            var query =
                from v in db.Courses
                where v.Subject == subject && v.Number == number
                select new { v.Subject, v.Number };

            if (query.Count() > 0 || number.ToString() == null || name == null)
                return Json(new { success = false });

            Course c = new Course();
            c.Subject = subject;
            c.Number = number;
            c.Name = name;

            db.Courses.Add(c);
            db.SaveChanges();
            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {

            bool exists = false;
            var query =
               from v in db.Classes
               join v2 in db.Courses
               on v.CourseId equals v2.CourseId
               select new { v2.Subject, v2.Number, v.Semester, v.Year, v.Start, v.End, v.Location, v.ProfId };

            foreach (var v in query)
            {
                if ((v.Location == location && v.Start >= TimeOnly.FromDateTime(start) && v.End <= TimeOnly.FromDateTime(end) ||
                            v.Start <= TimeOnly.FromDateTime(start) && v.End == TimeOnly.FromDateTime(end) ||
                            v.Start == TimeOnly.FromDateTime(start) && v.End >= TimeOnly.FromDateTime(end) ||
                            v.Start <= TimeOnly.FromDateTime(start) && v.End >= TimeOnly.FromDateTime(end)) &&
                            (v.Subject == subject && v.Number == number && v.Semester == season && v.Year == year) ||
                            (v.Subject == subject && v.Number == number && v.Semester == season && v.Year == year))
                    exists = true;
            }

            if (exists == true || instructor == null)
            {
                return Json(new { success = false });
            }

            Class c = new Class();
            c.Year = (uint)year;
            c.Semester = season;
            c.Start = TimeOnly.FromDateTime(start);
            c.End = TimeOnly.FromDateTime(end);
            c.Location = location;
            c.ProfId = instructor;

            var query2 =
                from cr in db.Courses
                where cr.Subject == subject && cr.Number == number
                select cr.CourseId;
            foreach (var v in query2)
            {
                c.CourseId = v;
                break;
            }

            db.Classes.Add(c);
            db.SaveChanges();
            return Json(new { success = true });
        }


        /*******End code to modify********/

    }
}

