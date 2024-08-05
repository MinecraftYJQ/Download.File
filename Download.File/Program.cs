using System;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task DownloadFileAsync(string url, string filename, IProgress<double> progress)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using (var request = await client.GetStreamAsync(url))
            {
                using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[81920];
                    var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
                    var downloadedBytes = 0;

                    while (true)
                    {
                        var bytesRead = await request.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                            break;

                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        downloadedBytes += bytesRead;

                        // Report progress
                        progress?.Report((double)downloadedBytes / totalBytes * 100);
                    }
                }
            }
        }
    }

    static void Main(string[] args)
    {
        Console.Write("请输入下载地址:");
        string urll = Console.ReadLine();
        var urls = new[] { urll };
        Console.Write("请修改文件名(留空即为默认名称):");
        string name = urll.Split('/')[urll.Split('/').Length - 1];
        string temp = Console.ReadLine();
        if (temp == "")
        {
            name = urll.Split('/')[urll.Split('/').Length - 1];
        }
        else
        {
            name = temp;
        }
        var filenames = new[] { name };

        // Use Parallel to download multiple files at once
        Parallel.ForEach(urls, (url, state, index) =>
        {
            var progress = new Progress<double>(p => Console.Write($"File: {(int)p}%\r"));
            DownloadFileAsync(url, filenames[index], progress).Wait();
        });

        Console.WriteLine("All files have been downloaded.");
    }
}