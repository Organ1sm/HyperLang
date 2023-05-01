﻿using System.Collections.Immutable;
using Hyper.Core.Binding;
using Hyper.Core.Diagnostic;
using Hyper.Core.Symbols;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hyper.Core.Emit;

internal sealed class Emitter
{
    private DiagnosticBag _diagnostics = new();

    private readonly Dictionary<TypeSymbol, TypeReference?> _knownTypes;
    private readonly MethodReference?                       _consoleWriteLineReference;
    private readonly AssemblyDefinition                     _assemblyDefinition;

    private Emitter(string moduleName,
                    string[] references)
    {
        var assemblies = new List<AssemblyDefinition>();
        foreach (var re in references)
        {
            try
            {
                var assembly = AssemblyDefinition.ReadAssembly(re);
                assemblies.Add(assembly);
            }
            catch (BadImageFormatException)
            {
                _diagnostics.ReportInvalidReference(re);
            }
        }

        var builtInTypes = new List<(TypeSymbol type, string MetadataName)>()
        {
            (TypeSymbol.Any, "System.Object"),
            (TypeSymbol.Bool, "System.Boolean"),
            (TypeSymbol.Int, "System.Int32"),
            (TypeSymbol.String, "System.String"),
            (TypeSymbol.Void, "System.Void"),
        };

        var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
        _assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);
        _knownTypes = new Dictionary<TypeSymbol, TypeReference?>();

        foreach (var (typeSymbol, metadataName) in builtInTypes)
        {
            var typeReference = ResolveType(typeSymbol.Name, metadataName);
            _knownTypes.Add(typeSymbol, typeReference);
        }

        TypeReference? ResolveType(string minskName, string metadataName)
        {
            var foundTypes = assemblies.SelectMany(a => a.Modules)
                                       .SelectMany(m => m.Types)
                                       .Where(t => t.FullName == metadataName)
                                       .ToArray();

            switch (foundTypes.Length)
            {
                case 1:
                {
                    var typeReference = _assemblyDefinition.MainModule.ImportReference(foundTypes[0]);
                    return typeReference;
                }
                case 0:
                    _diagnostics.ReportRequiredTypeNotFound(minskName, metadataName);
                    break;
                default:
                    _diagnostics.ReportRequiredTypeAmbiguous(minskName, metadataName, foundTypes);
                    break;
            }

            return null;
        }

        MethodReference? ResolveMethod(string typeName, string methodName, string[] parameterTypeNames)
        {
            var foundTypes = assemblies.SelectMany(a => a.Modules)
                                       .SelectMany(m => m.Types)
                                       .Where(t => t.FullName == typeName)
                                       .ToArray();

            switch (foundTypes.Length)
            {
                case 1:
                {
                    var foundType = foundTypes[0];
                    var methods   = foundType.Methods.Where(m => m.Name == methodName);

                    foreach (var method in methods)
                    {
                        if (method.Parameters.Count != parameterTypeNames.Length)
                            continue;

                        var allParametersMatch = true;

                        for (var i = 0; i < parameterTypeNames.Length; i++)
                        {
                            if (method.Parameters[i].ParameterType.FullName != parameterTypeNames[i])
                            {
                                allParametersMatch = false;
                                break;
                            }
                        }

                        if (!allParametersMatch)
                            continue;

                        return _assemblyDefinition.MainModule.ImportReference(method);
                    }

                    _diagnostics.ReportRequiredMethodNotFound(typeName, methodName, parameterTypeNames);
                    return null;
                }
                case 0:
                    _diagnostics.ReportRequiredTypeNotFound(null, typeName);
                    break;
                default:
                    _diagnostics.ReportRequiredTypeAmbiguous(null, typeName, foundTypes);
                    break;
            }

            return null;
        }


        _consoleWriteLineReference = ResolveMethod("System.Console", "WriteLine", new[] {"System.String"});
    }

    public static ImmutableArray<Diagnostic.Diagnostic> Emit(BoundProgram program,
                                                             string moduleName,
                                                             string[] references,
                                                             string outputPath)
    {
        if (program.Diagnostics.Any())
            return program.Diagnostics;

        var emitter = new Emitter(moduleName, references);
        return emitter.Emit(program, outputPath);
    }

    public ImmutableArray<Diagnostic.Diagnostic> Emit(BoundProgram program, string outputPath)
    {
        if (_diagnostics.Any())
            return _diagnostics.ToImmutableArray();

        var objectType = _knownTypes[TypeSymbol.Any];
        var typeDefinition =
            new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, objectType);
        _assemblyDefinition.MainModule.Types.Add(typeDefinition);

        var voidType   = _knownTypes[TypeSymbol.Void];
        var mainMethod = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Private, voidType);
        typeDefinition.Methods.Add(mainMethod);

        var ilProcessor = mainMethod.Body.GetILProcessor();
        ilProcessor.Emit(OpCodes.Ldstr, "Hello world from Hyper!");
        ilProcessor.Emit(OpCodes.Call, _consoleWriteLineReference);
        ilProcessor.Emit(OpCodes.Ret);

        _assemblyDefinition.EntryPoint = mainMethod;

        _assemblyDefinition.Write(outputPath);

        return _diagnostics.ToImmutableArray();
    }
}
