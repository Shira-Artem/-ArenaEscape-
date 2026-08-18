using UnityEngine;

public static class SaveSystem
{
    // v9: режим одного забега.
    // Сохранения/чекпоинты отключены: при смерти игрок начинает карту заново.
    const string KEY_X  = "AE_X";
    const string KEY_Y  = "AE_Y";
    const string KEY_Z  = "AE_Z";
    const string KEY_HP = "AE_HP";
    const string KEY_SC = "AE_SC";
    const string KEY_OK = "AE_OK";

    public static void Save(Vector3 pos, int hp, int score)
    {
        // Намеренно ничего не сохраняем.
        Delete();
    }

    public static bool HasSave() => false;

    public static void Load(out Vector3 pos, out int hp, out int score)
    {
        pos   = new Vector3(0f, 1.2f, 2f);
        hp    = 100;
        score = 0;
    }

    public static void Delete()
    {
        PlayerPrefs.DeleteKey(KEY_X); PlayerPrefs.DeleteKey(KEY_Y);
        PlayerPrefs.DeleteKey(KEY_Z); PlayerPrefs.DeleteKey(KEY_HP);
        PlayerPrefs.DeleteKey(KEY_SC); PlayerPrefs.DeleteKey(KEY_OK);
        PlayerPrefs.Save();
    }
}
