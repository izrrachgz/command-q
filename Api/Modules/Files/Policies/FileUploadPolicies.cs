namespace Api.Modules.Files.Policies
{
    /// <summary>
    /// Provides reusable policies for general, document, image, audio, video and ZIP uploads.
    /// </summary>
    internal static class FileUploadPolicies
    {
        public const int MaximumFileSizeMegabytes = 200;
        public const long MaximumFileSizeBytes = MaximumFileSizeMegabytes * 1024L * 1024L;
        public const long MaximumRequestSizeBytes = MaximumFileSizeBytes + (1024L * 1024L);

        private static readonly IReadOnlyDictionary<string, string[]> DocumentMediaTypes =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = ["application/pdf"],
                [".doc"] = ["application/msword"],
                [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
                [".xls"] = ["application/vnd.ms-excel"],
                [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
                [".ppt"] = ["application/vnd.ms-powerpoint"],
                [".pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation"],
                [".odt"] = ["application/vnd.oasis.opendocument.text"],
                [".ods"] = ["application/vnd.oasis.opendocument.spreadsheet"],
                [".odp"] = ["application/vnd.oasis.opendocument.presentation"],
                [".rtf"] = ["application/rtf", "text/rtf"],
                [".txt"] = ["text/plain"],
                [".csv"] = ["text/csv", "application/csv"],
                [".md"] = ["text/markdown", "text/plain"]
            };

        private static readonly IReadOnlyDictionary<string, string[]> ImageMediaTypes =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [".jpg"] = ["image/jpeg"],
                [".jpeg"] = ["image/jpeg"],
                [".png"] = ["image/png"],
                [".gif"] = ["image/gif"],
                [".webp"] = ["image/webp"],
                [".bmp"] = ["image/bmp"],
                [".tif"] = ["image/tiff"],
                [".tiff"] = ["image/tiff"],
                [".heic"] = ["image/heic"],
                [".heif"] = ["image/heif"]
            };

        private static readonly IReadOnlyDictionary<string, string[]> AudioMediaTypes =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [".mp3"] = ["audio/mpeg"],
                [".wav"] = ["audio/wav", "audio/x-wav"],
                [".m4a"] = ["audio/mp4", "audio/x-m4a"],
                [".aac"] = ["audio/aac"],
                [".ogg"] = ["audio/ogg"],
                [".flac"] = ["audio/flac"]
            };

        private static readonly IReadOnlyDictionary<string, string[]> VideoMediaTypes =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [".mp4"] = ["video/mp4"],
                [".m4v"] = ["video/mp4", "video/x-m4v"],
                [".mov"] = ["video/quicktime"],
                [".webm"] = ["video/webm"],
                [".avi"] = ["video/x-msvideo"],
                [".mpeg"] = ["video/mpeg"],
                [".mpg"] = ["video/mpeg"]
            };

        private static readonly IReadOnlyDictionary<string, string[]> ZipMediaTypes =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [".zip"] = ["application/zip", "application/x-zip-compressed"]
            };

        public static FileUploadPolicy Documents { get; } = Create(DocumentMediaTypes);
        public static FileUploadPolicy Images { get; } = Create(ImageMediaTypes);
        public static FileUploadPolicy Audio { get; } = Create(AudioMediaTypes);
        public static FileUploadPolicy Videos { get; } = Create(VideoMediaTypes);
        public static FileUploadPolicy ZipArchives { get; } = Create(ZipMediaTypes);
        public static FileUploadPolicy General { get; } = Create(Combine(
            DocumentMediaTypes,
            ImageMediaTypes,
            AudioMediaTypes,
            VideoMediaTypes,
            ZipMediaTypes));

        private static FileUploadPolicy Create(IReadOnlyDictionary<string, string[]> mediaTypes)
        {
            return new FileUploadPolicy(MaximumFileSizeBytes, mediaTypes);
        }

        private static IReadOnlyDictionary<string, string[]> Combine(params IReadOnlyDictionary<string, string[]>[] policies)
        {
            return policies
                .SelectMany(policy => policy)
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
