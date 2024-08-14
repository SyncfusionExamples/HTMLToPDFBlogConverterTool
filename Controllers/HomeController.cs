using HTMLToPDF_WebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Drawing;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Parsing;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace HTMLToPDF_WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
        public ActionResult ExportToPDF(URL url)
        {
            //Initialize HTML to PDF converter.
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();

            //Initialize Blink converter settings.
            BlinkConverterSettings blinkConverterSettings = new BlinkConverterSettings();

            //Enable lazy load images.
            blinkConverterSettings.EnableLazyLoadImages = true;

            //Set the page size and orientation of the PDF document.
            blinkConverterSettings.PdfPageSize = PdfPageSize.A4;
            blinkConverterSettings.Orientation = PdfPageOrientation.Landscape;

            //Set the Scale of the PDF document.
            blinkConverterSettings.Scale = 1.0f;

            //Set the margin of the PDF document.
            blinkConverterSettings.Margin.All = 0;

            //Set the JavaScript to remove the unwanted elements from the HTML page.
            //Set the JavaScript to remove the unwanted elements from the HTML page.
            blinkConverterSettings.JavaScript = "document.getElementById(\"liveChatApp\").remove();\ndocument.getElementById(\"wpfront-scroll-top\").remove();\n document.getElementById(\"top-section\").remove(); \n document.getElementById(\"main-menu-section\").remove(); \n document.getElementById(\"home-page-header\").remove();\n document.getElementById(\"social-icon\").remove(); \n document.getElementById(\"subscription-section\").remove();\n document.getElementById(\"toc-section\").remove();\n document.getElementById(\"category-ad-section\").remove();\n document.getElementById(\"comments-section\").remove();\n document.getElementById(\"cookie\").remove()";

            //Assign the header element to PdfHeader of Blink converter settings.
            blinkConverterSettings.PdfHeader = AddHeader(blinkConverterSettings.PdfPageSize, url.HeaderText);

            //Assign the footer element to PdfFooter of Blink converter settings.
            blinkConverterSettings.PdfFooter = AddFooter(blinkConverterSettings.PdfPageSize);

            blinkConverterSettings.AdditionalDelay = 20000;

            //Assign the Blink converter settings to HTML converter.
            htmlConverter.ConverterSettings = blinkConverterSettings;

            if(url.BlogLink == string.Empty)
            {
                throw new PdfException("Blog link is empty. Kindly add Blog link in the text box before performing conversion");
            }

            PdfDocument document = htmlConverter.Convert(url.BlogLink);
            //Create memory stream.
            MemoryStream stream = new MemoryStream();
            //Save and close the document. 
            document.Save(stream);
            document.Close();       
            

            //Load the PDF document
            PdfLoadedDocument doc = new PdfLoadedDocument(stream);
            
            //Get first page from document
            PdfLoadedPage? page = doc.Pages[0] as PdfLoadedPage;

            //Create PDF graphics for the page
            PdfGraphics graphics = page.Graphics;

            // Gets the stream of the Image URL.
            Stream receiveStream;
            if (url.ImageURL != null)
            {
                receiveStream = DownloadImage(url.ImageURL);
            }
            else
            {
                /* Below URL act as a default URL if TextBox is empty. It can be modified */
                receiveStream = DownloadImage("https://www.syncfusion.com/blogs/wp-content/uploads/2023/11/NET-MAUI.png");
            }
            
            //Load the image
            PdfBitmap image = new PdfBitmap(receiveStream);

            // Define the position and size for the image
            float x = 570;
            float y = 330;
            float width = 235;
            float height = 245;

            // Draw the image on the page
            page.Graphics.DrawImage(image, x, y, width, height);

            // Define the URL for the hyperlink

            string AdURL = string.Empty;
            
            if(url.AdURL != null) 
            { 
                AdURL = url.AdURL;
            }
            else
            {
                /* Below URL act as a default URL if AdURL TextBox is empty. It can be modified */
                AdURL = "https://www.syncfusion.com/downloads/maui";
            }

            // Create a hyperlink annotation
            PdfUriAnnotation hyperlink = new PdfUriAnnotation(new RectangleF(x, y, width, height), AdURL);
            hyperlink.Border = new PdfAnnotationBorder(0); // Set border to 0 for invisible border

            // Add the hyperlink annotation to the page
            page.Annotations.Add(hyperlink);

            //Creating the stream object
            stream = new MemoryStream();
            //Save the document as stream
            doc.Save(stream);
            //Close the document
            doc.Close(true);
            return File(stream.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf, "HTML-to-PDF.pdf");
        }
        private static PdfPageTemplateElement AddHeader(SizeF pdfPageSize, string headerText)
        {
            //Create PDF page template element for header with bounds.
            PdfPageTemplateElement header = new PdfPageTemplateElement(new RectangleF(0, 0, pdfPageSize.Height, 50));
            //Create font and brush for header element.
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
            var color = Color.FromArgb(179, 179, 179);
            PdfBrush brush = new PdfSolidBrush(color);
            if(headerText == string.Empty)
            {
                headerText = "Syncfusion Blog";
            }
            //Draw the header string in header template element. 
            header.Graphics.DrawString(headerText, font, brush, new PointF(200, 20));

            return header;
        }

        private static PdfPageTemplateElement AddFooter(SizeF pdfPageSize)
        {
            //Create PDF page template element for footer with bounds.
            PdfPageTemplateElement footer = new PdfPageTemplateElement(new RectangleF(0, 0, pdfPageSize.Height, 50));
            //Create font and brush for header element.
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
            //Create page number field.
            PdfPageNumberField pageNumber = new PdfPageNumberField(font, PdfBrushes.Black);
            //Create page count field.
            PdfPageCountField count = new PdfPageCountField(font, PdfBrushes.Black);
            var color = Color.FromArgb(179, 179, 179);
            PdfBrush brush = new PdfSolidBrush(color);
            //Add the fields in composite fields.
            PdfCompositeField compositeField = new PdfCompositeField(font, brush, "Page {0} of {1}", pageNumber, count);
            //Draw the composite field in footer
            compositeField.Draw(footer.Graphics, new PointF(250, 20));
            return footer;
        }
        public static Stream DownloadImage(string url)
        {
            using (WebClient client = new())
            {
                byte[] imageBytes = client.DownloadData(url);
                return new MemoryStream(imageBytes);
            }
        }
    }
}
