using System.Globalization;
using System.Collections.Generic;

static class Locale {
    static string CurrentLocale = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    static int CurrentLocaleIndex = 0;
    
    static List<Language> Langs = new List<Language>();

    static bool init = GenDict();
    static bool GenDict() {

        var en = new Language("en");
        Langs.Add(en);
        en.AddString("PatchTarget",          "Patch target");
        en.AddString("InstallationNotFound", "Google Chrome installation not found!");
        en.AddString("IsPortable",           "If you are using portable version, place this application in folder with");
        en.AddString("IsRunning",            "Can't access the file. Google Chrome is running? Close it and try again.");
        en.AddString("Patching",             "Trying to apply patch, please wait...");
        en.AddString("Done",                 "Patch applied successfully!");
        en.AddString("AlreadyPatched",       "Patch was already applied before. Revert the file to original state?");
        en.AddString("Reverted",             "File was reverted to original state. Now the patch is not applied.");
        en.AddString("Failed",               "Patching failed! Looks like file is wrong or patcher is outdated.");
        en.AddString("VariationsCleared",    "Variations definitions in \"Local State\" file cleared!");
        en.AddString("NeedAdmin",            "Not enough privileges to access the file. Application will be restarted with administrator privileges.");
        en.AddString("ConfirmUAC",           "You must confirm UAC prompt to continue!\nOr just run application with administrator privileges.");
        en.AddString("UnknownError",         "Unknown error while writing file.");

        var ru = new Language("ru");
        Langs.Add(ru);
        ru.AddString("PatchTarget",          "Цель патча");
        ru.AddString("InstallationNotFound", "Не найден установленный Google Chrome!");
        ru.AddString("IsPortable",           "Если вы используете portable-версию, поместите эту программу в директорию с");
        ru.AddString("IsRunning",            "Не удалось получить доступ к файлу. Google Chrome запущен? Закройте его и попробуйте снова.");
        ru.AddString("Patching",             "Попытка применения патча, подождите...");
        ru.AddString("Done",                 "Патч успешно применен!");
        ru.AddString("AlreadyPatched",       "Патч уже был применен ранее. Вернуть файл к оригинальному состоянию?");
        ru.AddString("Reverted",             "Файл возвращен к оригинальному состоянию. Патч теперь не применен.");
        ru.AddString("Failed",               "Не удалось применить патч! Похоже, что файл неверный, либо патчер устарел.");
        ru.AddString("VariationsCleared",    "Определения вариаций в файле \"Local State\" очищены!");
        ru.AddString("NeedAdmin",            "Недостаточно прав для доступа к файлу. Приложение будет перезапущено с правами администратора.");
        ru.AddString("ConfirmUAC",           "Для продолжения необходимо подтвердить запрос UAC!\nЛибо сразу запустите приложение с правами администратора.");
        ru.AddString("UnknownError",         "Неизвестная ошибка при записи файла.");
        
        for(int i = 0; i < Langs.Count && CurrentLocaleIndex == 0; i++)
            if(Langs[i].Name == CurrentLocale)
                CurrentLocaleIndex = i;

        return true;
    }
    
    public static string GetString(string locName) {
        return (Langs[CurrentLocaleIndex].GetString(locName) ?? Langs[0].GetString(locName)) ?? $"< FAILED TO FIND LOCALE STRING : {locName} >";
    }

    class Language {
        internal string Name { get; }
        Dictionary<string, string> strings = new Dictionary<string, string>();

        internal Language(string Name) {
            this.Name = Name;
        }

        internal void AddString(string locName, string text) {
            strings.Add(locName, text);
        }
        
        internal string GetString(string locName) {
            return strings[locName];
        }
    }
}
