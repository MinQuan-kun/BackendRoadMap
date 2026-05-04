using CloudinaryDotNet.Actions;

public interface ICloudinaryService
{
    Task<ImageUploadResult> UploadImageAsync(IFormFile file, string subFolder);
    Task<DeletionResult> DeleteImageAsync(string publicId);
}