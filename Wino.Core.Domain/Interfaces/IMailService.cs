﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using Wino.Core.Domain.Entities.Mail;
using Wino.Core.Domain.Entities.Shared;
using Wino.Core.Domain.Models.MailItem;

namespace Wino.Core.Domain.Interfaces;

public interface IMailService
{
    Task<MailCopy> GetSingleMailItemAsync(string mailCopyId, string remoteFolderId);
    Task<MailCopy> GetSingleMailItemAsync(Guid uniqueMailId);
    Task<List<IMailItem>> FetchMailsAsync(MailListInitializationOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all mail copies for all folders.
    /// </summary>
    /// <param name="accountId">Account to remove from</param>
    /// <param name="mailCopyId">Mail copy id to remove.</param>
    Task DeleteMailAsync(Guid accountId, string mailCopyId);

    Task ChangeReadStatusAsync(string mailCopyId, bool isRead);
    Task ChangeFlagStatusAsync(string mailCopyId, bool isFlagged);

    Task CreateAssignmentAsync(Guid accountId, string mailCopyId, string remoteFolderId);
    Task DeleteAssignmentAsync(Guid accountId, string mailCopyId, string remoteFolderId);

    Task<bool> CreateMailAsync(Guid accountId, NewMailItemPackage package);

    /// <summary>
    /// Maps new mail item with the existing local draft copy.
    /// In case of failure, it returns false.
    /// Then synchronizers must insert a new mail item.
    /// </summary>
    /// <param name="accountId">Id of the account. It's important to map to the account since if the user use the same account with different providers, this call must map the correct one.</param>
    /// <param name="localDraftCopyUniqueId">UniqueId of the local draft copy.</param>
    /// <param name="newMailCopyId">New assigned remote mail item id.</param>
    /// <param name="newDraftId">New assigned draft id if exists.</param>
    /// <param name="newThreadId">New message's thread/conversation id.</param>
    /// <returns>True if mapping is done. False if local copy doesn't exists.</returns>
    Task<bool> MapLocalDraftAsync(Guid accountId, Guid localDraftCopyUniqueId, string newMailCopyId, string newDraftId, string newThreadId);

    /// <summary>
    /// Maps new mail item with the existing local draft copy.
    /// </summary>
    /// <param name="newMailCopyId"></param>
    /// <param name="newDraftId"></param>
    /// <param name="newThreadId"></param>
    Task MapLocalDraftAsync(string newMailCopyId, string newDraftId, string newThreadId);

    Task UpdateMailAsync(MailCopy mailCopy);

    /// <summary>
    /// Gets the new inserted unread mails after the synchronization.
    /// </summary>
    /// <param name="accountId">Account id.</param>
    /// <param name="downloadedMailCopyIds">
    /// Mail ids that synchronizer tried to download. If there was an issue with the
    /// Items that tried and actually downloaded may differ. This function will return only new inserted ones.
    /// </param>
    /// <returns>Newly inserted unread mails inside the Inbox folder.</returns>
    Task<List<MailCopy>> GetDownloadedUnreadMailsAsync(Guid accountId, IEnumerable<string> downloadedMailCopyIds);

    /// <summary>
    /// Returns the account that this mail copy unique id is assigned.
    /// Used in toast notification handler.
    /// </summary>
    /// <param name="uniqueMailId">Unique id of the mail item.</param>
    /// <returns>Account that mail belongs to.</returns>
    Task<MailAccount> GetMailAccountByUniqueIdAsync(Guid uniqueMailId);

    /// <summary>
    /// Checks whether the given mail copy id exists in the database.
    /// Safely used for Outlook to prevent downloading the same mail twice.
    /// For Gmail, it should be avoided since one mail may belong to multiple folders.
    /// </summary>
    /// <param name="mailCopyId">Native mail id of the message.</param>
    Task<bool> IsMailExistsAsync(string mailCopyId);

    /// <summary>
    /// Returns all mails for given folder id.
    /// </summary>
    /// <param name="folderId">Folder id to get mails for</param>
    Task<List<MailCopy>> GetMailsByFolderIdAsync(Guid folderId);

    /// <summary>
    /// Returns all unread mails for given folder id.
    /// </summary>
    /// <param name="folderId">Folder id to get unread mails for.</param>
    Task<List<MailCopy>> GetUnreadMailsByFolderIdAsync(Guid folderId);

    /// <summary>
    /// Checks whether the mail exists in the folder.
    /// When deciding Create or Update existing mail, we need to check if the mail exists in the folder.
    /// </summary>
    /// <param name="mailCopyId">MailCopy id</param>
    /// <param name="folderId">Folder's local id.</param>
    /// <returns>Whether mail exists in the folder or not.</returns>
    Task<bool> IsMailExistsAsync(string mailCopyId, Guid folderId);

    /// <summary>
    /// Creates a draft MailCopy and MimeMessage based on the given options.
    /// For forward/reply it would include the referenced message.
    /// </summary>
    /// <param name="accountId">AccountId which should have new draft.</param>
    /// <param name="draftCreationOptions">Options like new email/forward/draft.</param>
    /// <returns>Draft MailCopy and Draft MimeMessage as base64.</returns>
    Task<(MailCopy draftMailCopy, string draftBase64MimeMessage)> CreateDraftAsync(Guid accountId, DraftCreationOptions draftCreationOptions);

    /// <summary>
    /// Returns ids 
    /// </summary>
    /// <param name="folderId"></param>
    /// <param name="uniqueIds"></param>
    /// <returns></returns>
    Task<List<MailCopy>> GetExistingMailsAsync(Guid folderId, IEnumerable<UniqueId> uniqueIds);
}
