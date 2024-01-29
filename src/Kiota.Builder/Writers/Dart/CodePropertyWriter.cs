﻿using System;
using Kiota.Builder.CodeDOM;
using Kiota.Builder.Extensions;

namespace Kiota.Builder.Writers.Dart;
public class CodePropertyWriter : BaseElementWriter<CodeProperty, DartConventionService>
{
    public CodePropertyWriter(DartConventionService conventionService) : base(conventionService) { }
    public override void WriteCodeElement(CodeProperty codeElement, LanguageWriter writer)
    {
        ArgumentNullException.ThrowIfNull(codeElement);
        ArgumentNullException.ThrowIfNull(writer);
        if (codeElement.ExistsInExternalBaseType) return;
        var propertyType = conventions.GetTypeString(codeElement.Type, codeElement);
        var isNullableReferenceType = !propertyType.EndsWith('?')
                                      && codeElement.IsOfKind(
                                            CodePropertyKind.Custom,
                                            CodePropertyKind.QueryParameter);// Other property types are appropriately constructor initialized
        conventions.WriteShortDescription(codeElement.Documentation.Description, writer);
        conventions.WriteDeprecationAttribute(codeElement, writer);
        if (isNullableReferenceType)
        {
            WritePropertyInternal(codeElement, writer, $"{propertyType}?");
        }

        WritePropertyInternal(codeElement, writer, propertyType);// Always write the normal way
    }

    private void WritePropertyInternal(CodeProperty codeElement, LanguageWriter writer, string propertyType)
    {
        if (codeElement.Parent is not CodeClass parentClass)
            throw new InvalidOperationException("The parent of a property should be a class");

        var backingStoreProperty = parentClass.GetBackingStoreProperty();
        var setterAccessModifier = codeElement.ReadOnly && codeElement.Access > AccessModifier.Private ? "private " : string.Empty;
        var simpleBody = $"get; {setterAccessModifier}set;";
        var defaultValue = string.Empty;

        var attributes = conventions.GetAccessModifierAttribute(codeElement.Access);
        if (!string.IsNullOrEmpty(attributes))
            writer.WriteLine(attributes);

        switch (codeElement.Kind)
        {
            case CodePropertyKind.RequestBuilder:
                writer.WriteLine($"get {propertyType} {conventions.GetAccessModifierPrefix(codeElement.Access)}{codeElement.Name.ToFirstCharacterUpperCase()} {{");
                writer.IncreaseIndent();
                conventions.AddRequestBuilderBody(parentClass, propertyType, writer, prefix: "return ");
                writer.DecreaseIndent();
                writer.WriteLine("}");
                break;
            case CodePropertyKind.AdditionalData when backingStoreProperty != null:
            case CodePropertyKind.Custom when backingStoreProperty != null:
                var backingStoreKey = codeElement.WireName;
                writer.WriteLine($"{conventions.GetAccessModifier(codeElement.Access)} {propertyType} {codeElement.Name.ToFirstCharacterUpperCase()} {{");
                writer.IncreaseIndent();
                writer.WriteLine($"get {{ return {backingStoreProperty.Name.ToFirstCharacterUpperCase()}?.Get<{propertyType}>(\"{backingStoreKey}\"); }}");
                writer.WriteLine($"set {{ {backingStoreProperty.Name.ToFirstCharacterUpperCase()}?.Set(\"{backingStoreKey}\", value); }}");
                writer.DecreaseIndent();
                writer.WriteLine("}");
                break;
            case CodePropertyKind.ErrorMessageOverride when parentClass.IsErrorDefinition:
                if (parentClass.GetPrimaryMessageCodePath(static x => x.Name.ToFirstCharacterUpperCase(), static x => x.Name.ToFirstCharacterUpperCase(), "?.") is string primaryMessageCodePath && !string.IsNullOrEmpty(primaryMessageCodePath))
                    writer.WriteLine($"public override {propertyType} {codeElement.Name.ToFirstCharacterUpperCase()} {{ get => {primaryMessageCodePath} ?? string.Empty; }}");
                else
                    writer.WriteLine($"public override {propertyType} {codeElement.Name.ToFirstCharacterUpperCase()} {{ get => base.Message; }}");
                break;
            case CodePropertyKind.QueryParameter when codeElement.IsNameEscaped:
                writer.WriteLine($"[QueryParameter(\"{codeElement.SerializationName}\")]");
                goto default;
            case CodePropertyKind.QueryParameters:
                defaultValue = $" = new {propertyType}();";
                goto default;
            default:
                writer.WriteLine($"{conventions.GetAccessModifier(codeElement.Access)} {propertyType} {codeElement.Name.ToFirstCharacterUpperCase()} {{ {simpleBody} }}{defaultValue}");
                break;
        }
    }
}
