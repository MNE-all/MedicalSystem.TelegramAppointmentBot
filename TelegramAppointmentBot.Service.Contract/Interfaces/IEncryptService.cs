namespace TelegramAppointmentBot.Service.Contract.Interfaces
{
    public interface IEncryptService
    {
        /// <summary>
        /// Инициализирует ключи для пользователя
        /// </summary>
        /// <param name="userId"></param>
        Task Init(long userId);

        /// <summary>
        /// Защифровывает текст
        /// </summary>
        Task<byte[]> Encrypt(string text, byte[] Key, byte[] IV);


        /// <summary>
        /// Расшифровывает в текст
        /// </summary>
        Task<string> Decrypt(byte[] cipherText, byte[] Key, byte[] IV);

    }
}
