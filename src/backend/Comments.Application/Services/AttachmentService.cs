using Comments.Application.Abstractions;
using Comments.Application.Common;
using Comments.Application.Dtos;
using Comments.Domain.Entities;
using Comments.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;

namespace Comments.Application.Services;

public class AttachmentService(IImageProcessor imageProcessor) : IAttachmentService
{
    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif" };

    public Attachment Create(UploadedFile file)
    {
        var extension = Path.GetExtension(file.FileName);

        if (ImageExtensions.Contains(extension))
            return CreateImage(file);

        if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            return CreateText(file);

        throw Invalid("Only JPG, GIF, PNG images or TXT files are allowed.");
    }

    private Attachment CreateImage(UploadedFile file)
    {
        ProcessedImage processed;
        try
        {
            processed = imageProcessor.ResizeToFit(
                file.Content, AttachmentRules.MaxImageWidth, AttachmentRules.MaxImageHeight);
        }
        catch
        {
            throw Invalid("The image is invalid or not a supported format (JPG, GIF, PNG).");
        }

        return new Attachment
        {
            Type = AttachmentType.Image,
            FileName = file.FileName,
            ContentType = processed.ContentType,
            Content = processed.Content,
            SizeBytes = processed.Content.Length
        };
    }

    private static Attachment CreateText(UploadedFile file)
    {
        if (file.Content.Length > AttachmentRules.MaxTextBytes)
            throw Invalid($"Text file must be at most {AttachmentRules.MaxTextBytes / 1024} KB.");

        return new Attachment
        {
            Type = AttachmentType.Text,
            FileName = file.FileName,
            ContentType = "text/plain",
            Content = file.Content,
            SizeBytes = file.Content.Length
        };
    }

    private static ValidationException Invalid(string message) =>
        new([new ValidationFailure("File", message)]);
}
