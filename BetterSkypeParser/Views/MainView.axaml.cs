using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Avalonia.Layout;
using BetterSkypeParser.Models;
using BetterSkypeParser.ViewModels;
using DynamicData;
using System;
using System.Linq;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Controls.Primitives;
using System.Globalization;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BetterSkypeParser.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
    }

    private async void ConversationsLB_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            ((MainViewModel)DataContext!).OpenConversation((Conversation)e.AddedItems[0]!);

            if (MessagesLB.Items != null && MessagesLB.Items.Count > 0)
            {
                MessagesLB.IsVisible = true;
                await Dispatcher.UIThread.InvokeAsync(() => MessagesLB.ScrollIntoView(MessagesLB.Items[MessagesLB.Items.Count-1]!));
            }
            else
            {
                MessagesLB.IsVisible = false;
            }
        }
    }

    private async void OpenFileB_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this)!;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang.Resources.OpenFile,
            AllowMultiple = false,
            SuggestedFileName = "messages.json",
            FileTypeFilter = [new FilePickerFileType(Lang.Resources.JSONFile) { Patterns = ["*.json"] }]
        });

        if (files.Count > 0)
        {
            ((MainViewModel)DataContext!).OpenFile(files[0].Path.LocalPath);
        }

        if (ConversationsLB.Items != null && ConversationsLB.Items.Count > 0)
        {
            await Dispatcher.UIThread.InvokeAsync(() => ConversationsLB.ScrollIntoView(ConversationsLB.Items[0]!));
        }
    }

    private void OpenImageB_Click(object? sender, RoutedEventArgs e)
    {
        ConversationsDP.IsEnabled = false;
        MessagesDP.IsEnabled = false;

        BigImageTB.IsChecked = true;

        BigImageI.Source = ((Image)((Button)sender!).Content!).Source;
        BigImageI.MaxWidth = BigImageI.Source!.Size.Width;
        BigImageI.MaxHeight = BigImageI.Source!.Size.Height;
    }

    private async void AboutB_Click(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams()
            {
                ContentTitle = Lang.Resources.About,
                ContentHeader = Lang.Resources.About,
                ContentMessage = $"Better Skype Parser v.0.5{Environment.NewLine}FireBoTer, 2025{Environment.NewLine}fireboter@yahoo.com" +
                                 (new Random().Next(4) == 0 ? $"{Environment.NewLine}All wrongs reserved" : string.Empty),
                WindowIcon = ((Window)this.Parent!).Icon!,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                HyperLinkParams = new HyperLinkParams
                {
                    Text = "  GitHub",
                    Action = new Action(() =>
                    {
                        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        var url = "https://github.com/FireBoT-er/BetterSkypeParser";

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            using var proc = new Process { StartInfo = { UseShellExecute = true, FileName = url } };
                            proc.Start();
                            return;
                        }

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            Process.Start("x-www-browser", url);
                            return;
                        }

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            Process.Start("open", url);
                            return;
                        }

                        throw new Exception("invalid url: " + url);
                    })
                }
            });

        await box.ShowWindowDialogAsync((Window)this.Parent!);
    }

    private void OpenImageB_Unloaded(object? sender, RoutedEventArgs e)
    {
        ((Message?)((Button)sender!).DataContext)?.GifPlayer?.Dispose();
    }

    private void BigImageTB_Click(object? sender, RoutedEventArgs e)
    {
        ConversationsDP.IsEnabled = true;
        MessagesDP.IsEnabled = true;
    }

    private void OpenExtra(object? sender, RoutedEventArgs e)
    {
        var extraWindow = new ExtraWindow() /*{ DataContext = new ExtraWindowViewModel() }*/;
        extraWindow.ShowDialog((Window)this.Parent!);
    }

    private void GoToContact(object? sender, RoutedEventArgs e)
    {
        var conversation = ConversationsLB.Items.Cast<Conversation>().First(c => c.id == ((Button)sender!).CommandParameter!.ToString());
        ConversationsLB.SelectedItem = conversation;
    }

    private async void GoToMessage(object? sender, RoutedEventArgs e)
    {
        Button senderButton = (Button)sender!;
        if (senderButton.Content is StackPanel stackPanel)
        {
            if (stackPanel.Children.FirstOrDefault(c => c is Button)?.IsPointerOver ?? false)
            {
                return;
            }
        }

        (Conversation conversation, string messageid) = ((Conversation, string))senderButton.CommandParameter!;
        ConversationsLB.SelectedItem = conversation;

        Message message = MessagesLB.Items.Cast<Message>().First(m => m.id == messageid);
        MessagesLB.SelectedItem = message;
        await Dispatcher.UIThread.InvokeAsync(() => MessagesLB.ScrollIntoView(message));
    }

    private void SystemMessagesWP_Loaded(object? sender, RoutedEventArgs e)
    {
        WrapPanel wrapPanel = (WrapPanel)sender!;
        Message message = (Message)wrapPanel.DataContext!;

        if (message.AdditionalDisplayables != null && message.HasBorder)
        {
            var textBlockInner = new TextBlock() { Text = message.AdditionalDisplayables[0].displayable, Margin = new Thickness(0, 0, 3, 0), FontWeight = FontWeight.DemiBold };

            ToolTip.SetTip(textBlockInner, message.AdditionalDisplayables[0].toolTip);
            ToolTip.SetShowOnDisabled(textBlockInner, true);

            wrapPanel.Children.Add(textBlockInner);
        }

        wrapPanel.Children.Add(new TextBlock() { Text = message.content, Margin = new Thickness(0, 0, 3, 0) });

        if (message.AdditionalDisplayables != null && message.HasBorder)
        {
            for (int i = 1; i < message.AdditionalDisplayables.Count; i++)
            {
                var textBlockInner = new TextBlock() { Text = $"{message.AdditionalDisplayables[i].displayable}{(i == message.AdditionalDisplayables.Count-1 ? string.Empty : ",")}",
                                                       Margin = new Thickness(0, 0, 3, 0), FontWeight = FontWeight.DemiBold };

                ToolTip.SetTip(textBlockInner, message.AdditionalDisplayables[i].toolTip);
                ToolTip.SetShowOnDisabled(textBlockInner, true);

                wrapPanel.Children.Add(textBlockInner);
            }
        }
    }

    private async void MessageContentSP_Loaded(object? sender, RoutedEventArgs e)
    {
        StackPanel stackPanel = (StackPanel)sender!;
        Message message = (Message)stackPanel.DataContext!;
        SelectableTextBlock selectableTextBlock = CreateSTB(message);

        switch (message.messagetype)
        {
            case "RichText":
                if (message.RichTextContent != null)
                {
                    SelectableTextBlock currentSTB = selectableTextBlock;
                    Control? quoteControl = null;

                    int additionalPosition = 0;
                    bool addSTB = true;

                    foreach (var (type, content) in message.RichTextContent)
                    {
                        if (type != "quote_start" && type != "quote_end" && type != "uriobject" && addSTB)
                        {
                            stackPanel.Children.Add(currentSTB);
                            addSTB = false;
                        }

                        switch (type)
                        {
                            case "a":
                                currentSTB.Inlines!.Add(new HyperlinkButton() { Content = content.StartsWith("mailto:") ? content.Remove(0, 7) : content, NavigateUri = new Uri(content), Padding = new Thickness(1, 0, 1, 3), MaxWidth = stackPanel.MaxWidth });
                                break;
                            case "ss":
                                var selectableTextBlockSS = new SelectableTextBlock() { Text = content, TextWrapping = TextWrapping.Wrap, FontStyle = FontStyle.Italic };
                                ToolTip.SetTip(selectableTextBlockSS, Lang.Resources.Emoticon);

                                currentSTB.Inlines!.Add(selectableTextBlockSS);
                                break;
                            case "b":
                                currentSTB.Inlines!.Add(new Run(content) { FontWeight = FontWeight.DemiBold, BaselineAlignment = BaselineAlignment.Center });
                                break;
                            case "i":
                                currentSTB.Inlines!.Add(new Run(content) { FontStyle = FontStyle.Italic, BaselineAlignment = BaselineAlignment.Center });
                                break;
                            case "u":
                                currentSTB.Inlines!.Add(new Run(content) { TextDecorations = TextDecorations.Underline, BaselineAlignment = BaselineAlignment.Center });
                                break;
                            case "s":
                                currentSTB.Inlines!.Add(new Run(content) { TextDecorations = TextDecorations.Strikethrough, BaselineAlignment = BaselineAlignment.Center });
                                break;
                            case "quote_start":
                                TextBlock quoteAuthor = new() { Text = message.AdditionalDisplayables![additionalPosition].displayable, FontWeight = FontWeight.DemiBold, IsEnabled = false };
                                ToolTip.SetTip(quoteAuthor, message.AdditionalDisplayables[additionalPosition++].toolTip);
                                ToolTip.SetShowOnDisabled(quoteAuthor, true);

                                SelectableTextBlock selectableTextBlockQuote = new() { TextWrapping = TextWrapping.Wrap };

                                string[] conversation_messageid = content.Split('\n');
                                Button? goToMessage = null;
                                if (!string.IsNullOrWhiteSpace(conversation_messageid[1]))
                                {
                                    Conversation? conversation = conversation_messageid[0] == "fbt:this" ? (Conversation)ConversationsLB.SelectedItem! : ConversationsLB.Items.Cast<Conversation>().FirstOrDefault(c => c.id == conversation_messageid[0]);

                                    if (conversation != null)
                                    {
                                        selectableTextBlockQuote.IsEnabled = false;

                                        goToMessage = new() { Content = selectableTextBlockQuote, CommandParameter = (conversation, conversation_messageid[1]), Background = Brush.Parse("#53535340"), Padding = new Thickness(5) };
                                        goToMessage.Click += GoToMessage;
                                        goToMessage.Cursor = new Cursor(StandardCursorType.Arrow);
                                    }
                                }

                                TextBlock quoteTime = new() { Text = message.AdditionalDisplayables![additionalPosition++].displayable, Foreground = Brushes.Gray, IsEnabled = false };

                                StackPanel stackPanelInner = new() { Spacing = 5 };
                                stackPanelInner.Children.Add(quoteAuthor);
                                stackPanelInner.Children.Add(goToMessage != null ? goToMessage : selectableTextBlockQuote);
                                stackPanelInner.Children.Add(quoteTime);

                                stackPanel.Children.Add(new Border() { BorderThickness = new Thickness(2, 0, 0, 0), BorderBrush = Brush.Parse("#40FFFFFF"), Padding = new Thickness(7, 0, 0, 0), Child = stackPanelInner });

                                currentSTB = selectableTextBlockQuote;
                                addSTB = false;

                                quoteControl = goToMessage != null ? goToMessage : stackPanelInner;
                                break;
                            case "quote_end":
                                currentSTB = CreateSTB(message);
                                addSTB = true;

                                quoteControl = null;
                                break;
                            case "uriobject":
                                StackPanel currentSP = stackPanel;

                                if (quoteControl != null)
                                {
                                    currentSP = new() { Spacing = 5 };

                                    if (quoteControl is StackPanel panel)
                                    {
                                        panel.Children[1] = currentSP;
                                    }
                                    else
                                    {
                                        ((Button)quoteControl).Content = currentSP;
                                    }

                                    if (!string.IsNullOrWhiteSpace(currentSTB.Text) || currentSTB.Inlines?.Count > 0)
                                    {
                                        currentSP.Children.Add(currentSTB);
                                    }
                                }

                                if (message.image != null || message.GifPlayer != null)
                                {
                                    var openImageB = DisplayImage(message, currentSP);

                                    if (message.GifPlayer != null)
                                    {
                                        try
                                        {
                                            var gifPlayer = new GifPlayer((Image)openImageB.Content!, message.AdditionalDisplayables![additionalPosition++].displayable);
                                            message.GifPlayer = gifPlayer;
                                            await gifPlayer.Play();

                                            openImageB.IsHitTestVisible = false;
                                        }
                                        catch
                                        {
                                            stackPanel.Children.Remove(openImageB);
                                            message.GifPlayer?.Dispose();

                                            currentSTB = CreateSTB(message);
                                            currentSTB.Text = Lang.Resources.GIFNotLoaded;
                                            currentSTB.Foreground = Brushes.Gray;
                                            currentSTB.IsEnabled = false;

                                            currentSP.Children.Add(currentSTB);
                                        }
                                    }
                                }

                                currentSTB = CreateSTB(message);
                                currentSTB.Text = content;
                                currentSP.Children.Add(currentSTB);

                                currentSTB = CreateSTB(message);
                                addSTB = true;
                                break;
                            case "at":
                                DisplayContact(message, currentSTB, additionalPosition++);
                                break;
                            case "#text": default:
                                currentSTB.Inlines!.Add(new Run(content) { BaselineAlignment = BaselineAlignment.Center });
                                break;
                        }
                    }
                }
                break;
            case "RichText/Contacts":
                DisplayBasicText(message.content, stackPanel, selectableTextBlock);
                DisplayContact(message, selectableTextBlock, 0);
                break;
            case "RichText/UriObject":
                DisplayImage(message, stackPanel);
                DisplayBasicText(message.content, stackPanel, selectableTextBlock);
                break;
            default:
                DisplayBasicText(message.content, stackPanel, selectableTextBlock);
                break;
        }
    }

    private static SelectableTextBlock CreateSTB(Message message) => new() { TextWrapping = TextWrapping.Wrap, HorizontalAlignment = message.HorizontalAlignment, IsEnabled = message.IsSelectable };

    private static void DisplayBasicText(string? content, StackPanel stackPanel, SelectableTextBlock selectableTextBlock)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            stackPanel.Children.Add(selectableTextBlock);
            selectableTextBlock.Inlines!.Add(new Run(content) { BaselineAlignment = BaselineAlignment.Center });
        }
    }

    private void DisplayContact(Message message, SelectableTextBlock selectableTextBlock, int additionalPosition)
    {
        var selectableTextBlockInner = new SelectableTextBlock() { Text = message.AdditionalDisplayables![additionalPosition].displayable, FontWeight = FontWeight.DemiBold, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };

        if (!string.IsNullOrWhiteSpace(message.ContactId))
        {
            selectableTextBlockInner.IsEnabled = false;

            var button = new Button() { Content = selectableTextBlockInner, CommandParameter = message.ContactId };
            button.Click += GoToContact;
            button.Cursor = new Cursor(StandardCursorType.Arrow);

            ToolTip.SetTip(button, Lang.Resources.GoToContact);

            selectableTextBlock.Inlines!.Add(button);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(message.AdditionalDisplayables[additionalPosition].toolTip))
            {
                ToolTip.SetTip(selectableTextBlockInner, message.AdditionalDisplayables[additionalPosition].toolTip);
                ToolTip.SetShowOnDisabled(selectableTextBlockInner, true);
            }

            selectableTextBlock.Inlines!.Add(selectableTextBlockInner);
        }
    }

    private Button DisplayImage(Message message, StackPanel stackPanel)
    {
        Button button = new() { HorizontalAlignment = message.HorizontalAlignment };
        button.Click += OpenImageB_Click;
        button.Unloaded += OpenImageB_Unloaded;
        button.Content = new Image() { Source = message.image, MaxHeight = 300, StretchDirection = StretchDirection.DownOnly };

        stackPanel.Children.Add(button);

        return button;
    }
}
