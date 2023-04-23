using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Formats.Asn1.AsnWriter;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query =
             from s in db.Students
             join e in db.Enrolleds
             on s.UId equals e.UId

             join cl in db.Classes
             on e.ClassId equals cl.ClassId

             join c in db.Courses
             on cl.CourseId equals c.CourseId
             where c.Subject == subject && c.Number == num && cl.Semester == season && cl.Year == year

             select new
             {
                 fname = s.FName,
                 lname = s.LName,
                 uid = s.UId,
                 dob = s.Dob,
                 grade = e.Grade
             };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category == null)
            {
                var nullQuery =
                    from course in db.Courses
                    join cl in db.Classes
                    on course.CourseId equals cl.CourseId

                    join ac in db.AssignmentCategories
                    on cl.ClassId equals ac.ClassId

                    join a in db.Assignments
                    on ac.CategoryId equals a.CategoryId
                    where course.Subject == subject && course.Number == num && cl.Year == year && cl.Semester == season
                    select new
                    {
                        aname = a.Name,
                        cname = ac.Name,
                        due = a.Due,
                        submissions = (from sub in db.Submissions where sub.AssignmentId == a.AssignmentId select sub).Count()
                    };

                return Json(nullQuery.ToArray());
            }

            // otherwise find specific one
            var asgnQuery =
                from course in db.Courses
                join cl in db.Classes
                on course.CourseId equals cl.CourseId

                join ac in db.AssignmentCategories
                on cl.ClassId equals ac.ClassId

                join a in db.Assignments
                on ac.CategoryId equals a.CategoryId
                where course.Subject == subject && course.Number == num && cl.Year == year && cl.Semester == season
                && ac.Name == category
                select new
                {
                    aname = a.Name,
                    cname = ac.Name,
                    due = a.Due,
                    submissions = (from sub in db.Submissions where sub.AssignmentId == a.AssignmentId select sub).Count()
                };

            return Json(asgnQuery.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query =
                from course in db.Courses
                join cl in db.Classes
                on course.CourseId equals cl.CourseId

                join ac in db.AssignmentCategories
                on cl.ClassId equals ac.ClassId
                where course.Subject == subject && course.Number == num && cl.Year == year && cl.Semester == season
                select new { name = ac.Name, weight = ac.Weight };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var classQuery =
                from course in db.Courses
                join cl in db.Classes
                on course.CourseId equals cl.CourseId

                where course.Subject == subject && course.Number == num && cl.Semester == season && cl.Year == year
                select cl;

            var asgnQuery =
                from q in classQuery
                join a in db.AssignmentCategories
                on q.ClassId equals a.ClassId

                select a.Name;

            foreach (var v in asgnQuery)
            {
                //ac with given name already exists
                if (v == category)
                {
                    return Json(new { success = false });
                }
            }

            AssignmentCategory assignCat = new AssignmentCategory();
            assignCat.Name = category;
            assignCat.Weight = (uint)catweight;
            assignCat.ClassId = classQuery.Single().ClassId;
            db.AssignmentCategories.Add(assignCat);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var classQuery =
                from course in db.Courses
                join cl in db.Classes
                on course.CourseId equals cl.CourseId

                where course.Subject == subject && course.Number == num && cl.Semester == season && cl.Year == year
                select cl;

            var asgnQuery =
                from q in classQuery
                join ac in db.AssignmentCategories
                on q.ClassId equals ac.ClassId
                where ac.Name == category
                select ac;

            Assignment a = new Assignment();
            a.Name = asgname;
            a.Points = (uint)asgpoints;
            a.Due = asgdue;
            a.Contents = asgcontents;
            a.CategoryId = asgnQuery.Single().CategoryId;
            db.Assignments.Add(a);

            // update all student scores
            var uidsQuery =
                from cl in classQuery
                join e in db.Enrolleds
                on cl.ClassId equals e.ClassId
                select new {e.ClassId, e.UId}; 

            foreach (var stud in uidsQuery.ToArray())
            {
                double grade = CalcGradeForStudent(subject, num, season, year, stud.UId, stud.ClassId);

                string letter = ToLetterGrade(grade);
                var enrQuery = from e in db.Enrolleds
                               where e.UId == stud.UId && e.ClassId == stud.ClassId
                               select e;

                enrQuery.ToArray()[0].Grade = letter;
            }
            db.SaveChanges();
            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var subQuery =
                from course in db.Courses
                join cl in db.Classes
                on course.CourseId equals cl.CourseId

                join ac in db.AssignmentCategories
                on cl.ClassId equals ac.ClassId

                join a in db.Assignments
                on ac.CategoryId equals a.CategoryId

                join sub in db.Submissions
                on a.AssignmentId equals sub.AssignmentId

                join stud in db.Students
                on sub.UId equals stud.UId

                where course.Subject == subject && course.Number == num && cl.Year == year && cl.Semester == season
                && ac.Name == category && a.Name == asgname
                select new {fname = stud.FName, lname = stud.LName, uid = stud.UId, time = sub.Time, score = sub.Score};

            return Json(subQuery.ToArray());
        }

        // helper
        private double CalcGradeForStudent(string subject, int num, string season, int year, string uid, int classId)
        {
            var allCatQuery =
                from ac in db.AssignmentCategories
                where ac.ClassId == classId
                select ac;

            double total = 0;
            int totalWeight = 0;
            foreach (var v in allCatQuery.ToList())
            {
                if (v.Weight.HasValue)
                    totalWeight += (int)v.Weight;

                double? catGrade = CalcGradeForStudentCategory(subject, num, season, year, uid, v.Name);
                if (catGrade.HasValue)
                    total += (double)catGrade;
            }

            return total * (100 / totalWeight);
        }

        // helper method
        private double? CalcGradeForStudentCategory(string subject, int num, string season, int year, string uid, string category)
        {
            var query1 =
                from course in db.Courses
                join cl in db.Classes
                on course.CourseId equals cl.CourseId

                join ac in db.AssignmentCategories
                on cl.ClassId equals ac.ClassId

                join a in db.Assignments
                on ac.CategoryId equals a.CategoryId
                where course.Subject == subject && course.Number == num && cl.Year == year && cl.Semester == season && ac.Name == category
                select new { cname = ac.Name, a.AssignmentId, a.Contents, a.Name, a.Due, a.Points, weight = ac.Weight };

            var query2 =
                from q in query1  // query1 holds the assignments for the class
                join s in db.Submissions
                on new { A = (int?)q.AssignmentId, B = uid } equals new { A = s.AssignmentId, B = s.UId }
                into joined
                from j in joined.DefaultIfEmpty()
                select new { aname = q.Name, cname = q.cname, due = q.Due, score = j.Score, points = q.Points, weight = q.weight };

            double totalS = 0;
            double totalP = 0;
            foreach (var v in query2)
            {
                if (v.score.HasValue) // not a null score
                    totalS += (double)v.score;
             
                if(v.points.HasValue)
                    totalP += (double)v.points;
            }

            return (totalS / totalP) * query2.First().weight;
        }

        // Helper method to grade assignments
        private string ToLetterGrade(double g)
        {
            if (g >= 93)
                return "A";
            else if (g >= 90)
                return "A-";
            else if (g >= 87)
                return "B+";
            else if (g >= 83)
                return "B";
            else if (g >= 80)
                return "B-";
            else if (g >= 77)
                return "C+";
            else if (g >= 73)
                return "C";
            else if (g >= 70)
                return "C-";
            else if (g >= 67)
                return "D+";
            else if (g >= 63)
                return "D";
            else if (g >= 60)
                return "D-";
            else
                return "E";
        }

        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var sQuery =
                from course in db.Courses
                join cl in db.Classes
                on course.CourseId equals cl.CourseId

                join ac in db.AssignmentCategories
                on cl.ClassId equals ac.ClassId

                join a in db.Assignments
                on ac.CategoryId equals a.CategoryId

                join sub in db.Submissions
                on a.AssignmentId equals sub.AssignmentId

                where course.Subject == subject && course.Number == num && cl.Year == year && cl.Semester == season
                && ac.Name == category && a.Name == asgname && sub.UId == uid
                select new {sub, a, ac };

            var student = sQuery.Single();
            if (student != null)
            {
                student.sub.Score = (uint)score; // update student score
                db.SaveChanges();

                double grade = CalcGradeForStudent(subject, num, season, year, uid, student.ac.ClassId);

                string letter = ToLetterGrade(grade);
                var enrQuery = from e in db.Enrolleds
                               where e.UId == uid && e.ClassId == student.ac.ClassId
                               select e;

                enrQuery.ToArray()[0].Grade = letter;
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query =
                from p in db.Professors
                join cl in db.Classes
                on p.UId equals cl.ProfId

                join course in db.Courses
                on cl.CourseId equals course.CourseId

                where p.UId == uid

                select new { subject = course.Subject, number = course.Number, name = course.Name, season = cl.Semester, year = cl.Year };
            return Json(query.ToArray());
        }
        /*******End code to modify********/
    }
}

