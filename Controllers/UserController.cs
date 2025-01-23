using Microsoft.AspNetCore.Mvc;
using AdelSalamUserDetailsApp.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AdelSalamUserDetailsApp.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;

        public UserController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7260/api/"); // Replace with your API base URL
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // GET: User/Index
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: User/Submit
        [HttpPost]
        public async Task<IActionResult> Submit(UserDetails model, List<IFormFile> Images)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Prepare the form data
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(model.Name), "Name");
            formData.Add(new StringContent(model.Email), "Email");
            formData.Add(new StringContent(model.PhoneNumber), "PhoneNumber");
            formData.Add(new StringContent(model.SignatureData), "SignatureData");

            foreach (var image in Images)
            {
                var fileContent = new StreamContent(image.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                formData.Add(fileContent, "Images", image.FileName);
            }

            // Call the API
            var response = await _httpClient.PostAsync("user/submit", formData);

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                return File(pdfBytes, "application/pdf", "UserDetails.pdf");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
                return View("Index", model);
            }
        }
    }
}