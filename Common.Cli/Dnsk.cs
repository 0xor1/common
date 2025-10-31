using System.Text.RegularExpressions;
using Common.Shared;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;

namespace Common.Cli;

public partial class Dnsk
{
    private static readonly Key DnskKey = new("dnsk");
    private readonly ILogger<Dnsk> _log;

    public Dnsk(ILogger<Dnsk> log)
    {
        _log = log;
        Console.WriteLine(_log.IsEnabled(LogLevel.Critical));
    }

    /// <summary>
    /// Clones the dnsk repo in <paramref name="reposPath"/> directory to <paramref name="key"/>
    /// </summary>
    /// <param name="reposPath">The path to the parent directory where code repos are stored</param>
    /// <param name="key">The Key of the new repo</param>
    [Command("dnsk")]
    public async Task Run([Argument] string reposPath, [Argument] Key key)
    {
        var dnskPath = Path.Join(reposPath, DnskKey.ToString());
        var newPath = Path.Join(reposPath, key.ToString());
        Throw.OpIf(!Directory.Exists(dnskPath), $"{dnskPath} directory doesn't exists.");
        Throw.OpIf(Directory.Exists(newPath), $"{newPath} directory already exists.");
        Directory.CreateDirectory(newPath);
        await CopyDir(dnskPath, newPath, key, false);
    }

    private async Task CopyDir(string src, string dst, Key key, bool isProj = true)
    {
        _log.LogInformation("Copying {Src} to {Dst}", src, dst);
        _log.LogInformation("Creating Directory {Dst}", dst);
        Directory.CreateDirectory(dst);
        Directory
            .GetFiles(src)
            .ForEach(async file =>
            {
                var fileName = ReplaceDnsk(Path.GetFileName(file), key);
                var dstFile = Path.Join(dst, fileName);
                _log.LogInformation("Copying {File} to {DstFile}", file, dstFile);
                if (ShouldJustCopy(file))
                {
                    // for the git repo files and non text files just do direct copy
                    File.Copy(file, dstFile);
                    return;
                }
                var content = await File.ReadAllTextAsync(file);
                content = ReplaceDnsk(content, key);
                await File.WriteAllTextAsync(dstFile, content);
            });
        Directory
            .GetDirectories(src)
            .Where(x =>
                !isProj || x.Split(Path.DirectorySeparatorChar).Last() is not ("bin" or "obj")
            )
            .ForEach(async dir =>
            {
                var dirName = dir.Split(Path.DirectorySeparatorChar).Last();
                await CopyDir(
                    dir,
                    Path.Join(dst, ReplaceDnsk(dirName, key)),
                    key,
                    dirName.StartsWith($"{DnskKey.ToPascal()}.")
                );
            });
    }

    private static string ReplaceDnsk(string src, Key key) =>
        src.Replace(DnskKey.ToPascal(), key.ToPascal()).Replace(DnskKey.ToCamel(), key.ToCamel());

    private static bool ShouldJustCopy(string file) =>
        file.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}")
        || SkipExtensionsRegex().IsMatch(file);

    [GeneratedRegex(@"\.(png|ico)$")]
    private static partial Regex SkipExtensionsRegex();
}
