using System.IO;
using System;
using System.Text.Json;
using System.Collections.Generic;
using ReactiveUI;
using BetterSkypeParser.Models;
using System.Linq;
using HtmlAgilityPack;
using Avalonia.Media.Imaging;
using Avalonia.Layout;
using System.Text;
using System.Web;
using DynamicData;

namespace BetterSkypeParser.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string? _Id;

    public string? Id
    {
        get => _Id;
        set => this.RaiseAndSetIfChanged(ref _Id, value);
    }

    private string? _IdTT;

    public string? IdTT
    {
        get => _IdTT;
        set => this.RaiseAndSetIfChanged(ref _IdTT, value);
    }

    private string? _ExportDate;

    public string? ExportDate
    {
        get => _ExportDate;
        set => this.RaiseAndSetIfChanged(ref _ExportDate, value);
    }

    private List<Conversation>? _Conversations;

    public List<Conversation>? Conversations
    {
        get => _Conversations;
        set => this.RaiseAndSetIfChanged(ref _Conversations, value);
    }

    private Conversation? _SelectedConversation;

    public Conversation? SelectedConversation
    {
        get => _SelectedConversation;
        set => this.RaiseAndSetIfChanged(ref _SelectedConversation, value);
    }

    private string? _SelectedConversationDisplayId;

    public string? SelectedConversationDisplayId
    {
        get => _SelectedConversationDisplayId;
        set => this.RaiseAndSetIfChanged(ref _SelectedConversationDisplayId, value);
    }

    private List<Message>? _MessageList;

    public List<Message>? MessageList
    {
        get => _MessageList;
        set => this.RaiseAndSetIfChanged(ref _MessageList, value);
    }

    private int? _MessagesCount;

    public int? MessagesCount
    {
        get => _MessagesCount;
        set => this.RaiseAndSetIfChanged(ref _MessagesCount, value);
    }

    private string? currentPath;

    private readonly List<string> visitedConversations = new();

    private List<(string from, string displayName)> users = new();

    public void OpenFile(string path)
    {
        //TODO: UTC+0

        MainModel? mainModel = JsonSerializer.Deserialize<MainModel>(File.ReadAllText(path));

        currentPath = Path.GetDirectoryName(path);

        if (mainModel == null || (string.IsNullOrWhiteSpace(mainModel.userId) && !mainModel.exportDate.HasValue && mainModel.conversations == null))
        {
            // TODO: file null message

            Id = null;
            ExportDate = null;
            Conversations = null;
            SelectedConversation = null;
            return;
        }

        if (!string.IsNullOrWhiteSpace(mainModel.userId))
        {
            var parsedId = IdPrefixesParser(mainModel.userId);
            Id = parsedId.Id;
            IdTT = parsedId.Description;
        }

        ExportDate = mainModel.exportDate.HasValue ? mainModel.exportDate.Value.ToString("g") : null;

        if (mainModel.conversations != null)
        {
            users = mainModel.conversations.Where(c => c.threadProperties == null).Select(c => c.MessageList?.Where(m => !string.IsNullOrWhiteSpace(m.from) && !string.IsNullOrWhiteSpace(m.displayName)).Select(m => (from: IdPrefixesParser(m.from!).Id, displayName: m.displayName!)).Distinct()).Where(r => r != null).SelectMany(r => r!).Distinct().ToList();
            users.Add((from: Id, displayName: Lang.Resources.You)!);
            users.Remove((from: "concierge", displayName: "Skype"));

            visitedConversations.Clear();

            foreach (var conversation in mainModel.conversations)
            {
                if (!string.IsNullOrWhiteSpace(conversation.displayName))
                {
                    string[] ids = conversation.displayName.Split(", ");
                    if (ids.Length > 1)
                    {
                        conversation.displayName = string.Join(", ", ParseIdsPrefixesAndConvertToNames(ids.ToList()).Select(r => r.displayName));
                    }
                    else if (conversation.displayName == Id)
                    {
                        conversation.displayName = Lang.Resources.You;
                    }
                }

                if (conversation.properties != null && conversation.MessageList != null && conversation.MessageList.Count > 0)
                {
                    conversation.properties!.lastimreceivedtime = conversation.MessageList![0].originalarrivaltime;
                }
            }

            Conversations = mainModel.conversations.OrderByDescending(c => c.properties?.lastimreceivedtime).ToList();

            MessageList = null;
        }

        //var a = Conversations.Select(c => c.MessageList!.Select(m => m.messagetype).Distinct()).SelectMany(r => r).Distinct().ToList();

        //var htmlDoc = new HtmlDocument();
        //List<string> b = new();
        //foreach (var message in Conversations!.Select(c => c.MessageList!.Where(m => m.messagetype?.Contains("RichText") ?? false)).SelectMany(r => r))
        //{
        //    if (!string.IsNullOrWhiteSpace(message.content))
        //    {
        //        htmlDoc.LoadHtml(message.content);
        //        var docNode = htmlDoc.DocumentNode;

        //        foreach (var item in docNode.ChildNodes.Where(n => n.Name == "uriobject"))
        //        {
        //            b.Add(item.GetAttributeValue("type", string.Empty));
        //        }
        //    }
        //}
        //b = b.Distinct().ToList();
        //var q = string.Join(", ", b);
    }

    public void OpenConversation(Conversation conversation)
    {
        if (conversation.id != null && !visitedConversations.Contains(conversation.id))
        {
            var parsedId = IdPrefixesParser(conversation.id);
            conversation.DisplayId = parsedId.ToString();

            if (parsedId.Description == Lang.Resources.Cast)
            {
                conversation.IsCast = true;
            }
            else if (parsedId.Description == Lang.Resources.Thread)
            {
                conversation.IsThread = true;
            }

            if (conversation.MessageList != null)
            {
                conversation.MessageList.RemoveAll(m => m.properties?.isserversidegenerated?.ToLower().Equals("true") ?? false);

                foreach (var message in conversation.MessageList)
                {
                    //TODO: deleted type
                    if (!string.IsNullOrWhiteSpace(message.content))
                    {
                        message.content = message.content.Trim().Trim('\r', '\n');

                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(message.content);
                        //ChildNodes //Attributes
                        var docNode = htmlDoc.DocumentNode;

                        switch (message.messagetype)
                        {
                            case "Text":
                                break;
                            case "RichText":
                                message.content = null;
                                message.RichTextContent = RichTextParser(docNode.ChildNodes, message);
                                break;
                            case "RichText/Contacts":
                                message.content = $"{Lang.Resources.SentContact} ";

                                var contactRTC = docNode.ChildNodes[0].Element("c")!;
                                SetContactInfo(message, contactRTC.GetAttributeValue("s", string.Empty), contactRTC.GetAttributeValue("f", string.Empty));
                                break;
                            case "RichText/Media_Album":
                                message.content = "Album (TODO)";
                                break;
                            case "RichText/Media_AudioMsg":
                                message.IsSelectable = false;
                                message.content = Lang.Resources.AudioMessage;
                                break;
                            case "RichText/Media_CallRecording":
                                message.content = "Call Recording (TODO)";
                                break;
                            case "RichText/Media_FlikMsg":
                                message.content = GetFlikMsgContent(docNode.ChildNodes[0]);
                                break;
                            case "RichText/Media_GenericFile":
                                message.content = GetGenericFileContent(docNode.ChildNodes[0]);
                                break;
                            case "RichText/UriObject":
                                message.content = SetImageInfo(message, docNode.ChildNodes[0]);
                                break;
                            case "InviteFreeRelationshipChanged/Initialized":
                                message.content = $"{Lang.Resources.ContactRequest}:{Environment.NewLine}{Environment.NewLine}{message.content}";
                                break;
                            case "PopCard": case "Notice":
                                message.IsSelectable = false;

                                //concierge
                                List<ContentModel>? contentModels = JsonSerializer.Deserialize<List<ContentModel>>(message.content);
                                message.content = contentModels?[0].contentType;
                                break;
                            case "Event/Call":
                                message.IsSelectable = false;

                                switch (docNode.ChildNodes[0].GetAttributeValue("type", string.Empty))
                                {
                                    case "started":
                                        message.content = Lang.Resources.CallStarted;
                                        break;
                                    case "ended":
                                        message.content = Lang.Resources.CallEnded;
                                        break;
                                    case "missed":
                                        message.content = Lang.Resources.MissedCall;
                                        break;
                                }

                                //TODO: calllogs

                                //var partsEC = docNode.ChildNodes[0].Elements("part");
                                //var namesEC = partsEC.Select(p => p.Element("name").InnerText).ToArray();
                                //var durationEC = partsEC.Select(p => p.Element("duration")?.InnerText);

                                break;
                            case "ThreadActivity/AddMember":
                                List<string> idsTAAM = [docNode.ChildNodes[0].Element("initiator")!.InnerText];
                                idsTAAM.AddRange(docNode.ChildNodes[0].Elements("target").Select(t => t.InnerText));

                                message.AdditionalDisplayables = ParseIdsPrefixesAndConvertToNames(idsTAAM);
                                message.content = $"{(message.AdditionalDisplayables[0].displayable == Lang.Resources.You ? Lang.Resources.AddMemberPlural : Lang.Resources.AddMemberSingular)} {(message.AdditionalDisplayables.Count > 2 ? $"{Lang.Resources.Members}:" : Lang.Resources.Member)}";
                                break;
                            case "ThreadActivity/DeleteMember":
                                List<string> idsTADM = [docNode.ChildNodes[0].Element("initiator")!.InnerText];
                                idsTADM.AddRange(docNode.ChildNodes[0].Elements("target").Select(t => t.InnerText));

                                message.AdditionalDisplayables = ParseIdsPrefixesAndConvertToNames(idsTADM);

                                if (idsTADM.Count == 2 && idsTADM[0] == idsTADM[1])
                                {
                                    message.AdditionalDisplayables = message.AdditionalDisplayables.Take(1).ToList();
                                    message.content = $"{(message.AdditionalDisplayables[0].displayable == Lang.Resources.You ? Lang.Resources.DeleteYourselfPlural : Lang.Resources.DeleteYourselfSingular)} {(conversation.IsCast ? Lang.Resources.CastAccusative : Lang.Resources.ThreadAccusative)}";
                                }
                                else
                                {
                                    message.content = $"{(message.AdditionalDisplayables[0].displayable == Lang.Resources.You ? Lang.Resources.DeleteMemberPlural : Lang.Resources.DeleteMemberSingular)} {(message.AdditionalDisplayables.Count > 2 ? $"{Lang.Resources.Members}:" : Lang.Resources.Member)}";
                                }
                                break;
                            case "ThreadActivity/HistoryDisclosedUpdate":
                                message.AdditionalDisplayables = ParseIdsPrefixesAndConvertToNames([docNode.ChildNodes[0].Element("initiator")!.InnerText]);

                                bool isDisclosedTAHDU = docNode.ChildNodes[0].Element("value")!.InnerText.ToLower().Equals("true");
                                bool isPluralTAHDU = message.AdditionalDisplayables[0].displayable == Lang.Resources.You;

                                string statusTAHDU = isDisclosedTAHDU
                                    ? (isPluralTAHDU ? Lang.Resources.HistoryDisclosedPlural : Lang.Resources.HistoryDisclosedSingular)
                                    : (isPluralTAHDU ? Lang.Resources.HistoryHiddenPlural : Lang.Resources.HistoryHiddenSingular);

                                message.content = $"{statusTAHDU} {Lang.Resources.MessageHistory}";
                                break;
                            case "ThreadActivity/JoiningEnabledUpdate":
                                message.AdditionalDisplayables = ParseIdsPrefixesAndConvertToNames([docNode.ChildNodes[0].Element("initiator")!.InnerText]);

                                bool isEnabledTAJEU = docNode.ChildNodes[0].Element("value")!.InnerText.ToLower().Equals("true");
                                bool isPluralTAJEU = message.AdditionalDisplayables[0].displayable == Lang.Resources.You;

                                string statusTAJEU = isEnabledTAJEU
                                    ? (isPluralTAJEU ? Lang.Resources.JoiningEnabledPlural : Lang.Resources.JoiningEnabledSingular)
                                    : (isPluralTAJEU ? Lang.Resources.JoiningDisabledPlural : Lang.Resources.JoiningDisabledSingular);

                                message.content = $"{statusTAJEU} {Lang.Resources.Joining} {(conversation.IsCast ? Lang.Resources.CastDative : Lang.Resources.ThreadDative)}";
                                break;
                            case "ThreadActivity/RoleUpdate":
                                List<string> idsTARU = [docNode.ChildNodes[0].Element("initiator")!.InnerText];

                                var targetTARU = docNode.ChildNodes[0].Elements("target").ElementAt(0);
                                idsTARU.Add(targetTARU.Element("id")!.InnerText);

                                string roleTARU = targetTARU.Element("role")!.InnerText.ToLower();
                                switch (roleTARU)
                                {
                                    case "user":
                                        roleTARU = Lang.Resources.UserRole;
                                        break;
                                    case "admin":
                                        roleTARU = Lang.Resources.AdminRole;
                                        break;
                                    default:
                                        break;
                                }

                                message.AdditionalDisplayables = ParseIdsPrefixesAndConvertToNames(idsTARU);
                                message.content = $"{(message.AdditionalDisplayables[0].displayable == Lang.Resources.You ? Lang.Resources.RoleUpdatePlural : Lang.Resources.RoleUpdateSingular)} {string.Format(Lang.Resources.RoleUpdateFormat, roleTARU)}";
                                break;
                            case "ThreadActivity/TopicUpdate":
                                message.AdditionalDisplayables = ParseIdsPrefixesAndConvertToNames([docNode.ChildNodes[0].Element("initiator")!.InnerText]);
                                message.content = $"{(message.AdditionalDisplayables[0].displayable == Lang.Resources.You ? Lang.Resources.TopicUpdatePlural : Lang.Resources.TopicUpdateSingular)} {Lang.Resources.TopicUpdateNameTo} «{docNode.ChildNodes[0].Element("value")!.InnerText}»";
                                break;
                            default:
                                message.content = $"{Lang.Resources.MessageTypeUnknown}{Environment.NewLine}{Environment.NewLine}{message.content}";
                                break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(message.from))
                    {
                        var parsedIdFrom = IdPrefixesParser(message.from);
                        message.from = parsedIdFrom.ToString();

                        if (parsedIdFrom.Id == Id)
                        {
                            message.displayName = Lang.Resources.You;
                            message.HorizontalAlignment = HorizontalAlignment.Right;
                        }
                        else if (parsedIdFrom.Id.StartsWith("ID: "))
                        {
                            message.HorizontalAlignment = HorizontalAlignment.Center;
                            message.HasBorder = true;
                        }
                        else
                        {
                            message.displayName ??= parsedIdFrom.Id;
                        }
                    }
                }

                List<Message> MessageList = new();

                foreach (var group in conversation.MessageList.Where(m => m.originalarrivaltime.HasValue).OrderBy(m => m.originalarrivaltime).GroupBy(m => (m.originalarrivaltime!.Value.Day, m.originalarrivaltime!.Value.Month, m.originalarrivaltime!.Value.Year)))
                {
                    var messages = group.Select(g => g);
                    MessageList.Add(new Message() { content = messages.ToList()[0].originalarrivaltime!.Value.ToShortDateString(), HorizontalAlignment = HorizontalAlignment.Center });
                    MessageList.AddRange(messages);
                }

                conversation.MessageList = MessageList.ToList();
            }

            visitedConversations.Add(conversation.id);
        }

        SelectedConversationDisplayId = conversation.DisplayId;
        MessageList = conversation.MessageList;
        MessagesCount = conversation.MessageList?.Count(m => m.HorizontalAlignment != HorizontalAlignment.Center);
    }

    private List<(string type, string content)> RichTextParser(HtmlNodeCollection htmlNodes, Message message)
    {
        List<(string type, string content)> richTextContent = new();
        foreach (var node in htmlNodes)
        {
            switch (node.Name)
            {
                case "#text": case "b": case "i": case "u": case "s":
                    richTextContent.Add((node.Name, HttpUtility.HtmlDecode(node.InnerText)));
                    break;
                case "a":
                    richTextContent.Add((node.Name, node.GetAttributeValue("href", string.Empty)));
                    break;
                case "ss":
                    //TODO: ss pictures (file: picture - aliases)
                    richTextContent.Add((node.Name, $" {node.InnerText} "));
                    break;
                case "quote":
                    message.AdditionalDisplayables ??= new();

                    ParsedId authorId = IdPrefixesParser(node.GetAttributeValue("author", string.Empty));
                    message.AdditionalDisplayables.Add((users.FirstOrDefault(u => u.from == authorId.Id).displayName ?? node.GetAttributeValue("authorname", string.Empty), authorId.ToString()));

                    message.AdditionalDisplayables.Add((DateTimeOffset.FromUnixTimeSeconds(long.Parse(node.GetAttributeValue("timestamp", string.Empty))).UtcDateTime.ToLocalTime().ToString("G"), string.Empty));

                    string conversationId = node.GetAttributeValue("conversation", string.Empty);
                    richTextContent.Add(("quote_start", $"{(IdPrefixesParser(conversationId).Id == Id ? "fbt:this" : conversationId)}\n{node.GetAttributeValue("messageid", string.Empty)}"));
                    richTextContent.AddRange(RichTextParser(node.ChildNodes, message));
                    richTextContent.Add(("quote_end", string.Empty));
                    break;
                case "uriobject":
                    //1657698102966 //1722509325286 //1715247248450
                    //1674819333738 //1674819153253 //1707682642972

                    message.AdditionalDisplayables ??= new();

                    switch (node.GetAttributeValue("type", string.Empty))
                    {
                        case "Picture.1":
                            richTextContent.Add((node.Name, SetImageInfo(message, node)));
                            break;
                        case "SWIFT.1":
                            RichTextCardModel? richTextCardModel = JsonSerializer.Deserialize<RichTextCardModel>(Encoding.UTF8.GetString(Convert.FromBase64String(node.Element("swift")!.GetAttributeValue("b64", string.Empty))));

                            if (richTextCardModel != null)
                            {
                                richTextContent.Add((node.Name, $"{richTextCardModel.attachments![0].content!.images![0].alt} ({richTextCardModel.attachments![0].content!.subtitle})"));
                                message.AdditionalDisplayables.Add((richTextCardModel.attachments![0].content!.images![0].url!, string.Empty));

                                message.GifPlayer = new();
                            }
                            else
                            {
                                richTextContent.Add((node.Name, Lang.Resources.UnknownGIFFile));
                            }
                            break;
                        case "Audio.1":
                            richTextContent.Add((node.Name, Lang.Resources.AudioMessage));
                            break;
                        case "Video.2/CallRecording.1":
                            //TODO: call recording second
                            richTextContent.Add((node.Name, "Call Recording (TODO)"));
                            break;
                        case "Video.1/Flik.1":
                            richTextContent.Add((node.Name, GetFlikMsgContent(node)));
                            break;
                        case string s when s.StartsWith("File."):
                            richTextContent.Add((node.Name, GetGenericFileContent(node)));
                            break;
                        default:
                            richTextContent.Add((node.Name, $"{Lang.Resources.FileTypeUnknown}{Environment.NewLine}{Environment.NewLine}{node.OuterHtml}"));
                            break;
                    }
                    break;
                case "at":
                    SetContactInfo(message, node.GetAttributeValue("id", string.Empty), node.InnerText);
                    richTextContent.Add((node.Name, string.Empty));
                    break;
                case "e_m": case "c_i": case "legacyquote":
                    break;
                default:
                    richTextContent.Add((node.Name, $"{Lang.Resources.MessageTypeUnknown}{Environment.NewLine}{Environment.NewLine}{node.OuterHtml}"));
                    break;
            }
        }

        return richTextContent;
    }

    private void SetContactInfo(Message message, string contactId, string contactName)
    {
        message.AdditionalDisplayables ??= new();

        var contactConversation = Conversations!.FirstOrDefault(c => c.id?.Contains(contactId) ?? false);

        message.ContactId = contactConversation != null ? contactConversation!.id : string.Empty;
        message.AdditionalDisplayables.Add((contactName, contactConversation == null ? IdPrefixesParser(contactId).ToString() : string.Empty));
    }

    private static string GetFlikMsgContent(HtmlNode node) => $"{Lang.Resources.Moji}: {node.Element("originalname")!.GetAttributeValue("v", string.Empty)}";

    private static string GetGenericFileContent(HtmlNode node) => $"{Lang.Resources.File}: {node.Element("originalname")!.GetAttributeValue("v", string.Empty)} ({BitsToString.Convert(node.Element("filesize")!.GetAttributeValue("v", 0L))})";

    private string SetImageInfo(Message message, HtmlNode node)
    {
        bool fileNotFound = false;
        string fileName = message.amsreferences != null ? message.amsreferences[0] : node.GetAttributeValue("doc_id", string.Empty);
        if (!string.IsNullOrEmpty(fileName))
        {
            var mediaDirectory = new DirectoryInfo($"{currentPath}{Path.DirectorySeparatorChar}media{Path.DirectorySeparatorChar}");

            if (mediaDirectory.Exists)
            {
                var imageFile = mediaDirectory.GetFiles($"{fileName}.1*", SearchOption.TopDirectoryOnly);

                if (imageFile.Length > 0)
                {
                    using FileStream fstream = new(imageFile[0].FullName, FileMode.Open);
                    message.image = new Bitmap(fstream);
                }
                else
                {
                    fileNotFound = true;
                }
            }
            else
            {
                fileNotFound = true;
            }
        }
        else
        {
            fileNotFound = true;
        }

        string originalName = node.Element("originalname")!.GetAttributeValue("v", string.Empty);
        bool originalNameIsNullOrWhiteSpace = string.IsNullOrWhiteSpace(originalName);

        return $"{(fileNotFound ? Lang.Resources.NoImageAvailable : string.Empty)}{(!originalNameIsNullOrWhiteSpace && fileNotFound ? Environment.NewLine : string.Empty)}{(originalNameIsNullOrWhiteSpace ? string.Empty : $"{originalName} ({BitsToString.Convert(node.Element("filesize")!.GetAttributeValue("v", 0L))})")}";
    }

    private List<(string displayName, string parsedId)> ParseIdsPrefixesAndConvertToNames(List<string> ids)
    {
        List<(string, string)> result = new();

        for (int i = 0; i < ids.Count; i++)
        {
            var parsedId = IdPrefixesParser(ids[i]);
            result.Add((users.FirstOrDefault(u => u.from == parsedId.Id).displayName ?? (parsedId.Id.StartsWith("ID: ") ? parsedId.Description  : parsedId.Id), parsedId.ToString()));
        }

        return result;
    }

    private record ParsedId(string Id, string Separator, string Description)
    {
        public override string ToString() => string.Join("", Id, Separator, Description);
    }

    private static ParsedId IdPrefixesParser(string id)
    {
        //TODO: 48
        List<string> prefixesList = new();
        StringBuilder clearIdBuilder = new();
        bool prefix19 = false;

        foreach (string prefix in id.Split(':'))
        {
            switch (prefix)
            {
                case "4": prefixesList.Add(Lang.Resources.PhoneNumber); break;
                case "8": prefixesList.Add("Skype"); break;
                case "19": prefix19 = true; break;
                case "28": prefixesList.Add($"Skype {Lang.Resources.SystemAccount}"); break;
                case "live": prefixesList.Add("Microsoft"); break;
                case "guest": prefixesList.Add(Lang.Resources.GuestAccount); break;
                default:
                    if (prefix19)
                    {
                        string[] prefix19parts = prefix.Split('@');
                        prefixesList.Add(prefix19parts[1] == "cast.skype" ? Lang.Resources.Cast : Lang.Resources.Thread);
                        clearIdBuilder.Append($"{prefix19parts[0]}");
                    }
                    else
                    {
                        clearIdBuilder.Append($"{prefix}");
                    }
                    break;
            }
        }

        return new ParsedId
        (
            prefix19 ? $"ID: {clearIdBuilder}" : clearIdBuilder.ToString(),
            prefixesList.Count > 0 ? Environment.NewLine : string.Empty,
            prefix19 ? prefixesList[0] : (prefixesList.Count > 0 ? $"{Lang.Resources.AccountLinkedTo} {string.Join(", ", prefixesList)}" : string.Empty)
        );
    }
}
