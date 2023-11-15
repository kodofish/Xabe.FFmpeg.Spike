using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace ConsoleApplication
{
    /// <summary>
    /// 這支程式主要是試驗使用 Xabe.FFmpeg 套件 進行 MP4 轉 WEBM 的功能
    /// 套件官網：https://ffmpeg.xabe.net/index.html
    /// </summary>
    internal class Program
    {
        /// <summary>
        ///     下載 FFMpeg 執行檔
        /// </summary>
        private static async Task DownloadFFMpeg()
        {
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Full);
            Console.WriteLine("FFMpeg downloaded.");
        }

        public static void Main(string[] args)
        {
            DownloadFFMpeg().GetAwaiter().GetResult();

            Console.WriteLine(FFmpeg.ExecutablesPath);

            // [Download Sample Videos / Dummy Videos For Demo Use](https://sample-videos.com/)
            // 這個網站有許多影片可以下載, 用來做測試
            var file = RunDownloadVideo("https://sample-videos.com/video123/mp4/720/big_buck_bunny_720p_10mb.mp4").GetAwaiter().GetResult();

            RunConversion(file).GetAwaiter().GetResult();

            RunGetThumbnail(file).GetAwaiter().GetResult();

            Console.ReadLine();
        }
        
        /// <summary>
        /// 傳入一個 mp4 檔案的 url, 將這個檔案下載到本地端
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<FileInfo> RunDownloadVideo(string url)
        {
            //先建立一個資料夾, 用來存放下載的影片檔案
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Demo");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            //使用傳入的 url 來解析檔案名稱, 並且將檔案名稱加到資料夾路徑後面
            var file = new FileInfo(Path.Combine(directory, Path.GetFileName(url)));

            //如果檔案已經存在, 就先刪除
            if (file.Exists)
                file.Delete();
            
            //使用 httpClient 來下載檔案
            
            using (var httpClient = new System.Net.WebClient())
            {
                //使用 GetByteArrayAsync 方法, 將檔案下載到指定路徑
                await httpClient.DownloadFileTaskAsync(url, file.FullName);
            }

            //印出下載完成的訊息
            Console.WriteLine($"{file.FullName} file downloaded.");

            return file;
        }

        //實作一個方法, 傳入 FileInfo 型別的參數, FileInfo 會是 Mp4 影片檔, 要從這個影片擷取封面圖檔
        private static async Task RunGetThumbnail(FileSystemInfo file)
        {
            //將影片檔的路徑, 改成跟影片檔同一個資料夾, 並且將副檔名改成 jpg
            var outputFileName = Path.ChangeExtension(file.FullName, ".png");

            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }

            var mediaInfo = await FFmpeg.GetMediaInfo(file.FullName);

            var videoStream = mediaInfo.VideoStreams.First()?.SetCodec(VideoCodec.png);

            var result = await FFmpeg.Conversions.New()
                             .AddStream(videoStream)
                             .ExtractNthFrame(10, (s)=> outputFileName)
                             .Start();

            //印出轉檔完成的訊息
            Console.WriteLine($"{outputFileName} file completed. Duration: {result.Duration}");
        }

        /// <summary>
        ///     將影片轉成 webm 格式
        /// </summary>
        /// <param name="file"></param>
        private static async Task RunConversion(FileSystemInfo file)
        {
            var outputFileName = Path.ChangeExtension(file.FullName, ".webm");

            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }

            var snippet = FFmpeg.Conversions.FromSnippet.ToWebM(file.FullName, outputFileName).GetAwaiter().GetResult();
            var result = await snippet.Start();

            Console.WriteLine($"{outputFileName} file completed. Duration: {result.Duration}");
        }
    }
}