﻿using System;
using System.Linq;

using Kiota.Builder.CodeDOM;
using Kiota.Builder.Extensions;

namespace Kiota.Builder.Writers.http;
public class CodeEnumWriter : BaseElementWriter<CodeEnum, HttpConventionService>
{
    public CodeEnumWriter(HttpConventionService conventionService) : base(conventionService) { }
    public override void WriteCodeElement(CodeEnum codeElement, LanguageWriter writer)
    {
        ArgumentNullException.ThrowIfNull(codeElement);
        ArgumentNullException.ThrowIfNull(writer);
        if (!codeElement.Options.Any())
            return;

        if (codeElement.Parent is CodeNamespace codeNamespace)
        {
            writer.StartBlock($"extension {codeNamespace.Name} {{");
        }
        writer.StartBlock($"public enum {codeElement.Name.ToFirstCharacterUpperCase()} : String {{"); //TODO docs
        writer.WriteLines(codeElement.Options
                        .Select(static x => x.Name.ToFirstCharacterUpperCase())
                        .Select(static (x, idx) => $"case {x}"));
        //TODO static parse function?
        //enum and ns are closed by the code block end writer
    }
}
