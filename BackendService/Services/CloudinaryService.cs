using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using BackendService.Configurations;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly string _baseFolder;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var acc = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
        _cloudinary = new Cloudinary(acc);
        _baseFolder = config.Value.BaseFolder;
    }

    public async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string subFolder)
    {
        var uploadResult = new ImageUploadResult();
        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                // Thiết lập đường dẫn thư mục
                Folder = $"{_baseFolder}/{subFolder}",
                Transformation = new Transformation().Height(500).Width(500).Crop("fill") //resize ảnh
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }
        return uploadResult;
    }
}