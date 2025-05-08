using System;
using System.Collections.Generic;

namespace BetterSkypeParser.Models
{
#pragma warning disable IDE1006
    public class ContentModel
    {
        public string? type { get; set; }
        public string? schemaVersion { get; set; }
        public string? clientVersion { get; set; }
        public string? campaignId { get; set; }
        public string? contentType { get; set; }
        public int? cardType { get; set; }
        public bool? quietCard { get; set; }
        public string? iconUrl { get; set; }
        public string? campaignGuid { get; set; }
        public string? language { get; set; }
        public List<int>? platformList { get; set; }
        public DateTime? validUntilTimestamp { get; set; }
        public InnerContent? content { get; set; }
        public string? experimentName { get; set; }
        public string? contentCategory { get; set; }
        public string? validForPeriod { get; set; }
        public string? delayForPeriod { get; set; }
        public string? persistencyMode { get; set; }
        public bool? showOnlyTitle { get; set; }
        public Telemetry? telemetry { get; set; }
        public List<ContentAttachment>? attachments { get; set; }
    }

    public class InnerContent
    {
        public string? actionUri { get; set; }
        public string? mainActionUri { get; set; }
        public string? mainActionTarget { get; set; }
        public string? title { get; set; }
        public string? modalTitle { get; set; }
        public string? text { get; set; }
        public Media? media { get; set; }
        public List<ButtonModel>? buttons { get; set; }
        public string? backgroundColor { get; set; }
        public string? titleColor { get; set; }
        public string? subtitle { get; set; }
        public string? subtitleColor { get; set; }
        public string? textColor { get; set; }
    }

    public class Media
    {
        public string? url { get; set; }
        public string? mediaType { get; set; }
        public string? desktopUrl { get; set; }
        public int? width { get; set; }
    }

    public class ButtonModel
    {
        public string? actionUri { get; set; }
        public string? title { get; set; }
        public string? backgroundColor { get; set; }
        public string? textColor { get; set; }
        public string? actionTarget { get; set; }
    }

    public class Telemetry
    {
        public string? campaignId { get; set; }
        public string? variantId { get; set; }
        public string? iteration { get; set; }
    }

    public class ContentAttachment
    {
        public string? contentType { get; set; }
        public string? iconUrl { get; set; }
        public InnerContent? content { get; set; }
    }
#pragma warning restore IDE1006
}
