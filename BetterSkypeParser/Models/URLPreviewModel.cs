namespace BetterSkypeParser.Models
{
#pragma warning disable IDE1006
    public class URLPreviewModel
    {
        public string? key { get; set; }
        public URLValue? value { get; set; }
    }

    public class URLValue
    {
        public string? url { get; set; }
        public string? target_url { get; set; }
        public string? size { get; set; }
        public string? status_code { get; set; }
        public string? content_type { get; set; }
        public string? site { get; set; }
        public string? category { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public string? favicon { get; set; }
        public PictureMeta? favicon_meta { get; set; }
        public string? thumbnail { get; set; }
        public PictureMeta? thumbnail_meta { get; set; }
        public string? largethumbnail { get; set; }
        public PictureMeta? largethumbnail_meta { get; set; }
        public string? user_pic { get; set; }
    }

    public class PictureMeta
    {
        public int? width { get; set; }
        public int? height { get; set; }
    }
#pragma warning restore IDE1006
}
