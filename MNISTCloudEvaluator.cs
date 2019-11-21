using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMNIST
{
    public class MNISTCloudEvaluator
    {

        public string Evaluate(Bitmap image, string url,string token = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "";
            }

            var body = ConvertImageToPixelData(image);
            var result = "n/a";

            string jsonData = "{\"data\": [["+body+"]]}";
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(token)) {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                var response = client.PostAsync(url,new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                result = response.Content.ReadAsStringAsync().Result;
                result = result.Replace("\"[", "").Replace("]\"","");
            }

            return result;
        }

        private string ConvertImageToPixelData(Bitmap image)
        {
            int width = 28;
            int height = 28;

            image = ResizeImage(image, new Size(width, height));
            List<string> pixelData = new List<string>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = image.GetPixel(x, y);

                    float pixelValue = (color.R + color.G + color.B) / 3;
                    var black = (255 - pixelValue)/255;

                    pixelData.Add((black).ToString().Replace(",","."));
                }
            }

            var pixels = string.Join(",", pixelData);

            return pixels;
        }

        private static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            Bitmap b = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
            }
            return b;
        }

    }


}
