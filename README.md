# AlchemyLocalizations
A personal project that implements a localizations system for Unity.

## Key features:
* Google sheets synchronizations
* Advanced access with static keys
* Simple configuration with a dedicated editor window
* Loading based on the addressables package

## Early beta NOTE!
The AlchemyLocalizations package is currently in an early beta stage. It is functional, but sometimes it may not work as intended and may be missing some features.
In the near future I will be working on the following:
* documentation and tutorials
* tests for different types of players (especially Android)
* better validation

## Installation
1. Open the package manager window.
2. Click the plus button and select "Add package from git URL...".
3. Paste the link: `https://github.com/kempnymaciej/alchemy-localizations.git?path=/package#0.1.0`

## Quick start
1. Create a Google sheets project and set it public to read.
2. Create sheets and name them e.g. `General`, `UI`, `Tips`, ...
3. Fill sheets like this (languages in the first row and keys in the first column):

| Keys          | English               | Polish    |
| ---           | ---                   | ---       |
| GoodNight     | Good night!           | Dobranoc! |
| TimeToSleep   | It's time to sleep.   | Czas spać.|

4. In the Unity, click Window/AlchemyBow/Localizations/Settings.
5. Click the button in the window to create a localization folder.
6. Fill all required fields:
 * **SpreadsheedId** - You can find the spreadsheet ID in a sheets URL: https://docs.google.com/spreadsheets/d/**SpreadsheedId**/edit#gid=0
 * **Class Name** - The name of the class to bake.
 * **Class Namespace Name** - The namespace of the class to bake.
 * **Class Folder Path** - The path to the class folder. (Ensure the folder is in range of an assembly that referenses `AlchemyBow.Localizations`!)
 * **Languages** - Languages you used in the sheets.
 * **Group Names** - Names of the sheets you created.
7. Click the "Synchronize Localization" button.
8. Initialize a Localizator:
```csharp
//Coroutine example:
private IEnumerator LoadLocalizator()
{
    localizator = new Localizator(YourAccessClassName.GetLocalizatorConfig());
    var request = localizator.SetLanguage(LocalizatorConfig.DefaultLanguage);
    yield return new WaitUntil(() => request.Finished);
    // ...
}

//Callback example:
private void Start()
{
    localizator = new Localizator(YourAccessClassName.GetLocalizatorConfig());
    localizator.ValidLocalizationsSet += OnLanguageSet;
}
private void OnLanguageSet(Localizator localizator)
{
    // ...
}
```
9. Use localizator:
```csharp
Debug.Log(localizator[YourAccessClassName.General.GoodNight]);
Debug.Log(localizator[YourAccessClassName.General.TimeToSleep]);
```