using HTMLToPDF_WebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Drawing;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Parsing;
using System.ComponentModel;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace HTMLToPDF_WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static float headerHeight = 30;
        private static float footerHeight = 60;

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
            if (url.BlogLink != null)
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
                var elementsToRemove = new[]
                {
                "scroll-progress-bar-container",
                "detail-page-secondary-header",
                "post-tags-section",
                "cookie",
                "wpfront-scroll-top",
                "top-section",
                "main-menu-section",
                "subscription-section",
                "toc-section",
                "category-ad-section",
                "comments-section",
                "main-footer-policy",
                "main-footer-desktop",
                "boldchat-host"
            };

                var scriptBuilder = new StringBuilder();
                foreach (var element in elementsToRemove)
                {
                    scriptBuilder.AppendLine($"document.getElementById(\"{element}\").remove();");
                }

                blinkConverterSettings.JavaScript = scriptBuilder.ToString();

                //Assign the header element to PdfHeader of Blink converter settings.
                blinkConverterSettings.PdfHeader = AddHeader(blinkConverterSettings.PdfPageSize, url.HeaderText);

                //Assign the footer element to PdfFooter of Blink converter settings.
                blinkConverterSettings.PdfFooter = AddFooter(blinkConverterSettings.PdfPageSize, url.BlogLink);

                blinkConverterSettings.AdditionalDelay = 20000;

                //Assign the Blink converter settings to HTML converter.
                htmlConverter.ConverterSettings = blinkConverterSettings;

                if (url.BlogLink == string.Empty)
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
                float y = 170;
                float width = 235;
                float height = 245;

                // Draw the image on the page
                page.Graphics.DrawImage(image, x, y, width, height);

                // Define the URL for the hyperlink

                string AdURL = string.Empty;

                if (url.AdURL != null)
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

                // Hex color code for hyperlink color
                var color = ConvertHexToRGB("0057FF");
                PdfBrush brush = new PdfSolidBrush(color);

                for (int i = 0; i < doc.Pages.Count; i++)
                {
                    bool isPageRemoved = false;
                    if (i == doc.Pages.Count - 1)
                    {
                        // Get the first page of the loaded PDF document
                        PdfPageBase lastPage = doc.Pages[i];

                        TextLineCollection lineCollection = new TextLineCollection();

                        // Extract text from the first page with bounds
                        lastPage.ExtractText(out lineCollection);

                        RectangleF textBounds = new RectangleF(0, headerHeight, blinkConverterSettings.PdfPageSize.Height, blinkConverterSettings.PdfPageSize.Width - headerHeight - footerHeight);
                     
                        string extractText = "";

                        //Get the text provided in the bounds
                        foreach (var txtLine in lineCollection.TextLine)
                        {
                            foreach (TextWord word in txtLine.WordCollection)
                            {
                                // Check if the word is within textBounds by comparing coordinates
                                if (word.Bounds.Left >= textBounds.Left &&
                                    word.Bounds.Right <= textBounds.Right &&
                                    word.Bounds.Top >= textBounds.Top &&
                                    word.Bounds.Bottom <= textBounds.Bottom)
                                {
                                    extractText += " " + word.Text;
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(extractText))
                        {
                            doc.Pages.Remove(lastPage);
                            isPageRemoved = true;
                        }
                    }
                    if (!isPageRemoved)
                    {
                        PdfTextWebLink weblink = new PdfTextWebLink();
                        weblink.Text = " View blog Link";
                        weblink.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 8, PdfFontStyle.Bold);
                        weblink.Brush = brush;
                        weblink.Url = url.BlogLink;
                        weblink.DrawTextWebLink(doc.Pages[i].Graphics, new PointF(250, 570));
                        isPageRemoved = false;
                    }
                }
                //Creating the stream object
                stream = new MemoryStream();
                //Save the document as stream
                doc.Save(stream);
                //Close the document
                doc.Close(true);
                return File(stream.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf, "HTML-to-PDF.pdf");
            }
            else
            {
                return View();
            }
        }
        private static PdfPageTemplateElement AddHeader(SizeF pdfPageSize, string headerText)
        {
            //Create PDF page template element for header with bounds.
            PdfPageTemplateElement header = new PdfPageTemplateElement(new RectangleF(0, 0, pdfPageSize.Height, headerHeight));
            //Create font and brush for header element.
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
            if (headerText == string.Empty)
            {
                headerText = "Syncfusion Blog";
            }

            float x = (pdfPageSize.Height / 2) - (font.MeasureString(headerText).Width / 2);
            float y = 15 - (font.Height / 2);

            // Hex color code for header background color
            var color = ConvertHexToRGB("FFFFFF");
            PdfBrush brush = new PdfSolidBrush(color);

            header.Graphics.DrawRectangle(brush, new RectangleF(0, 0, pdfPageSize.Height, 30));

            // Hex color code for header Background stoke color
            color = ConvertHexToRGB("E5EDF3");
            brush = new PdfSolidBrush(color);

            header.Graphics.DrawLine(new PdfPen(color),new PointF(0,30),new PointF(pdfPageSize.Height,30));

            //Hex color code for header text color
            color = ConvertHexToRGB("475569");
            brush = new PdfSolidBrush(color);

            //Draw the header string in header template element. 
            header.Graphics.DrawString(headerText, font, brush, new PointF(x, y));

            return header;
        }

        private static PdfPageTemplateElement AddFooter(SizeF pdfPageSize, string url)
        {
            //Create PDF page template element for footer with bounds.
            PdfPageTemplateElement footer = new PdfPageTemplateElement(new RectangleF(0, 0, pdfPageSize.Height, footerHeight));
            //Create font and brush for header element.
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
            //Create page number field.
            PdfPageNumberField pageNumber = new PdfPageNumberField(font, PdfBrushes.Black);
            //Create page count field.
            PdfPageCountField count = new PdfPageCountField(font, PdfBrushes.Black);

            // Hex color code for footer CompositeField text color
            var color = ConvertHexToRGB("475569");
            PdfBrush brush = new PdfSolidBrush(color);
            //Add the fields in composite fields.
            PdfCompositeField compositeField = new PdfCompositeField(font, brush, "Page {0} of {1}", pageNumber, count);

            float x = pdfPageSize.Height - (font.MeasureString("Page {99} of {99}").Width + 20);
            float y = 35;


            // Hex color code for footer background color
            color = ConvertHexToRGB("FAFBFF");
            brush = new PdfSolidBrush(color);

            footer.Graphics.DrawRectangle(brush, new RectangleF(0, 0, pdfPageSize.Height,60));

            // Hex color code for footer background stoke color
            color = ConvertHexToRGB("E5EDF3");
            brush = new PdfSolidBrush(color);

            footer.Graphics.DrawLine(new PdfPen(color), new PointF(0, 0), new PointF(pdfPageSize.Height, 0));

            //Draw the composite field in footer
            compositeField.Draw(footer.Graphics, new PointF(x, y));

            FileStream logoImage = new FileStream("wwwroot/images/logo.png", FileMode.Open, FileAccess.Read);

            //Draw the logo
            PdfBitmap logo = new PdfBitmap(logoImage);
            //Draw the logo on the footer
            footer.Graphics.DrawImage(logo, new RectangleF(20, 0, 75, 40));

            //Hex color code for footer text color
            color = ConvertHexToRGB("475569");
            brush = new PdfSolidBrush(color);

            footer.Graphics.DrawString("Copyright 2001 - Present. Syncfusion, Inc. All Rights Reserved. |", font, brush, new PointF(20, 35));
            return footer;
        }
        public static Stream DownloadImage(string url)
        {
            using (WebClient client = new())
            {
                // Set the User-Agent header to mimic a request from a browser
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                byte[] imageBytes = client.DownloadData(url);
                return new MemoryStream(imageBytes);
            }
        }
        public static Color ConvertHexToRGB(string hexColor)
        {
            // Convert hex to integer values for red, green, and blue
            int r = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return Color.FromArgb(r, g, b);
        }
    }
}
