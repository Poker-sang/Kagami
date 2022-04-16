using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static Kagami.SourceGenerator.Utilities;

namespace Kagami.SourceGenerator;

internal static partial class TypeWithAttributeDelegates
{
    public static string? GenerateHelp(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, List<AttributeData> attributeList)
    {

        var specifiedAttribute = attributeList[0];
        if (specifiedAttribute.ConstructorArguments[0].Value is not string beforeHelp)
            return null;

        var defaultPrefix = "";
        if (specifiedAttribute.NamedArguments[0].Key is "DefaultPrefix" && specifiedAttribute.NamedArguments[0].Value.Value is string dp)
            defaultPrefix = dp;

        var stringBuilder = new StringBuilder().AppendLine(@$"#nullable enable

using Konata.Core;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using Konata.Core.Events.Model;
using System.Threading.Tasks;

namespace {typeSymbol.ContainingNamespace.ToDisplayString()};

partial class {typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}
{{
    public static async Task<MessageBuilder?> GetReply(Bot bot, GroupMessageEvent group)
    {{
        var textChain = group.Chain.GetChain<TextChain>();
        if (textChain is null)
            return null;

        return textChain.Content.Split(' ')[0] switch
        {{
            @""{defaultPrefix}help"" => Help(),");
        var getReplyEndAndHelpBegin = new StringBuilder($@"            _ => null
        }};
    }}

    public static MessageBuilder Help() => new MessageBuilder(""{beforeHelp}\n"")
");
        const string helpEndAndClassEnd = ";\n}";

        foreach (var member in typeSymbol.GetMembers()
                     .Where(member => member is { Kind: SymbolKind.Method })
                     .Cast<IMethodSymbol>())
        {
            if (member.GetAttributes().FirstOrDefault(attributeData => attributeData.AttributeClass!.ToDisplayString() is "Kagami.Attributes.HelpAttribute") is not { }
attribute)
                continue;

            if (attribute.ConstructorArguments[0].Value is not string summary)
                continue;

            string? name = null;
            var prefix = defaultPrefix;

            foreach (var namedArgument in attribute.NamedArguments)
                if (namedArgument.Value.Value is string value)
                    switch (namedArgument.Key)
                    {
                        case "Prefix":
                            prefix = value;
                            break;
                        case "Name":
                            name = value;
                            break;
                    }

            var isAsync = member.IsAsync ? "await " : "";

            var parameters = new StringBuilder();
            foreach (var parameter in member.Parameters)
            {
                switch (parameter.Type.ToDisplayString())
                {
                    case "Konata.Core.Bot":
                        parameters.Append("bot, ");
                        break;
                    case "Konata.Core.Events.Model.GroupMessageEvent":
                        parameters.Append("group, ");
                        break;
                    case "Konata.Core.Message.MessageChain":
                        parameters.Append("group.Chain, ");
                        break;
                    case "Konata.Core.Message.Model.TextChain":
                        parameters.Append("textChain, ");
                        break;
                }
            }

            if (parameters.Length is not 0)
                parameters.Remove(parameters.Length - 2, 2);

            stringBuilder.AppendLine($"{Spacing(3)}@\"{name ?? prefix + member.Name.ToLowerInvariant()}\" => {isAsync}{member.Name}({parameters}),");

            getReplyEndAndHelpBegin.AppendLine($@"{Spacing(3)}.Text(@""- {name ?? prefix + member.Name.ToLowerInvariant()}
  {summary}
"")");
        }
        stringBuilder.Append(getReplyEndAndHelpBegin).Remove(stringBuilder.Length - 2, 2).Append(helpEndAndClassEnd);
        return stringBuilder.ToString();
    }
}