namespace API.Helpers;

public class FileSizeHelper
{
    public static string GetFileSize(long size)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        var order = 0;
        while (size >= 1024 && order < sizes.Length - 1) {
            order++;
            size = size/1024;
        }

        return $"{size:0.##}{sizes[order]}";
    } 
}