using System.ComponentModel.DataAnnotations;

namespace GenerateQRCode.Models
{
    public class QRCodeModel
    {
        [Display(Name = "Enter QRCode Text")]
        public string QRCodeText { get; set; }

        public int Size { get; set; }
    }

    public class QRCodeResponse 
    {
        public string QrCodeUri { get; set; }
        public string QrCodeName { get; set; }
    }
}
