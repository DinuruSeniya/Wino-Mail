﻿using System;
using System.Globalization;
using System.Linq;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Wino.Core.Domain;
using Wino.Core.Domain.Entities.Shared;
using Wino.Core.Domain.Enums;
using Wino.Core.Domain.Models.MailItem;
using Wino.Core.UWP.Controls;

namespace Wino.Helpers;

public static class XamlHelpers
{
    private const string TwentyFourHourTimeFormat = "HH:mm";
    private const string TwelveHourTimeFormat = "hh:mm tt";

    #region Converters

    public static bool IsMultiple(int count) => count > 1;
    public static bool ReverseIsMultiple(int count) => count < 1;
    public static PopupPlacementMode GetPlaccementModeForCalendarType(CalendarDisplayType type)
    {
        return type switch
        {
            CalendarDisplayType.Week => PopupPlacementMode.Right,
            _ => PopupPlacementMode.Bottom,
        };
    }

    public static Visibility ReverseBoolToVisibilityConverter(bool value) => value ? Visibility.Collapsed : Visibility.Visible;
    public static Visibility ReverseVisibilityConverter(Visibility visibility) => visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    public static bool ReverseBoolConverter(bool value) => !value;
    public static bool ShouldDisplayPreview(string text) => text.Any(x => char.IsLetter(x));
    public static bool CountToBooleanConverter(int value) => value > 0;
    public static bool ObjectEquals(object obj1, object obj2) => object.Equals(obj1, obj2);
    public static Visibility CountToVisibilityConverter(int value) => value > 0 ? Visibility.Visible : Visibility.Collapsed;
    public static Visibility CountToVisibilityConverterWithThreshold(int value, int threshold) => value > threshold ? Visibility.Visible : Visibility.Collapsed;
    public static InfoBarSeverity InfoBarSeverityConverter(InfoBarMessageType messageType)
    {
        return messageType switch
        {
            InfoBarMessageType.Information => InfoBarSeverity.Informational,
            InfoBarMessageType.Success => InfoBarSeverity.Success,
            InfoBarMessageType.Warning => InfoBarSeverity.Warning,
            InfoBarMessageType.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational,
        };
    }

    public static SolidColorBrush GetReadableTextColor(string backgroundColor)
    {
        if (!backgroundColor.StartsWith("#")) throw new ArgumentException("Hex color must start with #.");

        backgroundColor = backgroundColor.TrimStart('#');

        if (backgroundColor.Length == 6)
        {
            var r = int.Parse(backgroundColor.Substring(0, 2), NumberStyles.HexNumber);
            var g = int.Parse(backgroundColor.Substring(2, 2), NumberStyles.HexNumber);
            var b = int.Parse(backgroundColor.Substring(4, 2), NumberStyles.HexNumber);

            // Calculate relative luminance
            double luminance = (0.2126 * GetLinearValue(r)) +
                               (0.7152 * GetLinearValue(g)) +
                               (0.0722 * GetLinearValue(b));

            return luminance > 0.5 ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
        }
        else
        {
            throw new ArgumentException("Hex color must be 6 characters long (e.g., #RRGGBB).");
        }
    }

    private static double GetLinearValue(int colorComponent)
    {
        double sRGB = colorComponent / 255.0;
        return sRGB <= 0.03928 ? sRGB / 12.92 : Math.Pow((sRGB + 0.055) / 1.055, 2.4);
    }

    public static Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode NavigationViewDisplayModeConverter(SplitViewDisplayMode splitViewDisplayMode)
    {
        return splitViewDisplayMode switch
        {
            SplitViewDisplayMode.CompactOverlay => Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Compact,
            SplitViewDisplayMode.CompactInline => Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal,
            SplitViewDisplayMode.Overlay => Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Expanded,
            SplitViewDisplayMode.Inline => Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Expanded,
            _ => Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal,
        };
    }

    public static string GetColorFromHex(Color color) => color.ToHex();
    public static Color GetWindowsColorFromHex(string hex) => hex.ToColor();

