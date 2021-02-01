using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using youtube_mp3_video_converter.Models;

namespace youtube_mp3_video_converter.Controllers
{
    public class homeController : Controller
    {
        youtubeContext yc = new youtubeContext();

        public ActionResult Index(string videoID)
        {
            if (videoID != null)
                ViewBag.videoID = videoID;
            List<ConvertedVideo> videoList = yc.ConvertedVideo.OrderByDescending(x => x.convertDate).ToList();
            ConvertedVideo video = null;
            foreach (var item in videoList.Select(x => x.videoId).Distinct().Take(5))
            {
                video = videoList.FirstOrDefault(x => x.videoId == item);
                if (video != null)
                    ViewBag.latestConvertedList +=
                        "<div class='card mt-2 p-1 bg-light'>" +
                            "<a href='?videoID="+item+"'> " +
                                "<div class='row mt-1'>" +
                                    "<div class='col-3'>" +
                                        "<img src='" + video.thumbnail_url + "' style='width:100%;height:100%;object-fit:contain;' />" +
                                    "</div>" +
                                    "<div class='col-9'><i style='font-size:12px; color:black;'>" + video.convertDate.ToString("dd.MM.yyyy HH:mm:ss")+"</i><p>" + (video.title.Length>40?video.title.Substring(0,40)+"...":video.title) + "</p></div>" +
                                "</div>" +
                            "</a>" +
                        "</div>";
                video = null;
            }
            foreach (var item in videoList.GroupBy(x => x.videoId).OrderByDescending(x => x.Count()).Take(5))
            {
                video = videoList.FirstOrDefault(x => x.videoId == item.Key);
                ViewBag.mostDownloadedList +=
                    "<div class='card mt-2 p-1 bg-light'>" +
                        "<a href='?videoID=" + item + "'> " +
                            "<div class='row mt-1'>" +
                                "<div class='col-3'>" +
                                    "<img src='" + video.thumbnail_url + "' style='width:100%;height:100%;object-fit:contain;' />" +
                                "</div>" +
                                "<div class='col-9'><i style='font-size:12px; color:black;'>" + videoList.Count(x=>x.videoId== item.Key)+" Kez İndirildi</i><p>" + (video.title.Length>40?video.title.Substring(0,40)+"...":video.title) + "</p></div>" +
                            "</div>" +
                        "</a>" +
                    "</div>";
            }
            return View();
        }

        [Route("youtube")]
        public JsonResult Youtube(string videoID, string convertIDs)
        {
            if (videoID == null || convertIDs == null)
                return Json(new string[] { "Boş alan bırakmayınız." }, JsonRequestBehavior.AllowGet);

            HttpClient client = new HttpClient();
            Task<string> data = client.GetStringAsync("https://noembed.com/embed?url=https://www.youtube.com/watch?v=" + videoID);
            try
            {
                data.Wait();


                ConvertedVideo content = JsonConvert.DeserializeObject<ConvertedVideo>(data.Result);
                if (content.thumbnail_url == null && content.title == null)
                {
                    ErrorHandle handle = JsonConvert.DeserializeObject<ErrorHandle>(data.Result);
                    if(handle.error == null)
                        return Json(new string[] { "Veri alınırken hata oluştu. Lütfen tekrar deneyiniz." }, JsonRequestBehavior.AllowGet);

                    switch (handle.error)
                    {
                        case "400 Bad Request":
                            {
                                return Json(new string[] { "Video linki bozuk olabilir. Lütfen tekrar deneyiniz." }, JsonRequestBehavior.AllowGet);
                            }
                        case "403 Forbidden":
                            {
                                return Json(new string[] { "Video dönüştürülemiyor. Lütfen tekrar deneyiniz.\nOlmuyorsa kilitli olabilir." }, JsonRequestBehavior.AllowGet);
                            }
                        default:
                            return Json(new string[] { "Veri alınırken hata oluştu. Lütfen tekrar deneyiniz." }, JsonRequestBehavior.AllowGet);
                    }
                }

                
                string buttons = "";
                foreach (var item in convertIDs.Split(','))
                {
                    string[] items = item.Split('/');
                    if (items[5] == "mp3")
                        buttons += "<a href='" + item + "' class='btn btn-primary'>" + items[6] + " kbps</a>";
                    else
                    {
                        switch (items[6])
                        {
                            case "18":
                                {
                                    buttons += "<a href='" + item + "' class='btn btn-primary'>360p</a>";
                                    break;
                                }
                            case "22":
                                {
                                    buttons += "<a href='" + item + "' class='btn btn-primary'>720p</a>";
                                    break;
                                }
                            default:
                                {

                                    buttons += "<a href='" + item + "' class='btn btn-primary'>" + items[6] + "p</a>";
                                    break;
                                }
                        }
                    }

                }

                content.videoId = videoID;
                content.convertDate = DateTime.Now;
                yc.ConvertedVideo.Add(content);
                yc.SaveChanges();
                return Json(new string[] { content.title, content.thumbnail_url, buttons }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new string[] { "Veri alınırken hata oluştu. Lütfen tekrar deneyiniz." }, JsonRequestBehavior.AllowGet);
            }
        }
    }
    class ErrorHandle
    {
        public string error { get; set; }
    }
}