using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using PolarKeeper.Business;
using PolarKeeper.Business.API;
using PolarKeeper.Models.PolarLib;
using PolarKeeper.ViewModels;

namespace PolarKeeper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string code)
        {
            if (code.IsNullOrWhiteSpace())
            {
                return View(new PolarPersonalTrainerLogin());
            }
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var collection = new NameValueCollection();
                collection.Add("grant_type", "authorization_code");
                collection.Add("code", code);
                collection.Add("client_id", "96e1a47b0adb4957bd52c4e1a612a352");
                collection.Add("client_secret", "384c2c416d3746f6aa03a071792ad735");
                collection.Add("redirect_uri", "http://localhost:49258");
                var response = wc.UploadValues("https://runkeeper.com/apps/token", collection);
                var accessTokenJson = Encoding.UTF8.GetString(response);
                JObject o = JObject.Parse(accessTokenJson);
                var accessToken = o["access_token"].ToString();
                var cookie = new HttpCookie("runKeeperAccessToken") {Value = accessToken};
                HttpContext.Response.SetCookie(cookie);
            }
            return View(new PolarPersonalTrainerLogin());
        }

        [HttpPost]
        public ActionResult Upload()
        {
            for (var i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file == null || file.ContentLength <= 0)
                    break;


                byte[] filedata = null;
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    filedata = binaryReader.ReadBytes(file.ContentLength);
                }
                var doc = new XmlDocument();
                doc.LoadXml(Encoding.UTF8.GetString(filedata));

                if (doc.DocumentElement != null)
                {
                    if (doc.DocumentElement.Name == "polar-exercise-data")
                    {
                        var exercises = PPTExtract.convertXmlToExercises(doc);
                        var viewModels = new List<ExerciseViewModel>();
                        var count = 0;
                        foreach (var exercise in exercises.OrderBy(m => m.sport).ThenByDescending(m => m.time))
                        {
                            count++;
                            var viewModel = new ExerciseViewModel
                            {
                                Id = count,
                                calories = exercise.calories,
                                distance = exercise.distance,
                                duration = exercise.duration,
                                heartRateAvg = exercise.heartRate.average,
                                heartRateMax = exercise.heartRate.maximum,
                                heartRateRest = exercise.heartRate.resting,
                                sport = exercise.sport,
                                time = exercise.time,
                                Uploaded = false
                            };
                            viewModels.Add(viewModel);
                        }
                        return View("Exercises", viewModels);
                    }
                    return View("Error");
                }

            }
            return null;
        }

        [HttpPost]
        public ActionResult UploadToRunkeeper(ExerciseViewModel exercise)
        {
            var cookie = Request.Cookies["runKeeperAccessToken"];
            if (cookie != null)
            {
                var response = RunkeeperApi.AddActivity(new PPTExercise
                {
                    calories = exercise.calories,
                    distance = exercise.distance,
                    duration = exercise.duration,
                    sport = exercise.sport,
                    time = exercise.time,
                    heartRate = new HeartRate
                    {
                        average = exercise.heartRateAvg,
                        maximum = exercise.heartRateMax,
                        resting = exercise.heartRateRest
                    }
                }, cookie.Value);
                if (response)
                {
                    exercise.Uploaded = true;
                    return PartialView("ExerciseDetail", exercise);
                }
            }
            return View("Error");
        }

        public ActionResult Authorize()
        {
            return Redirect("https://runkeeper.com/apps/authorize?client_id=96e1a47b0adb4957bd52c4e1a612a352&response_type=code&redirect_uri=http://localhost:49258");
        }

        public ActionResult GetUserName()
        {
            var cookie = Request.Cookies["runKeeperAccessToken"];
            if (cookie != null)
            {
                var profile = RunkeeperApi.GetUser(cookie.Value);
                return PartialView("Username",profile.Name);
            }
            return View("Error");
        }

        public ActionResult Logout()
        {
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var cookie = Request.Cookies["runKeeperAccessToken"];
                var collection = new NameValueCollection {{"access_token", cookie.Value}};
                var response = wc.UploadValues("https://runkeeper.com/apps/de-authorize", collection);
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
                Session.Abandon();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult LoginPolarPersonalTrainer(PolarPersonalTrainerLogin pptLogin)
        {
            PolarPersonalTrainerScraper.Login(pptLogin.Email, pptLogin.Password);
            return RedirectToAction("Index");
        }
    }
}