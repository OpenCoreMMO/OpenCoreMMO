using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace NeoServer.Server.Compiler.Compilers;

internal static class CSharpCompiler
{
    public static (Assembly assembly, byte[], byte[]) Compile(params Source[] sourceCodes)
    {
        using var peStream = new MemoryStream();
        var compilation = GenerateCode(sourceCodes);

        var assemblyName = "Extensions";
        var symbolsName = Path.ChangeExtension(assemblyName, "pdb");

        using var assemblyStream = new MemoryStream();
        using var symbolsStream = new MemoryStream();

        var emitOptions = new EmitOptions(
            debugInformationFormat: DebugInformationFormat.PortablePdb,
            pdbFilePath: symbolsName);

        var embeddedTexts = sourceCodes.Select(x => EmbeddedText.FromSource(x.Path, x.SourceText));

        var result = compilation.Emit(
            assemblyStream,
            symbolsStream,
            embeddedTexts: embeddedTexts,
            options: emitOptions);

        if (!result.Success)
            throw new Exception(string.Join("\n", result.Diagnostics.Select(x => x.GetMessage())));

        assemblyStream.Seek(0, SeekOrigin.Begin);
        symbolsStream?.Seek(0, SeekOrigin.Begin);

        var assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream, symbolsStream);
        return (assembly, assemblyStream.ToArray(), symbolsStream.ToArray());
    }

    private static CSharpCompilation GenerateCode(params Source[] sourceCodes)
    {
        var syntaxTrees = new SyntaxTree[sourceCodes.Length];
        var i = 0;

        foreach (var source in sourceCodes)
        {
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
            syntaxTrees[i++] = source.GetSourceTree(options);
        }

        var references = GetAssemblies()
            .Select(GetRawMetadataReference);

        return CSharpCompilation.Create("Extensions",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug,
                allowUnsafe: true,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default).WithPlatform(Platform.AnyCpu));
    }

    private static MetadataReference GetRawMetadataReference(Assembly assembly)
    {
        unsafe
        {
            return assembly.TryGetRawMetadata(out var blob, out var length)
                ? AssemblyMetadata
                    .Create(ModuleMetadata.CreateFromMetadata((IntPtr)blob, length))
                    .GetReference()
                : throw new InvalidOperationException($"Could not get raw metadata for type {assembly.GetType()}");
        }
    }

    private static IEnumerable<Assembly> GetAssemblies()
    {
        var returnAssemblies = new List<Assembly>();
        var loadedAssemblies = new HashSet<string>();
        var assembliesToCheck = new Queue<Assembly>();

        assembliesToCheck.Enqueue(Assembly.GetEntryAssembly());

        while (assembliesToCheck.Count != 0)
        {
            var assemblyToCheck = assembliesToCheck.Dequeue();

            foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
                if (!loadedAssemblies.Contains(reference.FullName))
                {
                    var assembly = Assembly.Load(reference);
                    assembliesToCheck.Enqueue(assembly);
                    loadedAssemblies.Add(reference.FullName);
                    returnAssemblies.Add(assembly);
                }
        }

        return returnAssemblies;
    }
}