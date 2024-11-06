using Eclipse.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Eclipse.Controllers;

[Route("api/file/{fileName}")]
[ApiController]
public class FileController : ControllerBase
{
    private readonly IFileRepository _fileRepo;

    public FileController(IFileRepository fileRepo)
    {
        _fileRepo = fileRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string fileName)
    {
        await using var imageStream = await _fileRepo.GetFile(fileName);
        imageStream?.Seek(0, SeekOrigin.Begin);

        using (var image = await Image.LoadAsync(imageStream))
        {
            image.Mutate(x => x
                .Resize(new ResizeOptions
                {
                    Size = new Size(375, 500),
                    Mode = ResizeMode.Max
                })
            );

            var compressedImageStream = new MemoryStream();
            await image.SaveAsync(compressedImageStream, new PngEncoder());
            compressedImageStream.Seek(0, SeekOrigin.Begin);

            return File(compressedImageStream, "image/png");
        }
    }

    [HttpGet("small")]
    public async Task<IActionResult> Small(string fileName)
    {
        using (var imageStream = await _fileRepo.GetFile(fileName))
        {
            using (var memoryStream = new MemoryStream())
            {
                if (imageStream != null) await imageStream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var image = await Image.LoadAsync(memoryStream))
                {
                    image.Mutate(x => x
                        .Resize(new ResizeOptions
                        {
                            Size = new Size(75, 100),
                            Mode = ResizeMode.Max
                        })
                    );

                    var compressedImageStream = new MemoryStream();
                    await image.SaveAsync(compressedImageStream, new PngEncoder());
                    compressedImageStream.Seek(0, SeekOrigin.Begin);

                    return File(compressedImageStream, "image/png");
                }
            }
        }
    }
}