using Kagami.Attributes;
using Kagami.Function;
using Kagami.GenerateHelpImage;
using Konata.Core.Common;
using System.Reflection;
using System.Text;
using static Kagami.GenerateHelpImage.Methods.Color;
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

        var commandLine = "";
        if (methodInfo.CustomAttributes.FirstOrDefault(t => t.AttributeType == typeof(PermissionAttribute)) is { } permission
            && (RoleType)permission.ConstructorArguments[0].Value! is var role and not RoleType.Member)
            commandLine += role switch
            {
                RoleType.Admin => "*需要管理员权限".Run(Comment) + "\n",
                RoleType.Owner => "*需要群主权限".Run(Comment) + "\n",
                _ => throw new ArgumentOutOfRangeException()
            };

        commandLine += methodInfo.Name.ToLower().Run(Method);
        var commandArgs = new StringBuilder();
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
                commandArgs.Append(isNullable ? $" [-{argsNames[i]}]" : $" -{argsNames[i]}");
                records.Append(argsNames[i].Run(Arg) + ": ".Run(Summary));
                if (type.IsEnum)
                {
                    records.AppendLine();
                    foreach (var field in type.GetFields())
                        if (field.CustomAttributes.FirstOrDefault(t => t.AttributeType == typeof(EnumHelpAttribute)) is
                            { } enumHelp
                            && enumHelp.ConstructorArguments[0].Value is string enumSummary)
                            records.AppendLine(field.Name.ToLower().Run(Class) + $"→ {enumSummary}".Run(Summary));
                }
                else records.AppendLine(type.Name.ToLower().Run(Class));
            }
        allText.Append(commandLine);
        if (commandArgs.Length is not 0)
            allText.Append(commandArgs.ToString().Run(Arg));
        allText.AppendLine().AppendLine(summary.Run(Summary));
        if (records.Length is not 0)
            allText.Append(records);
        allText.AppendLine();
    }

allText.Replace("\n", "<LineBreak/>\n");
File.WriteAllText(Commands.HelpPath + "help.txt", allText.ToString());
