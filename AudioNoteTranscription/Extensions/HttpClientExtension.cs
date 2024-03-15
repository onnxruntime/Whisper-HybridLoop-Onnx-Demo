using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudioNoteTranscription.Extensions
{
    public static class HttpClientExtension
    {
        public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string FileName)
        {
            using (var s = await client.GetStreamAsync(uri))
            {
                using (var fs = new FileStream(FileName, FileMode.CreateNew))
                {
                    await s.CopyToAsync(fs);
                }
            }
        }
    }
}
