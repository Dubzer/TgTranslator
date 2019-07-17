using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Extensions;
using TgTranslator.Interfaces;

namespace TgTranslator
{
    public class FrancLanguageDetector : ILanguageDetector
    {
        public async Task<string> DetectLanguageAsync(string text)
        {
            // Text without English symbols
            string textWOEng = Regex.Replace(text.WithoutLinks(), "[a-zA-Z0-9 -]", "");

            // The second thing counts percentage of non-English symbols, and if it's > 8%, then translation is not required
            if (!string.IsNullOrWhiteSpace(textWOEng) && textWOEng.Length * 100 / text.Length > 8)
            {
                return await MakeRequest(textWOEng);
            }

            return await MakeRequest(text.WithoutLinks());
        }

        private Task<string> MakeRequest(string text)
        {
            HttpWebRequest request = WebRequest.Create("http://127.0.0.1:41873") as HttpWebRequest;

            string textBase64 = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
            request.Headers.Add("Text", textBase64);
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.ASCII);
            
            return reader.ReadToEndAsync();
        }
    }
}