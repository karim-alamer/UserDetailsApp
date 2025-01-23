using Microsoft.AspNetCore.Mvc;
using AdelSalamUserDetailsApp.Models;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace AdelSalamUserDetailsApp.Models
{
    public class UserDetails
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please upload at least one image")]
        public List<IFormFile> Images { get; set; }

        [Required(ErrorMessage = "Signature is required")]
        public string SignatureData { get; set; } // Base64 encoded signature
    }
}