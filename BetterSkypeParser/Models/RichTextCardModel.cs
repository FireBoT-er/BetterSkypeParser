using System.Collections.Generic;

namespace BetterSkypeParser.Models
{
#pragma warning disable IDE1006
    public class RichTextCardModel
    {
        public string? type { get; set; }
        public List<CardAttachment>? attachments { get; set; }
    }

    public class CardAttachment
    {
        public string? contentType { get; set; }
        public Content? content { get; set; }
    }

    public class Content
    {
        public bool? shareable { get; set; }
        public string? subtitle { get; set; }
        public List<CardImage>? images { get; set; }
        public string? aspect { get; set; }
        public Dimensions? dimensions { get; set; }
    }

    public class Dimensions
    {
        public int? width { get; set; }
        public int? height { get; set; }
    }

    public class CardImage
    {
        public string? alt { get; set; }
        public string? url { get; set; }
        public Tap? tap { get; set; }
        public string? type { get; set; }
        public string? stillUrl { get; set; }
        public int? frames { get; set; }
    }

    public class Tap
    {
        public string? type { get; set; }
    }
#pragma warning restore IDE1006
}
