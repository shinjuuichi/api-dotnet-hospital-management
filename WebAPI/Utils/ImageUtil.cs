namespace WebAPI.Utils;

public static class ImageUtil
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
    private static readonly string ImageFolder = Path.Combine(Directory.GetCurrentDirectory(), "Images");

    static ImageUtil()
    {
        if (!Directory.Exists(ImageFolder))
        {
            Directory.CreateDirectory(ImageFolder);
        }
    }

    public static async Task<string> SaveImageAsync(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            throw new ArgumentException("Image file is required");

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Invalid image format. Allowed formats: " + string.Join(", ", AllowedExtensions));

        if (imageFile.Length > 5 * 1024 * 1024)
            throw new ArgumentException("Image file size cannot exceed 5MB");

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(ImageFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await imageFile.CopyToAsync(stream);

        return fileName;
    }

    public static void RemoveImage(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return;

        var filePath = Path.Combine(ImageFolder, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static string GetImageUrl(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return string.Empty;

        return $"/images/{fileName}";
    }

    public static string GetImagePath(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return string.Empty;

        return Path.Combine(ImageFolder, fileName);
    }
}
