using FinalExam.Models;
using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace FinalExam.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        //declare FirebaseAuthenticationProvider
        FirebaseAuthProvider auth;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            //for firebase authentication
            auth = new FirebaseAuthProvider(
                new FirebaseConfig("AIzaSyA5yP3zWfEIFL8c63hPn1I20BHQBOZ8WTs"));
        }

        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("_UserToken");
            if (token != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("SignIn");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //for registration
        public IActionResult Registration()
        {
            return View();
        }

        //for registration conf
        [HttpPost]
        public async Task<IActionResult> Registration(LoginModel loginModel)
        {
            try
            {
                await auth.CreateUserWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);
                var fbAuthLink = await auth.SignInWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);
                string token = fbAuthLink.FirebaseToken;

                if (token != null)
                {
                    HttpContext.Session.SetString("_UserToken", token);
                    return RedirectToAction("Index");
                }
            }
            catch (FirebaseAuthException ex)
            {
                var FirebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, FirebaseEx.error.message);
                return View(loginModel);
            }
            return View();
        }

        //for signin
        public IActionResult SignIn()
        {
            return View();
        }

        //for sign in conf
        [HttpPost]
        public async Task<IActionResult> SignIn(LoginModel loginModel)
        {
            try
            {
                var fbAuthLink = await auth
                    .SignInWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);
                string token = fbAuthLink?.FirebaseToken;
                if (token != null)
                {
                    HttpContext.Session.SetString("_UserToken", token);
                    return RedirectToAction("Index");
                }
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.error.message);
                return View(loginModel);
            }
            return View();
        }

        //for logging out of the user
        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("_UserToken");
            return RedirectToAction("SignIn");
        }
    }
}