using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace ConsoleApplication
{
    internal class Program
    {
        /// <summary>
        ///     下載 FFmpeg 執行檔
        /// </summary>
        private static async Task DownloadFFmpeg()
        {
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Full);
            Console.WriteLine("FFmpeg downloaded.");
        }

        public static void Main(string[] args)
        {
            DownloadFFmpeg().GetAwaiter().GetResult();

            Console.WriteLine(FFmpeg.ExecutablesPath);

            var file = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Demo", "SampleVideo_1280x720_1mb.mp4"));

            RunConversion(file).GetAwaiter().GetResult();

            RunGetThumbnail(file).GetAwaiter().GetResult();

            Console.ReadLine();
        }

        private static string outputFileNameBuilder(string number)
        {
            return "fileNameNo" + number + ".png";
        }


        //實作一個方法, 傳入 FileInfo 型別的參數, FileInfo 會是 Mp4 影片檔, 要從這個影片擷取封面圖檔
        private static async Task RunGetThumbnail(FileInfo file)
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
        private static async Task RunConversion(FileInfo file)
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