using Avalonia.Layout;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BetterSkypeParser.Models
{
#pragma warning disable IDE1006
    public class MainModel
    {
        public string? userId { get; set; } //done

        private DateTime? exportDateInner; //done
        public DateTime? exportDate { get => exportDateInner.HasValue ? exportDateInner.Value.ToLocalTime() : null; set => exportDateInner = value; }
        
        public List<Conversation>? conversations { get; set; }
    }

    public class Conversation
    {
        public string? id { get; set; } //done
        public string? displayName { get; set; } //done
        public long? version { get; set; } //extra
        public ConversationProperties? properties { get; set; }
        public ThreadProperties? threadProperties { get; set; }
        public List<Message>? MessageList { get; set; }

        [JsonIgnore]
        public string DisplayId { get; set; } = string.Empty;
        [JsonIgnore]
        public bool IsCast { get; set; } = false;
        [JsonIgnore]
        public bool IsThread { get; set; } = false;
    }

    public class ConversationProperties
    {
        public bool? conversationblocked { get; set; } //extra
        
        private DateTime? lastimreceivedtimeInner; //done
        public DateTime? lastimreceivedtime { get => lastimreceivedtimeInner.HasValue ? lastimreceivedtimeInner.Value.ToLocalTime() : null; set => lastimreceivedtimeInner = value; }
        
        public string? consumptionhorizon { get; set; } //extra
        public string? conversationstatus { get; set; } //extra
    }

    public class ThreadProperties
    {
        public int? membercount { get; set; } //done+about

        [JsonPropertyName("members")]
        public string? membersInner { private get; set; } //about
        [JsonIgnore]
        public List<string>? members { get => string.IsNullOrWhiteSpace(membersInner) ? null : JsonSerializer.Deserialize<List<string>>(membersInner); set => membersInner = value == null ? null : JsonSerializer.Serialize(value); }

        public string? topic { get; set; } //about
        public string? joiningEnabled { get; set; } //about
        public ConsumptionHorizons? consumptionhorizons { get; set; }

        //TODO: no data
        public object? membersBlocked { get; set; }
        public object? membersNicknames { get; set; }
        public object? picture { get; set; }
        public object? description { get; set; }
        public object? guidelines { get; set; }
        public object? shareJoinLink { get; set; }
        public object? searchVisible { get; set; }
        public object? websiteText { get; set; }
        public object? websiteUrl { get; set; }
    }

    public class ConsumptionHorizons
    {
        public string? version { get; set; } //extra
        public List<ConsumptionHorizon>? consumptionhorizons { get; set; }
    }

    public class ConsumptionHorizon
    {
        public string? id { get; set; } //extra
        public string? consumptionhorizon { get; set; } //extra
    }

    public class Message
    {
        public string? id { get; set; } //extraM
        public string? displayName { get; set; } //done

        private DateTime? originalarrivaltimeInner; //done

        public DateTime? originalarrivaltime { get => originalarrivaltimeInner.HasValue ? originalarrivaltimeInner.Value.ToLocalTime() : null; set => originalarrivaltimeInner = value; }
        
        public string? messagetype { get; set; } //main
        public long? version { get; set; } //extraM

        //TODO: json[] or html or text
        public string? content { get; set; }
        [JsonIgnore]
        public List<(string type, string content)>? RichTextContent { get; set; }

        public string? conversationid { get; set; } //extraM
        public string? from { get; set; } //done
        public MessageProperties? properties { get; set; }

        public List<string>? amsreferences { get; set; } //main
        [JsonIgnore]
        public Bitmap? image { get; set; }
        [JsonIgnore]
        public GifPlayer? GifPlayer { get; set; }

        [JsonIgnore]
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        [JsonIgnore]
        public bool HasBorder { get; set; } = false;
        [JsonIgnore]
        public bool IsSelectable { get; set; } = true;
        [JsonIgnore]
        public string? ContactId { get; set; }
        [JsonIgnore]
        public List<(string displayable, string toolTip)>? AdditionalDisplayables { get; set; }
    }

    public class MessageProperties
    {
        [JsonPropertyName("callLog")]
        public string? callLogInner { private get; set; }
        [JsonIgnore]
        public CallLogModel? callLog { get => string.IsNullOrWhiteSpace(callLogInner) ? null : JsonSerializer.Deserialize<CallLogModel>(callLogInner); set => callLogInner = value == null ? null : JsonSerializer.Serialize(value); }

        [JsonPropertyName("urlpreviews")]
        public string? urlpreviewsInner { private get; set; }
        [JsonIgnore]
        public List<URLPreviewModel>? urlpreviews { get => string.IsNullOrWhiteSpace(urlpreviewsInner) ? null : JsonSerializer.Deserialize<List<URLPreviewModel>>(urlpreviewsInner); set => urlpreviewsInner = value == null ? null : JsonSerializer.Serialize(value); }

        [JsonPropertyName("forwardMetadata")]
        public string? forwardMetadataInner { private get; set; }
        [JsonIgnore]
        public ForwardMetadataModel? forwardMetadata { get => string.IsNullOrWhiteSpace(forwardMetadataInner) ? null : JsonSerializer.Deserialize<ForwardMetadataModel>(forwardMetadataInner); set => forwardMetadataInner = value == null ? null : JsonSerializer.Serialize(value); }

        [JsonPropertyName("starred")]
        public string? starredInner { private get; set; }
        [JsonIgnore]
        public StarredModel? starred { get => string.IsNullOrWhiteSpace(starredInner) ? null : JsonSerializer.Deserialize<StarredModel>(starredInner); set => starredInner = value == null ? null : JsonSerializer.Serialize(value); }

        [JsonPropertyName("edittime")]
        public string? edittimeInner { private get; set; } //done
        [JsonIgnore]
        public DateTime? edittime { get => string.IsNullOrWhiteSpace(edittimeInner) ? null : DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(edittimeInner)).UtcDateTime.ToLocalTime(); }
        
        [JsonPropertyName("deletetime")]
        public string? deletetimeInner { private get; set; } //done
        [JsonIgnore]
        public DateTime? deletetime { get => string.IsNullOrWhiteSpace(deletetimeInner) ? null : DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(deletetimeInner)).UtcDateTime.ToLocalTime(); }

        public string? isserversidegenerated { get; set; } //extraM
        //TODO: multiple images
        public string? albumId { get; set; }
        public List<Emotion>? emotions { get; set; }
        public List<Poll>? poll { get; set; }
    }

    public class Emotion
    {
        public string? key { get; set; } //main
        public List<User>? users { get; set; }
    }

    public class User
    {
        public string? mri { get; set; } //extraM
        public long? time { get; set; } //main

        [JsonPropertyName("value")]
        public string? valueInner { private get; set; }
        [JsonIgnore]
        public UserValueModel? value { get => string.IsNullOrWhiteSpace(valueInner) ? null : JsonSerializer.Deserialize<UserValueModel>(valueInner); set => valueInner = value == null ? null : JsonSerializer.Serialize(value); }
    }

    public class Poll
    {
        public string? key { get; set; } //extraM
        public List<User>? users { get; set; }
    }
#pragma warning restore IDE1006
}
