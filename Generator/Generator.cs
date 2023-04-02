using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Generator;
public partial class Generator
{
    [GeneratedRegex("{(.*?)}")]
    private static partial Regex CurlyBracesregex();
    private readonly Regex _curlyBraceRegex = CurlyBracesregex();

    public void GenerateTranslations()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Generating translations");
        var translationsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "src", "translations");
        var translationFiles = Directory.GetFiles(Path.Combine(translationsDirectory, "src"));
        var mainTranslationFile = Path.Combine(translationsDirectory, "src", "da.json");
        var mainTranslationText = File.ReadAllText(mainTranslationFile);
        var mainTranslationEntries = JsonConvert.DeserializeObject<Dictionary<string, string>>(mainTranslationText);
        if (mainTranslationEntries is null)
        {
            throw new Exception($"Error reading main translation file {mainTranslationFile}");
        };
        foreach (var file in translationFiles)
        {
            var text = File.ReadAllText(file);
            var translationEntries = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            if (translationEntries is null)
            {
                continue;
            };
            var hasMissingKeys = !mainTranslationEntries.All(e => translationEntries.ContainsKey(e.Key));
            if (hasMissingKeys)
            {
                AddMissingKeys(mainTranslationEntries, file, translationEntries);
            }

            var generatedFile = "export const translator = {\n";
            foreach (var entry in translationEntries)
            {
                var parameters = GetParameters(entry.Value);
                generatedFile += $"{entry.Key}: ({parameters}) => {{return `{GenerateReturnValue(entry.Value)}`}},\n";
            }
            generatedFile += "}";
            var oldFileName = new FileInfo(file).Name;
            var generatedFileName = new FileInfo(file).Name.Substring(0, oldFileName.Length - 4) + "js";
            File.WriteAllText(Path.Combine(translationsDirectory, "generated", generatedFileName), generatedFile);
        }
        Console.WriteLine("Finished generating translations");
    }

    private static void AddMissingKeys(Dictionary<string, string> mainTranslationEntries, string file, Dictionary<string, string> translationEntries)
    {
        var missingEntries = mainTranslationEntries.Where(e => !translationEntries.ContainsKey(e.Key)).ToArray();
        var missingKeys = missingEntries.Select(e => e.Key).ToArray();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warning: The file {file.Substring(file.Length - 7)} has missing translation keys.\n" +
            $"The following keys will be added to the file and their values will be populated with values from the main translation file:\n{string.Join("\n", missingKeys)}");
        foreach (var entry in missingEntries)
        {
            translationEntries[entry.Key] = entry.Value;
        }
        var newJson = JsonConvert.SerializeObject(translationEntries, Formatting.Indented);
        File.WriteAllText(file, newJson);
        Console.ForegroundColor = ConsoleColor.Green;
    }

    private string GetParameters(string value)
    {
        var matches = GetMatches(value);
        var parameters = "";
        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var parameter = match.Groups[1].Value;
            var isLastMatch = i == matches.Count - 1;
            parameters += isLastMatch ? $"{parameter}" : $"{parameter}, ";
        }
        return parameters;
    }


    private string GenerateReturnValue(string value)
    {
        var matches = GetMatches(value);
        var returnValue = value;
        foreach (Match match in matches)
        {
            var parameter = match.Value; // {name}, {name@gmail.com}
            returnValue = returnValue.Replace(parameter, $"${parameter}");
        }
        return returnValue;
    }

    private MatchCollection GetMatches(string value)
    {
        return _curlyBraceRegex.Matches(value);
    }

}
