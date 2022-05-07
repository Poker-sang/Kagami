using Kagami.Attributes;
using Kagami.Function;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Reflection;
using System.Text;
using Args = System.Collections.ObjectModel.ReadOnlyCollection<System.Reflection.CustomAttributeTypedArgument>;


var allText = new StringBuilder();

foreach (var methodInfo in typeof(Commands).GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
    if (methodInfo.CustomAttributes.FirstOrDefault(t => t.AttributeType == typeof(HelpAttribute)) is { } help)
    {
        if (help.ConstructorArguments[0].Value is not string summary)
            continue;
        if (((Args)help.ConstructorArguments[1].Value!)
            .Select(t => (string)t.Value!).ToArray() is not { } argsNames)
            continue;

        var commandLine = new StringBuilder(methodInfo.Name.ToLower());
        var records = new StringBuilder();

        if (argsNames.Length is not 0
            && methodInfo.CustomAttributes
                .FirstOrDefault(t => t.AttributeType == typeof(HelpArgsAttribute)) is { } helpArgs
            && ((Args)helpArgs.ConstructorArguments[0].Value!).Select(t => (Type)t.Value!).ToArray() is { } argsTypes
            && argsTypes.Length == argsNames.Length)
            for (var i = 0; i < argsTypes.Length; i++)
            {
                var isNullable = false;
                var type = argsTypes[i];
                if (type.IsGenericType)
                {
                    var innerType = type.GetGenericArguments()[0];
                    if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        isNullable = true;
                        type = innerType;
                    }
                }
                commandLine.Append(isNullable ? $" [-{argsNames[i]}]" : $" -{argsNames[i]}");
                records.AppendLine($"{argsNames[i]}: {type.Name}");
            }
        allText.Append(commandLine).AppendLine().AppendLine(summary).Append(records).AppendLine();
    }

var fontFamily = new FontCollection().Add(Commands.EnvPath + "JetBrainsMono-Regular.ttf");
var font = new Font(fontFamily, 20);

using var image = await Image.LoadAsync<Rgba32>(Commands.EnvPath + "desktop.png");

image.Mutate(x => x.DrawText(allText.ToString(), font, Color.White, new PointF(50, 50)));

await image.SaveAsync(Commands.HelpImage);