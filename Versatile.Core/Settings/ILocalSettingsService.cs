namespace Versatile.Core.Settings;

public interface ILocalSettingsService
{
    T? ReadSetting<T>(string key);

    void SaveSetting<T>(string key, T value);
    void SaveSetting();
}
