using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TgTranslator;

namespace Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Removes links from string
        /// </summary>
        /// <returns>string without links</returns>
        public static string WithoutLinks(this string inputString)
        {
            if (Regex.Matches(inputString, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?").Count != 0)
            {
                return Regex.Replace(inputString, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?", "");
            }

            return inputString;
        }

        public static string WithoutArguments(this string inputString)
        {
            return inputString.Split(' ')[0];
        }
        
        /// <summary>
        /// Checks if user is an administrator
        /// </summary>
        public static async Task<bool> IsAdministrator(this User user, long chatId)
        {
            ChatMember[] chatAdmins = await Program.BotClient.GetChatAdministratorsAsync(chatId);

            return chatAdmins.Any(x => x.User.Id == user.Id);
        }
    }
}


