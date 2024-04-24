using System;
using System.IO;
using Newtonsoft.Json;
using dotenv.net;

namespace SettingsManager
{
    public class WindowManager
  {
    private string settingsFilePath;
    public string WindowTitle { get; set; }
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public bool IsFullScreen { get; set; }
    public bool IsBorderless { get; set; }

    // Constructor
    public WindowManager()
    {
      WindowTitle = "My Game";
      WindowWidth = 800;
      WindowHeight = 600;
      IsFullScreen = false;
      IsBorderless = false;
      DotEnv.Load(new DotEnvOptions(envFilePaths: new[] {"..\\..\\..\\.env"}));
      settingsFilePath = Environment.GetEnvironmentVariable("SETTINGS_FILE_PATH");
    }

    public void SaveSettings()
    {
      string json = JsonConvert.SerializeObject(this);
      File.WriteAllText(settingsFilePath, json);
    }

    public void LoadSettings()
    {
      if (File.Exists(settingsFilePath))
      {
        string json = File.ReadAllText(settingsFilePath);
        WindowManager settings = JsonConvert.DeserializeObject<WindowManager>(json);

        // Copy loaded settings to this instance
        WindowTitle = settings.WindowTitle;
        WindowWidth = settings.WindowWidth;
        WindowHeight = settings.WindowHeight;
        IsFullScreen = settings.IsFullScreen;
        IsBorderless = settings.IsBorderless;
      }
    }
  }
}