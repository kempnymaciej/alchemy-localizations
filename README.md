# AlchemyLocalizations
AlchemyLocalizations is a personal project that implements a localizations system for Unity engine.

## Key features:
* Google sheets synchronizations.
* Supports all special characters including end lines.
* Advanced access logic with constant keys.
* Simple configuration with a dedicated editor window.

#### Early beta NOTE!
The AlchemyLocalizations package is currently in an early beta stage. It is functional, but sometimes it may not work as intended and may be missing some features.
In the near future I will be working on the following:
* Documentation and tutorials.
* Better validation.

## Installation via PackageManager
1. Open the package manager window.
2. Click the plus button and select "Add package from git URL...".
3. Paste the link: `https://github.com/kempnymaciej/alchemy-localizations.git?path=/package#v0.3.1`.

Some versions of Unity may have trouble downloading the git package with a specific tag ([see](https://issuetracker.unity3d.com/issues/package-resolution-error-when-using-a-git-dependency-referencing-an-annotated-tag-in-its-git-url)). In this case, clone the repository and copy the contents of the package directory to your project.

### Prepare sheets
1. Create a Google Sheets project, name it and set it **public to read**.
2. Create and name sheets. For example: `General`, `Tips`, ...
3. Fill sheets like this (languages in the first row; keys in the first column):

| Keys          | English         | Polish       |
| :---:         | :---:           | :---:        |
| GoodNight     | Good night!     | Dobranoc!    |
| GoodMorning   | Good morning!   | Dzień dobry! |

### Synchronize in Unity
1. In the Unity, click Window/AlchemyBow/Localizations/Settings.
2. Click the button in the window to create a localization folder.
3. Fill all required fields:

| Field                     | Description|
| :---:                     | :--- |
|**SpreadsheedId**          | You can find the spreadsheet ID in a sheets URL: `https://docs.google.com/spreadsheets/d/<SpreadsheedId>/edit#gid=0` |
|**Class Name**             | The name of the access class to bake. (By default Keys)|
|**Class Namespace Name**   | The namespace of the access class. |
|**Class Folder Path**      | The path to the access class folder. **(Ensure the folder is in an assembly that references `AlchemyBow.Localizations`!)** |
|**Languages**              | Languages you used in the sheets. |
|**Group Names**            | Names of the sheets you created. |

7. Click the "Synchronize Localization" button.

### Use in code:
1. Initialize a Localizator in runtime scripts:
```csharp
//Coroutine example:
private IEnumerator LoadLocalizator()
{
    localizator = new Localizator(Keys.GetLocalizatorConfig());
    var request = localizator.SetLanguage(LocalizatorConfig.DefaultLanguage);
    yield return new WaitUntil(() => request.Finished);
    // ...
}
```
```csharp
//Callback example:
private void Start()
{
    localizator = new Localizator(Keys.GetLocalizatorConfig());
    localizator.ValidLocalizationsSet += OnLanguageSet;
    localizator.SetLanguage(LocalizatorConfig.DefaultLanguage);
}
private void OnLanguageSet(Localizator localizator)
{
    // ...
}
```
2. Use localizator:
```csharp
Debug.Log(localizator[Keys.General.GoodNight]);
Debug.Log(localizator[Keys.General.GoodMorning]);
Debug.Log(localizator[Keys.Tips.HaveFun]);
```

## Advances access:
Each localization you create can be accessed with a constant integer key:
```csharp
Debug.Log(localizator[Keys.General.GoodNight]);
```

You can also access the entire group:
```csharp
Debug.Log(localizator[Keys.General._FirstKey]);
Debug.Log(localizator[Keys.General._LastKey]);
Debug.Log(localizator[Random.Range(Keys.General._FirstKey, Keys.General._LastKey + 1)]);
```

Additionally, if the keys in the group contain numbers, static accessors are generated:
```csharp
Debug.Log(localizator[Keys.General.Message1]);
Debug.Log(localizator[Keys.General.Message99]);
// ==
Debug.Log(localizator[Keys.General.MessageX(1)]);
Debug.Log(localizator[Keys.General.MessageX(99)]);
```

## String access
Sometimes you may need to access localizations with string keys. The example below presents how to do it:
```csharp
private IEnumerator Example()
{
    localizator = new Localizator(Keys.GetLocalizatorConfig());
    // Create a settings object.
    var settings = new LocalizatorRequestSettings(localizator);
    // Add groups you want to load.
    // (You can add all groups at once with `.AddAllGroupsToLoad()`)
    settings.AddGroupsToLoad(Keys.General._GroupIndex, Keys.Tips._GroupIndex);
    // Add groups you want to access with string keys:
    // (You can add all groups at once with `.AddAllGroupsToStringMap()`)
    settings.AddGroupsToStringMap(Keys.General._GroupIndex);
    // Use SetLanguage() overload that accepts the settings.
    var request = localizator.SetLanguage(LocalizatorConfig.DefaultLanguage, settings);
    yield return new WaitUntil(() => request.Finished);

    Debug.Log(localizator["General.GoodMorning"]);
}
```