    public static SolidColorBrush GetSolidColorBrushFromHex(string colorHex) => string.IsNullOrEmpty(colorHex) ? new SolidColorBrush(Colors.Transparent) : new SolidColorBrush(colorHex.ToColor());
    public static Visibility IsSelectionModeMultiple(ListViewSelectionMode mode) => mode == ListViewSelectionMode.Multiple ? Visibility.Visible : Visibility.Collapsed;
    public static FontWeight GetFontWeightBySyncState(bool isSyncing) => isSyncing ? FontWeights.SemiBold : FontWeights.Normal;
    public static FontWeight GetFontWeightByChildSelectedState(bool isChildSelected) => isChildSelected ? FontWeights.SemiBold : FontWeights.Normal;
    public static Visibility StringToVisibilityConverter(string value) => string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible;
    public static Visibility StringToVisibilityReversedConverter(string value) => string.IsNullOrWhiteSpace(value) ? Visibility.Visible : Visibility.Collapsed;
    public static string GetMailItemDisplaySummaryForListing(bool isDraft, DateTime receivedDate, bool prefer24HourTime)
    {
        if (isDraft)
            return Translator.Draft;
        else
        {
            var localTime = receivedDate.ToLocalTime();

            return prefer24HourTime ? localTime.ToString(TwentyFourHourTimeFormat) : localTime.ToString(TwelveHourTimeFormat);
        }
    }
    public static string GetCreationDateString(DateTime date, bool prefer24HourTime)
    {
        var localTime = date.ToLocalTime();
        return $"{localTime.ToString("D", CultureInfo.DefaultThreadCurrentUICulture)} {(prefer24HourTime ? localTime.ToString(TwentyFourHourTimeFormat) : localTime.ToString(TwelveHourTimeFormat))}";
    }
    public static string GetMailGroupDateString(object groupObject)
    {
        if (groupObject is string stringObject)
            return stringObject;

        object dateObject = null;

        // From regular mail header template
        if (groupObject is DateTime groupedDate)
            dateObject = groupedDate;
        else if (groupObject is IGrouping<object, IMailItem> groupKey)
        {
            // From semantic group header.
            dateObject = groupKey.Key;
        }

        if (dateObject != null)
        {
            if (dateObject is DateTime dateTimeValue)
            {
                if (dateTimeValue == DateTime.Today)
                    return Translator.Today;
                else if (dateTimeValue == DateTime.Today.AddDays(-1))
                    return Translator.Yesterday;
                else
                {
                    return dateTimeValue.ToString("D", CultureInfo.DefaultThreadCurrentUICulture);
                }

            }
            else
                return dateObject.ToString();
        }

        return Translator.UnknownDateHeader;
    }
    public static bool ConnectionStatusEquals(WinoServerConnectionStatus winoServerConnectionStatus, WinoServerConnectionStatus connectionStatus) => winoServerConnectionStatus == connectionStatus;

    #endregion

    #region Wino Font Icon Transformation

    public static WinoIconGlyph GetWinoIconGlyph(FilterOptionType type) => type switch
    {
        FilterOptionType.All => WinoIconGlyph.Mail,
        FilterOptionType.Unread => WinoIconGlyph.MarkUnread,
        FilterOptionType.Flagged => WinoIconGlyph.Flag,
        FilterOptionType.Mentions => WinoIconGlyph.NewMail,
        FilterOptionType.Files => WinoIconGlyph.Attachment,
        _ => WinoIconGlyph.None,
    };

    public static WinoIconGlyph GetWinoIconGlyph(SortingOptionType type) => type switch
    {
        SortingOptionType.Sender => WinoIconGlyph.SortTextDesc,
        SortingOptionType.ReceiveDate => WinoIconGlyph.SortLinesDesc,
        _ => WinoIconGlyph.None,
    };

