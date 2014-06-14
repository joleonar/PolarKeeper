using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using PolarKeeper.Business.Helpers;

namespace PolarKeeper.Business
{
    public static class PolarPersonalTrainerScraper
    {
        public static bool Login(string email, string password)
        {
            //try
            //{
            //    var requestString = "email=" + HttpUtility.UrlEncode(email) + "&password=" +
            //                        HttpUtility.UrlEncode(password) + "&.action=login&tz=-120";
            //    var request = (HttpWebRequest)WebRequest.Create("https://www.polarpersonaltrainer.com/index.ftl");
            //    request.Method = "POST";
            //    request.ContentType = "application/x-www-form-urlencoded";
            //    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            //    {
            //        streamWriter.Write(requestString);
            //        streamWriter.Flush();
            //        streamWriter.Close();
            //    }
            //    var response = (HttpWebResponse)request.GetResponse();

            //    return true;
            //}
            //catch
            //{
            //    return false;
            //}

            using (var wb = new WebClient())
            {
                wb.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var parameters = "email=" + HttpUtility.UrlEncode(email) + "&password=" +
                                 HttpUtility.UrlEncode(password) + "&.action=login&tz=-120";
                try
                {
                    var response = wb.UploadString("https://www.polarpersonaltrainer.com/index.ftl", parameters);
                    var response2 = wb.DownloadString("http://www.polarpersonaltrainer.com/user/index.ftl");
                    return true;
                }
                catch (Exception e)
                {
                    var error = e.Message;
                    return false;
                }
            }
        }
    }
}