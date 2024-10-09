﻿using System;
using System.Linq;

using Kiota.Builder.CodeDOM;

namespace Kiota.Builder.Writers.http;
public class CodeNamespaceWriter : BaseElementWriter<CodeNamespace, HttpConventionService>
{
    public CodeNamespaceWriter(HttpConventionService conventionService) : base(conventionService) { }
    public override void WriteCodeElement(CodeNamespace codeElement, LanguageWriter writer)
    {
        ArgumentNullException.ThrowIfNull(codeElement);
        ArgumentNullException.ThrowIfNull(writer);
        var segments = codeElement.Name.Split(".");
        var lastSegment = segments.Last();
        var parentNamespaces = string.Join('.', segments[..^1]);
        writer.WriteLine($"extension {parentNamespaces} {{");
        writer.IncreaseIndent();
        writer.WriteLine($"public struct {lastSegment} {{");
        writer.WriteLine("}");
        writer.DecreaseIndent();
        writer.WriteLine("}");
    }
}