    public static WinoIconGlyph GetWinoIconGlyph(MailOperation operation)
    {
        return operation switch
        {
            MailOperation.None => WinoIconGlyph.None,
            MailOperation.Archive => WinoIconGlyph.Archive,
            MailOperation.UnArchive => WinoIconGlyph.UnArchive,
            MailOperation.SoftDelete => WinoIconGlyph.Delete,
            MailOperation.HardDelete => WinoIconGlyph.Delete,
            MailOperation.Move => WinoIconGlyph.Forward,
            MailOperation.MoveToJunk => WinoIconGlyph.Blocked,
            MailOperation.MoveToFocused => WinoIconGlyph.None,
            MailOperation.MoveToOther => WinoIconGlyph.None,
            MailOperation.AlwaysMoveToOther => WinoIconGlyph.None,
            MailOperation.AlwaysMoveToFocused => WinoIconGlyph.None,
            MailOperation.SetFlag => WinoIconGlyph.Flag,
            MailOperation.ClearFlag => WinoIconGlyph.ClearFlag,
            MailOperation.MarkAsRead => WinoIconGlyph.MarkRead,
            MailOperation.MarkAsUnread => WinoIconGlyph.MarkUnread,
            MailOperation.MarkAsNotJunk => WinoIconGlyph.Blocked,
            MailOperation.Ignore => WinoIconGlyph.Ignore,
            MailOperation.Reply => WinoIconGlyph.Reply,
            MailOperation.ReplyAll => WinoIconGlyph.ReplyAll,
            MailOperation.Zoom => WinoIconGlyph.Zoom,
            MailOperation.SaveAs => WinoIconGlyph.Save,
            MailOperation.Print => WinoIconGlyph.Print,
            MailOperation.Find => WinoIconGlyph.Find,
            MailOperation.Forward => WinoIconGlyph.Forward,
            MailOperation.DarkEditor => WinoIconGlyph.DarkEditor,
            MailOperation.LightEditor => WinoIconGlyph.LightEditor,
            MailOperation.ViewMessageSource => WinoIconGlyph.ViewMessageSource,
            _ => WinoIconGlyph.None,
        };
    }

    public static WinoIconGlyph GetPathGeometry(FolderOperation operation)
    {
        return operation switch
        {
            FolderOperation.None => WinoIconGlyph.None,
            FolderOperation.Pin => WinoIconGlyph.Pin,
            FolderOperation.Unpin => WinoIconGlyph.UnPin,
            FolderOperation.MarkAllAsRead => WinoIconGlyph.MarkRead,
            FolderOperation.DontSync => WinoIconGlyph.DontSync,
            FolderOperation.Empty => WinoIconGlyph.EmptyFolder,
            FolderOperation.Rename => WinoIconGlyph.Rename,
            FolderOperation.Delete => WinoIconGlyph.Delete,
            FolderOperation.Move => WinoIconGlyph.Forward,
            FolderOperation.TurnOffNotifications => WinoIconGlyph.TurnOfNotifications,
            FolderOperation.CreateSubFolder => WinoIconGlyph.CreateFolder,
            _ => WinoIconGlyph.None,
        };
    }

    public static WinoIconGlyph GetSpecialFolderPathIconGeometry(SpecialFolderType specialFolderType)
    {
        return specialFolderType switch
        {
            SpecialFolderType.Inbox => WinoIconGlyph.SpecialFolderInbox,
            SpecialFolderType.Starred => WinoIconGlyph.SpecialFolderStarred,
            SpecialFolderType.Important => WinoIconGlyph.SpecialFolderImportant,
            SpecialFolderType.Sent => WinoIconGlyph.SpecialFolderSent,
            SpecialFolderType.Draft => WinoIconGlyph.SpecialFolderDraft,
            SpecialFolderType.Archive => WinoIconGlyph.SpecialFolderArchive,
            SpecialFolderType.Deleted => WinoIconGlyph.SpecialFolderDeleted,
            SpecialFolderType.Junk => WinoIconGlyph.SpecialFolderJunk,
            SpecialFolderType.Chat => WinoIconGlyph.SpecialFolderChat,
            SpecialFolderType.Category => WinoIconGlyph.SpecialFolderCategory,
            SpecialFolderType.Unread => WinoIconGlyph.SpecialFolderUnread,
            SpecialFolderType.Forums => WinoIconGlyph.SpecialFolderForums,
            SpecialFolderType.Updates => WinoIconGlyph.SpecialFolderUpdated,
            SpecialFolderType.Personal => WinoIconGlyph.SpecialFolderPersonal,
            SpecialFolderType.Promotions => WinoIconGlyph.SpecialFolderPromotions,
            SpecialFolderType.Social => WinoIconGlyph.SpecialFolderSocial,
            SpecialFolderType.Other => WinoIconGlyph.SpecialFolderOther,
            SpecialFolderType.More => WinoIconGlyph.SpecialFolderMore,
            _ => WinoIconGlyph.None,
        };
    }


