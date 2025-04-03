namespace MauiClientApp.Models
{
    public class CvPdf
    {
        public int cv_id { get; set; }
        public string file_name { get; set; }
        public long file_size { get; set; }
        public byte[] file_data { get; set; }
    }
} 