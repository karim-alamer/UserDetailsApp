using Microsoft.AspNetCore.Mvc;
using AdelSalamUserDetailsApp.Models;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using iText.Kernel.Colors;

namespace AdelSalamUserDetailsApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpPost("submit")]
        public IActionResult Submit([FromForm] UserDetails model, [FromForm] List<IFormFile> Images)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                // Save uploaded images
                var imagePaths = new List<string>();
                foreach (var image in Images)
                {
                    if (image.Length > 0)
                    {
                        var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        if (!Directory.Exists(imagesFolder))
                        {
                            Directory.CreateDirectory(imagesFolder);
                        }

                        var filePath = Path.Combine(imagesFolder, image.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }
                        imagePaths.Add(filePath);
                    }
                }

                // Generate PDF
                var pdfPath = GeneratePdf(model, imagePaths);

                // Return the PDF file
                var fileBytes = System.IO.File.ReadAllBytes(pdfPath);
                return File(fileBytes, "application/pdf", "UserDetails.pdf");
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.Error.WriteLine($"Error generating PDF: {ex.Message}");
                Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Include detailed error information in the response for easier debugging
                return StatusCode(500, new
                {
                    message = "An error occurred while generating the PDF.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        private string GeneratePdf(UserDetails model, List<string> imagePaths)
        {
            var pdfFileName = Guid.NewGuid().ToString() + ".pdf";
            var pdfsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs");
            if (!Directory.Exists(pdfsFolder))
            {
                Directory.CreateDirectory(pdfsFolder);
            }

            var pdfPath = Path.Combine(pdfsFolder, pdfFileName);

            using (var writer = new PdfWriter(pdfPath))
            {
                var pdf = new PdfDocument(writer);

                // Explicitly add a page to the document
                pdf.AddNewPage();

                var document = new Document(pdf);

                // Add background color
                var canvas = new iText.Kernel.Geom.Rectangle(0, 0, pdf.GetDefaultPageSize().GetWidth(), pdf.GetDefaultPageSize().GetHeight());
                var pdfCanvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(pdf.GetFirstPage());
                pdfCanvas.SaveState();
                pdfCanvas.SetFillColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY);
                pdfCanvas.Rectangle(canvas);
                pdfCanvas.Fill();
                pdfCanvas.RestoreState();

                // Add header
                document.Add(new Paragraph("User Details Report")
                    .SetFontSize(24)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))
                    .SetFontColor(new DeviceRgb(0, 51, 102)));

                document.Add(new Paragraph("\n"));

                // User details table
                var detailsTable = new Table(2).SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100)).SetMarginBottom(20);
                detailsTable.AddCell(new Cell().Add(new Paragraph("Name").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                detailsTable.AddCell(new Cell().Add(new Paragraph(model.Name)));

                detailsTable.AddCell(new Cell().Add(new Paragraph("Email").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                detailsTable.AddCell(new Cell().Add(new Paragraph(model.Email)));

                detailsTable.AddCell(new Cell().Add(new Paragraph("Phone Number").SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD))));
                detailsTable.AddCell(new Cell().Add(new Paragraph(model.PhoneNumber)));

                document.Add(detailsTable);

                // Images section
                if (imagePaths.Count > 0)
                {
                    document.Add(new Paragraph("Uploaded Images").SetFontSize(18).SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD)).SetMarginBottom(10));
                    foreach (var imagePath in imagePaths)
                    {
                        if (System.IO.File.Exists(imagePath))
                        {
                            var pdfImage = new iText.Layout.Element.Image(ImageDataFactory.Create(imagePath));
                            pdfImage.ScaleToFit(400, 300);
                            pdfImage.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                            document.Add(pdfImage);
                            document.Add(new Paragraph("\n"));
                        }
                    }
                }

                // Signature section
                if (!string.IsNullOrEmpty(model.SignatureData))
                {
                    document.Add(new Paragraph("Signature").SetFontSize(18).SetFont(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD)).SetMarginBottom(10));

                    var signatureBytes = Convert.FromBase64String(model.SignatureData.Split(',')[1]);
                    var signatureImage = new iText.Layout.Element.Image(ImageDataFactory.Create(signatureBytes));
                    signatureImage.ScaleToFit(300, 100);
                    signatureImage.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                    document.Add(signatureImage);
                }

                // Footer
                var footerCanvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(pdf.GetPage(pdf.GetNumberOfPages()));
                footerCanvas.BeginText()
                    .SetFontAndSize(iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA), 10)
                    .MoveText(250, 30)
                    .ShowText("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    .EndText();

                document.Close();
            }

            return pdfPath;
        }

    }
}