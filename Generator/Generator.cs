using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Generator;
public class Generator
{
    private readonly Regex _curlyBraceRegex = new Regex("{(.*?)}");

    public void GenerateTranslations()
    {
        var translationsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "src", "translations");
        var translationFiles = Directory.GetFiles(Path.Combine(translationsDirectory, "src"));
        var mainTranslationFile = Path.Combine(translationsDirectory, "src", "da.json");
        var mainTranslationText = File.ReadAllText(mainTranslationFile);
        var mainTranslationEntries = JsonConvert.DeserializeObject<Dictionary<string, string>>(mainTranslationText);
        foreach (var file in translationFiles)
        {
            var text = File.ReadAllText(file);
            var translationEntries = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);

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
            if (!mainTranslationEntries.All(e => translationEntries.ContainsKey(e.Key)))
            {
                var missingKeys = mainTranslationEntries.Where(e => !translationEntries.ContainsKey(e.Key)).Select(e => e.Key).ToArray();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Warning: {file} is missing translation keys:\n{string.Join("\n", missingKeys)}");
            }
        }
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
