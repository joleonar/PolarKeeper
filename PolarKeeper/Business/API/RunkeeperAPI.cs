using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using PolarKeeper.Business.Helpers;
using PolarKeeper.Models;
using PolarKeeper.Models.PolarLib;

namespace PolarKeeper.Business.API
{
    public static class RunkeeperApi
    {
        public static bool AddActivity(PPTExercise exercise, string authorization)
        {
            try
            {
                var collection = new NameValueCollection
                    {
                        {"type", exercise.sport},
                        {"start_time", exercise.time.ToString("R")},
                        {"total_distance", exercise.distance.ToString()},
                        {"duration", Convert.ToInt32(exercise.duration.TotalSeconds).ToString()},
                        {"average_heart_rate", exercise.heartRate.average.ToString()},
                        {"total_calories", exercise.calories.ToString()},
                        {"notes", ""}
                    };
                var dictionary = APIHelper.NvcToDictionary(collection, false);
                var json = new JavaScriptSerializer().Serialize(dictionary);
                var request = (HttpWebRequest)WebRequest.Create("https://api.runkeeper.com/fitnessActivities");
                request.Method = "POST";
                request.ContentType = "application/vnd.com.runkeeper.NewFitnessActivity+json";
                request.Headers["Authorization"] = "Bearer " + authorization;
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var response = (HttpWebResponse)request.GetResponse();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        

        public static RunkeeperUserProfile GetUser(string authorization)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://api.runkeeper.com/profile");
                request.Method = "GET";
                request.ContentType = "application/vnd.com.runkeeper.Profile+json";
                request.Headers["Authorization"] = "Bearer " + authorization;
                var response = (HttpWebResponse)request.GetResponse();
                var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                var responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();

                var profile = JsonConvert.DeserializeObject<RunkeeperUserProfile>(responseFromServer);

                return profile;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}