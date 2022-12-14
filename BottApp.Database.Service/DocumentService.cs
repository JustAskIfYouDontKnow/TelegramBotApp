using BottApp.Database.Document;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public class DocumentService : IDocumentService
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;


    public DocumentService(IUserRepository userRepository, IDocumentRepository documentRepository)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
    }

    
    public async Task UploadFile(Message message, ITelegramBotClient _botClient)
    {
        var documentType = message.Type.ToString();
        var fileInfo = await _botClient.GetFileAsync(message.Document.FileId);
        var filePath = fileInfo.FilePath;
        var extension = Path.GetExtension(filePath);


        var rootPath = Directory.GetCurrentDirectory() + "/DATA/";

        var user = await _userRepository.GetOneByUid(message.Chat.Id);

        var newPath = Path.Combine(rootPath, user.TelegramFirstName + "___" + user.UId, documentType, extension);

        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
        }


        var destinationFilePath = newPath + $"/{user.TelegramFirstName}__{Guid.NewGuid().ToString("N")}__{user.UId}__{extension}";

        ///
        await _documentRepository.CreateModel(user.Id, documentType, extension, DateTime.Now, destinationFilePath, null, DocumentInPath.Base, null);
        ///

        await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        await _botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();


        await _botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо! Ваш документ загружен в базу данных.");
    }

    


    public async Task<bool> UploadVoteFile(Message message, ITelegramBotClient _botClient, InNomination inNomination, string? caption)
    {
        var documentType = message.Type.ToString();
        if (message.Photo == null) return false;

        var fileInfo = await _botClient.GetFileAsync(message.Photo[^1].FileId);
        var filePath = fileInfo.FilePath;
        var extension = Path.GetExtension(filePath);

        var rootPath = Directory.GetCurrentDirectory() + "/DATA/Votes";

        var user = await _userRepository.GetOneByUid(message.Chat.Id);

        var newPath = Path.Combine(rootPath, user.TelegramFirstName + "___" + user.UId, documentType, extension);

        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
        }


        var destinationFilePath =
            newPath + $"/{user.TelegramFirstName}__{Guid.NewGuid().ToString("N")}__{user.UId}__{extension}";

        ///
        var model = await _documentRepository.CreateModel(user.Id, documentType, extension, DateTime.Now, destinationFilePath, caption, DocumentInPath.Votes, inNomination);
        ///


        await _botClient.SendPhotoAsync(
            AdminSettings.AdminChatId,
            message.Photo[^1].FileId,
            $"ID: {model.Id} \n" +
            $"Описание: {caption}\n" +
            $"Номинация: {model.DocumentNomination}\n" +
            $"Отправил пользователь ID {user.Id}, UID {user.UId} @{message.Chat.Username}",
            replyMarkup: Keyboard.ApproveDeclineDocumetKeyboard
        );
        

    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        await _botClient.DownloadFileAsync(filePath, fileStream);
        fileStream.Close();

        return true;
    }
}