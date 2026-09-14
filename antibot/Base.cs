using System.Text.Json;

namespace antibot
{
    internal class Base
    {
        public static List<InfoUser> Users = new List<InfoUser>();
        public static List<AdminUser> Admins = new List<AdminUser>();

        private static string usersPath = "database.json";
        private static string adminsPath = "admins.json";

        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // ------------------ ЗАГРУЗКА ЮЗЕРОВ ------------------
        public static void LoadUsers()
        {
            if (File.Exists(usersPath))
            {
                string json = File.ReadAllText(usersPath);
                Users = JsonSerializer.Deserialize<List<InfoUser>>(json);
            }
        }

        // ------------------ ЗАГРУЗКА АДМИНОВ ------------------
        public static void LoadAdmins()
        {
            if (File.Exists(adminsPath))
            {
                string json = File.ReadAllText(adminsPath);
                Admins = JsonSerializer.Deserialize<List<AdminUser>>(json);
            }
        }

        // ------------------ СОХРАНЕНИЕ ------------------
        public static void SaveUsers()
        {
            File.WriteAllText(usersPath, JsonSerializer.Serialize(Users, options));
        }

        public static void SaveAdmins()
        {
            File.WriteAllText(adminsPath, JsonSerializer.Serialize(Admins, options));
        }
    }
}
