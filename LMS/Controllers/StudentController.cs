using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query =
               from e in db.Enrolleds
               join c in db.Classes
               on e.ClassId equals c.ClassId
               join course in db.Courses
               on c.CourseId equals course.CourseId
               where e.UId == uid
               select new { subject = course.Subject, number = course.Number, name = course.Name, season = c.Semester, year = c.Year, grade = e.Grade };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query1 =
                from course in db.Courses
                join cl in db.Classes
                on course.CourseId equals cl.CourseId

                join ac in db.AssignmentCategories
                on cl.ClassId equals ac.ClassId

                join a in db.Assignments
                on ac.CategoryId equals a.CategoryId
                where course.Subject == subject && course.Number == num && cl.Year == year && cl.Semester == season
                select new { cname = ac.Name, a.AssignmentId, a.Contents, a.Name, a.Due, a.Points };

            var query2 =
                from q in query1  // query1 holds the assignments for the class
                join s in db.Submissions
                on new { A = (int?)q.AssignmentId, B = uid } equals new { A = s.AssignmentId , B = s.UId }
                into joined
                from j in joined.DefaultIfEmpty()
                select new { aname = q.Name, cname = q.cname, due = q.Due, score = j.Score};

            return Json(query2.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var asgnQuery =
                from course in db.Courses
                join cl in db.Classes
                on course.CourseId equals cl.CourseId

                join ac in db.AssignmentCategories
                on cl.ClassId equals ac.ClassId

                join a in db.Assignments
                on ac.CategoryId equals a.CategoryId
                where course.Subject == subject && course.Number == num && cl.Year == year && cl.Semester == season
                && ac.Name == category && a.Name == asgname
                select a;

            if(asgnQuery.Count() == 0)
                return Json(new { success = false });

            var subQuery = from sub in db.Submissions
                           where sub.AssignmentId == asgnQuery.Single().AssignmentId && sub.UId == uid
                           select sub;

            Submission? updateSub = subQuery.SingleOrDefault();
            if (updateSub != null) // no prev sub
            {
                updateSub.Contents = contents;
                updateSub.Time = DateTime.Now;
                db.SaveChanges();
            }
            else // otherwise update row
            {
                Submission submission = new Submission();
                submission.Time = DateTime.Now;
                submission.UId = uid;
                submission.Score = 0;
                submission.Contents = contents;
                submission.AssignmentId = asgnQuery.Single().AssignmentId;

                db.Submissions.Add(submission);
                db.SaveChanges();
            }

            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            // enroll
            var classIDQuery =
                from course in db.Courses
                join cl in db.Classes
                on course.CourseId equals cl.CourseId
                where course.Subject == subject && course.Number == num && cl.Year == year && cl.Semester == season

                select cl.ClassId;

            if (classIDQuery.Count() == 0) // if we didnt find a class
                return Json(new { success = false });

            var alreadyEnrolled =
                from e in db.Enrolleds
                where e.ClassId == classIDQuery.First() && e.UId == uid
                select e.UId;

            foreach (var v in alreadyEnrolled)
                if (v == uid)
                    return Json(new { success = false });

            Enrolled enrolled = new Enrolled();
            enrolled.UId = uid;
            enrolled.ClassId = classIDQuery.First();
            enrolled.Grade = "--";
            db.Enrolleds.Add(enrolled);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var query =
                from e in db.Enrolleds
                where e.UId == uid
                select new { e.Grade };

            if (query.Count() == 0)
            {
                return Json(new { gpa = 0.0 });
            }

            int counter = query.Count();

            double dgpa = 0.0;
            foreach (var v in query)
            {
                if (v.Grade != "--")
                {
                    if (v.Grade == "A")
                    {
                        dgpa += 4 * 4.0;
                    }
                    else if (v.Grade == "A-")
                    {
                        dgpa += 4 * 3.7;
                    }
                    else if (v.Grade == "B+")
                    {
                        dgpa += 4 * 3.3;
                    }
                    else if (v.Grade == "B")
                    {
                        dgpa += 4 * 3.0;
                    }
                    else if (v.Grade == "B-")
                    {
                        dgpa += 4 * 2.7;
                    }
                    else if (v.Grade == "C+")
                    {
                        dgpa += 4 * 2.3;
                    }
                    else if (v.Grade == "C")
                    {
                        dgpa += 4 * 2.0;
                    }
                    else if (v.Grade == "C-")
                    {
                        dgpa += 4 * 1.7;
                    }
                    else if (v.Grade == "D+")
                    {
                        dgpa += 4 * 1.3;
                    }
                    else if (v.Grade == "D")
                    {
                        dgpa += 4 * 1.0;
                    }
                    else if (v.Grade == "D-")
                    {
                        dgpa += 4 * 0.7;
                    }
                    else
                    {
                        dgpa += 0.0;
                    }
                }
            }
            dgpa /= (4 * counter);
            return Json(new { gpa = dgpa });
        }
        /*******End code to modify********/

    }
}

