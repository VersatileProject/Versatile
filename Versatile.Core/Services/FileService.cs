using System.Reflection;
using System.Text;

using Newtonsoft.Json;

using Versatile.Core.Contracts.Services;
using Versatile.Core.Helpers;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Versatile.Core.Services;

public class FileService : IFileService
{
    public T Read<T>(string folderPath, string fileName)
    {
        var path = Path.Combine(folderPath, fileName);
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        return default;
    }

    public void Save<T>(string folderPath, string fileName, T content)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileContent = JsonConvert.SerializeObject(content);
        File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
    }

    public void Delete(string folderPath, string fileName)
    {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
        {
            File.Delete(Path.Combine(folderPath, fileName));
        }
    }


    // https://github.com/microsoft/microsoft-ui-xaml/issues/6172
    // Some APIs are not currently supported in unpackaged apps. We are aiming to fix this in the next stable release. A few examples:
    // StorageFile.GetFileFromApplicationUriAsync
    public static async Task<Stream> GetStream(string uriString)
    {
        if (RuntimeHelper.IsMSIX)
        {
            var uri = new Uri(uriString);
            var sf = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var stream = await sf.OpenStreamForReadAsync();
            return stream;
        }
        else
        {
            if (uriString.StartsWith("ms-appx:"))
            {
                var filepath = uriString.Replace("ms-appx:///", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\");
                var stream = File.OpenRead(filepath);
                return stream;
            }
            return null;
        }
    }
}