    public static WinoIconGlyph GetProviderIcon(MailProviderType providerType, SpecialImapProvider specialImapProvider)
    {
        if (specialImapProvider == SpecialImapProvider.None)
        {
            return providerType switch
            {
                MailProviderType.Outlook => WinoIconGlyph.Microsoft,
                MailProviderType.Gmail => WinoIconGlyph.Google,
                MailProviderType.IMAP4 => WinoIconGlyph.IMAP,
                _ => WinoIconGlyph.None,
            };
        }
        else
        {
            return specialImapProvider switch
            {
                SpecialImapProvider.iCloud => WinoIconGlyph.Apple,
                SpecialImapProvider.Yahoo => WinoIconGlyph.Yahoo,
                _ => WinoIconGlyph.None,
            };
        }
    }
    public static WinoIconGlyph GetProviderIcon(MailAccount account)
        => GetProviderIcon(account.ProviderType, account.SpecialImapProvider);

    public static Geometry GetPathGeometry(string pathMarkup)
    {
        string xaml =
        "<Path " +
        "xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
        "<Path.Data>" + pathMarkup + "</Path.Data></Path>";
        var path = XamlReader.Load(xaml) as Windows.UI.Xaml.Shapes.Path;

        Geometry geometry = path.Data;
        path.Data = null;
        return geometry;
    }

    #endregion

    #region Internationalization

    public static string GetOperationString(MailOperation operation)
    {
        return operation switch
        {
            MailOperation.None => "unknown",
            MailOperation.Archive => Translator.MailOperation_Archive,
            MailOperation.UnArchive => Translator.MailOperation_Unarchive,
            MailOperation.SoftDelete => Translator.MailOperation_Delete,
            MailOperation.HardDelete => Translator.MailOperation_Delete,
            MailOperation.Move => Translator.MailOperation_Move,
            MailOperation.MoveToJunk => Translator.MailOperation_MoveJunk,
            MailOperation.MoveToFocused => Translator.MailOperation_MoveFocused,
            MailOperation.MoveToOther => Translator.MailOperation_MoveOther,
            MailOperation.AlwaysMoveToOther => Translator.MailOperation_AlwaysMoveOther,
            MailOperation.AlwaysMoveToFocused => Translator.MailOperation_AlwaysMoveFocused,
            MailOperation.SetFlag => Translator.MailOperation_SetFlag,
            MailOperation.ClearFlag => Translator.MailOperation_ClearFlag,
            MailOperation.MarkAsRead => Translator.MailOperation_MarkAsRead,
            MailOperation.MarkAsUnread => Translator.MailOperation_MarkAsUnread,
            MailOperation.MarkAsNotJunk => Translator.MailOperation_MarkNotJunk,
            MailOperation.Seperator => string.Empty,
            MailOperation.Ignore => Translator.MailOperation_Ignore,
            MailOperation.Reply => Translator.MailOperation_Reply,
            MailOperation.ReplyAll => Translator.MailOperation_ReplyAll,
            MailOperation.Zoom => Translator.MailOperation_Zoom,
            MailOperation.SaveAs => Translator.MailOperation_SaveAs,
            MailOperation.Find => Translator.MailOperation_Find,
            MailOperation.Forward => Translator.MailOperation_Forward,
            MailOperation.DarkEditor => string.Empty,
            MailOperation.LightEditor => string.Empty,
            MailOperation.Print => Translator.MailOperation_Print,
            MailOperation.ViewMessageSource => Translator.MailOperation_ViewMessageSource,
            MailOperation.Navigate => Translator.MailOperation_Navigate,
            _ => "unknown",
        };
    }

    public static string GetOperationString(FolderOperation operation)
    {
        return operation switch
        {
            FolderOperation.None => string.Empty,
            FolderOperation.Pin => Translator.FolderOperation_Pin,
            FolderOperation.Unpin => Translator.FolderOperation_Unpin,
            FolderOperation.MarkAllAsRead => Translator.FolderOperation_MarkAllAsRead,
            FolderOperation.DontSync => Translator.FolderOperation_DontSync,
            FolderOperation.Empty => Translator.FolderOperation_Empty,
            FolderOperation.Rename => Translator.FolderOperation_Rename,
            FolderOperation.Delete => Translator.FolderOperation_Delete,
            FolderOperation.Move => Translator.FolderOperation_Move,
            FolderOperation.CreateSubFolder => Translator.FolderOperation_CreateSubFolder,
            _ => string.Empty,
        };
    }

    #endregion
}
