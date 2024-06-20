using ExcelDataReader;
using GenerateQRCode.Models;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace GenerateQRCode.Controllers
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

        [HttpGet]
        public IActionResult CreateQRCode()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult CreateQRCode(QRCodeModel qRCode)
        {
            QRCodeGenerator QrGenerator = new QRCodeGenerator();
            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(qRCode.QRCodeText, QRCodeGenerator.ECCLevel.Q);
            QRCode QrCode = new QRCode(QrCodeInfo);
            Bitmap QrBitmap = QrCode.GetGraphic(10);
            byte[] BitmapArray = QrBitmap.BitmapToByteArray();
            string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
            ViewBag.QrCodeUri = QrUri;
            ViewBag.QrCodeTag = qRCode.QRCodeText;
            return View();
        }

        [HttpGet]
        public IActionResult ExcelFileReader()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExcelFileReader(IFormFile file, QRCodeModel qrCode)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Upload File
            if (file != null && file.Length > 0)
            {
                var uploadDirectory = $"{Directory.GetCurrentDirectory()}\\wwwroot\\Uploads";

                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                var filePath = Path.Combine(uploadDirectory, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                //Read File
                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    var excelData = new List<List<object>>();
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            while (reader.Read())
                            {
                                var rowData = new List<object>();
                                for (int column = 0; column < reader.FieldCount; column++)
                                {
                                    rowData.Add(reader.GetValue(column));
                                }
                                excelData.Add(rowData);
                            }
                        } while (reader.NextResult());

                        ViewBag.ExcelData = excelData;
                        ViewBag.ResQRCode = qRCodeResponses(excelData,qrCode.Size);
                    }
                }
            }



            return View();
        }

        public List<QRCodeResponse> qRCodeResponses(List<List<object>> datalist,int px)
        {
            var res = new List<QRCodeResponse>();

            if (datalist != null && datalist.Count > 1) 
            {
                for (int i = 1; i < datalist.Count; i++)
                {
                    var dataQR = new QRCodeResponse();
                    foreach (var item in datalist[i])
                    {
                        QRCodeGenerator QrGenerator = new QRCodeGenerator();
                        QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(item.ToString(), QRCodeGenerator.ECCLevel.Q);
                        QRCode QrCode = new QRCode(QrCodeInfo);
                        Bitmap QrBitmap = QrCode.GetGraphic(px);
                        byte[] BitmapArray = QrBitmap.BitmapToByteArray();
                        string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
                        dataQR.QrCodeUri = QrUri;
                        dataQR.QrCodeName = item.ToString();

                        res.Add(dataQR);
                    }
                }
            }

            return res;
        }
    }

    

    //Extension method to convert Bitmap to Byte Array
    public static class BitmapExtension
    {
        public static byte[] BitmapToByteArray(this Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}