using FinalExam.Models;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FinalExam.Controllers
{
    public class BooksController : Controller
    {
        //firebase realtime database authentication
        IFirebaseConfig conf = new FirebaseConfig
        {
            AuthSecret = "XkmEmJK6PXPD5fV9kHAkTQYJwlGxzf0TjCRd6470",
            BasePath = "https://cloud2-3d44a-default-rtdb.firebaseio.com/"
            /*AuthSecret = "adUN3i0obYxt8s4XqeDnSLCGZYBVNBCNv1yQP8OB",
            BasePath = "https://finalexam-c6528-default-rtdb.firebaseio.com"*/
        };
        IFirebaseClient client;

        // load inserted data from realtime database
        public IActionResult Index()
        {
            client = new FireSharp.FirebaseClient(conf);
            FirebaseResponse response = client.Get("Books");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<Books>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    list.Add(JsonConvert.DeserializeObject<Books>(((JProperty)item).Value.ToString()));
                }
            }
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }

        //book insertion to Firebase 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Books books)
        {
            try
            {
                client = new FireSharp.FirebaseClient(conf);
                var data = books;
                PushResponse response = client.Push("Books/", data);
                data.Id = response.Result.name;
                SetResponse setResponse = client.Set("Books/" + data.Id, data);

                if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ModelState.AddModelError(string.Empty, "Book added sucessfully");
                }

                else
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong");
                }
            }

            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View();
        }

        //return and display books record on Edit view 
        [HttpGet]
        public ActionResult Edit(string Id)
        {
            client = new FireSharp.FirebaseClient(conf);
            FirebaseResponse response = client.Get("Books/" + Id);
            Books data = JsonConvert.DeserializeObject<Books>(response.Body);
            return View(data);
        }

        // submit the updated record to realtime database
        [HttpPost]
        public ActionResult Edit(Books books)
        {
            client = new FireSharp.FirebaseClient(conf);
            SetResponse response = client.Set("Books/" + books.BookID, books);
            return RedirectToAction("Index");
        }

        //delete a record from realtime database 
        public ActionResult Delete(string Id)
        {
            client = new FireSharp.FirebaseClient(conf);
            FirebaseResponse response = client.Delete("Books/" + Id);
            return RedirectToAction("Index");
        }
    } 
}

